using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Services;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCalamitySalvagerShopUseRenderer : ItemCalamitySalvagerShopSourceRenderer
{
    private readonly MapSheet _mapSheet;
    private readonly ItemSheet _itemSheet;
    private readonly ITextureProvider _textureProvider;

    public override string HelpText => "Can the item be spent at the calamity salvager?";

    public ItemCalamitySalvagerShopUseRenderer(MapSheet mapSheet, ItemSheet itemSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(mapSheet, itemSheet, textureProvider, dalamudPluginInterface)
    {
        _mapSheet = mapSheet;
        _itemSheet = itemSheet;
        _textureProvider = textureProvider;
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
    public ItemCalamitySalvagerShopSourceRenderer(MapSheet mapSheet, ItemSheet itemSheet,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
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
        var firstItem = asSources[0];

        var costItems = asSources.SelectMany(c => c.GilShopItem.Costs).DistinctBy(d => d.Item.RowId).ToList();

        Span<ItemInfo> costItemInfos = stackalloc ItemInfo[costItems.Count];

        for (var index = 0; index < costItems.Count; index++)
        {
            costItemInfos[index] = new ItemInfo(
                costItems[index].Item.RowId,
                costItems[index].Count,
                costItems[index].IsHq ?? false
            );
        }

        DrawItems("Costs: ", costItemInfos);

        var rewardItems = asSources.SelectMany(c => c.GilShopItem.Rewards).DistinctBy(d => d.Item.RowId).ToList();

        Span<ItemInfo> rewardItemInfos = stackalloc ItemInfo[rewardItems.Count];

        for (var index = 0; index < rewardItems.Count; index++)
        {
            rewardItemInfos[index] = new ItemInfo(
                rewardItems[index].Item.RowId,
                rewardItems[index].Count,
                rewardItems[index].IsHq ?? false
            );
        }

        DrawItems("Rewards: ", rewardItemInfos);



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

        DrawMaps(sources);
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);

        var costs = asSource.GilShopItem.Costs.ToList();

        Span<ItemInfo> costItemInfos = stackalloc ItemInfo[costs.Count()];

        for (var index = 0; index < costs.Count; index++)
        {
            costItemInfos[index] = new ItemInfo(
                costs[index].Item.RowId,
                costs[index].Count,
                costs[index].IsHq ?? false
            );
        }

        DrawItems("Costs: ", costItemInfos);

        var rewardItems = asSource.GilShopItem.Rewards.ToList();

        Span<ItemInfo> rewardItemInfos = stackalloc ItemInfo[rewardItems.Count];

        for (var index = 0; index < rewardItems.Count; index++)
        {
            rewardItemInfos[index] = new ItemInfo(
                rewardItems[index].Item.RowId,
                rewardItems[index].Count,
                rewardItems[index].IsHq ?? false
            );
        }

        DrawItems("Rewards: ", rewardItemInfos);

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

        DrawMaps(asSource);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        if (asSource.MapIds == null || asSource.MapIds.Count == 0)
        {
            return asSource.GilShop.Name;
        }

        var maps = asSource.MapIds.Select(c => MapSheet.GetRow(c).FormattedName);
        return asSource.GilShop.Name + "(" + maps + ")";
    };

    public override Func<ItemSource, int> GetIcon => source => Icons.CalamitySalvagerBag;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var itemName = asSource.CostItem!.NameString;
        var count = asSource.Cost;
        var description = $"{itemName} x {count}";

        if (asSource.GilShopItem.Base.AchievementRequired.RowId != 0)
        {
            description += $" (Requires achievement: {asSource.GilShopItem.Base.AchievementRequired.Value.Name.ExtractText()})";
        }

        foreach (var quest in asSource.GilShopItem.Base.QuestRequired)
        {
            if (quest.RowId != 0)
            {
                description += ($" (Requires quest: {quest.Value.Name.ExtractText()})");
            }
        }

        return description;
    };
}