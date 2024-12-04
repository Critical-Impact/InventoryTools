using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Services;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemSpecialShopUseRenderer : ItemSpecialShopSourceRenderer
{
    public ItemSpecialShopUseRenderer(ImGuiService imGuiService, MapSheet mapSheet) : base(imGuiService, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Use;

    public override string HelpText => "Can the item be spent at a special currency shop?";

    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        return asSource.Item.Icon;
    };
}

public class ItemSpecialShopSourceRenderer : ItemInfoRenderer<ItemSpecialShopSource>
{
    private readonly ImGuiService _imGuiService;
    private readonly MapSheet _mapSheet;

    public ItemSpecialShopSourceRenderer(ImGuiService imGuiService, MapSheet mapSheet)
    {
        _imGuiService = imGuiService;
        _mapSheet = mapSheet;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.SpecialShop;
    public override string SingularName => "Special Shop";
    public override string PluralName => "Special Shops";
    public override string HelpText => "Can the item be purchased from a special currency shop?";
    public override bool ShouldGroup => true;

    public override byte MaxColumns => 1;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Shop];

    public override Func<List<ItemSource>, List<List<ItemSource>>>? CustomGroup => sources =>
    {
        return sources.GroupBy(c => (c.CostItems.Count, c.CostItems.FirstOrDefault()?.RowId ?? null)).Select(c => c.ToList()).ToList();
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var maps = asSource.MapIds == null || asSource.MapIds.Count == 0
            ? null
            : asSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName);

        ImGui.Text($"Shop: {asSource.Shop.Name}");
        ImGui.Text("Rewards:");
        using (ImRaii.PushIndent())
        {
            foreach (var reward in asSource.ShopListing.Rewards)
            {
                ImGui.Image(_imGuiService.GetIconTexture(reward.Item.Icon).ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                ImGui.SameLine();
                var itemName = reward.Item.NameString;
                var count = reward.Count;
                var costString = $"{itemName} x {count}";
                ImGui.Text(costString);
                if (reward.IsHq == true)
                {
                    ImGui.Image(_imGuiService.GetImageTexture("hq").ImGuiHandle,
                        new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                }
            }
        }
        ImGui.Text("Costs:");
        using (ImRaii.PushIndent())
        {
            foreach (var cost in asSource.ShopListing.Costs)
            {
                ImGui.Image(_imGuiService.GetIconTexture(cost.Item.Icon).ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                ImGui.SameLine();
                var itemName = cost.Item.NameString;
                var count = cost.Count;
                var costString = $"{itemName} x {count}";
                ImGui.Text(costString);
                if (cost.IsHq == true)
                {
                    ImGui.Image(_imGuiService.GetImageTexture("hq").ImGuiHandle,
                        new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                }
            }
        }

        if (maps != null)
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
        var costs = String.Join(", ",
            asSource.ShopListing.Costs.Select(c => c.Item.NameString + " (" + c.Count + ")"));
        if (asSource.ShopListing.Rewards.Count() > 1)
        {
            var rewards = String.Join(", ",
                asSource.ShopListing.Rewards.Select(c => c.Item.NameString + " (" + c.Count + ")"));
            return $"Costs {costs} - Rewards {rewards}";
        }

        return $"Costs {costs}";
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        return asSource.CostItems.FirstOrDefault()?.Icon ?? asSource.Item.Icon;
    };
}