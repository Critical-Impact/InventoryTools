using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Logic.Editors;
using InventoryTools.Services;
using OtterGui;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public class ScopePickerColumnSetting : ColumnSetting<List<InventorySearchScope>?>
{
    private readonly InventoryScopePicker _scopePicker;
    private readonly InventoryScopeCalculator _scopeCalculator;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly ImGuiService _imGuiService;
    private List<(Character? Character, List<InventoryCategory> Category)>? _categories;

    public ScopePickerColumnSetting(InventoryScopePicker scopePicker, InventoryScopeCalculator scopeCalculator, IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor, ImGuiService imGuiService)
    {
        _scopePicker = scopePicker;
        _scopeCalculator = scopeCalculator;
        _inventoryMonitor = inventoryMonitor;
        _characterMonitor = characterMonitor;
        _imGuiService = imGuiService;
    }
    public override List<InventorySearchScope>? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(configuration.Key, out List<InventorySearchScope>? list);
        return list;
    }

    public override bool HasValueSet(ColumnConfiguration configuration)
    {
        configuration.GetSetting(configuration.Key, out List<InventorySearchScope>? searchScopeList);
        return searchScopeList is not null;
    }

    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, List<InventorySearchScope>? newValue)
    {
        if (newValue is { Count: 0 } or null)
        {
            configuration.SetSetting(configuration.Key, (List<InventorySearchScope>?)null);
            _categories = null;
        }
        else
        {
            configuration.SetSetting(configuration.Key, newValue);
            _categories = null;
        }
    }

    public override string Key { get; set; } = "ScopePicker";
    public override string Name { get; set; } = "Inventory Search Scope";
    public override string HelpText { get; set; } = "Select the inventories you want to search inside.";
    public override List<InventorySearchScope>? DefaultValue { get; set; } = null;

    public override void Draw(ColumnConfiguration configuration, string? helpText)
    {
        var inventorySearchScopes = CurrentValue(configuration) ?? new();
        if (_scopePicker.Draw("##ScopePicker" + configuration.Key, inventorySearchScopes))
        {
            this.UpdateColumnConfiguration(configuration, inventorySearchScopes);
        }

        if (helpText != null)
        {
            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudWhite))
            {
                ImGui.Text("?");
            }
            ImGuiUtil.HoverTooltip("Please make sure you include at least one inventory that contains crystals otherwise the craft calculator will not work.");
        }

        var currentValue = CurrentValue(configuration);
        using var disabled = ImRaii.Disabled(currentValue == null);
        if (ImGui.Button("Test Scopes"))
        {
            if (currentValue != null)
            {
                _categories = _inventoryMonitor.AllItems.Where(c => _scopeCalculator.Filter(currentValue, c)).Select(c => (c.RetainerId, c.SortedCategory)).Distinct().GroupBy(c => c.RetainerId).Select(c => (_characterMonitor.GetCharacterById(c.Key), c.Select(d=> d.SortedCategory).ToList())).ToList();

            }
        }

        if (_categories is not null)
        {
            ImGui.Separator();
            ImGui.Text("The following inventories will be searched in: ");
            foreach (var s in _categories)
            {
                ImGui.TextUnformatted((s.Character?.Name ?? "Unknown Character") + " - " + (string.Join(", ", s.Category.Select(c => c.FormattedDetailedName()).ToList())));
            }
        }
    }

    public override void ResetFilter(ColumnConfiguration configuration)
    {
        UpdateColumnConfiguration(configuration, DefaultValue);
    }
}