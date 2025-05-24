using System;
using System.Linq;
using System.Threading.Tasks;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Services;
using Dalamud.Plugin.Services;
using InventoryTools.Logic;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestService
{
    private readonly EquipmentSuggestLevelFormField _levelField;
    private readonly EquipmentSuggestConfig _config;
    private readonly Lazy<EquipmentSuggestGrid> _equipmentSuggestGrid;
    private readonly EquipmentSuggestSourceTypeField _typeField;
    private readonly EquipmentSuggestModeSetting _modeSetting;
    private readonly IClientState _clientState;
    private readonly EquipmentSuggestClassJobFormField _classJobField;
    private readonly ClassJobSheet _classJobSheet;
    private readonly ClassJobCategorySheet _classJobCategorySheet;
    private readonly InventoryToolsConfiguration _configuration;

    public const uint StrengthId = 1;
    public const uint DexterityId = 2;
    public const uint VitalityId = 3;
    public const uint IntelligenceId = 4;
    public const uint MindId = 5;
    public const uint PietyId = 6;
    public const uint GatherPointsId = 10;
    public const uint CraftPointsId = 11;
    public const uint CraftsmanshipId = 70;
    public const uint ControlId = 71;
    public const uint GatheringId = 72;
    public const uint PerceptionId = 73;


    public EquipmentSuggestService(EquipmentSuggestLevelFormField levelField, EquipmentSuggestConfig config,
        Lazy<EquipmentSuggestGrid> equipmentSuggestGrid, EquipmentSuggestSourceTypeField typeField,
        EquipmentSuggestModeSetting modeSetting, IClientState clientState,
        EquipmentSuggestClassJobFormField classJobField, ClassJobSheet classJobSheet,
        ClassJobCategorySheet classJobCategorySheet, InventoryToolsConfiguration configuration)
    {
        _levelField = levelField;
        _config = config;
        _equipmentSuggestGrid = equipmentSuggestGrid;
        _typeField = typeField;
        _modeSetting = modeSetting;
        _clientState = clientState;
        _classJobField = classJobField;
        _classJobSheet = classJobSheet;
        _classJobCategorySheet = classJobCategorySheet;
        _configuration = configuration;
    }

    public void UseCurrentClassLevel()
    {
        var activeCharacter = _clientState.LocalPlayer;
        if (activeCharacter != null)
        {
            var currentMode = _modeSetting.CurrentValue(_configuration);
            if (currentMode == EquipmentSuggestMode.Class)
            {
                this._levelField.UpdateFilterConfiguration(_config, (int)activeCharacter.Level);
                this._classJobField.UpdateFilterConfiguration(_config, activeCharacter.ClassJob.RowId);
            }
            else
            {
                this._levelField.UpdateFilterConfiguration(_config, (int)activeCharacter.Level);
            }
        }
    }

    public int GetGearScore(ClassJobRow classJob, ClassJobCategoryRow classJobCategory, ItemRow itemRow)
    {
        int gearScore = (int)(itemRow.Base.LevelItem.RowId * 1000);
        for (var index = 0; index < itemRow.Base.BaseParam.Count; index++)
        {
            var baseParam = itemRow.Base.BaseParam[index];
            var baseParamValue = itemRow.Base.BaseParamValue[index];
            if (baseParam.RowId == 0)
            {
                continue;
            }

            if (baseParam.RowId == StrengthId)
            {
                gearScore += classJob.Base.ModifierStrength * baseParamValue;
            }
            else if (baseParam.RowId == DexterityId)
            {
                gearScore += classJob.Base.ModifierDexterity * baseParamValue;
            }
            else if (baseParam.RowId == VitalityId)
            {
                gearScore += classJob.Base.ModifierVitality * baseParamValue;
            }
            else if (baseParam.RowId == IntelligenceId)
            {
                gearScore += classJob.Base.ModifierIntelligence * baseParamValue;
            }
            else if (baseParam.RowId == MindId)
            {
                gearScore += classJob.Base.ModifierMind * baseParamValue;
            }
            else if (baseParam.RowId == PietyId)
            {
                gearScore += classJob.Base.ModifierPiety * baseParamValue;
            }
            else if (baseParam.RowId == GatherPointsId || baseParam.RowId == GatheringId || baseParam.RowId == PerceptionId)
            {
                gearScore += classJobCategory.IsGathering ? 100 * baseParamValue : 150 * baseParamValue;
            }
            else if (baseParam.RowId == CraftPointsId || baseParam.RowId == CraftsmanshipId || baseParam.RowId == ControlId)
            {
                gearScore += classJobCategory.IsCrafting ? 100 * baseParamValue : 150 * baseParamValue;
            }
            else
            {
                gearScore += baseParamValue * 100;
            }
        }
        for (var index = 0; index < itemRow.Base.BaseParamSpecial.Count; index++)
        {
            var baseParam = itemRow.Base.BaseParamSpecial[index];
            var baseParamValue = itemRow.Base.BaseParamValueSpecial[index];
            if (baseParam.RowId == 0)
            {
                continue;
            }

            if (baseParam.RowId == StrengthId)
            {
                gearScore += classJob.Base.ModifierStrength * baseParamValue;
            }
            else if (baseParam.RowId == DexterityId)
            {
                gearScore += classJob.Base.ModifierDexterity * baseParamValue;
            }
            else if (baseParam.RowId == VitalityId)
            {
                gearScore += classJob.Base.ModifierVitality * baseParamValue;
            }
            else if (baseParam.RowId == IntelligenceId)
            {
                gearScore += classJob.Base.ModifierIntelligence * baseParamValue;
            }
            else if (baseParam.RowId == MindId)
            {
                gearScore += classJob.Base.ModifierMind * baseParamValue;
            }
            else if (baseParam.RowId == PietyId)
            {
                gearScore += classJob.Base.ModifierPiety * baseParamValue;
            }
            else if (baseParam.RowId == GatherPointsId || baseParam.RowId == GatheringId || baseParam.RowId == PerceptionId)
            {
                gearScore += classJobCategory.IsGathering ? 100 * baseParamValue : 150 * baseParamValue;
            }
            else if (baseParam.RowId == CraftPointsId || baseParam.RowId == CraftsmanshipId || baseParam.RowId == ControlId)
            {
                gearScore += classJobCategory.IsCrafting ? 100 * baseParamValue : 150 * baseParamValue;
            }
            else
            {
                gearScore += baseParamValue * 100;
            }
        }
        return gearScore;
    }

    public async Task SelectHighestILvl()
    {
        var currentMode = _modeSetting.CurrentValue(_configuration);
        if (currentMode == EquipmentSuggestMode.Class)
        {
            var level = _levelField.CurrentValue(_config);
            var classJobId = _classJobField.CurrentValue(_config);
            var classJob = _classJobSheet.GetRowOrDefault(classJobId);
            if (classJob == null)
            {
                return;
            }

            var classJobCategory = _classJobCategorySheet.GetRow(classJob.Base.ClassJobCategory.RowId);

            var items = await _equipmentSuggestGrid.Value.GetItemsAsync();
            foreach (var item in items)
            {
                var highestILvl = item.SuggestedItems.SelectMany(c => c.Value.Select(d => d.Item))
                    .Where(c => c.Base.LevelEquip <= level).MaxBy(c => GetGearScore(classJob, classJobCategory, c));
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
            var items = await _equipmentSuggestGrid.Value.GetItemsAsync();
            foreach (var item in items)
            {
                if (item.ClassJobRow == null)
                {
                    continue;
                }
                var classJobCategory = _classJobCategorySheet.GetRow(item.ClassJobRow.RowId);

                var mainHand = item.SuggestedItems.SelectMany(c => c.Value.Select(d => d.Item))
                    .Where(c => c.Base.LevelEquip <= level && c.EquipSlotCategory?.Base.MainHand == 1).MaxBy(c => GetGearScore(item.ClassJobRow, classJobCategory, c));
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