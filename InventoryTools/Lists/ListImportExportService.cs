using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Services;
using Newtonsoft.Json;

namespace InventoryTools.Lists;

public class ListImportExportService
{
    public ListImportExportService(VersionInfo info)
    {
        CurrentVersion = (byte)info.ImportExportVersion;
    }

    public byte CurrentVersion { get; }

    public string ToBase64(FilterConfiguration configuration)
    {
        var toExport = configuration.Clone()!;
        toExport.DestinationInventories = new List<(ulong, InventoryCategory)>();
        toExport.SourceInventories = new List<(ulong, InventoryCategory)>();
        var json  = JsonConvert.SerializeObject(toExport);
        if (json == null)
        {
            throw new Exception("Failed to serialize configuration.");
        }
        var bytes = Encoding.UTF8.GetBytes(json).Prepend(CurrentVersion).ToArray();
        return bytes.ToCompressedBase64();
    }
    public bool FromBase64(string data, out FilterConfiguration filterConfiguration)
    {
        filterConfiguration = new FilterConfiguration();
        try
        {
            var bytes = data.FromCompressedBase64();
            if (bytes.Length == 0 || bytes[0] != CurrentVersion)
            {
                return false;
            }

            var json = Encoding.UTF8.GetString(bytes.AsSpan()[1..]);
            var deserializeObject = JsonConvert.DeserializeObject<FilterConfiguration>(json);
            if (deserializeObject == null)
            {
                return false;
            }

            deserializeObject.Key = Guid.NewGuid().ToString("N");
            filterConfiguration = deserializeObject;

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}