using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Interface.FormFields;
using AllaganLib.Interface.Grid;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Services;

namespace InventoryTools.EquipmentSuggest;

public sealed class EquipmentSuggestSuggestionColumn : StringFormField<EquipmentSuggestConfig>,
    IValueColumn<EquipmentSuggestConfig, EquipmentSuggestItem, MessageBase, string?>
{
    private readonly ItemSheet _itemSheet;
    private readonly ImGuiTooltipService _imGuiTooltipService;
    private readonly EquipmentSuggestLevelFormField _levelFormField;
    private readonly EquipmentSuggestSourceTypeField _typeField;
    private readonly EquipmentSuggestConfig _config;
    private readonly EquipmentSuggestViewModeSetting _viewModeSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly EquipmentSuggestModeSetting _modeSetting;
    public new ImGuiService ImGuiService { get; }
    public int Index { get; }
    public int Level { get; set; }

    public delegate EquipmentSuggestSuggestionColumn Factory(int index);

    public EquipmentSuggestSuggestionColumn(ImGuiService imGuiService, ItemSheet itemSheet,
        ImGuiTooltipService imGuiTooltipService, EquipmentSuggestLevelFormField levelFormField,
        EquipmentSuggestSourceTypeField typeField, EquipmentSuggestConfig config,
        EquipmentSuggestViewModeSetting viewModeSetting, InventoryToolsConfiguration configuration,
        EquipmentSuggestModeSetting modeSetting,
        int index) : base(imGuiService)
    {
        _itemSheet = itemSheet;
        _imGuiTooltipService = imGuiTooltipService;
        _levelFormField = levelFormField;
        _typeField = typeField;
        _config = config;
        _viewModeSetting = viewModeSetting;
        _configuration = configuration;
        _modeSetting = modeSetting;
        ImGuiService = imGuiService;
        Index = index;
        Key = "Suggestion" + Index;
        Level = index + 1;
    }

    public override string DefaultValue { get; set; } = "";
    public override string Key { get; set; }

    public IEnumerable<MessageBase>? Draw(EquipmentSuggestConfig config, EquipmentSuggestItem item, int rowIndex, int columnIndex)
    {
        var messages = new List<MessageBase>();
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var iconSize = _viewModeSetting.GetIconSize(_configuration);
            var containerSize = _viewModeSetting.GetIconContainerSize(_configuration);
            var items = item.SuggestedItems[Index].OrderByDescending(c => c.Item.Base.LevelItem.RowId).ToList();
            ImGuiService.WrapTableColumnElements("Items", items,
                iconSize * ImGui.GetIO().FontGlobalScale, containerSize * ImGui.GetIO().FontGlobalScale,
                searchResult =>
                {
                    var cursorPos = ImGui.GetCursorScreenPos();
                    var iconVec2 = new Vector2(iconSize);
                    var drawList = ImGui.GetWindowDrawList();
                    var outsideRange = searchResult.Item.Base.LevelEquip < this._levelFormField.CurrentValue(_config) - 3;
                    cursorPos.X -= iconSize / 2.0f - 1 * ImGui.GetIO().FontGlobalScale;
                    cursorPos.Y += iconSize / 2.0f - 8 * ImGui.GetIO().FontGlobalScale;


                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(searchResult.Item.Icon).ImGuiHandle,
                            new Vector2(iconSize, iconSize) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                            new Vector2(1, 1), 0))
                    {
                        var currentMode = _modeSetting.CurrentValue(_configuration);
                        if (currentMode == EquipmentSuggestMode.Class)
                        {
                            item.SelectedItem = searchResult;
                            var currentValue = _typeField.CurrentValue(config);
                            var firstSource =
                                searchResult.Item.Sources.FirstOrDefault(c => currentValue.Contains(c.Type));
                            if (firstSource == null)
                            {
                                firstSource = searchResult.Item.Sources.FirstOrDefault();
                            }

                            if (firstSource != null)
                            {
                                item.AcquisitionSource = firstSource.Type;
                            }
                        }
                        else
                        {
                            if (searchResult.Item.EquipSlotCategory?.Base.MainHand == 1)
                            {
                                item.SelectedItem = searchResult;
                                var currentValue = _typeField.CurrentValue(config);
                                var firstSource =
                                    searchResult.Item.Sources.FirstOrDefault(c => currentValue.Contains(c.Type));
                                if (firstSource == null)
                                {
                                    firstSource = searchResult.Item.Sources.FirstOrDefault();
                                }

                                if (firstSource != null)
                                {
                                    item.AcquisitionSource = firstSource.Type;
                                }
                            }
                            if (searchResult.Item.EquipSlotCategory?.Base.OffHand == 1)
                            {
                                item.SecondarySelectedItem = searchResult;
                                var currentValue = _typeField.CurrentValue(config);
                                var firstSource =
                                    searchResult.Item.Sources.FirstOrDefault(c => currentValue.Contains(c.Type));
                                if (firstSource == null)
                                {
                                    firstSource = searchResult.Item.Sources.FirstOrDefault();
                                }

                                if (firstSource != null)
                                {
                                    item.SecondaryAcquisitionSource = firstSource.Type;
                                }
                            }
                        }

                    }
                    _imGuiTooltipService.DrawItemTooltip(searchResult);
                    if (outsideRange && ImGui.IsItemHovered())
                    {
                        using (var tooltip = ImRaii.Tooltip())
                        {
                            ImGui.Separator();
                            ImGui.PushTextWrapPos();
                            ImGui.Text("This item is from outside the range visible as it's the closest item that matches, it has a lower level than the level of this column.");
                            ImGui.PopTextWrapPos();
                        }
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    }

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                    {
                        ImGui.OpenPopup("RightClickMenu");
                    }

                    if (outsideRange)
                    {
                        drawList.AddImage(ImGuiService.GetIconTexture(60955).ImGuiHandle, cursorPos,
                            cursorPos + iconVec2, Vector2.Zero, Vector2.One, ImGui.GetColorU32(ImGuiColors.ParsedGold));
                    }


                    using (var popup = ImRaii.Popup("RightClickMenu"))
                    {
                        if (popup.Success)
                        {
                            messages.AddRange(ImGuiService.ImGuiMenuService.DrawRightClickPopup(searchResult));
                        }
                    }
                    return true;
                });
        }
        return messages;
    }

    public bool DrawFilter(EquipmentSuggestConfig configuration, IColumn<EquipmentSuggestConfig, EquipmentSuggestItem, MessageBase> column, int columnIndex)
    {
        return false;
    }

    public List<MessageBase> DrawFooter(EquipmentSuggestConfig config, List<EquipmentSuggestItem> item, int columnIndex)
    {
        ImGui.TableNextColumn();
        return new();
    }

    public void SetupFilter(IColumn<EquipmentSuggestConfig, EquipmentSuggestItem, MessageBase> column, int columnIndex)
    {

    }

    public IEnumerable<EquipmentSuggestItem> Sort(EquipmentSuggestConfig configuration, IEnumerable<EquipmentSuggestItem> items, ImGuiSortDirection direction)
    {
        return items;
    }

    public IEnumerable<EquipmentSuggestItem> Filter(EquipmentSuggestConfig config, IEnumerable<EquipmentSuggestItem> items)
    {
        return items;
    }

    public string CsvExport(EquipmentSuggestItem item)
    {
        return "";
    }

    public override string Name {
        get => _levelFormField.GetCenteredValue(_config, Index).ToString();
        set { }
    }
    public string? RenderName { get; set; }
    public int Width { get; set; } = 250;
    public bool HideFilter { get; set; } = true;
    public bool IsHidden { get; set; }
    public ImGuiTableColumnFlags ColumnFlags { get; set; } = ImGuiTableColumnFlags.WidthStretch;
    public override string HelpText { get; set; } = "";
    public override string Version { get; } = "1.12.0.10";
    public string? CurrentValue(EquipmentSuggestItem item)
    {
        return "";
    }
}