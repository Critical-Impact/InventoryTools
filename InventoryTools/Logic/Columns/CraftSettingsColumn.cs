using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Ui.Widgets;
using OtterGui.Raii;
using ImGuiUtil = OtterGui.ImGuiUtil;
using Vector2 = FFXIVClientStructs.FFXIV.Common.Math.Vector2;

namespace InventoryTools.Logic.Columns;

public class CraftSettingsColumn : IColumn
{
    public ColumnCategory ColumnCategory => ColumnCategory.Crafting;
    private HoverButton _settingsIcon { get; } = new(PluginService.IconStorage.LoadIcon(66319),  new Vector2(22, 22));
    private TextureWrap _hqIcon { get; } = PluginService.IconStorage.LoadImage("hq");
    private TextureWrap _retainerIcon { get; } = PluginService.IconStorage.LoadIcon(60425);


    public string Name { get; set; } = "Settings";
    public float Width { get; set; } = 80;
    public string HelpText { get; set; } = "Modify each items craft settings in this column";
    public string FilterText { get; set; } = "";

    public string RenderName { get; } = "";

    public List<string>? FilterChoices { get; set; } = null;
    public bool HasFilter { get; set; } = false;
    public bool CanBeRemoved { get; } = false;
    public ColumnFilterType FilterType { get; set; } = ColumnFilterType.None;
    public bool IsDebug { get; set; } = false;
    public FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
    public bool AvailableInType(FilterType type)
    {
        return type == Logic.FilterType.CraftFilter;
    }

    public bool? CraftOnly { get; } = true;
    public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
    {
        return items;
    }

    public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
    {
        return items;
    }

    public IEnumerable<ItemEx> Filter(IEnumerable<ItemEx> items)
    {
        return items;
    }

    public IEnumerable<CraftItem> Filter(IEnumerable<CraftItem> items)
    {
        return items;
    }

    public IEnumerable<InventoryChange> Filter(IEnumerable<InventoryChange> items)
    {
        return items;
    }

    public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
    {
        return items;
    }

    public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
    {
        return items;
    }

    public IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items)
    {
        return items;
    }

    public IEnumerable<CraftItem> Sort(ImGuiSortDirection direction, IEnumerable<CraftItem> items)
    {
        return items;
    }

    public IEnumerable<InventoryChange> Sort(ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
    {
        return items;
    }

    public void Draw(FilterConfiguration configuration, InventoryItem item, int rowIndex)
    {
        return;
    }

    public void Draw(FilterConfiguration configuration, SortingResult item, int rowIndex)
    {
        return;
    }

    public void Draw(FilterConfiguration configuration, ItemEx item, int rowIndex)
    {
        return;
    }

    public void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
         ImGui.TableNextColumn();

        using (var popup = ImRaii.Popup("ConfigureItemSettings" + rowIndex))
        {
            if (popup.Success)
            {
                ImGui.Text("Configure Sourcing:");
                ImGui.Separator();


                if (Service.ExcelCache.ItemRecipes.ContainsKey(item.ItemId))
                {
                    var itemRecipes = Service.ExcelCache.ItemRecipes[item.ItemId];
                    if (itemRecipes.Count != 1)
                    {
                        var actualRecipes = itemRecipes.Select(c =>
                                Service.ExcelCache.GetRecipeExSheet().GetRow(c)!)
                            .OrderBy(c => c.CraftType.Value?.Name ?? "").ToList();
                        var recipeName = item.Recipe?.CraftType.Value?.Name ?? "";
                        using (var combo = ImRaii.Combo("##SetRecipe" + rowIndex, recipeName))
                        {
                            if (combo.Success)
                            {
                                foreach (var recipe in actualRecipes)
                                {
                                    if (ImGui.Selectable(recipe.CraftType.Value?.Name ?? "",
                                            recipeName == (recipe.CraftType.Value?.Name ?? "")))
                                    {
                                        if (item.IsOutputItem)
                                        {
                                            configuration.CraftList.SetCraftRecipe(item.ItemId,
                                                recipe.RowId);
                                            configuration.StartRefresh();
                                        }
                                        else
                                        {
                                            configuration.CraftList.UpdateCraftRecipePreference(item.ItemId,
                                                recipe.RowId);
                                            configuration.StartRefresh();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (item.Item.CanBeHq)
                {
                    var currentHQRequired = configuration.CraftList.GetHQRequired(item.ItemId);
                    var previewValue = "Use Default";
                    if (currentHQRequired != null)
                    {
                        previewValue = currentHQRequired.Value ? "Yes" : "No";
                    }
                    ImGui.Text("HQ Required:");
                    using (var combo = ImRaii.Combo("##SetHQRequired" + rowIndex, previewValue))
                    {
                        if (combo.Success)
                        {
                            if (ImGui.Selectable("Use Default"))
                            {
                                configuration.CraftList.UpdateHQRequired(item.ItemId, null);
                                configuration.NeedsRefresh = true;
                                configuration.NotifyConfigurationChange();
                            }
                            if (ImGui.Selectable("Yes"))
                            {
                                configuration.CraftList.UpdateHQRequired(item.ItemId, true);
                                configuration.NeedsRefresh = true;
                                configuration.NotifyConfigurationChange();
                            }
                            if (ImGui.Selectable("No"))
                            {
                                configuration.CraftList.UpdateHQRequired(item.ItemId, false);
                                configuration.NeedsRefresh = true;
                                configuration.NotifyConfigurationChange();
                            }
                        }
                    }
                }

                //Retrieve from retainer combo
                {
                    var craftRetainerRetrieval = configuration.CraftList.GetCraftRetainerRetrieval(item.ItemId);
                    var previewValue = "Use Default";
                    if (craftRetainerRetrieval != null)
                    {
                        switch (craftRetainerRetrieval.Value)
                        {
                            case CraftRetainerRetrieval.Yes:
                                previewValue = "Yes";
                                break;
                            case CraftRetainerRetrieval.No:
                                previewValue = "No";
                                break;
                            case CraftRetainerRetrieval.HQOnly:
                                previewValue = "HQ Only";
                                break;
                        }
                    }
                    ImGui.Text("Retrieve from Retainer:");
                    using (var combo = ImRaii.Combo("##SetRetrieveRetainer" + rowIndex, previewValue))
                    {
                        if (combo.Success)
                        {
                            if (ImGui.Selectable("Use Default"))
                            {
                                configuration.CraftList.UpdateCraftRetainerRetrieval(item.ItemId, null);
                                configuration.NeedsRefresh = true;
                                configuration.NotifyConfigurationChange();
                            }
                            if (ImGui.Selectable("Yes"))
                            {
                                configuration.CraftList.UpdateCraftRetainerRetrieval(item.ItemId, CraftRetainerRetrieval.Yes);
                                configuration.NeedsRefresh = true;
                                configuration.NotifyConfigurationChange();
                            }
                            if (ImGui.Selectable("No"))
                            {
                                configuration.CraftList.UpdateCraftRetainerRetrieval(item.ItemId, CraftRetainerRetrieval.No);
                                configuration.NeedsRefresh = true;
                                configuration.NotifyConfigurationChange();
                            }
                            if (ImGui.Selectable("HQ Only"))
                            {
                                configuration.CraftList.UpdateCraftRetainerRetrieval(item.ItemId, CraftRetainerRetrieval.HQOnly);
                                configuration.NeedsRefresh = true;
                                configuration.NotifyConfigurationChange();
                            }
                        }
                    }
                }

                if (item.Item.IngredientPreferences.Count != 0)
                {
                    var currentIngredientPreference =
                        configuration.CraftList.GetIngredientPreference(item.ItemId);
                    var previewValue = currentIngredientPreference?.FormattedName ?? "Use Default";
                    ImGui.Text("Source Preference:");
                    using (var combo = ImRaii.Combo("##SetIngredients" + rowIndex, previewValue))
                    {
                        if (combo.Success)
                        {
                            if (ImGui.Selectable("Use Default"))
                            {
                                configuration.CraftList.UpdateIngredientPreference(item.ItemId, null);
                                configuration.NeedsRefresh = true;
                                configuration.NotifyConfigurationChange();
                            }
                            foreach (var ingredientPreference in item.Item.IngredientPreferences)
                            {
                                if (ImGui.Selectable(ingredientPreference.FormattedName))
                                {
                                    configuration.CraftList.UpdateIngredientPreference(item.ItemId, ingredientPreference);
                                    configuration.NeedsRefresh = true;
                                    configuration.NotifyConfigurationChange();
                                }
                            }
                        }
                    }
                }
            }
        }
        var ingredientPreferenceDefault = configuration.CraftList.GetIngredientPreference(item.ItemId);
        var retainerRetrievalDefault = configuration.CraftList.GetCraftRetainerRetrieval(item.ItemId);
        var retainerRetrieval = retainerRetrievalDefault ?? (item.IsOutputItem ? CraftRetainerRetrieval.No : CraftRetainerRetrieval.Yes);
        var hqRequired = configuration.CraftList.GetHQRequired(item.ItemId);

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
        var icon = item.SourceIcon;
        ImGui.Image(PluginService.IconStorage[icon].ImGuiHandle,
            new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
        var itemRecipe = item.Recipe;
        if (itemRecipe != null)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
            {
                using var tt = ImRaii.Tooltip();
                ImGui.Text($"Recipe ({itemRecipe.CraftTypeEx.Value?.FormattedName ?? "Unknown"}): ");
                foreach (var ingredient in itemRecipe.Ingredients)
                {
                    var actualItem = ingredient.Item.Value;
                    var quantity = ingredient.Count;
                    if (actualItem != null)
                    {
                        ImGui.Text(actualItem.NameString + " : " + quantity);
                    }
                }
            }
        }
        else
        {
            ImGuiUtil.HoverTooltip(item.SourceName);
        }
        ImGui.SameLine();
        

        if(hqRequired == true)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
            ImGui.Image(_hqIcon.ImGuiHandle,new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale,new System.Numerics.Vector2(0,0), new System.Numerics.Vector2(1,1), new Vector4(0.9f,0.75f,0.14f,1f));
            ImGuiUtil.HoverTooltip("HQ");
            ImGui.SameLine();
        }
        else
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
            ImGui.Image(_hqIcon.ImGuiHandle,new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale,new System.Numerics.Vector2(0,0), new System.Numerics.Vector2(1,1), new Vector4(0.9f,0.75f,0.14f,0.2f));
            ImGuiUtil.HoverTooltip(item.Item.CanBeHq ? "No" : "Cannot be HQ");
            ImGui.SameLine();
        }
        if(retainerRetrieval is CraftRetainerRetrieval.HQOnly or CraftRetainerRetrieval.Yes)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
            ImGui.Image(_retainerIcon.ImGuiHandle,new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale,new System.Numerics.Vector2(0,0), new System.Numerics.Vector2(1,1), retainerRetrieval == CraftRetainerRetrieval.HQOnly ? new Vector4(0.9f,0.75f,0.14f,1f) : new Vector4(1f,1f,1f,1f));
            ImGuiUtil.HoverTooltip(retainerRetrieval == CraftRetainerRetrieval.HQOnly ? "HQ Only" : "Yes");
            ImGui.SameLine();
        }
        else
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
            ImGui.Image(_retainerIcon.ImGuiHandle,new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale,new System.Numerics.Vector2(0,0), new System.Numerics.Vector2(1,1), new Vector4(1f,1f,1f,0.2f));
            ImGuiUtil.HoverTooltip("No");
            ImGui.SameLine();
        }
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);

        if(_settingsIcon.Draw("cnf_" + rowIndex))
        {
            ImGui.OpenPopup("ConfigureItemSettings" + rowIndex);
        }

        
        if (ImGui.IsItemHovered())
        {
            using (var tooltip = ImRaii.Tooltip())
            {
                if (tooltip.Success)
                {

                    ImGui.TextUnformatted("Sourcing: " + (ingredientPreferenceDefault?.FormattedName ?? "Use Default"));
                    ImGui.TextUnformatted("Retainer: " + (retainerRetrievalDefault?.FormattedName() ?? "Use Default"));
                }
            }
        }
    }

    public void Draw(FilterConfiguration configuration, InventoryChange item, int rowIndex)
    {
        Draw(configuration, item.InventoryItem, rowIndex);
    }

    public string CsvExport(InventoryItem item)
    {
        return "";
    }

    public string CsvExport(SortingResult item)
    {
        return "";
    }

    public string CsvExport(ItemEx item)
    {
        return "";
    }

    public string CsvExport(CraftItem item)
    {
        return "";
    }

    public string CsvExport(InventoryChange item)
    {
        return "";
    }

    public dynamic? JsonExport(InventoryItem item)
    {
        return null;
    }

    public dynamic? JsonExport(SortingResult item)
    {
        return null;
    }

    public dynamic? JsonExport(ItemEx item)
    {
        return null;
    }

    public dynamic? JsonExport(CraftItem item)
    {
        return null;
    }

    public dynamic? JsonExport(InventoryChange item)
    {
        return null;
    }

    public void Setup(int columnIndex)
    {
        ImGui.TableSetupColumn(RenderName ?? Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
    }

    public IFilterEvent? DrawFooterFilter(FilterConfiguration configuration, FilterTable filterTable)
    {
        return null;
    }

    public event IColumn.ButtonPressedDelegate? ButtonPressed;
    public bool DrawFilter(string tableKey, int columnIndex)
    {
        return false;
    }

    public void Dispose()
    {
    }
}