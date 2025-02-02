using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Logic.Settings;


public class TooltipUseInformationEnabledSetting : GenericBooleanSetting
{
    public TooltipUseInformationEnabledSetting(ILogger<TooltipUseInformationEnabledSetting> logger, ImGuiService imGuiService) : base("TooltipUseInformationEnabled", "Use Information Enabled", "Should use information be shown in the tooltip? For example, the item is used in a recipe or as currency at a shop, etc", false, SettingCategory.ToolTips, SettingSubCategory.UseInformation, "1.11.0.11", logger, imGuiService)
    {
    }
    public override string WizardName { get; } = "Show Use Information";
    public override uint? Order { get; } = 0;
}

public class TooltipUseInformationModifierSetting : GenericEnumChoiceSetting<TooltipSourceModifier>
{
    public TooltipUseInformationModifierSetting(ILogger<TooltipUseInformationModifierSetting> logger, ImGuiService imGuiService) : base("TooltipUseInformationModifier", "Use Information Modifier Key", "Should the tooltip only be shown if a modifier key is pressed?", TooltipSourceModifier.Always, new()
    {
        {TooltipSourceModifier.Always, "Always"},
        {TooltipSourceModifier.Control, "Control"},
        {TooltipSourceModifier.Shift, "Shift"},
    }, SettingCategory.ToolTips, SettingSubCategory.UseInformation, "1.11.0.11", logger, imGuiService)
    {
    }
    public override uint? Order { get; } = 1;
}

public class TooltipUseInformationSetting : Setting<Dictionary<ItemInfoType, TooltipSourceSetting>>
{
    private readonly IFont _font;
    private readonly Dictionary<ItemInfoType, IItemInfoRenderer> _itemInfoRenderers;
    private readonly Dictionary<SourceIconGrouping,string> _choices;

    public TooltipUseInformationSetting(ILogger<TooltipUseInformationSetting> logger, IEnumerable<IItemInfoRenderer> itemInfoRenderers, ImGuiService imGuiService, IFont font) : base(logger, imGuiService)
    {
        _font = font;
        _itemInfoRenderers = itemInfoRenderers.Where(c => c.RendererType == RendererType.Source).ToDictionary(c => c.Type, c => c);
        _choices = new Dictionary<SourceIconGrouping, string>()
        {
            {SourceIconGrouping.Default, "Default"},
            {SourceIconGrouping.Grouped, "Grouped"},
            {SourceIconGrouping.Ungrouped, "Ungrouped"}
        };
    }

    public override Dictionary<ItemInfoType, TooltipSourceSetting> DefaultValue { get; set; } = new Dictionary<ItemInfoType, TooltipSourceSetting>();
    public override Dictionary<ItemInfoType, TooltipSourceSetting> CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipInfoUseSetting;
    }

    private TooltipSourceSetting? _draggedSetting;

    public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset, bool? disableColouring)
    {
        ImGui.LabelText("##" + Key + "Label", customName ?? Name);

        using (var table = ImRaii.Table("UseConfiguration", 4, ImGuiTableFlags.SizingFixedFit))
        {
            if (table)
            {
                var currentValue = CurrentValue(configuration);
                var updated = false;
                foreach (var sourceRenderer in _itemInfoRenderers)
                {
                    if (!currentValue.TryGetValue(sourceRenderer.Key, out TooltipSourceSetting? config))
                    {
                        updated = true;
                        config = new TooltipSourceSetting()
                        {
                            Type = sourceRenderer.Key,
                            Show = true,
                            Group = null,
                            Order = 999
                        };
                        currentValue[sourceRenderer.Key] = config;
                    }
                }

                if (updated)
                {
                    UpdateFilterConfiguration(configuration, currentValue);
                }

                for (var index = 0; index < currentValue.OrderBy(c => c.Value.Order).ToList().Count; index++)
                {
                    var item = currentValue.OrderBy(c => c.Value.Order).ToList()[index];
                    item.Value.Order = index;
                }


                ImGui.TableSetupColumn("Settings", ImGuiTableColumnFlags.NoHeaderLabel);
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Show");
                ImGui.TableSetupColumn("Group Mode");

                ImGui.TableHeadersRow();

                var currentValues = currentValue.OrderBy(c => c.Value.Order).Select(c => c.Value).ToList();
                for (var index = 0; index < currentValues.Count; index++)
                {
                    var tooltipSourceSetting = currentValues[index];
                    var sourceRenderer = _itemInfoRenderers[tooltipSourceSetting.Type];

                    ImGui.TableNextColumn();
                    using (ImRaii.PushFont(_font.IconFont))
                    {
                        ImGui.Button(FontAwesomeIcon.ArrowsUpDown.ToIconString());
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNS);
                    }
                    if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        if (_draggedSetting == null)
                        {
                            _draggedSetting = tooltipSourceSetting;
                        }
                    }
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                    {
                        _draggedSetting = null;
                        UpdateFilterConfiguration(configuration, currentValue);
                    }

                    if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && ImGui.IsItemHovered() && _draggedSetting != null)
                    {
                        if (_draggedSetting != tooltipSourceSetting)
                        {
                            var newIndex = currentValues.IndexOf(tooltipSourceSetting);
                            _draggedSetting.Order = newIndex;

                            var resortedList = currentValue.Select(c => c.Value).OrderBy(c => c.Order).ThenBy(c => c == _draggedSetting ? 0 : 1).ToList();
                            var orderChanged = false;
                            for (var i = 0; i < resortedList.Count; i++)
                            {
                                var reorder = resortedList[i];
                                if (reorder.Order != i)
                                {
                                    orderChanged = true;
                                    reorder.Order = i;
                                }
                            }

                            if (orderChanged)
                            {
                                UpdateFilterConfiguration(configuration, currentValue);
                            }
                        }
                    }
                    SourceIconGrouping sourceIconGrouping = SourceIconGrouping.Default;
                    var config = currentValue[tooltipSourceSetting.Type];

                    var show = config.Show;
                    if (config.Group != null)
                    {
                        sourceIconGrouping = config.Group.Value
                            ? SourceIconGrouping.Grouped
                            : SourceIconGrouping.Ungrouped;
                    }

                    using var _ = ImRaii.PushId(tooltipSourceSetting.Type.ToString());
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(sourceRenderer.SingularName);
                    ImGui.TableNextColumn();
                    if (ImGui.Checkbox("##Show", ref show))
                    {
                        tooltipSourceSetting.Show = show;
                        UpdateFilterConfiguration(configuration, currentValue);
                    }

                    ImGui.TableNextColumn();

                    var defaultGrouping = sourceRenderer.ShouldGroup;

                    ImGui.SetNextItemWidth(InputSize);
                    var previewValue = sourceIconGrouping == SourceIconGrouping.Default
                        ? $"{_choices[sourceIconGrouping]} ({_choices[defaultGrouping ? SourceIconGrouping.Grouped : SourceIconGrouping.Ungrouped]})"
                        : _choices[sourceIconGrouping];

                    using (var combo = ImRaii.Combo("##Combo", previewValue))
                    {
                        if (combo.Success)
                        {
                            foreach (var item in _choices)
                            {
                                var text = item.Value.Replace("\u0002\u001F\u0001\u0003", "-");
                                if (text == "")
                                {
                                    continue;
                                }

                                if (ImGui.Selectable(text, _choices[sourceIconGrouping] == text))
                                {
                                    if (item.Key == SourceIconGrouping.Grouped)
                                    {
                                        tooltipSourceSetting.Group = true;
                                    }
                                    else if (item.Key == SourceIconGrouping.Ungrouped)
                                    {
                                        tooltipSourceSetting.Group = false;
                                    }
                                    else
                                    {
                                        tooltipSourceSetting.Group = null;
                                    }

                                    UpdateFilterConfiguration(configuration, currentValue);
                                }
                            }
                        }
                    }
                }
            }
        }

    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, Dictionary<ItemInfoType, TooltipSourceSetting> newValue)
    {
        configuration.TooltipInfoSourceSetting = newValue;
        configuration.IsDirty = _draggedSetting == null;
    }

    public override string Key { get; set; } = "TooltipUseInformation";
    public override string Name { get; set; } = "Use Information Configuration";

    public override uint? Order { get; } = 2;

    public override string HelpText { get; set; } =
        "If the use information tooltip is enabled, how should the various uses be ordered/displayed/etc?";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.UseInformation;
    public override string Version { get; } = "1.11.0.11";
}