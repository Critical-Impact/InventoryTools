using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using InventoryTools.Services;

namespace InventoryTools.Logic.Columns.Abstract.ColumnSettings;

public sealed class StringColumnSetting : ColumnSetting<string?>
{
    private readonly string? _placeHolder;

    public delegate StringColumnSetting Factory(string key, string name, string helpText, string? defaultValue, string? placeHolder = null);

    public StringColumnSetting(string key, string name, string helpText, string? defaultValue, ImGuiService imGuiService, string? placeHolder = null)
    {
        _placeHolder = placeHolder;
        this.Key = key;
        this.Name = name;
        this.HelpText = helpText;
        this.DefaultValue = defaultValue;
        ImGuiService = imGuiService;
    }
    public override string? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(Key, out string? value);
        return value;
    }

    public override bool HasValueSet(ColumnConfiguration configuration)
    {
        return CurrentValue(configuration) != DefaultValue;
    }

    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, string? newValue)
    {
        if (newValue == "")
        {
            configuration.SetSetting(Key, (string?)null);
        }
        else
        {
            configuration.SetSetting(Key, newValue);
        }
    }

    public override string Key { get; set; }
    public ImGuiService ImGuiService { get; }
    public override string Name { get; set; }
    public override string HelpText { get; set; }
    public override string? DefaultValue { get; set; }

    public override bool Draw(ColumnConfiguration configuration, string? helpText)
    {
        var success = false;
        ImGui.SetNextItemWidth(LabelSize);
        var value = CurrentValue(configuration) ?? "";
        if (HasValueSet(configuration))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
            ImGui.LabelText("##" + Key + "Label", Name + ":");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.LabelText("##" + Key + "Label", Name + ":");
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(InputSize);
        if (_placeHolder != null ? ImGui.InputTextWithHint("##"+Key+"Input", _placeHolder, ref value, 500) : ImGui.InputText("##"+Key+"Input", ref value, 500))
        {
            UpdateColumnConfiguration(configuration, value);
            success = true;
        }

        ImGui.SameLine();
        ImGuiService.HelpMarker(HelpText);
        if (HasValueSet(configuration) && ShowReset)
        {
            ImGui.SameLine();
            if (ImGui.Button("Reset##" + Key + "Reset"))
            {
                ResetFilter(configuration);
                success = true;
            }
        }
        return success;
    }

    public override bool DrawFilter(ColumnConfiguration configuration, string? helpText)
    {
        var success = false;
        var value = CurrentValue(configuration) ?? "";

        ImGui.SetNextItemWidth(InputSize);
        if (ImGui.InputTextWithHint("##"+Key+"Input", Name, ref value, 500))
        {
            UpdateColumnConfiguration(configuration, value);
            success = true;
        }

        ImGui.SameLine();
        ImGuiService.HelpMarker(HelpText);
        if (HasValueSet(configuration) && ShowReset)
        {
            ImGui.SameLine();
            if (ImGui.Button("Reset##" + Key + "Reset"))
            {
                ResetFilter(configuration);
                success = true;
            }
        }
        return success;
    }

    public override void ResetFilter(ColumnConfiguration configuration)
    {
        UpdateColumnConfiguration(configuration, DefaultValue);
    }
}