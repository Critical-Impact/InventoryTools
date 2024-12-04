using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Logic.Filters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.Pages
{
    public class FilterPage : Page
    {
        private readonly IFilterService _filterService;
        private readonly IChatUtilities _chatUtilities;
        private readonly ListImportExportService _importExportService;
        private readonly IClipboardService _clipboardService;

        public FilterPage(ILogger<FilterPage> logger, ImGuiService imGuiService, IFilterService filterService, IChatUtilities chatUtilities, ListImportExportService importExportService, IClipboardService clipboardService) : base(logger, imGuiService)
        {
            _filterService = filterService;
            _chatUtilities = chatUtilities;
            _importExportService = importExportService;
            _clipboardService = clipboardService;
        }
        public override void Initialize()
        {

        }

        public void Initialize(FilterConfiguration filterConfiguration)
        {
            FilterConfiguration = filterConfiguration;
            Initialize();
        }

        public override string Name
        {
            get
            {
                return FilterConfiguration.Name;
            }
        }
        public FilterConfiguration FilterConfiguration;
        private bool _isSeparator = false;

        public override List<MessageBase>? Draw()
        {
            var filterConfiguration = FilterConfiguration;
            var filterName = filterConfiguration.Name;
            var labelName = "##" + filterConfiguration.Key;
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                ImGui.SetNextItemWidth(100);
                ImGui.LabelText(labelName + "FilterNameLabel", "Name: ");
                ImGui.SameLine();
                ImGui.InputText(labelName + "FilterName", ref filterName, 100);
                if (filterName != filterConfiguration.Name)
                {
                    filterConfiguration.Name = filterName;
                }

                ImGui.NewLine();
                if (ImGui.Button("Export Configuration to Clipboard"))
                {
                    var base64 = _importExportService.ToBase64(filterConfiguration);
                    _clipboardService.CopyToClipboard(base64);
                    _chatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
                }

                var filterType = filterConfiguration.FormattedFilterType;
                ImGui.SetNextItemWidth(100);
                ImGui.LabelText(labelName + "FilterTypeLabel", "Filter Type: ");
                ImGui.SameLine();
                ImGui.TextDisabled(filterType);

                ImGui.SetNextItemWidth(150);
                ImGui.LabelText(labelName + "DisplayInTabs", "Display in Tab List: ");
                ImGui.SameLine();
                var displayInTabs = filterConfiguration.DisplayInTabs;
                if (ImGui.Checkbox(labelName + "DisplayInTabsCheckbox", ref displayInTabs))
                {
                    if (displayInTabs != filterConfiguration.DisplayInTabs)
                    {
                        filterConfiguration.DisplayInTabs = displayInTabs;
                    }
                }
            }

            if (ImGui.BeginTabBar("###FilterConfigTabs", ImGuiTabBarFlags.FittingPolicyScroll))
            {
                foreach (var group in _filterService.GroupedFilters)
                {
                    var hasValuesSet = false;
                    foreach (var filter in group.Value)
                    {
                        if (filter.HasValueSet(filterConfiguration))
                        {
                            hasValuesSet = true;
                            break;
                        }
                    }

                    if (hasValuesSet)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                    }

                    var hasValues = group.Value.Any(filter =>
                        filter.AvailableIn.HasFlag(FilterType.SearchFilter) &&
                        filterConfiguration.FilterType.HasFlag(
                            FilterType.SearchFilter)
                        ||
                        (filter.AvailableIn.HasFlag(FilterType.SortingFilter) &&
                         filterConfiguration.FilterType.HasFlag(FilterType
                             .SortingFilter))
                        ||
                        (filter.AvailableIn.HasFlag(FilterType.CraftFilter) &&
                         filterConfiguration.FilterType.HasFlag(FilterType
                             .CraftFilter))
                        ||
                        (filter.AvailableIn.HasFlag(FilterType.HistoryFilter) &&
                         filterConfiguration.FilterType.HasFlag(FilterType
                             .HistoryFilter))
                        ||
                        (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                         filterConfiguration.FilterType.HasFlag(FilterType
                             .GameItemFilter))
                        ||
                        (filter.AvailableIn.HasFlag(FilterType.CuratedList) &&
                         filterConfiguration.FilterType.HasFlag(FilterType
                             .CuratedList)));
                    if (hasValues && ImGui.BeginTabItem(group.Key.FormattedName()))
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
                        if (group.Key == FilterCategory.CraftColumns)
                        {
                            using (var craftColumns = ImRaii.Child("craftColumns", new (0, -100)))
                            {
                                if (craftColumns.Success)
                                {
                                    group.Value.Single(c => c is CraftColumnsFilter or ColumnsFilter).Draw(filterConfiguration);
                                }
                            }
                            using (var otherFilters = ImRaii.Child("otherFilters", new (0, 0)))
                            {
                                if (otherFilters.Success)
                                {
                                    foreach (var filter in group.Value.Where(c => c is not CraftColumnsFilter && c is not ColumnsFilter))
                                    {
                                        filter.Draw(filterConfiguration);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var filter in group.Value)
                            {
                                if ((filter.AvailableIn.HasFlag(FilterType.SearchFilter) &&
                                     filterConfiguration.FilterType.HasFlag(FilterType.SearchFilter)
                                     ||
                                     (filter.AvailableIn.HasFlag(FilterType.SortingFilter) &&
                                      filterConfiguration.FilterType.HasFlag(FilterType.SortingFilter))
                                     ||
                                     (filter.AvailableIn.HasFlag(FilterType.CraftFilter) &&
                                      filterConfiguration.FilterType.HasFlag(FilterType.CraftFilter))
                                     ||
                                     (filter.AvailableIn.HasFlag(FilterType.HistoryFilter) &&
                                      filterConfiguration.FilterType.HasFlag(FilterType.HistoryFilter))
                                     ||
                                     (filter.AvailableIn.HasFlag(FilterType.CuratedList) &&
                                      filterConfiguration.FilterType.HasFlag(FilterType.CuratedList))
                                     ||
                                     (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                                      filterConfiguration.FilterType.HasFlag(FilterType.GameItemFilter))
                                    ))
                                {
                                    filter.Draw(filterConfiguration);
                                }
                            }
                        }

                        ImGui.PopStyleColor();
                        ImGui.EndTabItem();
                    }

                    if (hasValuesSet)
                    {
                        ImGui.PopStyleColor();
                    }
                }

                ImGui.EndTabBar();
            }

            return null;
        }

        public override bool IsMenuItem => _isSeparator;
        public override bool DrawBorder => true;
    }
}