using System;
using System.Linq;
using CriticalCommonLib.Services;
using InventoryTools.Logic;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestService
{
    private readonly EquipmentSuggestLevelFormField _levelField;
    private readonly EquipmentSuggestConfig _config;
    private readonly Lazy<EquipmentSuggestGrid> _equipmentSuggestGrid;
    private readonly EquipmentSuggestSourceTypeField _typeField;
    private readonly EquipmentSuggestModeSetting _modeSetting;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly EquipmentSuggestClassJobFormField _classJobField;
    private readonly InventoryToolsConfiguration _configuration;

    public EquipmentSuggestService(EquipmentSuggestLevelFormField levelField, EquipmentSuggestConfig config,
        Lazy<EquipmentSuggestGrid> equipmentSuggestGrid, EquipmentSuggestSourceTypeField typeField,
        EquipmentSuggestModeSetting modeSetting, ICharacterMonitor characterMonitor,
        EquipmentSuggestClassJobFormField classJobField,
        InventoryToolsConfiguration configuration)
    {
        _levelField = levelField;
        _config = config;
        _equipmentSuggestGrid = equipmentSuggestGrid;
        _typeField = typeField;
        _modeSetting = modeSetting;
        _characterMonitor = characterMonitor;
        _classJobField = classJobField;
        _configuration = configuration;
    }

    public void UseCurrentClassLevel()
    {
        var activeCharacter = _characterMonitor.ActiveCharacter;
        if (activeCharacter != null)
        {
            var currentMode = _modeSetting.CurrentValue(_configuration);
            if (currentMode == EquipmentSuggestMode.Class)
            {
                this._levelField.UpdateFilterConfiguration(_config, (int)activeCharacter.Level);
                this._classJobField.UpdateFilterConfiguration(_config, activeCharacter.ClassJob);
            }
            else
            {
                this._levelField.UpdateFilterConfiguration(_config, (int)activeCharacter.Level);
            }
        }
    }

    public void SelectHighestILvl()
    {
        var currentMode = _modeSetting.CurrentValue(_configuration);
        if (currentMode == EquipmentSuggestMode.Class)
        {
            var level = _levelField.CurrentValue(_config);
            var items = _equipmentSuggestGrid.Value.GetItems();
            foreach (var item in items)
            {
                var highestILvl = item.SuggestedItems.SelectMany(c => c.Value.Select(d => d.Item))
                    .Where(c => c.Base.LevelEquip <= level).MaxBy(c => c.Base.LevelItem.RowId);
                if (highestILvl != null)
                {
                    item.SelectedItem = new SearchResult(highestILvl);
                    var currentValue = _typeField.CurrentValue(_config);
                    var firstSource =
                        item.SelectedItem.Item.Sources.FirstOrDefault(c =>
                            currentValue.Contains(c.Type));
                    if (firstSource == null)
                    {
                        firstSource = item.SelectedItem.Item.Sources.FirstOrDefault();
                    }

                    if (firstSource != null)
                    {
                        item.AcquisitionSource = firstSource.Type;
                    }
                    else
                    {
                        item.AcquisitionSource = null;
                    }
                }
                else
                {
                    item.SelectedItem = null;
                    item.AcquisitionSource = null;
                }
            }
        }
        else if (currentMode == EquipmentSuggestMode.Tool)
        {
            var level = _levelField.CurrentValue(_config);
            var items = _equipmentSuggestGrid.Value.GetItems();
            foreach (var item in items)
            {
                var mainHand = item.SuggestedItems.SelectMany(c => c.Value.Select(d => d.Item))
                    .Where(c => c.Base.LevelEquip <= level && c.EquipSlotCategory?.Base.MainHand == 1).MaxBy(c => c.Base.LevelItem.RowId);
                if (mainHand != null)
                {
                    item.SelectedItem = new SearchResult(mainHand);
                    var currentValue = _typeField.CurrentValue(_config);
                    var firstSource =
                        item.SelectedItem.Item.Sources.FirstOrDefault(c =>
                            currentValue.Contains(c.Type));
                    if (firstSource == null)
                    {
                        firstSource = item.SelectedItem.Item.Sources.FirstOrDefault();
                    }

                    if (firstSource != null)
                    {
                        item.AcquisitionSource = firstSource.Type;
                    }
                    else
                    {
                        item.AcquisitionSource = null;
                    }
                }
                else
                {
                    item.SelectedItem = null;
                    item.AcquisitionSource = null;
                }

                var offHand = item.SuggestedItems.SelectMany(c => c.Value.Select(d => d.Item))
                    .Where(c => c.Base.LevelEquip <= level && c.EquipSlotCategory?.Base.OffHand == 1).MaxBy(c => c.Base.LevelItem.RowId);
                if (offHand != null)
                {
                    item.SecondarySelectedItem = new SearchResult(offHand);
                    var currentValue = _typeField.CurrentValue(_config);
                    var firstSource =
                        item.SecondarySelectedItem.Item.Sources.FirstOrDefault(c =>
                            currentValue.Contains(c.Type));
                    if (firstSource == null)
                    {
                        firstSource = item.SecondarySelectedItem.Item.Sources.FirstOrDefault();
                    }

                    if (firstSource != null)
                    {
                        item.SecondaryAcquisitionSource = firstSource.Type;
                    }
                    else
                    {
                        item.SecondaryAcquisitionSource = null;
                    }
                }
                else
                {
                    item.SecondarySelectedItem = null;
                    item.SecondaryAcquisitionSource = null;
                }
            }
        }
    }
}