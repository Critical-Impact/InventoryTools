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
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemSpearfishingSourceRenderer : ItemInfoRenderer<ItemSpearfishingSource>
{
    private readonly MapSheet _mapSheet;

    public ItemSpearfishingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _mapSheet = mapSheet;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.Spearfishing;
    public override string SingularName => "Spearfishing";
    public override string HelpText => "Can the item be gathered via spearfishing?";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Fishing];

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);

        var level = asSources.First().SpearfishingItemRow.Base.GatheringItemLevel.Value.GatheringItemLevel;
        ImGui.Text("Level:" + (level == 0 ? "N/A" : level));

        DrawMaps(sources);
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);

        var level = asSource.SpearfishingItemRow.Base.GatheringItemLevel.Value.GatheringItemLevel;
        ImGui.Text("Level:" + (level == 0 ? "N/A" : level));

        DrawMaps(source);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.Item.NameString;
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.Spearfishing;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var level = asSource.SpearfishingItemRow.Base.GatheringItemLevel.Value.GatheringItemLevel;

        return $"Level {(level == 0 ? "N/A" : level)} spot";
    };
}