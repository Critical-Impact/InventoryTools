using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Time;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Math;
using Humanizer;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings;
using Lumina.Data;

namespace InventoryTools.Services;

public class ItemInfoRenderService
{
    private readonly SourceIconGroupingSetting _sourceIconGroupingSetting;
    private readonly UseIconGroupingSetting _useIconGroupingSetting;
    private readonly ImGuiService _imGuiService;
    private readonly IPluginLog _pluginLog;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly Dictionary<Type,IItemInfoRenderer> _sourceRenderers;
    private readonly Dictionary<Type,IItemInfoRenderer> _useRenderers;
    private readonly Dictionary<ItemInfoType,IItemInfoRenderer> _sourceRenderersByItemInfoType;
    private readonly Dictionary<ItemInfoType,IItemInfoRenderer> _useRenderersByItemInfoType;

    public ItemInfoRenderService(IEnumerable<IItemInfoRenderer> itemRenderers, SourceIconGroupingSetting sourceIconGroupingSetting, UseIconGroupingSetting useIconGroupingSetting, ImGuiService imGuiService, IPluginLog pluginLog, InventoryToolsConfiguration configuration)
    {
        _sourceIconGroupingSetting = sourceIconGroupingSetting;
        _useIconGroupingSetting = useIconGroupingSetting;
        _imGuiService = imGuiService;
        _pluginLog = pluginLog;
        _configuration = configuration;
        var itemInfoRenderers = itemRenderers.ToList();
        _sourceRenderers = itemInfoRenderers.Where(c => c.RendererType == RendererType.Source).ToDictionary(c => c.ItemSourceType, c => c);
        _useRenderers = itemInfoRenderers.Where(c => c.RendererType == RendererType.Use).ToDictionary(c => c.ItemSourceType, c => c);
        _sourceRenderersByItemInfoType = itemInfoRenderers.Where(c => c.RendererType == RendererType.Source).ToDictionary(c => c.Type, c => c);
        _useRenderersByItemInfoType = itemInfoRenderers.Where(c => c.RendererType == RendererType.Use).ToDictionary(c => c.Type, c => c);

        #if DEBUG
        foreach (var itemType in Enum.GetValues<ItemInfoType>())
        {
            if (!_sourceRenderersByItemInfoType.ContainsKey(itemType) && !_useRenderersByItemInfoType.ContainsKey(itemType))
            {
                _pluginLog.Verbose($"Missing type {itemType}");
            }
        }
        #endif
    }

    public Dictionary<Type,IItemInfoRenderer> SourceRenderers => _sourceRenderers;
    public Dictionary<Type,IItemInfoRenderer> UseRenderers => _useRenderers;

    public string GetCategoryName(ItemInfoRenderCategory renderCategory)
    {
        switch (renderCategory)
        {
            case ItemInfoRenderCategory.Gathering:
                return "Gathering";
            case ItemInfoRenderCategory.Mining:
                return "Mining";
            case ItemInfoRenderCategory.Botany:
                return "Botany";
            case ItemInfoRenderCategory.EphemeralGathering:
                return "Gathering (Ephemeral)";
            case ItemInfoRenderCategory.TimedGathering:
                return "Gathering (Timed)";
            case ItemInfoRenderCategory.HiddenGathering:
                return "Gathering (Hidden)";
            case ItemInfoRenderCategory.Fishing:
                return "Fishing";
            case ItemInfoRenderCategory.Venture:
                return "Venture";
            case ItemInfoRenderCategory.ExplorationVenture:
                return "Venture (Exploration)";
            case ItemInfoRenderCategory.Crafting:
                return "Crafting";
            case ItemInfoRenderCategory.Leve:
                return "Leves";
            case ItemInfoRenderCategory.Duty:
                return "Duties";
            case ItemInfoRenderCategory.Shop:
                return "Shops";
            case ItemInfoRenderCategory.House:
                return "Housing";
        }

        return renderCategory.ToString().Titleize();
    }

    public bool HasSourceRenderer(ItemInfoType itemInfoType)
    {
        return _sourceRenderersByItemInfoType.ContainsKey(itemInfoType);
    }
    public bool HasUseRenderer(ItemInfoType itemInfoType)
    {
        return _useRenderersByItemInfoType.ContainsKey(itemInfoType);
    }

    public List<IItemInfoRenderer> GetSourcesByCategory(ItemInfoRenderCategory renderCategory)
    {
        return _sourceRenderers.Where(c => c.Value.Categories?.Contains(renderCategory) ?? false).Select(c => c.Value).ToList();
    }

    public List<IItemInfoRenderer> GetUsesByCategory(ItemInfoRenderCategory renderCategory)
    {
        return _useRenderers.Where(c => c.Value.Categories?.Contains(renderCategory) ?? false).Select(c => c.Value).ToList();
    }

    public (string Singular, string? Plural) GetSourceTypeName(ItemInfoType type)
    {
        if (_sourceRenderersByItemInfoType.TryGetValue(type, out var renderer))
        {
            return (renderer.SingularName, renderer.PluralName);
        }

        return (type.ToString(), null);
    }

    public string GetSourceHelpText(ItemInfoType type)
    {
        if (_sourceRenderersByItemInfoType.TryGetValue(type, out var renderer))
        {
            return renderer.HelpText;
        }

        return "Can this item be sourced via " + type.ToString();
    }

    public (string Singular, string? Plural) GetSourceTypeName(Type type)
    {
        if (_sourceRenderers.TryGetValue(type, out var renderer))
        {
            return (renderer.SingularName, renderer.PluralName);
        }

        return (type.ToString(), null);
    }

    public (string Singular, string? Plural) GetUseTypeName(ItemInfoType type)
    {
        if (_useRenderersByItemInfoType.TryGetValue(type, out var renderer))
        {
            return (renderer.SingularName, renderer.PluralName);
        }

        return (type.ToString(), null);
    }

    public string GetUseHelpText(ItemInfoType type)
    {
        if (_useRenderersByItemInfoType.TryGetValue(type, out var renderer))
        {
            return renderer.HelpText;
        }

        return "Can this item be used for " + type.ToString();
    }


    public (string Singular, string? Plural) GetUseTypeName(Type type)
    {
        if (_useRenderers.TryGetValue(type, out var renderer))
        {
            return (renderer.SingularName, renderer.PluralName);
        }

        return (type.ToString(), null);
    }

    public List<List<ItemSource>> GetGroupedSources(List<ItemSource> allItemSources)
    {
        return GroupItemSources(_sourceRenderers, allItemSources);
    }

    public List<MessageBase> DrawSource(string id, List<ItemSource> itemSources, Vector2 iconSize)
    {
        return DrawItemSource(RendererType.Source, id, itemSources, iconSize);
    }

    public string GetSourceName(ItemSource itemSource)
    {
        var sourceRenderer = this._sourceRenderers.ContainsKey(itemSource.GetType()) ? this._sourceRenderers[itemSource.GetType()] : null;
        return sourceRenderer?.GetName(itemSource) ?? itemSource.Item.NameString;
    }

    public int GetSourceIcon(ItemSource itemSource)
    {
        var sourceRenderer = this._sourceRenderers.ContainsKey(itemSource.GetType()) ? this._sourceRenderers[itemSource.GetType()] : null;
        return sourceRenderer?.GetIcon(itemSource) ?? itemSource.Item.Icon;
    }

    public List<List<ItemSource>> GetGroupedUses(List<ItemSource> allItemSources)
    {
        return GroupItemSources(_useRenderers, allItemSources);
    }

    public List<MessageBase> DrawUse(string id, List<ItemSource> itemSources, Vector2 iconSize)
    {
        return DrawItemSource(RendererType.Use, id, itemSources, iconSize);
    }

    public string GetUseName(ItemSource itemSource)
    {
        var useRenderer = this._useRenderers.ContainsKey(itemSource.GetType()) ? this._useRenderers[itemSource.GetType()] : null;
        return useRenderer?.GetName(itemSource) ?? itemSource.Item.NameString;
    }

    public int GetUseIcon(ItemSource itemSource)
    {
        var useRenderer = this._useRenderers.ContainsKey(itemSource.GetType()) ? this._useRenderers[itemSource.GetType()] : null;
        return useRenderer?.GetIcon(itemSource) ?? itemSource.Item.Icon;
    }

    private List<List<ItemSource>> GroupItemSources(Dictionary<Type,IItemInfoRenderer> renderers, List<ItemSource> allItemSources)
    {
        List<List<ItemSource>> groupedItems = new List<List<ItemSource>>();
        var groupedByType = allItemSources.GroupBy(c => c.GetType());
        foreach (var group in groupedByType)
        {
            if (renderers.TryGetValue(group.Key, out var renderer))
            {
                var sourceGroupings = _sourceIconGroupingSetting.CurrentValue(_configuration);
                var useGroupings = _useIconGroupingSetting.CurrentValue(_configuration);
                if (renderer.RendererType == RendererType.Source && (sourceGroupings?.ContainsKey(group.Key) ?? false))
                {
                    if (sourceGroupings[group.Key])
                    {
                        groupedItems.Add(group.ToList());
                    }
                    else
                    {
                        foreach (var ungroupedItem in group)
                        {
                            groupedItems.Add([ungroupedItem]);
                        }
                    }
                }
                else if (renderer.RendererType == RendererType.Use && (useGroupings?.ContainsKey(group.Key) ?? false))
                {
                    if (useGroupings[group.Key])
                    {
                        groupedItems.Add(group.ToList());
                    }
                    else
                    {
                        foreach (var ungroupedItem in group)
                        {
                            groupedItems.Add([ungroupedItem]);
                        }
                    }
                }
                else if (renderer.CustomGroup != null)
                {
                    var customGrouping = renderer.CustomGroup.Invoke(group.ToList());
                    groupedItems.AddRange(customGrouping);
                }
                else
                {
                    if (renderer.ShouldGroup)
                    {
                        groupedItems.Add(group.ToList());
                    }
                    else
                    {
                        foreach (var ungroupedItem in group)
                        {
                            groupedItems.Add([ungroupedItem]);
                        }
                    }
                }
            }
            else
            {
                //If no renderer assume we'll leave them ungrouped
                foreach (var ungroupedItem in group)
                {
                    groupedItems.Add([ungroupedItem]);
                }
            }
        }

        return groupedItems;
    }

    private List<MessageBase> DrawItemSource(RendererType rendererType, string id, List<ItemSource> itemSources, Vector2 iconSize)
    {
        using var pushId = ImRaii.PushId(id);
        var messages = new List<MessageBase>();
        var firstItem = itemSources.First();
        var renderers = rendererType == RendererType.Source ? _sourceRenderers : _useRenderers;
        var icon = rendererType == RendererType.Source ? GetSourceIcon(firstItem) : GetUseIcon(firstItem);
        var sourceRenderer = renderers.ContainsKey(firstItem.GetType()) ? renderers[firstItem.GetType()] : null;

        var sourceIcon = _imGuiService.GetIconTexture(icon);

        var isButton = sourceRenderer?.OnClick != null;
        var hasTooltip = sourceRenderer?.DrawTooltip != null;
        var hasGroupedTooltip = sourceRenderer?.DrawTooltipGrouped != null;


        if (isButton && ImGui.ImageButton(sourceIcon.ImGuiHandle,
                new Vector2(iconSize.X, iconSize.Y) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                new Vector2(1, 1), 0))
        {
            if (itemSources.Count > 1)
            {
                ImGui.OpenPopup("PickItemSource");
            }
            else
            {
                var newMessages = sourceRenderer?.OnClick?.Invoke(firstItem);
                if (newMessages != null)
                {
                    messages.AddRange(newMessages);
                }
            }
        }
        else if(!isButton)
        {
            ImGui.Image(sourceIcon.ImGuiHandle,
                new Vector2(iconSize.X, iconSize.Y) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                new Vector2(1, 1));
        }

        if (isButton && itemSources.Count > 1)
        {
            using (var popup = ImRaii.Popup("PickItemSource"))
            {
                if (popup.Success)
                {
                    var typeName = (rendererType == RendererType.Source ? this.GetSourceTypeName(firstItem.GetType()) : this.GetUseTypeName(firstItem.GetType()));
                    ImGui.Text("Pick a " + (typeName.Plural ?? typeName.Singular));
                    ImGui.Separator();
                    for (var index = 0; index < itemSources.Count; index++)
                    {
                        var source = itemSources[index];
                        using (ImRaii.PushId(index))
                        {
                            if (ImGui.Selectable(sourceRenderer?.GetName(source) ?? "No Name"))
                            {
                                var newMessages = sourceRenderer?.OnClick?.Invoke(source);
                                if (newMessages != null)
                                {
                                    messages.AddRange(newMessages);
                                }
                            }
                        }
                    }
                }
            }
        }

        if ((hasTooltip || hasGroupedTooltip) && ImGui.IsItemHovered())
        {
            using var tt = ImRaii.Tooltip();
            if (tt.Success)
            {
                if (itemSources.Count > 1)
                {
                    var typeName = (rendererType == RendererType.Source ? this.GetSourceTypeName(firstItem.GetType()) : this.GetUseTypeName(firstItem.GetType()));
                    ImGui.Text(typeName.Plural ?? typeName.Singular);
                    ImGui.Separator();
                    if (hasGroupedTooltip)
                    {
                        sourceRenderer?.DrawTooltipGrouped?.Invoke(itemSources);
                    }
                    else
                    {

                        byte itemsPerRow = sourceRenderer?.MaxColumns ?? 3;
                        float childWidth = sourceRenderer?.TooltipChildWidth ?? 250;
                        float childHeight = sourceRenderer?.TooltipChildHeight ?? 150;

                        if (itemsPerRow == 1)
                        {
                            for (var index = 0; index < itemSources.Count; index++)
                            {
                                var source = itemSources[index];

                                sourceRenderer?.DrawTooltip.Invoke(source);
                                if (index != itemSources.Count - 1)
                                {
                                    ImGui.Separator();
                                }
                            }
                        }
                        else
                        {

                            for (var index = 0; index < itemSources.Count; index++)
                            {
                                var source = itemSources[index];

                                using (var child = ImRaii.Child($"Tooltip_{index}", new(childWidth, childHeight), false,
                                           ImGuiWindowFlags.NoScrollbar))
                                {
                                    if (child.Success)
                                    {
                                        sourceRenderer?.DrawTooltip.Invoke(source);
                                    }
                                }

                                if ((index + 1) % itemsPerRow != 0)
                                {
                                    ImGui.SameLine();
                                }
                            }
                        }

                    }
                }
                else
                {
                    ImGui.Text((rendererType == RendererType.Source
                        ? this.GetSourceTypeName(firstItem.GetType())
                        : this.GetUseTypeName(firstItem.GetType())).Singular);
                    ImGui.Separator();
                    sourceRenderer?.DrawTooltip.Invoke(firstItem);
                }
            }
        }
        else if(ImGui.IsItemHovered())
        {
            using var tt = ImRaii.Tooltip();
            if (tt.Success)
            {
                ImGui.Text("No tooltip configured for " + (rendererType == RendererType.Source
                    ? this.GetSourceTypeName(firstItem.GetType())
                    : this.GetUseTypeName(firstItem.GetType())).Singular + ", please report this!");
            }
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                ImGuiHoveredFlags.AnyWindow) &&
            ImGui.IsMouseReleased(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("RightClick");
        }

        using (var popup = ImRaii.Popup("RightClick"))
        {
            if (popup.Success)
            {
                if (sourceRenderer?.OnRightClick != null)
                {
                    sourceRenderer?.OnRightClick.Invoke(firstItem);
                }
                else
                {
                    _imGuiService.ImGuiMenuService.DrawRightClickPopup(rendererType == RendererType.Source ? firstItem.Item : (firstItem.CostItem ?? firstItem.Item), messages);
                }
            }
        }

        return messages;
    }

    public List<MessageBase> DrawItemSourceIconsContainer(string id, float rowSize, Vector2 iconSize, List<ItemSource> itemSources)
    {
        var messages = new List<MessageBase>();
        using var pushId = ImRaii.PushId(id);
        var count = 0;
        var groupedSources = GetGroupedSources(itemSources);
        _imGuiService.WrapTableColumnElements(id, groupedSources,
            rowSize,
            itemList =>
            {
                messages.AddRange(this.DrawSource(count.ToString(), itemList, iconSize));
                count++;
                return true;
            });

        return messages;
    }

    public List<MessageBase> DrawItemSourceIcons(string id, Vector2 iconSize, List<ItemSource> itemSources)
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

        var messages = new List<MessageBase>();
        using var pushId = ImRaii.PushId(id);
        var groupedSources = GetGroupedSources(itemSources);

        for (var index = 0; index < groupedSources.Count; index++)
        {
            var groupedSource = groupedSources[index];
            messages.AddRange(DrawSource(index.ToString(), groupedSource, iconSize));

            float lastButtonX2 = ImGui.GetItemRectMax().X;
            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;

            if (index + 1 < groupedSources.Count && nextButtonX2 < windowVisibleX2)
            {
                ImGui.SameLine();
            }
        }

        return messages;
    }

    public List<MessageBase> DrawItemUseIcons(string id, Vector2 iconSize, List<ItemSource> itemSources)
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

        var messages = new List<MessageBase>();
        using var pushId = ImRaii.PushId(id);
        var groupedSources = GetGroupedUses(itemSources);

        for (var index = 0; index < groupedSources.Count; index++)
        {
            var groupedSource = groupedSources[index];
            messages.AddRange(DrawUse(index.ToString(), groupedSource, iconSize));

            float lastButtonX2 = ImGui.GetItemRectMax().X;
            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;

            if (index + 1 < groupedSources.Count && nextButtonX2 < windowVisibleX2)
            {
                ImGui.SameLine();
            }
        }

        return messages;
    }

    public List<MessageBase> DrawItemUseIconsContainer(string id, float rowSize, Vector2 iconSize, List<ItemSource> itemSources)
    {
        var messages = new List<MessageBase>();
        using var pushId = ImRaii.PushId(id);
        var count = 0;
        var groupedSources = GetGroupedUses(itemSources);
        _imGuiService.WrapTableColumnElements(id, groupedSources,
            rowSize,
            itemList =>
            {
                messages.AddRange(this.DrawUse(count.ToString(), itemList, iconSize));
                count++;
                return true;
            });

        return messages;
    }
}