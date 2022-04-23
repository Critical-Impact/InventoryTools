using System;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Fonts;
using InventoryTools.Logic;

namespace InventoryTools.Sections
{
    public static class ConfigurationSection
    {
        private static int ConfigSelectedConfigurationPage
        {
            get => Configuration.SelectedConfigurationPage;
            set => Configuration.SelectedConfigurationPage = value;
        }

        private static InventoryToolsConfiguration Configuration => ConfigurationManager.Config;

        public static void Draw()
        {
            var highlightWhenItems = new string[] {"Always", "When Searching"};
            

            if (ImGui.BeginChild("###ivConfigList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (ImGui.Selectable("General", ConfigSelectedConfigurationPage == 0))
                {
                    ConfigSelectedConfigurationPage = 0;
                }

                if (ImGui.Selectable("Visuals", ConfigSelectedConfigurationPage == 1))
                {
                    ConfigSelectedConfigurationPage = 1;
                }

                if (ImGui.Selectable("Market Board", ConfigSelectedConfigurationPage == 2))
                {
                    ConfigSelectedConfigurationPage = 2;
                }

                if (ImGui.Selectable("Filters", ConfigSelectedConfigurationPage == 3))
                {
                    ConfigSelectedConfigurationPage = 3;
                }

                if (ImGui.Selectable("Import/Export", ConfigSelectedConfigurationPage == 4))
                {
                    ConfigSelectedConfigurationPage = 4;
                }

                ImGui.Separator();

                for (var index = 0; index < PluginService.PluginLogic.FilterConfigurations.Count; index++)
                {
                    var filterConfiguration = PluginService.PluginLogic.FilterConfigurations[index];
                    if (PluginFont.AppIcons.HasValue && filterConfiguration.Icon != null)
                    {
                        ImGui.PushFont(PluginFont.AppIcons.Value);
                        if (ImGui.Selectable(
                            filterConfiguration.Name + " " + filterConfiguration.Icon + "##" +
                            filterConfiguration.Key,
                            index + 5 == ConfigSelectedConfigurationPage))
                        {
                            ImGui.PopFont();
                            ConfigSelectedConfigurationPage = index + 5;
                        }
                        else
                        {
                            ImGui.PopFont();
                        }
                    }
                    else
                    {
                        if (ImGui.Selectable(
                            filterConfiguration.Name + "##" +
                            filterConfiguration.Key,
                            index + 5 == ConfigSelectedConfigurationPage))
                        {
                            ConfigSelectedConfigurationPage = index + 5;
                        }
                    }
                }

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("###ivConfigView", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (ConfigSelectedConfigurationPage == 0)
                {
                    GeneralConfigurationSection.Draw();
                }

                if (ConfigSelectedConfigurationPage == 1)
                {
                    bool colorRetainerList = Configuration.ColorRetainerList;
                    bool showItemNumberRetainerList = Configuration.ShowItemNumberRetainerList;
                    bool invertHighlighting = Configuration.InvertHighlighting;
                    bool invertTabHighlighting = Configuration.InvertTabHighlighting;
                    string highlightWhen = Configuration.HighlightWhen;
                    Vector4 highlightColor = Configuration.HighlightColor;
                    Vector4 tabHighlightColor = Configuration.TabHighlightColor;
                    Vector4 retainerListColor = Configuration.RetainerListColor;

                    ImGui.Text("Visuals:");
                    ImGui.Separator();

                    if (ImGui.ColorEdit4("Highlight Color?", ref highlightColor, ImGuiColorEditFlags.NoInputs))
                    {
                        Configuration.HighlightColor = highlightColor;
                    }

                    if (Configuration.HighlightColor.W == 0)
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(ImGuiColors.DalamudRed,
                            "The filter alpha is set to 0, your items will be invisible.");
                    }


                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "The color to set the highlighted items to.");

                    if (ImGui.ColorEdit4("Tab Highlight Color?", ref tabHighlightColor, ImGuiColorEditFlags.NoInputs))
                    {
                        Configuration.TabHighlightColor = tabHighlightColor;
                    }

                    if (Configuration.TabHighlightColor.W == 0)
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(ImGuiColors.DalamudRed,
                            "The filter alpha is set to 0, your items will be invisible.");
                    }


                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "The color to set the highlighted tabs(that contain filtered items) to.");

                    if (ImGui.ColorEdit4("Retainer List Highlight Color?", ref retainerListColor, ImGuiColorEditFlags.NoInputs))
                    {
                        Configuration.RetainerListColor = retainerListColor;
                    }

                    if (Configuration.RetainerListColor.W == 0)
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(ImGuiColors.DalamudRed,
                            "The filter alpha is set to 0, your retainer list items will be invisible.");
                    }


                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "The color to set the retainer(when the retainer contains filtered items) list to.");

                    if (ImGui.Checkbox("Invert Highlighting?", ref invertHighlighting))
                    {
                        Configuration.InvertHighlighting = !Configuration.InvertHighlighting;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "Should all the items not matching a filter be highlighted instead? This can be overridden in the filter configuration.");

                    if (ImGui.Checkbox("Invert Tab Highlighting?", ref invertTabHighlighting))
                    {
                        Configuration.InvertTabHighlighting = !Configuration.InvertTabHighlighting;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "Should all the tabs not matching a filter be highlighted instead? This can be overridden in the filter configuration.");

                    ImGui.SetNextItemWidth(205);
                    ImGui.LabelText("##HighlightWhen", "Highlight When?: ");
                    ImGui.SameLine();
                    var highlightWhenIndex = Array.IndexOf(highlightWhenItems, highlightWhen);
                    if (highlightWhenIndex == -1)
                    {
                        highlightWhenIndex++;
                    }

                    if (ImGui.Combo("##HighlightWhenCombo", ref highlightWhenIndex, highlightWhenItems,
                        highlightWhenItems.Length))
                    {
                        highlightWhen = highlightWhenItems[highlightWhenIndex];
                        if (highlightWhen != Configuration.HighlightWhen)
                        {
                            Configuration.HighlightWhen = highlightWhen;
                        }
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "When should the highlighting apply?");


                    if (ImGui.Checkbox("Color name in retainer list?", ref colorRetainerList))
                    {
                        Configuration.ColorRetainerList = !Configuration.ColorRetainerList;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "Should the name of the retainer in the summoning bell list be coloured if a relevant item is to be sorted or is available in their inventory?");
                    if (ImGui.Checkbox("Show item number in retainer list?", ref showItemNumberRetainerList))
                    {
                        Configuration.ShowItemNumberRetainerList = !Configuration.ShowItemNumberRetainerList;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "Should the name of the retainer in the summoning bell list have the number of items to be sorted or are available in their inventory?");
                }

                if (ConfigSelectedConfigurationPage == 2)
                {
                    ImGui.Text("Marketboard Settings:");
                    ImGui.Separator();

                    bool automaticallyDownloadMarketPrices = Configuration.AutomaticallyDownloadMarketPrices;
                    int marketRefreshTime = Configuration.MarketRefreshTimeHours;

                    if (ImGui.Checkbox("Automatically download prices?", ref automaticallyDownloadMarketPrices))
                    {
                        Configuration.AutomaticallyDownloadMarketPrices =
                            !Configuration.AutomaticallyDownloadMarketPrices;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "Should we automatically download prices for any item found?");

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "Should the inventories/configuration be automatically saved on a defined interval? While the plugin does save when the game is closed and when configurations are altered, it is not saved in cases of crashing so this attempts to alleviate this.");

                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Keep market prices for X hours:", ref marketRefreshTime))
                    {
                        if (marketRefreshTime != Configuration.MarketRefreshTimeHours)
                        {
                            Configuration.MarketRefreshTimeHours = marketRefreshTime;
                        }
                    }
                }

                if (ConfigSelectedConfigurationPage == 3)
                {
                    if (ImGui.CollapsingHeader("Filters", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5));
                        if (ImGui.BeginTable("FilterConfigTable", 3, ImGuiTableFlags.BordersV |
                                                                     ImGuiTableFlags.BordersOuterV |
                                                                     ImGuiTableFlags.BordersInnerV |
                                                                     ImGuiTableFlags.BordersH |
                                                                     ImGuiTableFlags.BordersOuterH |
                                                                     ImGuiTableFlags.BordersInnerH))
                        {
                            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 0);
                            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 1);
                            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 2);
                            ImGui.TableHeadersRow();
                            if (PluginService.PluginLogic.FilterConfigurations.Count == 0)
                            {
                                ImGui.TableNextRow();
                                ImGui.TableNextColumn();
                                ImGui.Text("No filters available.");
                                ImGui.TableNextColumn();
                                ImGui.TableNextColumn();
                            }

                            for (var index = 0; index < PluginService.PluginLogic.FilterConfigurations.Count; index++)
                            {
                                ImGui.TableNextRow();
                                var filterConfiguration = PluginService.PluginLogic.FilterConfigurations[index];
                                ImGui.TableNextColumn();
                                if (filterConfiguration.Name != "")
                                {
                                    ImGui.Text(filterConfiguration.Name);
                                    ImGui.SameLine();
                                }

                                if (PluginFont.AppIcons.HasValue && filterConfiguration.Icon != null)
                                {
                                    ImGui.PushFont(PluginFont.AppIcons.Value);
                                    ImGui.Text(filterConfiguration.Icon);
                                    ImGui.PopFont();
                                }

                                ImGui.TableNextColumn();
                                ImGui.Text(filterConfiguration.FormattedFilterType);
                                ImGui.TableNextColumn();
                                if (ImGui.SmallButton("Export Configuration##" + index))
                                {
                                    var base64 = filterConfiguration.ExportBase64();
                                    ImGui.SetClipboardText(base64);
                                    ChatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
                                }

                                ImGui.SameLine();
                                if (ImGui.SmallButton("Remove##" + index))
                                {
                                    ImGui.OpenPopup("Delete?##" + index);
                                }

                                ImGui.SameLine();
                                if (ImGui.SmallButton("Open as Window##" + index))
                                {
                                    filterConfiguration.OpenAsWindow = true;
                                }

                                if (ImGui.BeginPopupModal("Delete?##" + index))
                                {
                                    ImGui.Text(
                                        "Are you sure you want to delete this filter?.\nThis operation cannot be undone!\n\n");
                                    ImGui.Separator();

                                    if (ImGui.Button("OK", new Vector2(120, 0)))
                                    {
                                        PluginService.PluginLogic.RemoveFilter(filterConfiguration);
                                        ImGui.CloseCurrentPopup();
                                    }

                                    ImGui.SetItemDefaultFocus();
                                    ImGui.SameLine();
                                    if (ImGui.Button("Cancel", new Vector2(120, 0)))
                                    {
                                        ImGui.CloseCurrentPopup();
                                    }

                                    ImGui.EndPopup();
                                }
                            }

                            ImGui.EndTable();
                        }

                        ImGui.PopStyleVar();
                    }

                    if (ImGui.CollapsingHeader("Create Filters", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        if (ImGui.Button("Add Search Filter"))
                        {
                            PluginService.PluginLogic.FilterConfigurations.Add(new FilterConfiguration("New Search Filter",
                                Guid.NewGuid().ToString("N"), FilterType.SearchFilter));
                        }

                        ImGui.SameLine();
                        UiHelpers.HelpMarker(
                            "This will create a new filter that let's you search for specific items within your characters and retainers inventories.");

                        if (ImGui.Button("Add Sort Filter"))
                        {
                            PluginService.PluginLogic.FilterConfigurations.Add(new FilterConfiguration("New Sort Filter",
                                Guid.NewGuid().ToString("N"), FilterType.SortingFilter));
                        }

                        ImGui.SameLine();
                        UiHelpers.HelpMarker(
                            "This will create a new filter that let's you search for specific items within your characters and retainers inventories then determine where they should be moved to.");


                        if (ImGui.Button("Add Game Item Filter"))
                        {
                            PluginService.PluginLogic.FilterConfigurations.Add(new FilterConfiguration("New Game Item Filter",
                                Guid.NewGuid().ToString("N"), FilterType.GameItemFilter));
                        }

                        ImGui.SameLine();
                        UiHelpers.HelpMarker(
                            "This will create a filter that lets you search for all items in the game.");
                    }

                    if (ImGui.CollapsingHeader("Sample Filters", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.Text("Sample Filters:");
                        if (ImGui.Button("Items that can be bought for 100 gil or less +"))
                        {
                            PluginService.PluginLogic.AddSampleFilter100Gil();
                        }

                        ImGui.SameLine();
                        UiHelpers.HelpMarker(
                            "This will add a filter that will show all items that can be purchased from gil shops under 100 gil. It will look in both character and retainer inventories.");

                        if (ImGui.Button("Put away materials +"))
                        {
                            PluginService.PluginLogic.AddSampleFilterMaterials();
                        }

                        ImGui.SameLine();
                        UiHelpers.HelpMarker(
                            "This will add a filter that will be setup to quickly put away any excess materials. It will have all the material categories automatically added. When calculating where to put items it will try to prioritise existing stacks of items.");

                        if (ImGui.Button("Duplicated items across characters/retainers +"))
                        {
                            PluginService.PluginLogic.AddSampleFilterDuplicatedItems();
                        }

                        ImGui.SameLine();
                        UiHelpers.HelpMarker(
                            "This will add a filter that will provide a list of all the distinct stacks that appear in 2 sets of inventories. You can use this to make sure only one retainer has a specific type of item.");
                    }
                }

                if (ConfigSelectedConfigurationPage == 4)
                {
                    ImportExportSection.Draw();
                }

                if (ConfigSelectedConfigurationPage > 4)
                {
                    var selectedFilter = ConfigSelectedConfigurationPage - 5;
                    if (selectedFilter >= 0 && PluginService.PluginLogic.FilterConfigurations.Count > selectedFilter)
                    {
                        var filterConfiguration = PluginService.PluginLogic.FilterConfigurations[selectedFilter];
                        var filterName = filterConfiguration.Name;
                        var labelName = "##" + selectedFilter.ToString();
                        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.SetNextItemWidth(100);
                            ImGui.LabelText(labelName + "FilterNameLabel", "Name: ");
                            ImGui.SameLine();
                            ImGui.InputText(labelName + "FilterName", ref filterName, 100);
                            if (filterName != filterConfiguration.Name)
                            {
                                filterConfiguration.Name = filterName;
                            }

                            ImGui.SetNextItemWidth(100);
                            ImGui.LabelText(labelName + "IconNameLabel", "Tab Icon: ");
                            ImGui.SameLine();
                            var icons = FfxivAppIcons.Icons.Values.ToArray();
                            var icon = filterConfiguration.Icon == null
                                ? 0
                                : FfxivAppIcons.Icons.Keys.ToList().IndexOf(filterConfiguration.Icon);
                            icon = icon == -1 ? 0 : icon;
                            if (ImGui.Combo(labelName + "Icon", ref icon, icons, icons.Length))
                            {
                                var newIcon = FfxivAppIcons.Icons.Keys.ToList()[icon];
                                if (FfxivAppIcons.Icons.ContainsKey(newIcon) && newIcon != "")
                                {
                                    PluginLog.Log("icon set to " + newIcon);
                                    filterConfiguration.Icon = newIcon;
                                }
                                else
                                {
                                    PluginLog.Log("icon set to null");
                                    filterConfiguration.Icon = null;
                                }
                            }

                            ImGui.SameLine();
                            if (PluginFont.AppIcons.HasValue && filterConfiguration.Icon != null)
                            {
                                ImGui.PushFont(PluginFont.AppIcons.Value);
                                ImGui.Text(filterConfiguration.Icon);
                                ImGui.PopFont();
                            }

                            ImGui.NewLine();
                            if (ImGui.Button("Export Configuration to Clipboard"))
                            {
                                var base64 = filterConfiguration.ExportBase64();
                                ImGui.SetClipboardText(base64);
                                ChatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
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
                            foreach (var group in PluginService.PluginLogic.GroupedFilters)
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
                                    (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                                     filterConfiguration.FilterType.HasFlag(FilterType
                                         .GameItemFilter)));
                                if (hasValues && ImGui.BeginTabItem(group.Key.ToString()))
                                {
                                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
                                    foreach (var filter in group.Value)
                                    {
                                        if ((filter.AvailableIn.HasFlag(FilterType.SearchFilter) &&
                                             filterConfiguration.FilterType.HasFlag(FilterType.SearchFilter)
                                             ||
                                             (filter.AvailableIn.HasFlag(FilterType.SortingFilter) &&
                                              filterConfiguration.FilterType.HasFlag(FilterType.SortingFilter))
                                             ||
                                             (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                                              filterConfiguration.FilterType.HasFlag(FilterType.GameItemFilter))
                                            ))
                                        {
                                            filter.Draw(filterConfiguration);
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
                    }
                }

                ImGui.EndChild();
            }
        }
    }
}