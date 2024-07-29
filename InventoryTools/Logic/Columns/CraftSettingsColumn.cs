using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Internal;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Ui.Widgets;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;
using Vector2 = FFXIVClientStructs.FFXIV.Common.Math.Vector2;

namespace InventoryTools.Logic.Columns;

public class CraftSettingsColumn : IColumn
{
    private readonly ILogger<CraftSettingsColumn> _logger;
    private readonly ExcelCache _excelCache;
    public ImGuiService ImGuiService { get; }

    public CraftSettingsColumn(ILogger<CraftSettingsColumn> logger, ImGuiService imGuiService, ExcelCache excelCache)
    {
        _logger = logger;
        _excelCache = excelCache;
        ImGuiService = imGuiService;
    }
    public ColumnCategory ColumnCategory => ColumnCategory.Crafting;
    private HoverButton _settingsIcon = new();


    public string Name { get; set; } = "Settings";
    public float Width { get; set; } = 120;
    public string HelpText { get; set; } = "Modify each items craft settings in this column";
    public string FilterText { get; set; } = "";

    public string RenderName { get; } = "";

    public List<string>? FilterChoices { get; set; } = null;
    public bool HasFilter { get; set; } = false;
    public ColumnFilterType FilterType { get; set; } = ColumnFilterType.None;
    public bool IsDebug { get; set; } = false;
    public FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
    public virtual bool IsConfigurable => false;
    public bool IsDefault => true;
    
    public bool AvailableInType(FilterType type)
    {
        return type == Logic.FilterType.CraftFilter;
    }

    public bool? CraftOnly { get; } = true;
    public IEnumerable<InventoryItem> Filter(ColumnConfiguration columnConfiguration, IEnumerable<InventoryItem> items)
    {
        return items;
    }

    public IEnumerable<SortingResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SortingResult> items)
    {
        return items;
    }

    public IEnumerable<ItemEx> Filter(ColumnConfiguration columnConfiguration, IEnumerable<ItemEx> items)
    {
        return items;
    }

    public IEnumerable<CraftItem> Filter(ColumnConfiguration columnConfiguration, IEnumerable<CraftItem> items)
    {
        return items;
    }

    public IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration,
        IEnumerable<InventoryChange> items)
    {
        return items;
    }

    public IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
        IEnumerable<InventoryItem> items)
    {
        return items;
    }

    public IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
        IEnumerable<SortingResult> items)
    {
        return items;
    }

    public IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
        IEnumerable<ItemEx> items)
    {
        return items;
    }

    public IEnumerable<CraftItem> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
        IEnumerable<CraftItem> items)
    {
        return items;
    }

    public IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
        IEnumerable<InventoryChange> items)
    {
        return items;
    }

    public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        InventoryItem item,
        int rowIndex, int columnIndex)
    {
        return null;
    }

    public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SortingResult item,
        int rowIndex, int columnIndex)
    {
        return null;
    }

    public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        ItemEx item,
        int rowIndex, int columnIndex)
    {
        return null;
    }

    public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        CraftItem item,
        int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
        
        using (var popup = ImRaii.Popup("ConfigureItemSettings" + columnIndex + item.ItemId + (item.IsOutputItem ? "o" : "")))
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
                DrawMarketWorldSelector(configuration, item, rowIndex);
                DrawMarketPriceSelector(configuration, item, rowIndex);
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

        using (var popup = ImRaii.Popup("ConfigureMarketWorldPreference" + rowIndex))
        {
            if (popup.Success)
            {
                ImGui.Text("Prefer Market World:");
                ImGui.Separator();
            }
        }

        using (var popup = ImRaii.Popup("ConfigureItemPriceOverride" + rowIndex))
        {
            if (popup.Success)
            {
                ImGui.Text("Market Price:");
                ImGui.Separator();
            }
        }
        var ingredientPreferenceDefault = configuration.CraftList.GetIngredientPreference(item.ItemId);
        var retainerRetrievalDefault = configuration.CraftList.GetCraftRetainerRetrieval(item.ItemId);
        var retainerRetrieval = retainerRetrievalDefault ?? (item.IsOutputItem ? configuration.CraftList.CraftRetainerRetrievalOutput : configuration.CraftList.CraftRetainerRetrieval);
        var zonePreference = configuration.CraftList.GetZonePreference(item.IngredientPreference.Type, item.ItemId);
        var worldPreference = configuration.CraftList.GetMarketItemWorldPreference(item.ItemId);
        var priceOverride = configuration.CraftList.GetMarketItemPriceOverride(item.ItemId);
        var originalPos = ImGui.GetCursorPosY();
        DrawRecipeIcon(configuration,rowIndex, item);
        ImGui.SetCursorPosY(originalPos);
        DrawHqIcon(configuration, rowIndex, item);
        ImGui.SetCursorPosY(originalPos);
        DrawRetainerIcon(configuration, rowIndex, item, retainerRetrievalDefault, retainerRetrieval);
        ImGui.SetCursorPosY(originalPos);

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);

        if (_settingsIcon.Draw(ImGuiService.GetIconTexture(66319).ImGuiHandle, "cnf_" + rowIndex))
        {
            ImGui.OpenPopup("ConfigureItemSettings" + columnIndex + item.ItemId + (item.IsOutputItem ? "o" : ""));
        }

        if (ImGui.IsItemHovered())
        {
            using (var tooltip = ImRaii.Tooltip())
            {
                if (tooltip.Success)
                {

                    ImGui.TextUnformatted("Sourcing: " + (ingredientPreferenceDefault?.FormattedName ?? "Use Default"));
                    ImGui.TextUnformatted("Retainer: " + (retainerRetrievalDefault?.FormattedName() ?? "Use Default"));
                    ImGui.TextUnformatted("Zone: " + (zonePreference != null ? _excelCache.GetMapSheet().GetRow(zonePreference.Value)?.FormattedName ?? "Use Default" : "Use Default"));
                    if (item.Item.CanBePlacedOnMarket)
                    {
                        ImGui.TextUnformatted("Market World Preference: " + (worldPreference != null ? _excelCache.GetWorldSheet().GetRow(worldPreference.Value)?.FormattedName ?? "Use Default" : "Use Default"));
                        ImGui.TextUnformatted("Market Price Override: " + (priceOverride != null ? priceOverride.Value.ToString("N0") : "Use Default"));
                    }
                }
            }
        }
        return null;
    }

    private void DrawRetainerIcon(FilterConfiguration configuration, int rowIndex, CraftItem item, CraftRetainerRetrieval? defaultRetainerRetrieval, CraftRetainerRetrieval retainerRetrieval)
    {
        if (retainerRetrieval is CraftRetainerRetrieval.HQOnly or CraftRetainerRetrieval.Yes)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
            ImGui.Image(ImGuiService.GetIconTexture(Icons.RetainerIcon).ImGuiHandle, new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale,
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
            ImGui.Image(ImGuiService.GetIconTexture(Icons.RetainerIcon).ImGuiHandle, new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale,
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
            configuration.NeedsRefresh = true;
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
            ImGui.Image(ImGuiService.GetImageTexture("hq").ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale,
                new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1), new Vector4(0.9f, 0.75f, 0.14f, 1f));
            if (item.Item.CanBeHq && ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                if (hqRequired != null)
                {
                    configuration.CraftList.UpdateHQRequired(item.ItemId, false);
                    configuration.NeedsRefresh = true;
                }
                else
                {
                    configuration.CraftList.UpdateHQRequired(item.ItemId, true);
                    configuration.NeedsRefresh = true;
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
            ImGui.Image(ImGuiService.GetImageTexture("hq").ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale,
                new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1),
                new Vector4(0.9f, 0.75f, 0.14f, 0.2f));
            if (item.Item.CanBeHq && ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                if (hqRequired != null)
                {
                    configuration.CraftList.UpdateHQRequired(item.ItemId, null);
                    configuration.NeedsRefresh = true;
                }
                else
                {
                    configuration.CraftList.UpdateHQRequired(item.ItemId, true);
                    configuration.NeedsRefresh = true;
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

    private void DrawRecipeIcon(FilterConfiguration configuration, int rowIndex, CraftItem item)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
        var icon = item.SourceIcon;
        ImGui.Image(ImGuiService.GetIconTexture(icon).ImGuiHandle,
            new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
        var itemRecipe = item.Recipe;
        if (itemRecipe != null)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
            {
                var itemRecipes = _excelCache.ItemRecipes[item.ItemId];
                if (itemRecipes.Count != 1)
                {
                    var actualRecipes = itemRecipes.Select(c =>
                            _excelCache.GetRecipeExSheet().GetRow(c)!)
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
                            configuration.NeedsRefresh = true;
                        }
                        else
                        {
                            configuration.CraftList.UpdateCraftRecipePreference(item.ItemId,
                                newRecipe.RowId);
                            configuration.NeedsRefresh = true;
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

                if (_excelCache.GetItemRecipes(item.ItemId).Count > 1)
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
                    var actualItem = _excelCache.GetItemExSheet().GetRow(itemId);
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
                        _excelCache.GetItemExSheet().GetRow(item.IngredientPreference.LinkedItemId.Value)
                            ?.NameString ?? "Unknown Item" + " : " + item.IngredientPreference.LinkedItemQuantity.Value;
                    ImGui.Text(itemName);
                    if (item.IngredientPreference.LinkedItem2Id != null &&
                        item.IngredientPreference.LinkedItem2Quantity != null)
                    {
                        var itemName2 =
                            (_excelCache.GetItemExSheet().GetRow(item.IngredientPreference.LinkedItem2Id.Value)
                                ?.NameString ?? "Unknown Item") + " : " +
                            item.IngredientPreference.LinkedItem2Quantity.Value;
                        ImGui.Text(itemName2);
                    }

                    if (item.IngredientPreference.LinkedItem3Id != null &&
                        item.IngredientPreference.LinkedItem3Quantity != null)
                    {
                        var itemName3 =
                            (_excelCache.GetItemExSheet().GetRow(item.IngredientPreference.LinkedItem3Id.Value)
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

    private bool DrawSourceSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
        if (item.Item.IngredientPreferences.Count != 0)
        {
            var currentIngredientPreference =
                configuration.CraftList.GetIngredientPreference(item.ItemId);
            var previewValue = currentIngredientPreference?.FormattedName ?? "Use Default";
            ImGui.Text("Source Preference:");
            ImGui.SameLine();
            ImGuiService.HelpMarker("How should the item be sourced? As there are multiple ways to source an item, you can either rely on your list's ingredient sourcing (tab inside the craft list's settings) or you can override the source here.");
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

    private bool DrawMarketPriceSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
        if (item.IngredientPreference.Type is IngredientPreferenceType.Marketboard)
        {
            var priceOverride = configuration.CraftList.GetMarketItemPriceOverride(item.ItemId);
            var priceString = priceOverride?.ToString() ?? "";
            ImGui.Text("Market Price Override:");
            ImGui.SameLine();
            ImGuiService.HelpMarker("Override the price for this item. This is only used when no pricing is available. Use this to give you a rough estimate of the gil cost of your item.");
            if (ImGui.InputText("##MarketPricePreference" + rowIndex, ref priceString, 50))
            {
                if (priceString == "")
                {
                    configuration.CraftList.UpdateMarketItemPriceOverride(item.ItemId, null);
                    configuration.NeedsRefresh = true;
                }
                else if(UInt32.TryParse(priceString, out uint newValue))
                {
                    configuration.CraftList.UpdateMarketItemPriceOverride(item.ItemId, newValue);
                    configuration.NeedsRefresh = true;
                }
            }
        }

        return false;
    }
    
    private bool DrawMarketWorldSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
        if (item.IngredientPreference.Type is IngredientPreferenceType.Marketboard)
        {
            var worldId = configuration.CraftList.GetMarketItemWorldPreference(item.ItemId);
            var currentWorld = worldId != null ? _excelCache.GetWorldSheet().GetRow(worldId.Value) : null;
            var previewValue = currentWorld?.FormattedName ?? "Use Default";
            ImGui.Text("Market World Preference:");
            ImGui.SameLine();
            ImGuiService.HelpMarker("Override the market world preferences for this item. If you select a world here, the craft pricer will attempt to take prices from this world first then follow the normal rules for craft pricing.");
            using (var combo = ImRaii.Combo("##MarketWorldPreference" + rowIndex, previewValue))
            {
                if (combo.Success)
                {
                    if (ImGui.Selectable("Use Default"))
                    {
                        configuration.CraftList.UpdateItemWorldPreference(item.ItemId, null);
                        configuration.NeedsRefresh = true;
                        configuration.NotifyConfigurationChange();
                        return true;
                    }
                    var worlds = _excelCache.GetWorldSheet().Where(c => c.IsPublic).OrderBy(c => c.FormattedName).ToList();
                    foreach (var world in worlds)
                    {
                        if (ImGui.Selectable(world.FormattedName))
                        {
                            configuration.CraftList.UpdateItemWorldPreference(item.ItemId, world.RowId);
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
    
    private  bool DrawZoneSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
        if (item.IngredientPreference.Type is IngredientPreferenceType.Buy or IngredientPreferenceType.Item or IngredientPreferenceType.Mobs or IngredientPreferenceType.Mining or IngredientPreferenceType.Botany or IngredientPreferenceType.HouseVendor )
        {
            var mapIds = item.Item.GetSourceMaps(item.IngredientPreference.Type, item.IngredientPreference.LinkedItemId);
            if (mapIds.Count != 0)
            {
                var mapId = configuration.CraftList.GetZonePreference(item.IngredientPreference.Type,item.ItemId);
                var currentMap = mapId != null ? _excelCache.GetMapSheet().GetRow(mapId.Value) : null;
                var previewValue = currentMap?.FormattedName ?? "Use Default";
                ImGui.Text("Zone Preference:");
                ImGui.SameLine();
                ImGuiService.HelpMarker("Where should the item be sourced from? As there are sometimes multiple locations to source an item from, you can either rely on your list's zone preferences (tab inside the craft list's settings) or you can override the zone here.");
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

                        var maps = mapIds.Select(c => _excelCache.GetMapSheet().GetRow(c)).Where(c => c != null);
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

    private bool DrawRetainerRetrievalSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
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
        ImGui.SameLine();
        ImGuiService.HelpMarker("Should we source the item from your retainers? If there is a quantity available of the correct quality it will show up in the Items in Retainers/Bags section.");
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

    private bool DrawHqSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
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
            ImGui.SameLine();
            ImGuiService.HelpMarker("Should the item be HQ or NQ? For output items, the quantity needed will only reduce if you craft an item of the correct quality. For other materials this will dictate what is listed to retrieve and what counts towards the amount you need.");
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

    private bool DrawRecipeSelector(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
        if (_excelCache.ItemRecipes.ContainsKey(item.ItemId))
        {
            var itemRecipes = _excelCache.ItemRecipes[item.ItemId];
            if (itemRecipes.Count != 1)
            {
                var actualRecipes = itemRecipes.Select(c =>
                        _excelCache.GetRecipeExSheet().GetRow(c)!)
                    .OrderBy(c => c.CraftType.Value?.Name ?? "").ToList();
                var recipeName = item.Recipe?.CraftType.Value?.Name ?? "";
                ImGui.Text("Recipe:");
                ImGui.SameLine();
                ImGuiService.HelpMarker("Select which recipe you wish to use for this item. Some items can be crafted by multiple classes.");
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
                                    configuration.NeedsRefresh = true;
                                    return true;
                                }
                                else
                                {
                                    configuration.CraftList.UpdateCraftRecipePreference(item.ItemId,
                                        recipe.RowId);
                                    configuration.NeedsRefresh = true;
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

    public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        InventoryChange item,
        int rowIndex, int columnIndex)
    {
        return Draw(configuration, columnConfiguration, item.InventoryItem, rowIndex, columnIndex);
    }

    public void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
    {
        
    }

    public string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return "";
    }

    public string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return "";
    }

    public string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return "";
    }

    public string CsvExport(ColumnConfiguration columnConfiguration, CraftItem item)
    {
        return "";
    }

    public string CsvExport(ColumnConfiguration columnConfiguration, InventoryChange item)
    {
        return "";
    }

    public dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return null;
    }

    public dynamic? JsonExport(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return null;
    }

    public dynamic? JsonExport(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return null;
    }

    public dynamic? JsonExport(ColumnConfiguration columnConfiguration, CraftItem item)
    {
        return null;
    }

    public dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryChange item)
    {
        return null;
    }

    public void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration, int columnIndex)
    {
        ImGui.TableSetupColumn(columnConfiguration.Name ?? (RenderName ?? Name), ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
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
    
    public virtual void InvalidateSearchCache()
    {
        
    }

    public void Dispose()
    {
    }
    public virtual FilterType DefaultIn => Logic.FilterType.CraftFilter;
    public uint MaxFilterLength { get; set; } = 200;
}