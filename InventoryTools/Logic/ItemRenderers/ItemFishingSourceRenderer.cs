using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Time;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemFishingSourceRenderer : ItemInfoRenderer<ItemFishingSource>
{
    private readonly MapSheet _mapSheet;

    public ItemFishingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _mapSheet = mapSheet;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.Fishing;
    public override string HelpText => "Can the item be gathered via fishing?";
    public override string SingularName => "Fishing";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Fishing];

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);

        var level = asSources.First().FishParameter.Base.GatheringItemLevel.Value.GatheringItemLevel;
        ImGui.Text("Level:" + (level == 0 ? "N/A" : level));

        DrawMaps(sources);
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);

        var level = asSource.FishParameter.Base.GatheringItemLevel.Value.GatheringItemLevel;
        ImGui.Text("Level:" + (level == 0 ? "N/A" : level));

        DrawMaps(source);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.Item.NameString;
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.FishingIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var level = asSource.FishParameter.Base.GatheringItemLevel.Value.GatheringItemLevel;
        var fishingType = asSource.FishParameter.FishRecordType;

        return $"Level {(level == 0 ? "N/A" : level)} {fishingType} spot";
    };
}