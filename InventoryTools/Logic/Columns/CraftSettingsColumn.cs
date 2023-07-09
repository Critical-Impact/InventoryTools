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
    public float Width { get; set; } = 120;
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

        using (var popup = ImRaii.Popup("ConfigureItemSettings" + item.ItemId))
        {
            if (popup.Success)
            {
                ImGui.Text("Configure Sourcing:");
                ImGui.Separator();

                DrawRecipeSelector(configuration, item, rowIndex);
                DrawHqSelector(configuration, item, rowIndex);
                DrawRetainerRetrievalSelector(configuration, item, rowIndex);
                DrawSourceSelector(configuration, item, rowIndex);
                DrawZoneSelector(configuration, item, rowIndex);
            }
        }

        using (var popup = ImRaii.Popup("ConfigureRecipeSettings" + rowIndex))
        {
            if (popup.Success)
            {
                ImGui.Text("Configure Recipe:");
                ImGui.Separator();
                if (DrawRecipeSelector(configuration, item, rowIndex))
                {
                    ImGui.CloseCurrentPopup();
                }
            }
        }

        using (var popup = ImRaii.Popup("ConfigureHQSettings" + rowIndex))
        {
            if (popup.Success)
            {
                ImGui.Text("Configure HQ Required:");
                ImGui.Separator();
                if (DrawHqSelector(configuration, item, rowIndex))
                {
                    ImGui.CloseCurrentPopup();
                }
            }
        }

        using (var popup = ImRaii.Popup("ConfigureRetainerSettings" + rowIndex))
        {
            if (popup.Success)
            {
                ImGui.Text("Retrieve from Retainer:");
                ImGui.Separator();
                if (DrawRetainerRetrievalSelector(configuration, item, rowIndex))
                {
                    ImGui.CloseCurrentPopup();
                }
            }
        }
        var ingredientPreferenceDefault = configuration.CraftList.GetIngredientPreference(item.ItemId);
        var retainerRetrievalDefault = configuration.CraftList.GetCraftRetainerRetrieval(item.ItemId);
        var retainerRetrieval = retainerRetrievalDefault ?? (item.IsOutputItem ? configuration.CraftList.CraftRetainerRetrievalOutput : configuration.CraftList.CraftRetainerRetrieval);
        var zonePreference = configuration.CraftList.GetZonePreference(item.IngredientPreference.Type, item.ItemId);
        DrawRecipeIcon(configuration,rowIndex, item);
        DrawHqIcon(configuration, rowIndex, item);
        DrawRetainerIcon(configuration, rowIndex, item, retainerRetrievalDefault, retainerRetrieval);

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);

        if (_settingsIcon.Draw("cnf_" + rowIndex))
        {
            ImGui.OpenPopup("ConfigureItemSettings" + item.ItemId);
        }

        if (ImGui.IsItemHovered())
        {
            using (var tooltip = ImRaii.Tooltip())
            {
                if (tooltip.Success)
                {

                    ImGui.TextUnformatted("Sourcing: " + (ingredientPreferenceDefault?.FormattedName ?? "Use Default"));
                    ImGui.TextUnformatted("Retainer: " + (retainerRetrievalDefault?.FormattedName() ?? "Use Default"));
                    ImGui.TextUnformatted("Zone: " + (zonePreference != null ? Service.ExcelCache.GetMapSheet().GetRow(zonePreference.Value)?.FormattedName ?? "Use Default" : "Use Default"));
                }
            }
        }
    }

    private void DrawRetainerIcon(FilterConfiguration configuration, int rowIndex, CraftItem item, CraftRetainerRetrieval? defaultRetainerRetrieval, CraftRetainerRetrieval retainerRetrieval)
    {
        if (retainerRetrieval is CraftRetainerRetrieval.HQOnly or CraftRetainerRetrieval.Yes)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
            ImGui.Image(_retainerIcon.ImGuiHandle, new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale,
                new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1),
                retainerRetrieval == CraftRetainerRetrieval.HQOnly
                    ? new Vector4(0.9f, 0.75f, 0.14f, 1f)
                    : new Vector4(1f, 1f, 1f, 1f));
            ImGuiUtil.HoverTooltip((retainerRetrieval == CraftRetainerRetrieval.HQOnly ? "HQ Only" : "Yes") + (defaultRetainerRetrieval == null ? " (Default)" : ""));
            ImGui.SameLine();
        }
        else
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
            ImGui.Image(_retainerIcon.ImGuiHandle, new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale,
                new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1), new Vector4(1f, 1f, 1f, 0.2f));
            ImGuiUtil.HoverTooltip("No" + (defaultRetainerRetrieval == null ? " (Default)" : ""));
            ImGui.SameLine();
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            CraftRetainerRetrieval? newRetainerRetrieval;
            if (defaultRetainerRetrieval != null && retainerRetrieval == CraftRetainerRetrieval.No)
            {
                newRetainerRetrieval = CraftRetainerRetrieval.Yes;
            }
            else if (defaultRetainerRetrieval != null && retainerRetrieval == CraftRetainerRetrieval.Yes)
            {
                newRetainerRetrieval = CraftRetainerRetrieval.HQOnly;
            }
            else if (defaultRetainerRetrieval != null && retainerRetrieval == CraftRetainerRetrieval.HQOnly)
            {
                newRetainerRetrieval = null;
            }
            else
            {
                newRetainerRetrieval = CraftRetainerRetrieval.No;
            }
            configuration.CraftList.UpdateCraftRetainerRetrieval(item.ItemId, newRetainerRetrieval);
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("ConfigureRetainerSettings" + rowIndex);
        }
    }

    private void DrawHqIcon(FilterConfiguration configuration, int rowIndex, CraftItem item)
    {
        var hqRequired = configuration.CraftList.GetHQRequired(item.ItemId);

        var calculatedHqRequired = hqRequired ?? configuration.CraftList.HQRequired;

        if (calculatedHqRequired == true && item.Item.CanBeHq)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
            ImGui.Image(_hqIcon.ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale,
                new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1), new Vector4(0.9f, 0.75f, 0.14f, 1f));
            if (item.Item.CanBeHq && ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                if (hqRequired != null)
                {
                    configuration.CraftList.UpdateHQRequired(item.ItemId, false);
                    configuration.StartRefresh();
                }
                else
                {
                    configuration.CraftList.UpdateHQRequired(item.ItemId, true);
                    configuration.StartRefresh();
                }

            }
            else if (item.Item.CanBeHq && ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("ConfigureHQSettings" + rowIndex);
            }
            ImGuiUtil.HoverTooltip("HQ" + (hqRequired == null ? " (Default)" : ""));
            ImGui.SameLine();
        }
        else
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
            ImGui.Image(_hqIcon.ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale,
                new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1),
                new Vector4(0.9f, 0.75f, 0.14f, 0.2f));
            if (item.Item.CanBeHq && ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                if (hqRequired != null)
                {
                    configuration.CraftList.UpdateHQRequired(item.ItemId, null);
                    configuration.StartRefresh();
                }
                else
                {
                    configuration.CraftList.UpdateHQRequired(item.ItemId, true);
                    configuration.StartRefresh();
                }
            }
            else if (item.Item.CanBeHq && ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("ConfigureHQSettings" + rowIndex);
            }

            ImGuiUtil.HoverTooltip(item.Item.CanBeHq ? "No" + (hqRequired == null ? " (Default)" : "") : "Cannot be HQ");
            ImGui.SameLine();
        }
        
    }

    private static void DrawRecipeIcon(FilterConfiguration configuration, int rowIndex, CraftItem item)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
        var icon = item.SourceIcon;
        ImGui.Image(PluginService.IconStorage[icon].ImGuiHandle,
            new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
        var itemRecipe = item.Recipe;
        if (itemRecipe != null)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
            {
                var itemRecipes = Service.ExcelCache.ItemRecipes[item.ItemId];
                if (itemRecipes.Count != 1)
                {
                    var actualRecipes = itemRecipes.Select(c =>
                            Service.ExcelCache.GetRecipeExSheet().GetRow(c)!)
                        .OrderBy(c => c.CraftType.Value?.Name ?? "").ToList();
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        var currentRecipeIndex = actualRecipes.IndexOf(itemRecipe);
                        currentRecipeIndex++;
                        if (actualRecipes.Count <= currentRecipeIndex)
                        {
                            currentRecipeIndex = 0;
                        }

                        var newRecipe = actualRecipes[currentRecipeIndex];
                        if (item.IsOutputItem)
                        {
                            configuration.CraftList.SetCraftRecipe(item.ItemId,
                                newRecipe.RowId);
                            configuration.StartRefresh();
                        }
                        else
                        {
                            configuration.CraftList.UpdateCraftRecipePreference(item.ItemId,
                                newRecipe.RowId);
                            configuration.StartRefresh();
                        }
                    }
                    else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        ImGui.OpenPopup("ConfigureRecipeSettings" + rowIndex);
                    }
                }

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

                if (Service.ExcelCache.GetItemRecipes(item.ItemId).Count > 1)
                {
                    ImGui.NewLine();
                    ImGui.Text("Left Click: Next Recipe");
                    ImGui.Text("Right Click: Select Recipe");
                }
            }
        }
        else if (item.Item.CompanyCraftSequenceEx != null)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
            {
                using var tt = ImRaii.Tooltip();
                ImGui.Text($"Recipe (Company Craft): ");
                foreach (var ingredient in item.Item.CompanyCraftSequenceEx.MaterialsRequired(item.Phase))
                {
                    var itemId = ingredient.ItemId;
                    var actualItem = Service.ExcelCache.GetItemExSheet().GetRow(itemId);
                    var quantity = ingredient.Quantity;
                    if (actualItem != null)
                    {
                        ImGui.Text(actualItem.NameString + " : " + quantity);
                    }
                }
            }
        }
        else if (item.IngredientPreference.Type == IngredientPreferenceType.Item)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
            {
                using var tt = ImRaii.Tooltip();
                ImGui.Text($"Items: ");
                if (item.IngredientPreference.LinkedItemId != null && item.IngredientPreference.LinkedItemQuantity != null)
                {
                    var itemName =
                        Service.ExcelCache.GetItemExSheet().GetRow(item.IngredientPreference.LinkedItemId.Value)
                            ?.NameString ?? "Unknown Item" + " : " + item.IngredientPreference.LinkedItemQuantity.Value;
                    ImGui.Text(itemName);
                    if (item.IngredientPreference.LinkedItem2Id != null &&
                        item.IngredientPreference.LinkedItem2Quantity != null)
                    {
                        var itemName2 =
                            (Service.ExcelCache.GetItemExSheet().GetRow(item.IngredientPreference.LinkedItem2Id.Value)
                                ?.NameString ?? "Unknown Item") + " : " +
                            item.IngredientPreference.LinkedItem2Quantity.Value;
                        ImGui.Text(itemName2);
                    }

                    if (item.IngredientPreference.LinkedItem3Id != null &&
                        item.IngredientPreference.LinkedItem3Quantity != null)
                    {
                        var itemName3 =
                            (Service.ExcelCache.GetItemExSheet().GetRow(item.IngredientPreference.LinkedItem3Id.Value)
                                ?.NameString ?? "Unknown Item") + " : " +
                            item.IngredientPreference.LinkedItem3Quantity.Value;
                        ImGui.Text(itemName3);
                    }
                }
            }
        }
        else
        {
            ImGuiUtil.HoverTooltip(item.SourceName);
        }

        ImGui.SameLine();
    }

    private static bool DrawSourceSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
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
                        return true;
                    }

                    foreach (var ingredientPreference in item.Item.IngredientPreferences)
                    {
                        if (ImGui.Selectable(ingredientPreference.FormattedName))
                        {
                            configuration.CraftList.UpdateIngredientPreference(item.ItemId, ingredientPreference);
                            configuration.NeedsRefresh = true;
                            configuration.NotifyConfigurationChange();
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    
    private static bool DrawZoneSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
        if (item.IngredientPreference.Type is IngredientPreferenceType.Buy or IngredientPreferenceType.Item or IngredientPreferenceType.Mobs or IngredientPreferenceType.Mining or IngredientPreferenceType.Botany or IngredientPreferenceType.HouseVendor )
        {
            var mapIds = item.Item.GetSourceMaps(item.IngredientPreference.Type, item.IngredientPreference.LinkedItemId);
            if (mapIds.Count != 0)
            {
                var mapId = configuration.CraftList.GetZonePreference(item.IngredientPreference.Type,item.ItemId);
                var currentMap = mapId != null ? Service.ExcelCache.GetMapSheet().GetRow(mapId.Value) : null;
                var previewValue = currentMap?.FormattedName ?? "Use Default";
                ImGui.Text("Zone Preference:");
                using (var combo = ImRaii.Combo("##ZonePreference" + rowIndex, previewValue))
                {
                    if (combo.Success)
                    {
                        if (ImGui.Selectable("Use Default"))
                        {
                            configuration.CraftList.UpdateZonePreference(item.IngredientPreference.Type, item.ItemId, null);
                            configuration.NeedsRefresh = true;
                            configuration.NotifyConfigurationChange();
                            return true;
                        }

                        var maps = mapIds.Select(c => Service.ExcelCache.GetMapSheet().GetRow(c)).Where(c => c != null);
                        foreach (var map in maps)
                        {
                            if (map == null) continue;
                            if (ImGui.Selectable(map.FormattedName))
                            {
                                configuration.CraftList.UpdateZonePreference(item.IngredientPreference.Type, item.ItemId, map.RowId);
                                configuration.NeedsRefresh = true;
                                configuration.NotifyConfigurationChange();
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    private static bool DrawRetainerRetrievalSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
        //Retrieve from retainer combo
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
                    return true;
                }

                if (ImGui.Selectable("Yes"))
                {
                    configuration.CraftList.UpdateCraftRetainerRetrieval(item.ItemId, CraftRetainerRetrieval.Yes);
                    configuration.NeedsRefresh = true;
                    configuration.NotifyConfigurationChange();
                    return true;
                }

                if (ImGui.Selectable("No"))
                {
                    configuration.CraftList.UpdateCraftRetainerRetrieval(item.ItemId, CraftRetainerRetrieval.No);
                    configuration.NeedsRefresh = true;
                    configuration.NotifyConfigurationChange();
                    return true;
                }

                if (ImGui.Selectable("HQ Only"))
                {
                    configuration.CraftList.UpdateCraftRetainerRetrieval(item.ItemId, CraftRetainerRetrieval.HQOnly);
                    configuration.NeedsRefresh = true;
                    configuration.NotifyConfigurationChange();
                    return true;
                }
            }
        }

        return false;
    }

    private static bool DrawHqSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
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
                        return true;
                    }

                    if (ImGui.Selectable("Yes"))
                    {
                        configuration.CraftList.UpdateHQRequired(item.ItemId, true);
                        configuration.NeedsRefresh = true;
                        configuration.NotifyConfigurationChange();
                        return true;
                    }

                    if (ImGui.Selectable("No"))
                    {
                        configuration.CraftList.UpdateHQRequired(item.ItemId, false);
                        configuration.NeedsRefresh = true;
                        configuration.NotifyConfigurationChange();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static bool DrawRecipeSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
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
                                    return true;
                                }
                                else
                                {
                                    configuration.CraftList.UpdateCraftRecipePreference(item.ItemId,
                                        recipe.RowId);
                                    configuration.StartRefresh();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
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