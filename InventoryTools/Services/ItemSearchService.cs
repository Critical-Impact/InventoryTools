using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using InventoryTools.Logic.Editors;
using InventoryTools.Mediator;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Hosting;

namespace InventoryTools.Services;

public class ItemSearchService(MediatorService mediatorService, IClientState clientState, InventoryToolsConfiguration configuration, IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor, InventoryScopeCalculator calculator, IChatGui chatGui, ItemSheet itemSheet) : IHostedService, IMediatorSubscriber, IDisposable
{
    public ItemSheet ItemSheet { get; } = itemSheet;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        mediatorService.Subscribe<ItemSearchRequestedMessage>(this, ItemSearchRequested);
        return Task.CompletedTask;
    }

    private void ItemSearchRequested(ItemSearchRequestedMessage searchRequest)
    {
        if (!clientState.IsLoggedIn)
        {
            return;
        }
        var item = itemSheet.GetRowOrDefault(searchRequest.ItemId);
        if (item == null)
        {
            return;
        }
        var itemList = inventoryMonitor.AllItems.Where(c => c.ItemId == searchRequest.ItemId);

        if (configuration.ItemSearchScope != null && configuration.ItemSearchScope.Count != 0)
        {
            itemList = itemList.Where(c => calculator.Filter(configuration.ItemSearchScope, c));
        }
        var stringBuilder = new SeStringBuilder();



        stringBuilder.Append("Searching for ");

        stringBuilder.AddUiForeground(0x0225)
            .AddUiGlow(0x0226)
            .AddItemLinkRaw(item.RowId)
            .AddUiForeground(0x01F4)
            .AddUiGlow(0x01F5)
            .AddText($"{(char) SeIconChar.LinkMarker}")
            .AddUiGlowOff()
            .AddUiForegroundOff()
            .AddText(item.NameString)
            .Add(RawPayload.LinkTerminator)
            .AddUiGlowOff()
            .AddUiForegroundOff();

        stringBuilder.Append(":" + "\n");


        var itemsByRetainer = itemList.GroupBy(c => c.RetainerId);

        var searchResults = new List<string>();

        uint normalTotal = 0;
        uint hqTotal = 0;

        foreach (var itemByRetainer in itemsByRetainer)
        {
            var character = characterMonitor.GetCharacterById(itemByRetainer.Key);
            if(character == null) continue;
            var itemsByCategories = itemByRetainer.GroupBy(c => c.SortedCategory);
            searchResults.Add("  " + character.FormattedName + ":");
            foreach (var itemByCategory in itemsByCategories)
            {
                var normalQuantity = itemByCategory.Where(c => c.Flags == InventoryItem.ItemFlags.None).Sum(c => c.Quantity);
                var hqQuantity = itemByCategory.Where(c => c.Flags == InventoryItem.ItemFlags.HighQuality).Sum(c => c.Quantity);
                if(normalQuantity == 0 && hqQuantity == 0) continue;

                normalTotal = (uint)(normalTotal + normalQuantity);
                hqTotal = (uint)(hqTotal + hqQuantity);
                searchResults.Add($"    {(normalQuantity != 0 ? normalQuantity.ToString() :  "")}{(hqQuantity != 0 ? (normalQuantity != 0 ? "," : "") + hqQuantity + "\uE03c" : "")} found in {itemByCategory.Key.FormattedName()}");
            }
        }

        if (normalTotal == 0 && hqTotal == 0)
        {
            searchResults.Add("No results found.");
        }
        else
        {
            searchResults.Add($"Total: {(normalTotal != 0 ? normalTotal.ToString() :  "")}{(hqTotal != 0 ? "," + hqTotal + "\uE03c" : "")} found.");
        }

        stringBuilder.Append(String.Join("\n", searchResults));

        chatGui.Print(stringBuilder.BuiltString);

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        mediatorService.UnsubscribeAll(this);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        mediatorService.UnsubscribeAll(this);
    }

    public MediatorService MediatorService => mediatorService;
}