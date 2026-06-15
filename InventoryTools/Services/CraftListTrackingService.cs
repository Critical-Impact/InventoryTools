using System;
using System.Threading;
using System.Threading.Tasks;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Monitors.Enums;
using AllaganLib.Monitors.Interfaces;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game;
using InventoryTools.Logic;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class CraftListTrackingService : IHostedService
{
    private readonly ILogger<CraftListTrackingService> _logger;
    private readonly IAcquisitionMonitorService _acquisitionMonitorService;
    private readonly IListService _listService;
    private readonly IChatUtilities _chatUtilities;
    private readonly CraftTrackerTrackCraftsFilter _trackCraftsFilter;
    private readonly CraftTrackerTrackGatheringFilter _trackGatheringFilter;
    private readonly CraftTrackerTrackShoppingFilter _trackShoppingFilter;
    private readonly CraftTrackerTrackCombatDropFilter _trackCombatDropFilter;
    private readonly CraftTrackerTrackOtherFilter _trackOtherFilter;
    private readonly CraftTrackerTrackMarketBoardFilter _trackMarketBoardFilter;
    private readonly CraftReportProgressFilter _progressFilter;
    private readonly CraftReportTrackGatheringFilter _reportGatheringFilter;
    private readonly CraftReportTrackCraftingFilter _reportCraftingFilter;
    private readonly CraftReportTrackShoppingFilter _reportShoppingFilter;
    private readonly CraftReportTrackCombatDropFilter _reportCombatDropFilter;
    private readonly CraftReportTrackMarketBoardFilter _reportMarketBoardFilter;
    private readonly CraftReportTrackOtherFilter _reportOtherFilter;
    private readonly CraftReportCompletionOnlyFilter _completionOnlyFilter;
    private readonly CraftReportPlaySoundFilter _playSoundFilter;
    private readonly CraftReportSoundFilter _soundFilter;
    private readonly ItemSheet _itemSheet;
    private readonly CraftReportPrefixFilter _prefixFilter;
    private readonly IGameInteropService _gameInteropService;

    private const int PrefixColor = 31;
    private const int ItemNameColor = 504;
    private const int AmountColor = 546;

    public CraftListTrackingService(ILogger<CraftListTrackingService> logger,
        IAcquisitionMonitorService acquisitionMonitorService,
        IListService listService, IChatUtilities chatUtilities,
        CraftTrackerTrackCraftsFilter trackCraftsFilter, CraftTrackerTrackGatheringFilter trackGatheringFilter,
        CraftTrackerTrackShoppingFilter trackShoppingFilter, CraftTrackerTrackCombatDropFilter trackCombatDropFilter,
        CraftTrackerTrackOtherFilter trackOtherFilter, CraftTrackerTrackMarketBoardFilter trackMarketBoardFilter,
        CraftReportProgressFilter progressFilter, CraftReportTrackGatheringFilter reportGatheringFilter,
        CraftReportTrackCraftingFilter reportCraftingFilter, CraftReportTrackShoppingFilter reportShoppingFilter,
        CraftReportTrackCombatDropFilter reportCombatDropFilter,
        CraftReportTrackMarketBoardFilter reportMarketBoardFilter,
        CraftReportTrackOtherFilter reportOtherFilter, CraftReportCompletionOnlyFilter completionOnlyFilter,
        CraftReportPlaySoundFilter playSoundFilter, CraftReportSoundFilter soundFilter, ItemSheet itemSheet,
        CraftReportPrefixFilter prefixFilter, IGameInteropService gameInteropService)
    {
        _logger = logger;
        _acquisitionMonitorService = acquisitionMonitorService;
        _listService = listService;
        _chatUtilities = chatUtilities;
        _trackCraftsFilter = trackCraftsFilter;
        _trackGatheringFilter = trackGatheringFilter;
        _trackShoppingFilter = trackShoppingFilter;
        _trackCombatDropFilter = trackCombatDropFilter;
        _trackOtherFilter = trackOtherFilter;
        _trackMarketBoardFilter = trackMarketBoardFilter;
        _progressFilter = progressFilter;
        _reportGatheringFilter = reportGatheringFilter;
        _reportCraftingFilter = reportCraftingFilter;
        _reportShoppingFilter = reportShoppingFilter;
        _reportCombatDropFilter = reportCombatDropFilter;
        _reportMarketBoardFilter = reportMarketBoardFilter;
        _reportOtherFilter = reportOtherFilter;
        _completionOnlyFilter = completionOnlyFilter;
        _playSoundFilter = playSoundFilter;
        _soundFilter = soundFilter;
        _itemSheet = itemSheet;
        _prefixFilter = prefixFilter;
        _gameInteropService = gameInteropService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _acquisitionMonitorService.ItemAcquired += AcquisitionMonitorServiceOnItemAcquired;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _acquisitionMonitorService.ItemAcquired -= AcquisitionMonitorServiceOnItemAcquired;
        return Task.CompletedTask;
    }

    private GenericBooleanFilter? ReasonToggle(AcquisitionReason reason)
    {
        return reason switch
        {
            AcquisitionReason.Gathering => _reportGatheringFilter,
            AcquisitionReason.Crafting => _reportCraftingFilter,
            AcquisitionReason.Shopping => _reportShoppingFilter,
            AcquisitionReason.CombatDrop => _reportCombatDropFilter,
            AcquisitionReason.Marketboard => _reportMarketBoardFilter,
            AcquisitionReason.Other => _reportOtherFilter,
            _ => null,
        };
    }

    private void AcquisitionMonitorServiceOnItemAcquired(uint itemId, InventoryItem.ItemFlags itemFlags,
        int qtyIncrease, AcquisitionReason reason)
    {
        _logger.LogTrace("Item acquired through {Reason}, qty of {QtyIncrease}, item ID: {ItemId}", reason, qtyIncrease,
            itemId);

        var activeCraftList = _listService.GetActiveCraftList();
        if (activeCraftList != null && activeCraftList.FilterType == FilterType.CraftFilter)
        {
            if ((reason == AcquisitionReason.Crafting && _trackCraftsFilter.CurrentValue(activeCraftList) == false) ||
                (reason == AcquisitionReason.Gathering &&
                 _trackGatheringFilter.CurrentValue(activeCraftList) == false) ||
                (reason == AcquisitionReason.Shopping && _trackShoppingFilter.CurrentValue(activeCraftList) == false) ||
                (reason == AcquisitionReason.CombatDrop &&
                 _trackCombatDropFilter.CurrentValue(activeCraftList) == false) ||
                (reason == AcquisitionReason.Other && _trackOtherFilter.CurrentValue(activeCraftList) == false) ||
                (reason == AcquisitionReason.Marketboard &&
                 _trackMarketBoardFilter.CurrentValue(activeCraftList) == false)
               )
            {
                _logger.LogTrace("Craft list configured to not track {Reason}, not altering required item counts.",
                    reason);
                return;
            }

            _logger.LogTrace("Marking {Quantity} qty for item {ItemId} ({HqFlag}) as crafted.", qtyIncrease, itemId,
                itemFlags.ToString());
            var activeItem = activeCraftList.CraftList.GetItemById(itemId, itemFlags);
            var missing = activeItem?.QuantityMissingOverall;
            if (missing != null)
            {
                NotifyUser(activeCraftList, itemId, itemFlags, (uint)qtyIncrease, missing.Value, reason);
            }

            if (activeCraftList.CraftList.CraftListMode == CraftListMode.Normal)
            {
                activeCraftList.CraftList.MarkCrafted(itemId, itemFlags, (uint)qtyIncrease);
                if (activeCraftList is { IsEphemeralCraftList: true, CraftList.IsCompleted: true })
                {
                    _chatUtilities.Print("Ephemeral craft list '" + activeCraftList.Name +
                                         "' completed. List has been removed.");
                    _listService.RemoveList(activeCraftList);
                }
                else
                {
                    activeCraftList.NeedsRefresh = true;
                }
            }
        }
        else
        {
            _logger.LogTrace("Active craft list is either inactive or in stock mode.");
        }
    }

    private static bool IsToggleOn(GenericBooleanFilter toggle, FilterConfiguration list)
    {
        return toggle.CurrentValue(list) == true;
    }

    private void NotifyUser(FilterConfiguration activeCraftList, uint itemId, InventoryItem.ItemFlags itemFlags,
        uint qtyIncrease, uint quantityMissing, AcquisitionReason reason)
    {
        if (qtyIncrease <= 0)
        {
            return;
        }

        var remaining = (uint)Math.Max(0, (int)quantityMissing - qtyIncrease);
        var complete = remaining == 0;

        if (!IsToggleOn(_progressFilter, activeCraftList))
        {
            return;
        }

        var reasonToggle = ReasonToggle(reason);
        if (reasonToggle == null || !IsToggleOn(reasonToggle, activeCraftList))
        {
            _logger.LogTrace("Not reporting {ItemId}: report toggle for reason {Reason} is off", itemId, reason);
            return;
        }

        string? amountText;
        if (IsToggleOn(_completionOnlyFilter, activeCraftList))
        {
            amountText = complete ? $"completed" : null;
        }
        else if (complete)
        {
            amountText = $"completed";
        }
        else
        {
            amountText = $"{remaining} remaining";
        }

        if (amountText == null)
        {
            _logger.LogTrace("Not reporting {ItemId}: completion-only is on and it isn't complete yet", itemId);
            return;
        }

        var item = _itemSheet.GetRow(itemId);

        _logger.LogTrace("Reporting {ItemId} ({Reason}): {Missing} complete={Complete}", itemId, reason,
            quantityMissing, complete);
        PrintProgress(GetPrefix(activeCraftList), item.NameString, amountText);

        if (complete && IsToggleOn(_playSoundFilter, activeCraftList))
        {
            var soundId = _soundFilter.CurrentValue(activeCraftList);
            if (soundId != 0)
            {
                _gameInteropService.PlayChatSoundEffect(soundId);
            }
        }
    }

    private string GetPrefix(FilterConfiguration list)
    {
        return _prefixFilter.CurrentValue(list) switch
        {
            CraftReportPrefix.PluginName => "[AT]",
            CraftReportPrefix.CraftListName => $"[{list.Name}]",
            _ => string.Empty,
        };
    }

    private void PrintProgress(string prefix, string itemName, string amount)
    {
        var builder = new SeStringBuilder();
        if (!string.IsNullOrEmpty(prefix))
        {
            ChatUtilities.AddColoredText(builder, prefix, PrefixColor).AddText(" ");
        }

        ChatUtilities.AddColoredText(builder, itemName, ItemNameColor).AddText(" ");
        ChatUtilities.AddColoredText(builder, amount, AmountColor);
        _chatUtilities.Print(builder.BuiltString);
    }
}