using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Dalamud.Plugin;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services
{
    public class SupportDumpService
    {
        private readonly ConfigurationManagerService _configurationManagerService;
        private readonly IDalamudPluginInterface _pluginInterface;
        private readonly ILogger<SupportDumpService> _logger;

        public SupportDumpService(ConfigurationManagerService configurationManagerService,
            IDalamudPluginInterface pluginInterface, ILogger<SupportDumpService> logger)
        {
            _configurationManagerService = configurationManagerService;
            _pluginInterface = pluginInterface;
            _logger = logger;
        }

        public SupportDumpResult GenerateDump(string zipPath)
        {
            _configurationManagerService.Save();

            var targets = new List<(string EntryName, string? Path)>
            {
                ("InventoryTools.json", _configurationManagerService.ConfigurationFile),
                ("inventories.csv", _configurationManagerService.InventoryCsv),
                ("dalamud.log", ResolveDalamudLogPath()),
            };

            var includedFiles = new List<string>();
            var missingFiles = new List<string>();

            try
            {
                using var zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                foreach (var (entryName, path) in targets)
                {
                    if (path == null || !File.Exists(path))
                    {
                        _logger.LogWarning("Support dump could not find {EntryName} at {Path}.", entryName, path ?? "<unresolved>");
                        missingFiles.Add(entryName);
                        continue;
                    }

                    try
                    {
                        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                        using var entryStream = entry.Open();

                        using var sourceStream =
                            new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        sourceStream.CopyTo(entryStream);
                        includedFiles.Add(entryName);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Failed to add {EntryName} to the support dump: {Message}", entryName, e.Message);
                        missingFiles.Add(entryName);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create the support dump zip.");
                return new SupportDumpResult(false, zipPath, includedFiles, missingFiles);
            }

            return new SupportDumpResult(true, zipPath, includedFiles, missingFiles);
        }

        private string? ResolveDalamudLogPath()
        {
            var root = _pluginInterface.ConfigDirectory.Parent?.Parent;
            if (root == null)
            {
                return null;
            }

            var candidates = new[]
            {
                Path.Combine(root.FullName, "logs", "dalamud.log"),
                Path.Combine(root.FullName, "dalamud.log"),
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }
    }

    public class SupportDumpResult
    {
        public SupportDumpResult(bool success, string zipPath, List<string> includedFiles, List<string> missingFiles)
        {
            Success = success;
            ZipPath = zipPath;
            IncludedFiles = includedFiles;
            MissingFiles = missingFiles;
        }

        public bool Success { get; }
        public string ZipPath { get; }
        public List<string> IncludedFiles { get; }
        public List<string> MissingFiles { get; }
    }
}