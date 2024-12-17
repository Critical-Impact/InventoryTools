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

public class ItemCalamitySalvagerShopUseRenderer : ItemCalamitySalvagerShopSourceRenderer
{
    private readonly MapSheet _mapSheet;
    private readonly ItemSheet _itemSheet;

    public override string HelpText => "Can the item be spent at the calamity salvager?";

    public ItemCalamitySalvagerShopUseRenderer(MapSheet mapSheet, ItemSheet itemSheet) : base(mapSheet, itemSheet)
    {
        _mapSheet = mapSheet;
        _itemSheet = itemSheet;
    }

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);
        var allGilShops = asSources.ToList();
        var maps = allGilShops.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
            ? new List<string>()
            : shopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).Distinct().ToList();

        ImGui.Text($"{allGilShops.Count} items available for purchase with gil in {maps.Count} zones");
    };


    public override RendererType RendererType => RendererType.Use;
}

public class ItemCalamitySalvagerShopSourceRenderer : ItemInfoRenderer<ItemCalamitySalvagerShopSource>
{
    private readonly MapSheet _mapSheet;
    private readonly ItemSheet _itemSheet;

    public ItemCalamitySalvagerShopSourceRenderer(MapSheet mapSheet, ItemSheet itemSheet)
    {
        _mapSheet = mapSheet;
        _itemSheet = itemSheet;
    }

    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Shop];
    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.CalamitySalvagerShop;
    public override string SingularName => "Calamity Salvager";
    public override string PluralName => "Calamity Salvagers";
    public override bool ShouldGroup => true;
    public override string HelpText => "Can the item be purchased from the Calamity Salvager?";

    public override byte MaxColumns => 1;

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);
        var firstItem = asSources.First();
        var allCalamitySalvagerShops = asSources.Cast<ItemCalamitySalvagerShopSource>().ToList();
        var maps = allCalamitySalvagerShops.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
            ? new List<string>()
            : shopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).ToList();

        ImGui.Text("Costs:");

        using (ImRaii.PushIndent())
        {
            var itemName = firstItem.CostItem!.NameString;
            var count = firstItem.Cost;
            var costString = $"{itemName} x {count}";
            ImGui.Text(costString);

            if (firstItem.GilShopItem.Base.AchievementRequired.RowId != 0)
            {
                ImGui.Text(
                    $"Achievement Required: {firstItem.GilShopItem.Base.AchievementRequired.Value.Name.ExtractText()}");
            }

            foreach (var quest in firstItem.GilShopItem.Base.QuestRequired)
            {
                if (quest.RowId != 0)
                {
                    ImGui.Text(
                        $"Quest Required: {quest.Value.Name.ExtractText()}");
                }
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

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var maps = source.MapIds?.Select(c => _mapSheet.GetRow(c).FormattedName).ToList() ?? new List<string>();

        ImGui.Text("Costs:");

        using (ImRaii.PushIndent())
        {
            var itemName = asSource.CostItem!.NameString;
            var count = asSource.Cost;
            var costString = $"{itemName} x {count}";
            ImGui.Text(costString);

            if (asSource.GilShopItem.Base.AchievementRequired.RowId != 0)
            {
                ImGui.Text(
                    $"Achievement Required: {asSource.GilShopItem.Base.AchievementRequired.Value.Name.ExtractText()}");
            }

            foreach (var quest in asSource.GilShopItem.Base.QuestRequired)
            {
                if (quest.RowId != 0)
                {
                    ImGui.Text(
                        $"Quest Required: {quest.Value.Name.ExtractText()}");
                }
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

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        if (asSource.MapIds == null || asSource.MapIds.Count == 0)
        {
            return asSource.GilShop.Name;
        }

        var maps = asSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName);
        return asSource.GilShop.Name + "(" + maps + ")";
    };

    public override Func<ItemSource, int> GetIcon => source => Icons.CalamitySalvagerBag;
}