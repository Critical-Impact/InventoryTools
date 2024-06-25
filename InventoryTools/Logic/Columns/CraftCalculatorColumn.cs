using System;
using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class CraftCalculatorColumn : IntegerColumn, IDisposable
{
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly ICharacterMonitor _characterMonitor;

    public CraftCalculatorColumn(ILogger<CraftCalculatorColumn> logger, ImGuiService imGuiService, IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor) : base(logger, imGuiService)
    {
        _inventoryMonitor = inventoryMonitor;
        _characterMonitor = characterMonitor;
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Tools;
    public Dictionary<uint, uint>? _craftable;
    public CraftCalculator? _craftCalculator;

    public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        if (_craftable == null) return 0;
        return (int?)(_craftable.ContainsKey(searchResult.Item.RowId) ? _craftable[searchResult.Item.RowId] : 0);
    }

    public override string Name { get; set; } = "Craft Calculator";
    public override float Width { get; set; } = 80;

    public override string HelpText { get; set; } =
        "This will calculate the total amount of an item that could be crafted based on the items within your character and retainers.";

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override FilterType AvailableIn { get; } = Logic.FilterType.GameItemFilter;

    public override IFilterEvent? DrawFooterFilter(FilterConfiguration configuration, FilterTable filterTable)
    {
        ImGui.SameLine();
        
        if (_craftCalculator == null || !_craftCalculator.IsRunning)
        {
            if (ImGui.Button("Calculate Crafts"))
            {
                if (_craftCalculator == null)
                {
                    _craftCalculator = new CraftCalculator();
                    _craftCalculator.CraftingResult += CraftCalculatorOnCraftingResult;
                }

                var items = new List<CriticalCommonLib.Models.InventoryItem>();
                var playerBags = _inventoryMonitor.GetSpecificInventory(_characterMonitor.ActiveCharacterId,
                    InventoryCategory.CharacterBags);
                var crystalBags = _inventoryMonitor.GetSpecificInventory(_characterMonitor.ActiveCharacterId,
                    InventoryCategory.Crystals);
                var currencyBags = _inventoryMonitor.GetSpecificInventory(_characterMonitor.ActiveCharacterId,
                    InventoryCategory.Currency);
                var inventories = _inventoryMonitor.Inventories;
                foreach (var characterId in inventories)
                {
                    var character = _characterMonitor.GetCharacterById(characterId.Key);
                    if (character != null)
                    {
                        if (character.OwnerId == _characterMonitor.ActiveCharacterId)
                        {
                            foreach (var inventoryCategory in characterId.Value.GetAllInventories())
                            {
                                foreach (var inventory in inventoryCategory)
                                {
                                    if (inventory != null)
                                    {
                                        items.Add(inventory);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var item in playerBags)
                {
                    items.Add(item);
                }

                foreach (var item in crystalBags)
                {
                    items.Add(item);
                }

                foreach (var item in currencyBags)
                {
                    items.Add(item);
                }
                _craftCalculator.SetAvailableItems(items);                
                foreach (var item in filterTable.RenderSearchResults)
                {
                    _craftCalculator.AddItemId(item.Item.RowId);
                }
                _craftCalculator.StartProcessing();
            }
        }
        else if (_craftCalculator.IsRunning)
        {
            if (ImGui.Button("Stop Calculating Crafts"))
            {
                _craftCalculator.CancelProcessing();
            }
        }


        return null;
    }

    private void CraftCalculatorOnCraftingResult(object? sender, CraftingResultEventArgs e)
    {
        if (_craftable == null)
        {
            _craftable = new Dictionary<uint, uint>();
        }
        _craftable[e.ItemId] = e.CraftableQuantity ?? 0;
    }

    public override void Dispose()
    {
        if (!base.Disposed)
        {
            if (_craftCalculator != null)
            {
                _craftCalculator.CraftingResult -= CraftCalculatorOnCraftingResult;
                _craftCalculator.Dispose();
            }            
            base.Dispose();
        }
    }
}