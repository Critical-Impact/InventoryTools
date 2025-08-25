using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using InventoryTools.Logic.Editors;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Logic.Filters.Abstract;

public abstract class InventoryScopeFilter : Filter<List<InventorySearchScope>?>
{
    private readonly InventoryScopePicker _scopePicker;

    public InventoryScopeFilter(InventoryScopePicker scopePicker, ILogger<InventoryScopeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _scopePicker = scopePicker;
    }

    public abstract List<InventorySearchScope>? GenerateDefaultScope();

    public override List<InventorySearchScope>? CurrentValue(FilterConfiguration configuration)
    {
        configuration.GetFilter(Key, out List<InventorySearchScope>? list);
        return list ?? DefaultValue;
    }

    public override bool HasValueSet(FilterConfiguration configuration)
    {
        configuration.GetFilter(Key, out List<InventorySearchScope>? searchScopeList);

        return searchScopeList is not null && !searchScopeList.SequenceEqual(GenerateDefaultScope() ?? []);
    }

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override void Draw(FilterConfiguration configuration)
    {
        using var id = ImRaii.PushId(Key);
        if (DefaultValue == null)
        {
            DefaultValue = GenerateDefaultScope();
        }
        var currentScopes = CurrentValue(configuration) ?? [];
        if (HasValueSet(configuration))
        {
            ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            ImGui.LabelText("##Label", Name + ":");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.LabelText("##Label", Name + ":");
        }
        ImGui.Indent();
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
        {
            ImGui.PushTextWrapPos();
            ImGui.TextUnformatted(HelpText);
            ImGui.PopTextWrapPos();
        }
        ImGui.SetNextItemWidth(InputSize);
        if (_scopePicker.Draw("##" + Name, currentScopes))
        {
            UpdateFilterConfiguration(configuration, currentScopes);
        }
        if (HasValueSet(configuration) && ShowReset)
        {
            ImGui.SameLine();
            if (ImGui.Button("Reset##Reset"))
            {
                ResetFilter(configuration);
            }
        }
        ImGui.Unindent();
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        DefaultValue = GenerateDefaultScope();
        UpdateFilterConfiguration(configuration, DefaultValue);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<InventorySearchScope>? newValue)
    {
        if (newValue is null)
        {
            configuration.SetFilter(Key, (List<InventorySearchScope>?)null);
            DefaultValue = GenerateDefaultScope();
        }
        else
        {
            configuration.SetFilter(Key, newValue.ToList());
            DefaultValue = GenerateDefaultScope();
        }
    }
}