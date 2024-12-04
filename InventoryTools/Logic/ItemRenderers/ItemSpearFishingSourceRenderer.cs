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
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemSpearfishingSourceRenderer : ItemInfoRenderer<ItemSpearfishingSource>
{
    private readonly MapSheet _mapSheet;

    public ItemSpearfishingSourceRenderer(MapSheet mapSheet)
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

        var maps = asSources.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
            ? new List<string>()
            : shopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).Distinct().ToList();

        var level = asSources.First().SpearfishingItemRow.Base.GatheringItemLevel.Value.GatheringItemLevel;
        ImGui.Text("Level:" + (level == 0 ? "N/A" : level));

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
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);

        var maps = asSource.MapIds?.Select(c => _mapSheet.GetRow(c).FormattedName).Distinct().ToList() ?? [];

        var level = asSource.SpearfishingItemRow.Base.GatheringItemLevel.Value.GatheringItemLevel;
        ImGui.Text("Level:" + (level == 0 ? "N/A" : level));

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
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.Item.NameString;
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.Spearfishing;
}