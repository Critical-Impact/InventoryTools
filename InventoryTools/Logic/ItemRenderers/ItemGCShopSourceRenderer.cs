using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemGCShopUseRenderer : ItemGCShopSourceRenderer
{
    public ItemGCShopUseRenderer(MapSheet mapSheet, ExcelSheet<GCRankGridaniaMaleText> rankSheet) : base(mapSheet, rankSheet)
    {
    }

    public override string HelpText => "Can the item be spent at a grand company shop?";

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);

        var maps = asSources.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
            ? new List<string>()
            : shopSource.MapIds.Select(c => MapSheet.GetRow(c).FormattedName)).Distinct().ToList();

        ImGui.Text("Items that can be purchased:");

        using (ImRaii.PushIndent())
        {
            foreach (var asSource in asSources.DistinctBy(c => c.Item).Select(c => c.Item.NameString))
            {
                ImGui.TextUnformatted(asSource);
            }
        }

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

    public override RendererType RendererType => RendererType.Use;
}

public class ItemGCShopSourceRenderer : ItemInfoRenderer<ItemGCShopSource>
{
    public MapSheet MapSheet { get; }
    private readonly ExcelSheet<GCRankGridaniaMaleText> _rankSheet;

    public ItemGCShopSourceRenderer(MapSheet mapSheet, ExcelSheet<GCRankGridaniaMaleText> rankSheet)
    {
        MapSheet = mapSheet;
        _rankSheet = rankSheet;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.GCShop;
    public override string SingularName => "Grand Company Shop";
    public override string PluralName => "Grand Company Shops";
    public override string HelpText => "Can the item be purchased at your grand company shop?";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Shop];

    public override byte MaxColumns => 1;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var maps = asSource.MapIds?.Distinct().Select(c => MapSheet.GetRow(c).FormattedName).ToList() ?? new List<string>();

        ImGui.Text($"Cost: {asSource.CostItem!.NameString} x {asSource.GCScripShopItem.Base.CostGCSeals}");
        if (asSource.GCScripShopItem.Base.RequiredGrandCompanyRank.IsValid)
        {
            var genericRank = _rankSheet
                .GetRow(asSource.GCScripShopItem.Base.RequiredGrandCompanyRank.RowId).NameRank.ExtractText()
                .ToTitleCase();
            ImGui.Text($"Rank Required: " + genericRank);
        }

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
        if (asSource.MapIds == null || asSource.MapIds.Count == 0)
        {
            return asSource.GcShop.Name;
        }

        var maps = asSource.MapIds.Distinct().Select(c => MapSheet.GetRow(c).FormattedName);
        return asSource.GcShop.Name + "(" + maps + ")";
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        var grandCompanyRowId = asSource.GCScripShopItem.Category.GrandCompany.RowId;
        switch (grandCompanyRowId)
        {

        }
        return Icons.GrandCompany3;
    };
}