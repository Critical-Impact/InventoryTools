using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Ionide.KeepAChangelog;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using SemVersion;

namespace InventoryTools.Services;

public class ChangelogService
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IPluginLog _pluginLog;
    private readonly List<(SemanticVersion, DateTime, Domain.ChangelogData)> changeLogs;

    public ChangelogService(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog)
    {
        _pluginInterface = pluginInterface;
        _pluginLog = pluginLog;
        var assemblyLocation = _pluginInterface.AssemblyLocation.DirectoryName!;
        var fileName = Path.Combine(assemblyLocation, "CHANGELOG.md");
        var parsedLogs = Ionide.KeepAChangelog.Parser.parseChangeLog(new FileInfo(fileName));
        if (!parsedLogs.IsOk)
        {
            this.changeLogs = new();
            _pluginLog.Error($"Could not parse changelog file due to error: {parsedLogs.ErrorValue.Item1}");
        }
        else
        {
            this.changeLogs = new();
            foreach (var log in parsedLogs.ResultValue.Releases)
            {
                ChangeLogs.Add((log.Item1, log.Item2, log.Item3.Value));
            }

            this.changeLogs = ChangeLogs.OrderByDescending(c => c.Item1).ToList();
        }


    }

    public List<(SemanticVersion, DateTime, Domain.ChangelogData)> ChangeLogs => changeLogs;
}