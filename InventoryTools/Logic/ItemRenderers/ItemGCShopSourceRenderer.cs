using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemGCShopUseRenderer : ItemGCShopSourceRenderer
{
    public ItemGCShopUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ExcelSheet<GCRankGridaniaMaleText> rankSheet,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, rankSheet, textureProvider, dalamudPluginInterface)
    {
    }

    public override string HelpText => "Can the item be spent at a grand company shop?";

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);

        ImGui.Text("Items that can be purchased:");

        using (ImRaii.PushIndent())
        {
            foreach (var asSource in asSources.DistinctBy(c => c.Item).Select(c => c.Item.NameString))
            {
                ImGui.TextUnformatted(asSource);
            }
        }

        DrawMaps(sources);
    };

    public override RendererType RendererType => RendererType.Use;

}

public class ItemGCShopSourceRenderer : ItemInfoRenderer<ItemGCShopSource>
{
    public MapSheet MapSheet { get; }
    private readonly ExcelSheet<GCRankGridaniaMaleText> _rankSheet;
    private readonly ITextureProvider _textureProvider;

    public ItemGCShopSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet,
        ExcelSheet<GCRankGridaniaMaleText> rankSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        MapSheet = mapSheet;
        _rankSheet = rankSheet;
        _textureProvider = textureProvider;
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

        ImGui.Image(_textureProvider.GetFromGameIcon(new GameIconLookup(asSource.CostItem!.Icon)).GetWrapOrEmpty().ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
        ImGui.SameLine();
        ImGui.Text($"Cost: {asSource.CostItem.NameString} x {asSource.GCScripShopItem.Base.CostGCSeals}");
        if (asSource.GCScripShopItem.Base.RequiredGrandCompanyRank.IsValid)
        {
            var genericRank = _rankSheet
                .GetRow(asSource.GCScripShopItem.Base.RequiredGrandCompanyRank.RowId).NameRank.ExtractText()
                .ToTitleCase();
            ImGui.Text($"Rank Required: " + genericRank);
        }

        DrawMaps(source);
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
            case 1:
            case 2:
            case 3:
                break;

        }
        return Icons.GrandCompany3;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var description = $"{asSource.Shop.Name}";
        var rewards = string.Join(", ", asSource.GCScripShopItem.Rewards.Select(c => c.Item.NameString + " x " + c.Count + ""));
        var costs = string.Join(", ", asSource.GCScripShopItem.Costs.Select(c => c.Item.NameString + " x " + c.Count + ""));
        return $"{description} ({rewards}) for ({costs})";
    };
}