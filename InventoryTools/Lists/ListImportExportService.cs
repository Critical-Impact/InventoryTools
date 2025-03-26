using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Services;
using Newtonsoft.Json;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Lists;

public enum TCExportMode
{
    Required,
    Missing,
}

public class ListImportExportService
{
    private readonly ItemSheet _itemSheet;
    private readonly FilterConfiguration.Factory _filterConfigurationFactory;
    private readonly AutofacResolver _autofacResolver;

    public ListImportExportService(VersionInfo info , ItemSheet itemSheet, FilterConfiguration.Factory filterConfigurationFactory, AutofacResolver autofacResolver)
    {
        _itemSheet = itemSheet;
        _filterConfigurationFactory = filterConfigurationFactory;
        _autofacResolver = autofacResolver;
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
        filterConfiguration = _filterConfigurationFactory.Invoke();
        filterConfiguration.FilterType = FilterType.SearchFilter;
        try
        {
            var bytes = data.FromCompressedBase64();
            if (bytes.Length == 0 || bytes[0] != CurrentVersion)
            {
                return false;
            }

            var json = Encoding.UTF8.GetString(bytes.AsSpan()[1..]);
            var deserializeObject = JsonConvert.DeserializeObject<FilterConfiguration>(json,new JsonSerializerSettings()
            {
                ContractResolver = _autofacResolver
            });
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
    /// <param name="onlyCraftables"></param>
    /// <returns></returns>
    public List<(uint, uint)>? FromTCString(string teamCraftList, bool onlyCraftables = true)
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

                    var itemRow = _itemSheet.FirstOrDefault(c => c!.NameString == item, null);

                    if (itemRow != null && (!onlyCraftables || itemRow.CanBeCrafted))
                    {
                        output.Add(((uint)(itemRow.RowId + (isHq ? 500000 : 0)), (uint)numberOfItem));
                    }
                }

            }
        }

        if (output.Count == 0) return null;

        return output;
    }

    public List<(uint, uint)>? FromGarlandToolsUrl(string garlandToolsUrl)
    {
        garlandToolsUrl = HttpUtility.UrlDecode(garlandToolsUrl);
        if (string.IsNullOrEmpty(garlandToolsUrl))
        {
            return null;
        }

        if (!garlandToolsUrl.Contains("https://garlandtools.org/db/#group/"))
        {
            return null;
        }

        garlandToolsUrl = garlandToolsUrl.Replace("https://garlandtools.org/db/#group/", "");
        var listStart = garlandToolsUrl.IndexOf('{');
        if (listStart == -1)
        {
            return null;
        }
        garlandToolsUrl = garlandToolsUrl.Substring(listStart).TrimStart('{').TrimEnd('}');
        var items = garlandToolsUrl.Split('|');
        var results = new List<(uint, uint)>();
        foreach (var item in items)
        {
            var quantity = 1u;
            var itemId = item;
            if (itemId.Contains("item/"))
            {
                itemId = itemId.Replace("item/", "");
                if (itemId.Contains('+') || itemId.Contains(' '))
                {
#pragma warning disable S3220
                    var split = itemId.Split(['+',' ']);
#pragma warning restore S3220
                    itemId = split[0];
                    if (split.Length > 1 && uint.TryParse(split[1], out uint parsedQuantity))
                    {
                        quantity = parsedQuantity;
                    }
                }

                if (uint.TryParse(itemId, out var parsedItemId))
                {
                    var itemRow = _itemSheet.GetRowOrDefault(parsedItemId);
                    if (itemRow != null)
                    {
                        results.Add((itemRow.RowId, quantity));
                    }
                }
            }
        }
        return results;
    }

    public string ToTCString(List<CraftItem> craftItems, TCExportMode exportMode = TCExportMode.Required)
    {
        List<string> lines = new();
        lines.Add("Items :");
        foreach (var craftItem in craftItems)
        {
            var qty = craftItem.QuantityRequired;
            switch (exportMode)
            {
                case TCExportMode.Missing:
                    qty = craftItem.QuantityMissingOverall;
                    break;
            }

            if (qty != 0)
            {
                lines.Add(
                    $"{qty}x {craftItem.Name} {(craftItem.Flags == InventoryItem.ItemFlags.HighQuality ? " (HQ)" : "")}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    public string ToTCString(List<CuratedItem> curatedItems)
    {
        List<string> lines = new();
        lines.Add("Items :");
        foreach (var curatedItem in curatedItems)
        {
            var item = _itemSheet.GetRowOrDefault(curatedItem.ItemId);
            if (item == null) continue;
            lines.Add($"{curatedItem.Quantity}x {item.Base.Name.ExtractText()} {(curatedItem.ItemFlags == InventoryItem.ItemFlags.HighQuality ? " (HQ)" : "")}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public string ToTCString(List<SearchResult> searchResults)
    {
        List<string> lines = new();
        lines.Add("Items :");
        foreach (var searchResult in searchResults)
        {
            var item = _itemSheet.GetRowOrDefault(searchResult.Item.RowId);
            if (item == null) continue;
            if (searchResult.SortingResult != null && searchResult.InventoryItem != null)
            {
                    lines.Add($"{searchResult.SortingResult.Quantity}x {item.Base.Name.ExtractText()} {(searchResult.SortingResult.InventoryItem.Flags == InventoryItem.ItemFlags.HighQuality ? " (HQ)" : "")}");
            }
            else if (searchResult.CraftItem != null)
            {
                lines.Add($"{searchResult.CraftItem.QuantityRequired}x {item.Base.Name.ExtractText()} {(searchResult.CraftItem.Flags == InventoryItem.ItemFlags.HighQuality ? " (HQ)" : "")}");
            }
            else if (searchResult.InventoryItem != null)
            {
                lines.Add($"{searchResult.InventoryItem.Quantity}x {item.Base.Name.ExtractText()} {(searchResult.InventoryItem.Flags == InventoryItem.ItemFlags.HighQuality ? " (HQ)" : "")}");
            }
            else if (searchResult.CuratedItem != null)
            {
                lines.Add($"{searchResult.CuratedItem.Quantity}x {item.Base.Name.ExtractText()} {(searchResult.CuratedItem.ItemFlags == InventoryItem.ItemFlags.HighQuality ? " (HQ)" : "")}");
            }
            else
            {
                lines.Add($"1x {item.Base.Name.ExtractText()}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}