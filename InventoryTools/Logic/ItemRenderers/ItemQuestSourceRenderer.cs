using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemQuestUseRenderer : ItemQuestSourceRenderer
{
    public ItemQuestUseRenderer(ITextureProvider textureProvider, ItemSheet itemSheet, MapSheet mapSheet,
        List<FestivalName> festivalNames, IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, itemSheet, mapSheet, festivalNames, dalamudPluginInterface)
    {
    }

    public override string HelpText { get; } = "Is this item required for a quest?";

    public override RendererType RendererType { get; } = RendererType.Use;
}

public class ItemQuestSourceRenderer : ItemInfoRenderer<ItemQuestSource>
{
    private readonly ITextureProvider _textureProvider;
    private readonly ItemSheet _itemSheet;
    private readonly Dictionary<uint,string> _festivalNames;
    public override RendererType RendererType { get; } = RendererType.Source;
    public override ItemInfoType Type { get; } = ItemInfoType.Quest;
    public override string SingularName { get; } = "Quest";
    public override string HelpText { get; } = "Does this item come from a quest?";
    public override bool ShouldGroup { get; } = true;

    public ItemQuestSourceRenderer(ITextureProvider textureProvider, ItemSheet itemSheet, MapSheet mapSheet,
        List<FestivalName> festivalNames, IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _textureProvider = textureProvider;
        _itemSheet = itemSheet;
        _festivalNames = festivalNames.ToDictionary(c => c.FestivalId, c => c.Name);
    }

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var quest = asSource.Quest.Value;

        var questName = quest.Name.ToImGuiString();
        ImGui.Text("Name: " + questName);
        ImGui.Text("Expansion: " + quest.Expansion.Value.Name.ToImGuiString());
        if (quest.BeastTribe.RowId != 0)
        {
            ImGui.Text("Allied Society: " + quest.BeastTribe.Value.Name.ToImGuiString());
        }
        if (quest.Festival.RowId != 0 && _festivalNames.ContainsKey(quest.Festival.RowId))
        {
            ImGui.PushTextWrapPos();
            ImGui.Text("Only available from " + _festivalNames[quest.Festival.RowId]);
            ImGui.PopTextWrapPos();
        }

        ImGui.Text("Rewards:");
        using (ImRaii.PushIndent())
        {
            foreach (var reward in quest.Reward)
            {
                if (reward.Is<Item>())
                {
                    if (reward.RowId == 0)
                    {
                        continue;
                    }

                    var item = _itemSheet.GetRowOrDefault(reward.RowId);
                    if (item is not null)
                    {
                        ImGui.Image(
                            _textureProvider.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrEmpty()
                                .ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                        ImGui.SameLine();
                        var itemName = item.NameString;
                        var count = 1;
                        var costString = $"{itemName} x {count}";
                        ImGui.Text(costString);
                    }
                }
            }

            if (asSource.QuestClassJobReward != null && asSource.QuestClassJobRewardSubRowId != null)
            {
                var jobReward =  asSource.QuestClassJobReward.Value.Value[asSource.QuestClassJobRewardSubRowId.Value];
                for (var index = 0; index < jobReward.RewardItem.Count; index++)
                {
                    var rewardItem = jobReward.RewardItem[index];
                    if (rewardItem.RowId == 0)
                    {
                        continue;
                    }

                    var item = _itemSheet.GetRowOrDefault(rewardItem.RowId);
                    if (item is not null)
                    {
                        ImGui.Image(
                            _textureProvider.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrEmpty()
                                .ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                        ImGui.SameLine();
                        var itemName = item.NameString;
                        var count = jobReward.RewardAmount[index];
                        var costString = $"{itemName} x {count}";
                        ImGui.Text(costString);
                    }
                }
            }

            for (var index = 0; index < asSource.Quest.Value.ItemCatalyst.Count; index++)
            {
                var catalyst = asSource.Quest.Value.ItemCatalyst[index];
                var catalystCount = asSource.Quest.Value.ItemCountCatalyst[index];
                if (catalyst.RowId == 0)
                {
                    continue;
                }
                var item = _itemSheet.GetRowOrDefault(catalyst.RowId);
                if (item is not null)
                {
                    ImGui.Image(
                        _textureProvider.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrEmpty()
                            .ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                    ImGui.SameLine();
                    var itemName = item.NameString;
                    var count = catalystCount;
                    var costString = $"{itemName} x {count}";
                    ImGui.Text(costString);
                }
            }

            for (var index = 0; index < asSource.Quest.Value.OptionalItemReward.Count; index++)
            {
                var optionalReward = asSource.Quest.Value.OptionalItemReward[index];
                var optionalRewardCount = asSource.Quest.Value.OptionalItemCountReward[index];
                if (optionalReward.RowId == 0)
                {
                    continue;
                }
                var item = _itemSheet.GetRowOrDefault(optionalReward.RowId);
                if (item is not null)
                {
                    ImGui.Image(
                        _textureProvider.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrEmpty()
                            .ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                    ImGui.SameLine();
                    var itemName = item.NameString;
                    var count = optionalRewardCount;
                    var costString = $"{itemName} x {count} (Optional)";
                    ImGui.Text(costString);
                }
            }
        }

        if (asSource.QuestClassJobReward != null && asSource.QuestClassJobRewardSubRowId != null)
        {
            ImGui.Text("Required:");
            using (ImRaii.PushIndent())
            {
                var jobReward =  asSource.QuestClassJobReward.Value.Value[asSource.QuestClassJobRewardSubRowId.Value];
                for (var index = 0; index < jobReward.RequiredItem.Count; index++)
                {
                    var requiredItem = jobReward.RequiredItem[index];
                    if (requiredItem.RowId == 0)
                    {
                        continue;
                    }

                    var item = _itemSheet.GetRowOrDefault(requiredItem.RowId);
                    if (item is not null)
                    {
                        ImGui.Image(
                            _textureProvider.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrEmpty()
                                .ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                        ImGui.SameLine();
                        var itemName = item.NameString;
                        var count = jobReward.RequiredAmount[index];
                        var costString = $"{itemName} x {count}";
                        ImGui.Text(costString);
                    }
                }
            }
        }
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        var questName = asSource.Quest.Value.Name.ToImGuiString();
        return questName;
    };
    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        return (int)asSource.QuestIcon;
    };
    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var questName = asSource.Quest.Value.Name.ToImGuiString();
        var expansionName = asSource.Quest.Value.Expansion.Value.Name.ToImGuiString();
        return questName + " (" + expansionName + ")";
    };
}