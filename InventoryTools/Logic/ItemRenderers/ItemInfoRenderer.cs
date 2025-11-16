using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using InventoryTools.Extensions;
using Lumina.Excel;
using Lumina.Excel.Sheets;


namespace InventoryTools.Logic.ItemRenderers;

public abstract class ItemInfoRenderer<T> : IItemInfoRenderer where T : ItemSource
{
    public ITextureProvider TextureProvider { get; }
    public IDalamudPluginInterface DalamudPluginInterface { get; }
    public ItemSheet ItemSheet { get; }
    public MapSheet MapSheet { get; }

    public ItemInfoRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface,
        ItemSheet itemSheet, MapSheet mapSheet)
    {
        TextureProvider = textureProvider;
        DalamudPluginInterface = dalamudPluginInterface;
        ItemSheet = itemSheet;
        MapSheet = mapSheet;
    }

    public void DrawItems(string sectionName, IReadOnlyList<ItemInfo> items)
    {
        if (items.Count == 0)
        {
            return;
        }
        ImGui.TextUnformatted(sectionName);
        using (ImRaii.PushIndent())
        {
            foreach (var itemInfo in items)
            {
                if (itemInfo.ItemId == 0)
                    continue;

                var item = ItemSheet.GetRow(itemInfo.ItemId);
                ImGui.Image(
                    TextureProvider.GetFromGameIcon(new GameIconLookup(item.Icon, itemInfo.IsHighQuality ?? false)).GetWrapOrEmpty().Handle,
                    new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale
                );
                ImGui.SameLine();
                if (itemInfo.Count == null)
                {
                    ImGui.TextUnformatted($"{item.NameString}" + (itemInfo.IsOptional ?? false ? " (Optional)" : ""));
                }
                else
                {
                    ImGui.TextUnformatted($"{item.NameString} x {itemInfo.Count}" + (itemInfo.IsOptional ?? false ? " (Optional)" : ""));
                }
                if (itemInfo.Min != null && itemInfo.Max != null)
                {
                    ImGui.SameLine();
                    if (itemInfo.Min == itemInfo.Max)
                    {
                        ImGui.Text("(Drops 1)");
                    }
                    else
                    {
                        ImGui.Text("(Drops " + itemInfo.Min.Value + " - " + itemInfo.Max.Value + ")");
                    }
                }
            }
        }
    }

    public void DrawItems(string sectionName, IReadOnlyList<RowRef<Item>> items)
    {
        if (items.Count == 0)
        {
            return;
        }
        ImGui.TextUnformatted(sectionName);
        using (ImRaii.PushIndent())
        {
            foreach (var itemInfo in items)
            {
                if (itemInfo.RowId == 0)
                    continue;

                var item = ItemSheet.GetRow(itemInfo.RowId);
                ImGui.Image(
                    TextureProvider.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrEmpty().Handle,
                    new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale
                );
                ImGui.SameLine();
                ImGui.TextUnformatted($"{item.NameString}");
            }
        }
    }

    public void DrawMaps(List<ItemSource> sources)
    {
        if (sources.All(c => c.MapIds == null || c.MapIds.Count == 0))
        {
            return;
        }

        var maps = sources.SelectMany(source => source.MapIds == null || source.MapIds.Count == 0
            ? new List<string>()
            : source.MapIds.DistinctBy(c => c).Select(c => MapSheet.GetRow(c).FormattedName)).ToList();

        if (maps.Count != 0)
        {
            ImGui.Text("Maps:");
            using (ImRaii.PushIndent())
            {
                foreach (var map in maps)
                {
                    ImGui.Text(map);
                }
            }
        }

    }

    public void DrawMaps(ItemSource source)
    {
        if (source.MapIds == null)
        {
            return;
        }
        var maps = source.MapIds?.Select(c => MapSheet.GetRow(c).FormattedName).Distinct().ToList() ?? [];

        if (maps.Count != 0)
        {
            ImGui.Text("Maps:");
            using (ImRaii.PushIndent())
            {
                foreach (var map in maps)
                {
                    ImGui.Text(map);
                }
            }
        }
    }

    public void DrawLocations(string sectionName, List<ILocation> locations)
    {
        ImGui.TextUnformatted(sectionName);
        using (ImRaii.PushIndent())
        {
            foreach (var groupedMaps in locations.DistinctBy(c => (c.Map.RowId, c.Map, c.MapY)).GroupBy(c => c.Map.RowId))
            {
                if (!groupedMaps.Any())
                {
                    return;
                }

                var map = MapSheet.GetRowOrDefault(groupedMaps.Key);
                if (map != null)
                {
                    var spawns = string.Join(", ", groupedMaps.Select(location => $"{location.MapX.ToString("N2", CultureInfo.InvariantCulture)}/{location.MapY.ToString("N2", CultureInfo.InvariantCulture)}"));
                    ImGui.Text($"{map.FormattedName}");
                    using (ImRaii.PushIndent())
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted(spawns);
                        ImGui.PopTextWrapPos();
                    }
                }
            }
        }
    }

    public void DrawSection(string sectionName, List<string> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        ImGui.TextUnformatted(sectionName);
        using (ImRaii.PushIndent())
        {
            foreach (var item in items)
            {
                ImGui.TextUnformatted(item);
            }
        }
    }

    public Type ItemSourceType => typeof(T);
    public virtual IReadOnlyList<ItemInfoRenderCategory>? Categories => null;
    public abstract RendererType RendererType { get; }
    public abstract ItemInfoType Type { get; }
    public abstract string SingularName { get; }
    public virtual string? PluralName => null;
    public abstract string HelpText { get; }
    public abstract bool ShouldGroup { get; }
    public virtual Func<List<ItemSource>, List<List<ItemSource>>>? CustomGroup => null;
    public abstract Action<ItemSource> DrawTooltip { get; }
    public virtual Action<List<ItemSource>>? DrawTooltipGrouped => null;

    public virtual Func<ItemSource, List<MessageBase>?>? OnClick => null;
    public virtual Func<ItemSource, List<MessageBase>?>? OnRightClick => null;

    public virtual Func<ItemSource, List<MessageBase>>? DrawMenu => null;
    public abstract Func<ItemSource, string> GetName { get; }
    public abstract Func<ItemSource, int> GetIcon { get; }
    public abstract Func<ItemSource, string> GetDescription { get; }
    public virtual byte MaxColumns { get; set; } = 3;
    public virtual float TooltipChildWidth { get; set; } = 250;
    public virtual float TooltipChildHeight { get; set; } = 150;

    public T AsSource(ItemSource source)
    {
        return (T)source;
    }

    public List<T> AsSource(List<ItemSource> source)
    {
        return source.Cast<T>().ToList();
    }
}