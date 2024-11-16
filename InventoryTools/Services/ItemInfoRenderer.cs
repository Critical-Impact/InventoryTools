using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Time;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Math;
using Humanizer;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using InventoryTools.Extensions;

namespace InventoryTools.Services;


public class ItemInfoRenderer
{
    private readonly MapSheet _mapSheet;
    private readonly ItemSheet _itemSheet;
    private readonly ExcelSheet<GCRankGridaniaMaleText> _gcRankSheet;
    private readonly ISeTime _seTime;
    private readonly ImGuiService _imGuiService;

    public ItemInfoRenderer(MapSheet mapSheet, ItemSheet itemSheet, ExcelSheet<GCRankGridaniaMaleText> gcRankSheet, ISeTime seTime, ImGuiService imGuiService)
    {
        _mapSheet = mapSheet;
        _itemSheet = itemSheet;
        _gcRankSheet = gcRankSheet;
        _seTime = seTime;
        _imGuiService = imGuiService;
    }

    public (string Singular, string? Plural) GetSourceTypeName(ItemInfoType type)
    {
        switch (type)
        {
            case ItemInfoType.CraftRecipe:
                return ("Craft Recipe", "Craft Recipes");
            case ItemInfoType.FreeCompanyCraftRecipe:
                return ("Free Company Craft Recipe", null);
            case ItemInfoType.SpecialShop:
                return ("Special Shop", "Special Shops");
            case ItemInfoType.GilShop:
                return ("Gil Shop", "Gil Shops");
            case ItemInfoType.FCShop:
                return ("Free Company Shop", "Free Company Shops");
            case ItemInfoType.GCShop:
                return ("Grand Company Shop", "Grand Company Shops");
            case ItemInfoType.CashShop:
                return ("SQ Cash Shop", "SQ Cash Shops");
            case ItemInfoType.FateShop:
                return ("Fate Shop", "Fate Shops");
            case ItemInfoType.CalamitySalvagerShop:
                return ("Calamity Salvager", "Calamity Salvager Shops");
            case ItemInfoType.Mining:
                return ("Mining", null);
            case ItemInfoType.Quarrying:
                return ("Quarrying", null);
            case ItemInfoType.Logging:
                return ("Logging", null);
            case ItemInfoType.Harvesting:
                return ("Harvesting", null);
            case ItemInfoType.HiddenMining:
                return ("Mining (Hidden)", null);
            case ItemInfoType.HiddenQuarrying:
                return ("Quarrying (Hidden)", null);
            case ItemInfoType.HiddenLogging:
                return ("Logging (Hidden)", null);
            case ItemInfoType.HiddenHarvesting:
                return ("Harvesting (Hidden)", null);
            case ItemInfoType.TimedMining:
                return ("Mining (Timed)", null);
            case ItemInfoType.TimedQuarrying:
                return ("Quarrying (Timed)", null);
            case ItemInfoType.TimedLogging:
                return ("Logging (Timed)", null);
            case ItemInfoType.TimedHarvesting:
                return ("Harvesting (Timed)", null);
            case ItemInfoType.EphemeralQuarrying:
                return ("Quarrying (Ephemeral)", null);
            case ItemInfoType.EphemeralMining:
                return ("Mining (Ephemeral)", null);
            case ItemInfoType.EphemeralLogging:
                return ("Logging (Ephemeral)", null);
            case ItemInfoType.EphemeralHarvesting:
                return ("Harvesting (Ephemeral)", null);
            case ItemInfoType.Fishing:
                return ("Fishing", null);
            case ItemInfoType.Spearfishing:
                return ("Spearfishing", null);
            case ItemInfoType.Monster:
                return ("Monster", "Monsters");
            case ItemInfoType.Fate:
                return ("Fate", null);
            case ItemInfoType.Desynthesis:
                return ("Desynthesis", null);
            case ItemInfoType.Gardening:
                return ("Gardening", null);
            case ItemInfoType.Loot:
                return ("Loot", null);
            case ItemInfoType.SkybuilderInspection:
                return ("Sky Builder Inspection", null);
            case ItemInfoType.SkybuilderHandIn:
                return ("Sky Builder Hand In", null);
            case ItemInfoType.QuickVenture:
                return ("Quick Venture", null);
            case ItemInfoType.MiningVenture:
                return ("Mining Venture", null);
            case ItemInfoType.MiningExplorationVenture:
                return ("Mining Exploration Venture", "Mining Exploration Ventures");
            case ItemInfoType.BotanyVenture:
                return ("Botany Venture", null);
            case ItemInfoType.BotanyExplorationVenture:
                return ("Botany Exploration Venture", "Botany Exploration Ventures");
            case ItemInfoType.CombatVenture:
                return ("Combat Venture", null);
            case ItemInfoType.CombatExplorationVenture:
                return ("Combat Exploration Venture", "Combat Exploration Ventures");
            case ItemInfoType.FishingVenture:
                return ("Fishing Venture", null);
            case ItemInfoType.FishingExplorationVenture:
                return ("Fishing Exploration Venture", "Fishing Exploration Ventures");
            case ItemInfoType.Reduction:
                return ("Reduction", null);
            case ItemInfoType.Airship:
                return ("Airship Exploration", null);
            case ItemInfoType.Submarine:
                return ("Submarine Exploration", null);
            case ItemInfoType.DungeonChest:
                return ("Dungeon Chest", "Dungeon Chests");
            case ItemInfoType.DungeonBossDrop:
                return ("Dungeon Boss Drop", "Dungeon Boss Drops");
            case ItemInfoType.DungeonBossChest:
                return ("Dungeon Boss Chest", "Dungeon Boss Chests");
            case ItemInfoType.DungeonDrop:
                return ("Dungeon Drop", "Dungeon Drops");
            case ItemInfoType.CustomDelivery:
                return ("Custom Delivery", "Custom Deliveries");
            case ItemInfoType.Aquarium:
                return ("Aquarium", null);
            case ItemInfoType.GCDailySupply:
                return ("Grand Company Daily Supplly", null);
            case ItemInfoType.CraftLeve:
                return ("Craft Leve", "Craft Leves");
            case ItemInfoType.Armoire:
                return ("Armoire", null);
        }

        return (type.ToString(),  null);
    }

    public bool ShouldGroupSource(ItemInfoType itemInfoType)
    {
        switch (itemInfoType)
        {
            case ItemInfoType.CraftRecipe:
                return false;
            case ItemInfoType.FreeCompanyCraftRecipe:
                return false;
            case ItemInfoType.SpecialShop:
                return false;
            case ItemInfoType.Desynthesis:
                break;
            case ItemInfoType.Loot:
                break;
            case ItemInfoType.Reduction:
                break;
        }

        return true;
    }

    public void DrawSource<T>(List<T> items) where T : ItemSource
    {
        var firstItem = items.First();
        var sourceTypeNames = this.GetSourceTypeName(firstItem.Type);
        var sourceTypeName = sourceTypeNames.Singular;
        if (items.Count > 1 && sourceTypeNames.Plural != null)
        {
            sourceTypeName = sourceTypeNames.Plural;
        }

        if (firstItem is ItemCraftResultSource craftResultSource)
        {
            ImGui.Text(sourceTypeName + ":");
            using (ImRaii.PushIndent())
            {
                ImGui.Text($"Craft Type: {craftResultSource.Recipe.Base.CraftType.Value.Name}");
                ImGui.Text($"Yield: {craftResultSource.Recipe.Base.AmountResult}");
                ImGui.Text($"Difficulty: {craftResultSource.Recipe.Base.DifficultyFactor}");
                ImGui.Text($"Required Craftsmanship: {craftResultSource.Recipe.Base.RequiredCraftsmanship}");
            }

            ImGui.Text("Ingredients:");
            using (ImRaii.PushIndent())
            {
                foreach (var ingredient in craftResultSource.Recipe.IngredientCounts)
                {
                    var item = _itemSheet.GetRow(ingredient.Key);
                    ImGui.Text($"{item.NameString} x {ingredient.Value}");
                }
            }
        }
        else if (firstItem is ItemGCShopSource gcShopSource)
        {
            var allGcShopSources = items.Cast<ItemGCShopSource>().ToList();
            ImGui.Text(sourceTypeName + ":");
            var maps = allGcShopSources.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
                ? new List<string>()
                : shopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).ToList();

            using (ImRaii.PushIndent())
            {
                ImGui.Text($"Cost: Company Seal x {gcShopSource.GCScripShopItem.Base.CostGCSeals}");
                if (gcShopSource.GCScripShopItem.Base.RequiredGrandCompanyRank.IsValid)
                {
                    var genericRank = _gcRankSheet
                        .GetRow(gcShopSource.GCScripShopItem.Base.RequiredGrandCompanyRank.RowId).NameRank.ExtractText()
                        .ToTitleCase();
                    ImGui.Text($"Rank Required: " + genericRank);
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
        }
        else if (firstItem is ItemFccShopSource fccShopSource)
        {
            var allShopSources = items.Cast<ItemFccShopSource>().ToList();
            ImGui.Text(sourceTypeName + ":");
            var maps = allShopSources.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
                ? new List<string>()
                : shopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).ToList();

            using (ImRaii.PushIndent())
            {
                ImGui.Text($"Cost: Company Credit x {fccShopSource.FccShopListing.Cost.Count}");
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
        }
        else if (firstItem is ItemCompanyCraftResultSource companyCraftResultSource)
        {
            ImGui.Text(sourceTypeName + ":");
            using (ImRaii.PushIndent())
            {
                ImGui.Text($"Craft Type: {companyCraftResultSource.CompanyCraftSequence.Base.CompanyCraftType.Value.Name}");
            }
            ImGui.Text("Ingredients:");
            using (ImRaii.PushIndent())
            {
                foreach (var ingredient in companyCraftResultSource.CompanyCraftSequence.MaterialsRequired(null))
                {
                    var item = _itemSheet.GetRow(ingredient.ItemId);
                    ImGui.Text($"{item.NameString} x {ingredient.Quantity}");
                }
            }
        }
        else if (firstItem is ItemGatheringSource itemGatheringSource)
        {
            ImGui.Text(sourceTypeName + ":");
            var maps = itemGatheringSource.MapIds == null || itemGatheringSource.MapIds.Count == 0
                ? null
                : itemGatheringSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName);

            using (ImRaii.PushIndent())
            {
                var level = itemGatheringSource.GatheringItem.Base.GatheringItemLevel.Value.GatheringItemLevel;
                ImGui.Text("Level:" + (level == 0 ? "N/A" : level));
                var stars = itemGatheringSource.GatheringItem.Base.GatheringItemLevel.Value.Stars;
                ImGui.Text("Stars:" + (stars == 0 ? "N/A" : stars));
                var perceptionRequired = itemGatheringSource.GatheringItem.Base.PerceptionReq;
                ImGui.Text("Perception Required:" + (perceptionRequired == 0 ? "N/A" : stars));
            }

            if (itemGatheringSource.GatheringItem.AvailableAtTimedNode)
            {
                ImGui.Text("Maps:");
                using (ImRaii.PushIndent())
                {
                    foreach (var gatheringPoint in itemGatheringSource.GatheringItem.GatheringPoints)
                    {
                        var mapName = _mapSheet.GetRow(gatheringPoint.Base.TerritoryType.Value.Map.RowId).FormattedName;
                        var nextUptime = gatheringPoint.GatheringPointTransient.GetGatheringUptime()
                            ?.NextUptime(_seTime.ServerTime) ?? null;
                        if (nextUptime == null
                            || nextUptime.Equals(TimeInterval.Always)
                            || nextUptime.Equals(TimeInterval.Invalid)
                            || nextUptime.Equals(TimeInterval.Never))
                        {
                            continue;
                        }
                        if (nextUptime.Value.Start > TimeStamp.UtcNow)
                        {
                            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed))
                            {
                                ImGui.Text(mapName + ": up in " +
                                                  TimeInterval.DurationString(nextUptime.Value.Start, TimeStamp.UtcNow,
                                                      true));
                            }
                        }
                        else
                        {
                            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen))
                            {
                                ImGui.Text(mapName + " up for " +
                                                  TimeInterval.DurationString(nextUptime.Value.End, TimeStamp.UtcNow,
                                                      true));
                            }
                        }
                    }
                }
            }
            else
            {
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
            }
        }
        else if (firstItem is ItemFishingSource fishingSource)
        {
            ImGui.Text(sourceTypeName + ":");

            var allFishingShops = items.Cast<ItemFishingSource>().ToList();
            ImGui.Text(sourceTypeName + ":");
            var maps = allFishingShops.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
                ? new List<string>()
                : shopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).ToList();

            using (ImRaii.PushIndent())
            {
                var level = fishingSource.FishingSpotRow.Base.GatheringLevel;
                ImGui.Text("Level:" + (level == 0 ? "N/A" : level));
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
        }
        else if (firstItem is ItemSpecialShopSource specialShopSource)
        {
            ImGui.Text(sourceTypeName + ":");
            var maps = specialShopSource.MapIds == null || specialShopSource.MapIds.Count == 0
                ? null
                : specialShopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName);

            ImGui.Text("Costs:");
            using (ImRaii.PushIndent())
            {
                foreach (var cost in specialShopSource.ShopListing.Costs)
                {
                    var itemName = cost.Item.NameString;
                    var count = cost.Count;
                    var costString = $"{itemName} x {count}";
                    ImGui.Text(costString);
                    if (cost.IsHq == true)
                    {
                        ImGui.Image(_imGuiService.GetImageTexture("hq").ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                    }
                }
            }

            if (specialShopSource.ShopListing.Rewards.Count() > 1)
            {
                ImGui.Text("Rewards:");
                using (ImRaii.PushIndent())
                {
                    foreach (var reward in specialShopSource.ShopListing.Rewards)
                    {
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
        }
        else if (firstItem is ItemGilShopSource gilShopSource)
        {
            var allGilShops = items.Cast<ItemGilShopSource>().ToList();
            ImGui.Text(sourceTypeName + ":");
            var maps = allGilShops.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
                ? new List<string>()
                : shopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).ToList();

            ImGui.Text("Costs:");

            using (ImRaii.PushIndent())
            {
                var itemName = gilShopSource.CostItem!.NameString;
                var count = gilShopSource.Cost;
                var costString = $"{itemName} x {count}";
                ImGui.Text(costString);

                if (gilShopSource.GilShopItem.Base.AchievementRequired.RowId != 0)
                {
                    ImGui.Text($"Achievement Required: {gilShopSource.GilShopItem.Base.AchievementRequired.Value.Name.ExtractText()}");
                }

                foreach (var quest in gilShopSource.GilShopItem.Base.QuestRequired)
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
        }
        else if (firstItem is ItemFateShopSource fateShopSource)
        {
            var allShops = items.Cast<ItemFateShopSource>().ToList();
            ImGui.Text(sourceTypeName + ":");
            var maps = allShops.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
                ? new List<string>()
                : shopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).ToList();

            ImGui.Text("Costs:");

            using (ImRaii.PushIndent())
            {
                foreach (var cost in fateShopSource.ShopListing.Costs)
                {
                    var itemName = cost.Item.NameString;
                    var count = cost.Count;
                    var costString = $"{itemName} x {count}";
                    ImGui.Text(costString);
                    if (cost.IsHq == true)
                    {
                        ImGui.Image(_imGuiService.GetImageTexture("hq").ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
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
        }
        else if (firstItem is ItemSpearfishingSource spearfishingSource)
        {
            ImGui.Text(sourceTypeName + ":");

            var allSpearfishing = items.Cast<ItemSpearfishingSource>().ToList();
            ImGui.Text(sourceTypeName + ":");
            var maps = allSpearfishing.SelectMany(source => source.MapIds == null || source.MapIds.Count == 0
                ? new List<string>()
                : source.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).ToList();

            using (ImRaii.PushIndent())
            {
                var level = spearfishingSource.SpearfishingItemRow.Base.GatheringItemLevel.Value.GatheringItemLevel;
                ImGui.Text("Level:" + (level == 0 ? "N/A" : level));
                var stars = spearfishingSource.SpearfishingItemRow.Base.GatheringItemLevel.Value.Stars;
                ImGui.Text("Stars:" + (stars == 0 ? "N/A" : stars));
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
        }
        else if (firstItem is ItemFateSource fateSource)
        {
            var allFates = items.Cast<ItemFateSource>().ToList();
            ImGui.Text(sourceTypeName + ":");
            using (ImRaii.PushIndent())
            {
                foreach (var fate in allFates)
                {
                    ImGui.Text(fate.Fate.Base.Name.ExtractText());
                }
            }
        }
        else if (firstItem is ItemQuickVentureSource quickVentureSource)
        {
            ImGui.Text(sourceTypeName + ":");
            using (ImRaii.PushIndent())
            {
                ImGui.Text($"Venture Cost: {quickVentureSource.RetainerTaskRow.Base.VentureCost}");
                ImGui.Text(
                    $"Time: {quickVentureSource.RetainerTaskRow.Base.MaxTimemin.Minutes().ToHumanReadableString()}");
            }
        }
        else if (firstItem is ItemExplorationVentureSource)
        {
            var allVentures = items.Cast<ItemExplorationVentureSource>().ToList();
            foreach (var explorationVentureSource in allVentures)
            {
                ImGui.Text($"{explorationVentureSource.RetainerTaskRow.FormattedName}");
                using (ImRaii.PushIndent())
                {
                    ImGui.Text($"Venture Cost: {explorationVentureSource.RetainerTaskRow.Base.VentureCost}");
                    ImGui.Text($"Required Level: {explorationVentureSource.RetainerTaskRow.Base.RetainerLevel}");
                    if (explorationVentureSource.RetainerTaskRow.Base.RequiredGathering != 0)
                    {
                        ImGui.Text(
                            $"Required Gathering: {explorationVentureSource.RetainerTaskRow.Base.RequiredGathering}");
                    }

                    if (explorationVentureSource.RetainerTaskRow.Base.RequiredItemLevel != 0)
                    {
                        ImGui.Text(
                            $"Required Item Level: {explorationVentureSource.RetainerTaskRow.Base.RequiredItemLevel}");
                    }

                    ImGui.Text($"Experience: {explorationVentureSource.RetainerTaskRow.Base.Experience}");
                    ImGui.Text(
                        $"Time: {explorationVentureSource.RetainerTaskRow.Base.MaxTimemin.Minutes().ToHumanReadableString()}");
                }
            }
        }
        else if (firstItem is ItemVentureSource)
        {
            var allVentures = items.Cast<ItemVentureSource>().ToList();
            foreach (var ventureSource in allVentures)
            {
                ImGui.Text($"{ventureSource.RetainerTaskRow.FormattedName}");
                using (ImRaii.PushIndent())
                {
                    ImGui.Text($"Venture Cost: {ventureSource.RetainerTaskRow.Base.VentureCost}");
                    ImGui.Text($"Required Level: {ventureSource.RetainerTaskRow.Base.RetainerLevel}");
                    if (ventureSource.RetainerTaskRow.Base.RequiredGathering != 0)
                    {
                        ImGui.Text(
                            $"Required Gathering: {ventureSource.RetainerTaskRow.Base.RequiredGathering}");
                    }

                    if (ventureSource.RetainerTaskRow.Base.RequiredItemLevel != 0)
                    {
                        ImGui.Text(
                            $"Required Item Level: {ventureSource.RetainerTaskRow.Base.RequiredItemLevel}");
                    }

                    ImGui.Text($"Experience: {ventureSource.RetainerTaskRow.Base.Experience}");
                    ImGui.Text(
                        $"Time: {ventureSource.RetainerTaskRow.Base.MaxTimemin.Minutes().ToHumanReadableString()}");
                }
            }
        }
        else if (firstItem is ItemMonsterDropSource)
        {
            ImGui.Text(sourceTypeName + ":");
            foreach (var monsterDropSource in items.Cast<ItemMonsterDropSource>().ToList())
            {
                var allSources = items.Cast<ItemMonsterDropSource>().ToList();
                ImGui.Text(monsterDropSource.BNpcName.Base.Singular.ExtractText().ToTitleCase() + ":");

                var maps = allSources.SelectMany(source => source.MapIds == null || source.MapIds.Count == 0
                    ? new List<string>()
                    : source.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName)).Distinct().ToList();

                using (ImRaii.PushIndent())
                {
                    if (maps.Count != 0)
                    {
                        using (ImRaii.PushIndent())
                        {
                            foreach (var map in maps)
                            {
                                ImGui.Text(map);
                            }
                        }
                    }
                    else
                    {
                        ImGui.Text("No known locations.");
                    }
                }
            }
        }
        else if (firstItem is ItemSkybuilderInspectionSource skybuilderInspectionSource)
        {
            var inspectionSources = items.Cast<ItemSkybuilderInspectionSource>().ToList();
            ImGui.Text(sourceTypeName + ":");
            using (ImRaii.PushIndent())
            {
                foreach (var inspectionSource in inspectionSources)
                {
                    ImGui.Text($"{inspectionSource.CostItem!.NameString} x {inspectionSource.InspectionData.AmountRequired}");
                }
            }
        }
        else
        {
            ImGui.Text(sourceTypeName + ":");
            using var push = ImRaii.PushIndent();
            foreach (var item in items.Select(c => RenderSourceName(c)).Distinct())
            {
                ImGui.Text(item);
            }
        }
    }

    public string RenderSourceName<T>(T item) where T : ItemSource
    {
        switch (item.Type)
        {
            case ItemInfoType.CraftRecipe:
                if (item is ItemCraftResultSource craftResultSource)
                {
                    return craftResultSource.Recipe.Base.ItemResult.Value.Name.ExtractText() + "(" + (craftResultSource.Recipe.CraftType?.FormattedName ?? "Unknown") + ")";
                }
                break;
            case ItemInfoType.FreeCompanyCraftRecipe:
                if (item is ItemCompanyCraftResultSource itemCompanyCraftResult)
                {
                    return itemCompanyCraftResult.Item.Base.Name.ExtractText() + "(" + (itemCompanyCraftResult.CompanyCraftSequence.Base.CompanyCraftType.ValueNullable?.Name.ExtractText() ?? "Unknown") + ")";
                }
                break;
            case ItemInfoType.SpecialShop:
                if (item is ItemSpecialShopSource specialShopSource)
                {
                    var costs = String.Join(", ",
                        specialShopSource.ShopListing.Costs.Select(c => c.Item.NameString + " (" + c.Count + ")"));
                    if (specialShopSource.ShopListing.Rewards.Count() > 1)
                    {
                        var rewards = String.Join(", ",
                            specialShopSource.ShopListing.Rewards.Select(c => c.Item.NameString + " (" + c.Count + ")"));
                        return $"Costs {costs} - Rewards {rewards}";
                    }
                    return $"Costs {costs}";
                }
                break;
            case ItemInfoType.GilShop:
                if (item is ItemGilShopSource gilShopSource)
                {
                    if (gilShopSource.MapIds == null || gilShopSource.MapIds.Count == 0)
                    {
                        return gilShopSource.GilShop.Name;
                    }

                    var maps = gilShopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName);
                    return gilShopSource.GilShop.Name + "(" + maps + ")";
                }
                break;
            case ItemInfoType.FCShop:
                if (item is ItemFccShopSource fccShopSource)
                {
                    return fccShopSource.FccShop.Name;
                }
                break;
            case ItemInfoType.GCShop:
                if (item is ItemGCShopSource gcShopSource)
                {
                    return gcShopSource.GcShop.Name;
                }
                break;
            case ItemInfoType.CashShop:
                if (item is ItemCashShopSource cashShopSource)
                {
                    return (cashShopSource.FittingShopItemSetRow?.Base.Unknown6.ExtractText() ?? "Not in a set");
                }
                break;
            case ItemInfoType.FateShop:
                if (item is ItemFateShopSource fateShopSource)
                {
                    var costs = String.Join(", ",
                        fateShopSource.ShopListing.Costs.Select(c => c.Item.NameString + " (" + c.Count + ")"));
                    if (fateShopSource.ShopListing.Rewards.Count() > 1)
                    {
                        var rewards = String.Join(", ",
                            fateShopSource.ShopListing.Rewards.Select(c => c.Item.NameString + " (" + c.Count + ")").Distinct());
                        return $"Costs {costs} - Rewards {rewards}";
                    }
                    return $"Costs {costs}";
                }
                break;
            case ItemInfoType.CalamitySalvagerShop:
                if (item is ItemGilShopSource calamitySalvagerShopSource)
                {
                    if (calamitySalvagerShopSource.MapIds == null || calamitySalvagerShopSource.MapIds.Count == 0)
                    {
                        return calamitySalvagerShopSource.GilShop.Name;
                    }

                    var maps = calamitySalvagerShopSource.MapIds.Select(c => _mapSheet.GetRow(c).FormattedName);
                    return calamitySalvagerShopSource.GilShop.Name + "(" + maps + ")";
                }
                break;
            case ItemInfoType.Mining:
            case ItemInfoType.Quarrying:
            case ItemInfoType.Logging:
            case ItemInfoType.Harvesting:
            case ItemInfoType.HiddenMining:
            case ItemInfoType.HiddenQuarrying:
            case ItemInfoType.HiddenLogging:
            case ItemInfoType.HiddenHarvesting:
            case ItemInfoType.TimedMining:
            case ItemInfoType.TimedQuarrying:
            case ItemInfoType.TimedLogging:
            case ItemInfoType.TimedHarvesting:
            case ItemInfoType.EphemeralQuarrying:
            case ItemInfoType.EphemeralMining:
            case ItemInfoType.EphemeralLogging:
            case ItemInfoType.EphemeralHarvesting:
                //Handled with custom draw method
                if (item is ItemGatheringSource gatheringSource)
                {
                    return gatheringSource.Item.NameString;
                }
                break;
            case ItemInfoType.Fishing:
                if (item is ItemFishingSource fishingSource)
                {
                    return fishingSource.Item.NameString;
                }
                break;
            case ItemInfoType.Spearfishing:
                if (item is ItemSpearfishingSource spearfishingSource)
                {
                    return spearfishingSource.Item.NameString;
                }
                break;
                break;
            case ItemInfoType.Monster:
                if (item is ItemMonsterDropSource monsterDropSource)
                {
                    return monsterDropSource.BNpcName.Base.Singular.ExtractText().ToTitleCase();
                }
                break;
            case ItemInfoType.Fate:
                break;
            case ItemInfoType.Desynthesis:
                break;
            case ItemInfoType.Gardening:
                break;
            case ItemInfoType.Loot:
                if (item is ItemLootSource lootSource)
                {
                    return (lootSource.CostItem?.NameString ?? "Unknown");
                }
                break;
            case ItemInfoType.SkybuilderInspection:
                break;
            case ItemInfoType.SkybuilderHandIn:
                break;
            case ItemInfoType.QuickVenture:
                break;
            case ItemInfoType.MiningVenture:
                break;
            case ItemInfoType.MiningExplorationVenture:
                break;
            case ItemInfoType.BotanyVenture:
                break;
            case ItemInfoType.BotanyExplorationVenture:
                break;
            case ItemInfoType.CombatVenture:
                break;
            case ItemInfoType.CombatExplorationVenture:
                break;
            case ItemInfoType.FishingVenture:
                break;
            case ItemInfoType.FishingExplorationVenture:
                break;
            case ItemInfoType.Reduction:
                break;
            case ItemInfoType.Airship:
                if (item is ItemAirshipDropSource airshipDropSource)
                {
                    return airshipDropSource.AirshipExplorationPoint.Base.NameShort.ToString();
                }
                break;
            case ItemInfoType.Submarine:
                if (item is ItemSubmarineDropSource submarineDropSource)
                {
                    return submarineDropSource.SubmarineExploration.Base.Location.ToString();
                }
                break;
            case ItemInfoType.DungeonChest:
                if (item is ItemDungeonChestSource dungeonChestSource)
                {
                    return dungeonChestSource.ContentFinderCondition.Base.Name.ExtractText();
                }
                break;
            case ItemInfoType.DungeonBossDrop:
                if (item is ItemDungeonBossDropSource dungeonBossDropSource)
                {
                    return dungeonBossDropSource.ContentFinderCondition.Base.Name.ExtractText() + " - " + dungeonBossDropSource.BNpcName.Base.Singular.ExtractText().ToTitleCase();
                }
                break;
            case ItemInfoType.DungeonBossChest:
                if (item is ItemDungeonBossChestSource dungeonBossChestSource)
                {
                    return dungeonBossChestSource.ContentFinderCondition.Base.Name.ExtractText() + " - " + dungeonBossChestSource.BNpcName.Base.Singular.ExtractText().ToTitleCase();
                }
                break;
            case ItemInfoType.DungeonDrop:
                if (item is ItemDungeonDropSource dungeonDropSource)
                {
                    return dungeonDropSource.ContentFinderCondition.Base.Name.ExtractText();
                }
                break;
            case ItemInfoType.CustomDelivery:
                //Is a use
                break;
            case ItemInfoType.Aquarium:
                //Is a use
                break;
            case ItemInfoType.GCDailySupply:
                //Is a use
                break;
            case ItemInfoType.CraftLeve:
                //Is a use
                break;
            case ItemInfoType.Armoire:
                //Is a use
                break;
        }




        return item.CostItem?.NameString ?? item.Type.ToString();
    }

    public string RenderUseName<T>(T item) where T : ItemSource
    {
        if (item is ItemCraftRequirementSource craftRequirementSource)
        {
            //craftRequirementSource.;
        }
        if (item is ItemAquariumSource aquariumSource)
        {
            return "Aquarium: " + aquariumSource.AquariumFish.Base.AquariumWater.Value.Name.ExtractText() + " (" + aquariumSource.AquariumFish.Size + " )";
        }
        if (item is ItemArmoireSource armoireSource)
        {
            return "Armoire: " + (armoireSource.Cabinet.CabinetCategory?.Base.Category.Value.Text.ExtractText() ?? "Unknown");
        }

        return item.Type.ToString();
    }

    public ushort RenderSourceIcon<T>(T item) where T : ItemSource
    {
        switch (item.Type)
        {
            case ItemInfoType.CraftRecipe:
                if (item is ItemCraftResultSource craftResultSource)
                {
                    if (craftResultSource.Recipe.CraftType != null)
                    {
                        return craftResultSource.Recipe.CraftType.Icon;
                    }
                }
                break;
            case ItemInfoType.FreeCompanyCraftRecipe:
                return Icons.CraftIcon;
                break;
            case ItemInfoType.SpecialShop:
                if (item is ItemSpecialShopSource specialShopSource)
                {
                    return specialShopSource.CostItem?.Icon ?? specialShopSource.Item.Icon;
                }
                break;
            case ItemInfoType.GilShop:
                return _itemSheet.GetRow(1).Icon;
            case ItemInfoType.FCShop:
                return Icons.FreeCompanyCreditIcon;
            case ItemInfoType.GCShop:
                return Icons.GrandCompany3;
            case ItemInfoType.CashShop:
                return Icons.BagStar;
            case ItemInfoType.FateShop:
                return Icons.BicolorGemstone;
            case ItemInfoType.CalamitySalvagerShop:
                return Icons.CalamitySalvagerBag;
            case ItemInfoType.Mining:
            case ItemInfoType.Quarrying:
            case ItemInfoType.Logging:
            case ItemInfoType.Harvesting:
            case ItemInfoType.HiddenMining:
            case ItemInfoType.HiddenQuarrying:
            case ItemInfoType.HiddenLogging:
            case ItemInfoType.HiddenHarvesting:
            case ItemInfoType.TimedMining:
            case ItemInfoType.TimedQuarrying:
            case ItemInfoType.TimedLogging:
            case ItemInfoType.TimedHarvesting:
            case ItemInfoType.EphemeralQuarrying:
            case ItemInfoType.EphemeralMining:
            case ItemInfoType.EphemeralLogging:
            case ItemInfoType.EphemeralHarvesting:
                if (item is ItemGatheringSource gatheringSource)
                {
                    if (gatheringSource.Type == ItemInfoType.Mining)
                    {
                        return Icons.MiningIcon;
                    }

                    if (gatheringSource.Type == ItemInfoType.Quarrying)
                    {
                        return Icons.QuarryingIcon;
                    }

                    if (gatheringSource.Type == ItemInfoType.Harvesting)
                    {
                        return Icons.HarvestingIcon;
                    }

                    if (gatheringSource.Type == ItemInfoType.Logging)
                    {
                        return Icons.LoggingIcon;
                    }

                    if (gatheringSource.Type is ItemInfoType.TimedMining or ItemInfoType.HiddenMining or ItemInfoType.EphemeralMining)
                    {
                        return Icons.TimedMiningIcon;
                    }
                    if (gatheringSource.Type is ItemInfoType.TimedQuarrying or ItemInfoType.HiddenQuarrying or ItemInfoType.EphemeralQuarrying)
                    {
                        return Icons.TimedQuarryingIcon;
                    }

                    if (gatheringSource.Type is ItemInfoType.TimedHarvesting or ItemInfoType.HiddenHarvesting or ItemInfoType.EphemeralHarvesting)
                    {
                        return Icons.TimedHarvestingIcon;
                    }

                    if (gatheringSource.Type is ItemInfoType.TimedLogging or ItemInfoType.HiddenLogging or ItemInfoType.EphemeralLogging)
                    {
                        return Icons.TimedLoggingIcon;
                    }
                }
                break;
            case ItemInfoType.Fishing:
                return Icons.FishingIcon;
            case ItemInfoType.Spearfishing:
                return Icons.Spearfishing;
            case ItemInfoType.Monster:
                return Icons.MobIcon;
            case ItemInfoType.Fate:
                return Icons.Fate;
            case ItemInfoType.Desynthesis:
                return Icons.DesynthesisIcon;
            case ItemInfoType.Gardening:
                return Icons.SproutIcon;
            case ItemInfoType.Loot:
                if (item is ItemLootSource lootSource)
                {
                    return lootSource.CostItem?.Icon ?? Icons.LootIcon;
                }

                break;
            case ItemInfoType.SkybuilderInspection:
                return Icons.SkybuildersScripIcon;
            case ItemInfoType.SkybuilderHandIn:
                return Icons.SkybuildersScripIcon;
            case ItemInfoType.QuickVenture:
            case ItemInfoType.MiningVenture:
            case ItemInfoType.MiningExplorationVenture:
            case ItemInfoType.BotanyVenture:
            case ItemInfoType.BotanyExplorationVenture:
            case ItemInfoType.CombatVenture:
            case ItemInfoType.CombatExplorationVenture:
            case ItemInfoType.FishingVenture:
            case ItemInfoType.FishingExplorationVenture:
                return Icons.VentureIcon;
            case ItemInfoType.Reduction:
                return Icons.ReductionIcon;
            case ItemInfoType.Airship:
                return Icons.AirshipIcon;
            case ItemInfoType.Submarine:
                return Icons.SubmarineIcon;
            case ItemInfoType.DungeonChest:
                return Icons.Chest;
            case ItemInfoType.DungeonBossDrop:
                return Icons.DutyIcon;
            case ItemInfoType.DungeonBossChest:
                return Icons.GoldChest;
            case ItemInfoType.DungeonDrop:
                return Icons.DutyIcon;
            case ItemInfoType.CustomDelivery:
                break;
            case ItemInfoType.Aquarium:
                break;
            case ItemInfoType.GCDailySupply:
                break;
            case ItemInfoType.CraftLeve:
                break;
            case ItemInfoType.Armoire:
                break;
        }



        return item.Item.Icon;
    }

    public bool ShouldGroupUse(ItemInfoType itemInfoType)
    {
        switch (itemInfoType)
        {
            case ItemInfoType.Desynthesis:
            case ItemInfoType.Reduction:
            case ItemInfoType.Gardening:
            case ItemInfoType.CustomDelivery:
            case ItemInfoType.SkybuilderHandIn:
                return false;
        }

        return true;
    }
}