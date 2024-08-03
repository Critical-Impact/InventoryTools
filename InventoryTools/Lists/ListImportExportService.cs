using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Services;
using Newtonsoft.Json;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Lists;

public class ListImportExportService
{
    private readonly ExcelCache _excelCache;

    public ListImportExportService(VersionInfo info ,ExcelCache excelCache)
    {
        _excelCache = excelCache;
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
    
    /// <summary>
    /// Parses a teamcraft/AT list of items and returns the item ID and quantity of the item as a list of tuples. HQ items have 500000 added to them.
    /// </summary>
    /// <param name="teamCraftList"></param>
    /// <returns></returns>
    public List<(uint, uint)>? FromTCString(string teamCraftList)
    {
        if (string.IsNullOrEmpty(teamCraftList)) return null;
        List<(uint, uint)> output = new List<(uint, uint)>();
        using (System.IO.StringReader reader = new System.IO.StringReader(teamCraftList))
        {
            string line;
            while ((line = reader.ReadLine()!) != null)
            {
                var parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    continue;

                if (parts[0][^1] == 'x')
                {
                    int numberOfItem = int.Parse(parts[0].Substring(0, parts[0].Length - 1));
                    var builder = new StringBuilder();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        builder.Append(parts[i]);
                        builder.Append(" ");
                    }

                    var isHq = false;
                    var item = builder.ToString().Trim();

                    if (item.Contains("(HQ)"))
                    {
                        item = item.Replace("(HQ)", string.Empty);
                        isHq = true;
                    }

                    var itemEx = _excelCache.GetItemExSheet().FirstOrDefault(c => c!.NameString == item, null);

                    if (itemEx != null && _excelCache.CanCraftItem(itemEx.RowId))
                    {
                        output.Add(((uint)(itemEx.RowId + (isHq ? 500000 : 0)), (uint)numberOfItem));
                    }
                }

            }
        }

        if (output.Count == 0) return null;

        return output;
    }

    public string ToTCString(List<CraftItem> craftItems)
    {
        List<string> lines = new();
        lines.Add("Items :");
        foreach (var craftItem in craftItems)
        {
            lines.Add($"{craftItem.QuantityRequired}x {craftItem.Name} {(craftItem.Flags == InventoryItem.ItemFlags.HighQuality ? " (HQ)" : "")}");
        }
        
        return string.Join(Environment.NewLine, lines);
    }
    
    public string ToTCString(List<CuratedItem> curatedItems)
    {
        List<string> lines = new();
        lines.Add("Items :");
        foreach (var curatedItem in curatedItems)
        {
            var item = _excelCache.GetItemExSheet().GetRow(curatedItem.ItemId);
            if (item == null) continue;
            lines.Add($"{curatedItem.Quantity}x {item.Name} {(curatedItem.ItemFlags == InventoryItem.ItemFlags.HighQuality ? " (HQ)" : "")}");
        }
        
        return string.Join(Environment.NewLine, lines);
    }
}