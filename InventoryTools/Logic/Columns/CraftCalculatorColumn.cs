using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Logic.Editors;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Logic.Columns;

public class CraftCalculatorColumn : IntegerColumn, IDisposable
{
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly ScopePickerColumnSetting _scopePickerColumnSetting;
    private readonly InventoryScopeCalculator _scopeCalculator;

    public CraftCalculatorColumn(ILogger<CraftCalculatorColumn> logger, ImGuiService imGuiService, IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor, ScopePickerColumnSetting scopePickerColumnSetting, InventoryScopeCalculator scopeCalculator) : base(logger, imGuiService)
    {
        _inventoryMonitor = inventoryMonitor;
        _characterMonitor = characterMonitor;
        _scopePickerColumnSetting = scopePickerColumnSetting;
        _scopeCalculator = scopeCalculator;
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
        "This will calculate the total amount of an item that could be crafted based on the items within your character and retainers. You can override which inventories are looked in by selecting a custom scope below.";

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override FilterType AvailableIn { get; } = Logic.FilterType.GameItemFilter;

    public override void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
    {
        ImGui.NewLine();
        ImGui.Separator();
        ImGui.SetNextItemWidth(220);
        ImGui.LabelText("##" + configuration.Key + "Search", "Inventories to search in:");
        ImGui.SetNextItemWidth(250);
        ImGui.SameLine();
        _scopePickerColumnSetting.Draw(columnConfiguration, "Please make sure you include at least one inventory that contains crystals otherwise the craft calculator will not work. If no scopes are picked, the craft calculator will look in your active characters inventories and their retainers.");
        base.DrawEditor(columnConfiguration, configuration);
    }


    public override IFilterEvent? DrawFooterFilter(ColumnConfiguration columnConfiguration, FilterTable filterTable)
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

                var scopeSetting = _scopePickerColumnSetting.CurrentValue(columnConfiguration);

                if (scopeSetting == null)
                {

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
                }
                else
                {
                    var itemList = _inventoryMonitor.AllItems.Where(c => _scopeCalculator.Filter(scopeSetting, c)).ToList();
                    _craftCalculator.SetAvailableItems(itemList);
                }

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