using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;

using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Filters.Abstract;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class CraftColumnsFilter : SortedListFilter<ColumnConfiguration, IColumn>
    {
        private readonly IEnumerable<IColumn> _columns;
        private string _selectedColumnKey = "";
        private string _selectedColumnName = "";
        private string _selectedColumnHelp = "";
        private string _customName = "";
        private string _exportName = "";
        private bool _editMode = false;
        private ColumnConfiguration? _selectedColumnConfiguration;
        private IColumn? _selectedColumn;


        public CraftColumnsFilter(ILogger<CraftColumnsFilter> logger, ImGuiService imGuiService, IEnumerable<IColumn> columns) : base(logger, imGuiService)
        {
            _columns = columns;
        }
        public override Dictionary<ColumnConfiguration, (string, string?)> CurrentValue(FilterConfiguration configuration)
        {
            (string, string?) GetColumnDetails(ColumnConfiguration c)
            {
                return (c.Name ?? c.Column.Name, c.Column.HelpText);
            }

            return (configuration.CraftColumns ?? new List<ColumnConfiguration>()).ToDictionary(c => c, GetColumnDetails);
        }


        public override void UpdateFilterConfiguration(FilterConfiguration configuration, Dictionary<ColumnConfiguration, (string, string?)> newValue)
        {
            configuration.CraftColumns = newValue.Select(c => c.Key).ToList();
            configuration.TableConfigurationDirty = true;
        }

        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, new Dictionary<ColumnConfiguration, (string, string?)>());
        }

        public override string Key { get; set; } = "Craft Columns";
        public override string Name { get; set; } = "Craft Columns";
        public override string HelpText { get; set; } = "";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.CraftColumns;
        public override bool ShowReset { get; set; } = false;
        public override Dictionary<ColumnConfiguration, (string, string?)> DefaultValue { get; set; } = new();

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return configuration.CraftColumns != null && configuration.CraftColumns.Count != 0;
        }

        public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public override bool CanRemove { get; set; } = true;
        public override bool CanRemoveItem(FilterConfiguration configuration, ColumnConfiguration item)
        {
            return true;
        }

        public override IColumn? GetItem(FilterConfiguration configuration, ColumnConfiguration item)
        {
            return item.Column;
        }

        public void AddItem(FilterConfiguration configuration, ColumnConfiguration item)
        {
            var value = CurrentValue(configuration);
            value.Add(item, ("", null));
            UpdateFilterConfiguration(configuration, value);
        }

        public Dictionary<string, IColumn> GetAvailableItems(FilterConfiguration configuration)
        {
            var value = _columns;
            return value.Where(c => c.CraftOnly != false && c.AvailableInType(configuration.FilterType)).ToDictionary(c => c.GetType().Name, c => c);
        }

        private FilterType? _lastFilterType;
        private List<IGrouping<ColumnCategory, KeyValuePair<string, IColumn>>>? _groupedItems;
        public List<IGrouping<ColumnCategory, KeyValuePair<string, IColumn>>> GetGroupedItems(FilterConfiguration configuration)
        {
            if (_groupedItems == null || _lastFilterType == null || _lastFilterType != configuration.FilterType)
            {
                var availableItems = GetAvailableItems(configuration).OrderBy(c => c.Value.Name);
                _groupedItems = availableItems.OrderBy(c => c.Value.Name).GroupBy(c => c.Value.ColumnCategory).ToList();
                _lastFilterType = configuration.FilterType;
            }

            return _groupedItems;
        }

        public string SearchString
        {
            get => _searchString;
            set => _searchString = value;
        }

        private string _searchString = "";

        public override void DrawButtons(FilterConfiguration configuration, KeyValuePair<ColumnConfiguration, (string, string?)> item, int index)
        {
            base.DrawButtons(configuration, item, index);
            ImGui.SameLine();
            if (ImGui.Button("Edit##Column" + index))
            {
                EditItem(configuration, item.Key);
            }
        }

        private void EditItem(FilterConfiguration configuration, ColumnConfiguration item)
        {
            _editMode = true;
            _selectedColumnConfiguration = item;
            _selectedColumnKey = item.Key;
            _selectedColumnName = item.Column.Name;
            _selectedColumnHelp = item.Column.HelpText;
            _customName = item.Name ?? "";
            _exportName = item.ExportName ?? "";
        }

        public override void Draw(FilterConfiguration configuration)
        {
            var width = ImGui.GetContentRegionAvail().X / 2;
            var height = ImGui.GetWindowHeight() / 2 - (ImGui.GetStyle().ChildBorderSize * 2);
            bool collapse = width <= 450;

            using (var table = ImRaii.Child("columnEditTable", new Vector2(collapse ? 0 : width, collapse ? height : 0), true))
            {
                if (table.Success)
                {
                    var groupedItems = GetGroupedItems(configuration);
                    if (_selectedColumnKey == "")
                    {
                        ImGui.Text("Add Column");
                        ImGui.Separator();
                        var searchString = SearchString;
                        ImGui.InputText("##ItemSearch", ref searchString, 50);
                        if (_searchString != searchString)
                        {
                            SearchString = searchString;
                        }

                        ImGui.Separator();
                        if (_searchString == "")
                        {
                            ImGui.TextUnformatted("Start typing to search...");
                        }

                        ImGui.Separator();
                        var parsedSearchString = _searchString.ToParseable();
                        foreach (var groupedItem in groupedItems)
                        {
                            var hasColumns = false;
                            foreach (var column in groupedItem)
                            {
                                if (parsedSearchString == "" ||
                                    column.Value.Name.ToParseable().Contains(parsedSearchString) ||
                                    column.Value.HelpText.ToParseable().Contains(parsedSearchString))
                                {
                                    hasColumns = true;
                                }
                            }

                            if (!hasColumns)
                            {
                                continue;
                            }

                            if (ImGui.CollapsingHeader(groupedItem.Key.ToString(), ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var column in groupedItem)
                                {
                                    if (parsedSearchString != "" &&
                                        !column.Value.Name.ToParseable().Contains(parsedSearchString) &&
                                        !column.Value.HelpText.ToParseable().Contains(parsedSearchString))
                                    {
                                        continue;
                                    }

                                    ImRaii.Color? pushColor = null;
                                    if (_selectedColumn == column.Value)
                                    {
                                        pushColor = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                                    }

                                    if (ImGui.Selectable(column.Value.Name))
                                    {
                                        if (_selectedColumn == column.Value)
                                        {
                                            _selectedColumn = null;
                                        }
                                        else
                                        {
                                            _selectedColumn = column.Value;
                                        }
                                    }

                                    if (column.Value.DefaultIn.HasFlag(configuration.FilterType))
                                    {
                                        ImGui.SameLine();
                                        ImGui.Image(ImGuiService.GetIconTexture(Icons.SproutIcon).ImGuiHandle, new Vector2(16,16));
                                        ImGuiUtil.HoverTooltip("Default Column");
                                    }

                                    if (column.Value.IsConfigurable)
                                    {
                                        ImGui.SameLine();
                                        ImGui.Image(ImGuiService.GetIconTexture(Icons.WrenchIcon).ImGuiHandle, new Vector2(16,16));
                                        ImGuiUtil.HoverTooltip("Configurable");
                                    }

                                    if (pushColor != null)
                                    {
                                        pushColor.Pop();
                                    }

                                    if (_selectedColumn == column.Value)
                                    {
                                        ImGui.Separator();
                                        ImGui.PushTextWrapPos();
                                        ImGui.Text(column.Value.HelpText);
                                        ImGui.PopTextWrapPos();
                                        if (ImGui.Button("Add"))
                                        {
                                            _selectedColumnName = column.Value.Name;
                                            _selectedColumnHelp = column.Value.HelpText;
                                            _selectedColumnKey = column.Key;
                                            _selectedColumnConfiguration = new ColumnConfiguration(column.Key);
                                            _selectedColumnConfiguration.Column = column.Value;
                                            _customName = "";
                                            _exportName = "";
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (_selectedColumnKey != "")
                    {
                        string customName = _customName;
                        ImGui.Text(_selectedColumnName);
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        ImGui.Text(_selectedColumnHelp);
                        ImGui.PopTextWrapPos();
                        ImGui.Separator();
                        ImGui.SetNextItemWidth(LabelSize);
                        ImGui.LabelText("##" + Key + "Custom", "Custom Column Name: ");
                        ImGui.SetNextItemWidth(InputSize);
                        ImGui.SameLine();
                        if (ImGui.InputTextWithHint("##CustomColumnName", _selectedColumnName, ref customName, 100,
                                ImGuiInputTextFlags.None))
                        {
                            _customName = customName;
                        }

                        string exportName = _exportName;
                        ImGui.SetNextItemWidth(LabelSize);
                        ImGui.LabelText("##" + Key + "Export", "Custom Export Name: ");
                        ImGui.SetNextItemWidth(InputSize);
                        ImGui.SameLine();
                        if (ImGui.InputTextWithHint("##CustomExportName", _selectedColumnName, ref exportName, 100,
                                ImGuiInputTextFlags.None))
                        {
                            _exportName = exportName;
                        }

                        ImGui.SameLine();
                        var posX = ImGui.GetCursorPosX();

                        if (_selectedColumnConfiguration != null)
                        {
                            _selectedColumnConfiguration.Column.DrawEditor(_selectedColumnConfiguration, configuration);
                        }


                        ImGui.NewLine();
                        ImGui.SetCursorPosX(posX - ImGui.GetStyle().ItemSpacing.X - 40);
                        posX = ImGui.GetCursorPosX();
                        if (ImGui.Button(_editMode ? "Save" : "Add", new Vector2(40, 20)))
                        {
                            if (_editMode)
                            {
                                var columnConfiguration = configuration.GetCraftColumn(_selectedColumnKey);
                                if (columnConfiguration != null)
                                {
                                    columnConfiguration.Name = _customName == "" ? null : _customName;
                                    columnConfiguration.ExportName = _exportName == "" ? null : _exportName;
                                    UpdateFilterConfiguration(configuration, CurrentValue(configuration));
                                }

                                _selectedColumnName = "";
                                _selectedColumnKey = "";
                                _customName = "";
                                _editMode = false;
                            }
                            else
                            {
                                var columnConfiguration =
                                    _selectedColumnConfiguration ?? new ColumnConfiguration(_selectedColumnKey);
                                columnConfiguration.Name = _customName == "" ? null : _customName;
                                columnConfiguration.ExportName = _exportName == "" ? null : _exportName;
                                AddItem(configuration, columnConfiguration);

                                _selectedColumnName = "";
                                _selectedColumnKey = "";
                                _customName = "";
                            }
                        }
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(posX - ImGui.GetStyle().ItemSpacing.X - 50);
                        if (ImGui.Button("Cancel", new Vector2(50, 20)))
                        {
                            _selectedColumnName = "";
                            _selectedColumnKey = "";
                            _customName = "";
                            _editMode = false;
                        }
                    }
                }
            }

            if (!collapse)
            {
                ImGui.SameLine();
            }

            using (var table = ImRaii.Child("columnsTable", new Vector2(0, collapse ? height : 0), true))
            {
                if (table.Success)
                {
                    ImGui.Text("Current Columns:");
                    ImGui.Separator();
                    DrawTable(configuration);
                }
            }


        }
    }
}