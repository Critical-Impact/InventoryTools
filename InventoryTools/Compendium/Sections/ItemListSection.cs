using System;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Model;
using AllaganLib.Interface.FormFields;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;

namespace InventoryTools.Compendium.Sections;

public class ItemListSection : ViewSection
{
    private readonly ItemListSectionOptions _options;
    private readonly MediatorService _mediatorService;
    private readonly ImGuiService _imGuiService;
    private readonly ITextureProvider _textureProvider;
    private readonly ImGuiTooltipService _tooltipService;
    private readonly ImGuiMenuService _menuService;
    private readonly ItemInfoRenderService _itemInfoRenderService;
    private readonly IFont _font;

    public delegate ItemListSection Factory(ItemListSectionOptions options);

    public ItemListSection(ItemListSectionOptions options, MediatorService mediatorService, IFont font, ImGuiService imGuiService, ITextureProvider textureProvider, ImGuiTooltipService tooltipService, ImGuiMenuService menuService, ItemInfoRenderService itemInfoRenderService) : base(options, imGuiService)
    {
        _options = options;
        _mediatorService = mediatorService;
        _imGuiService = imGuiService;
        _textureProvider = textureProvider;
        _tooltipService = tooltipService;
        _menuService = menuService;
        _itemInfoRenderService = itemInfoRenderService;
        _font = font;
    }

    private string sectionModeId => _options.SectionId + "_mode";

    private ItemListSectionMode GetSectionMode(SectionState sectionState)
    {
        IConfigurable<Enum?> configurable = sectionState;
        return (ItemListSectionMode)(configurable.Get(sectionModeId) ?? ItemListSectionMode.Grid);
    }

    public override string SectionName => _options.SectionName;

    public override Action<SectionState>? DrawOptions => (sectionState) =>
    {
        if (ImGui.Selectable("Compact View"))
        {
            sectionState.Set(sectionModeId, ItemListSectionMode.Grid);
        }
        if (ImGui.Selectable("List View"))
        {
            sectionState.Set(sectionModeId, ItemListSectionMode.List);
        }
    };

    public override bool IsEmpty(SectionState sectionState)
    {
        return !_options.Items.Any();
    }

    public override void DrawSection(SectionState sectionState)
    {
        var iconSize =  32;
        var paddedIconSize = (iconSize * ImGui.GetIO().FontGlobalScale) + ImGui.GetStyle().FramePadding.X * 2;
        if (GetSectionMode(sectionState) == ItemListSectionMode.Grid)
        {
            _imGuiService.WrapElements(_options.SectionName + "Items", _options.Items, paddedIconSize, ImGui.GetStyle().ItemSpacing.X, item =>
            {
                if (ImGui.ImageButton(
                        _textureProvider.GetFromGameIcon(new GameIconLookup(item.ItemRow.Icon)).GetWrapOrEmpty()
                            .Handle,
                        new Vector2(iconSize, iconSize) * ImGui.GetIO().FontGlobalScale))
                {
                    _mediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.ItemId));
                }

                DrawOwnedMarker(item);

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                        ImGuiHoveredFlags.AnyWindow) &&
                    ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("RightClick" + item.ItemId);
                }

                _tooltipService.DrawItemTooltip(item);

                using (var popup = ImRaii.Popup("RightClick" + item.ItemId))
                {
                    if (popup)
                    {
                        _mediatorService.Publish(_menuService.DrawRightClickPopup(item.ItemRow));
                    }
                }

                return true;
            }, -1);
        }
        else
        {
            foreach (var item in _options.Items)
            {
                if (item.ItemId == 0)
                {
                    continue;
                }

                if (ImGui.ImageButton(
                        _textureProvider.GetFromGameIcon(new GameIconLookup(item.ItemRow.Icon)).GetWrapOrEmpty()
                            .Handle,
                        new Vector2(iconSize, iconSize)))
                {
                    _mediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.ItemId));
                }

                DrawOwnedMarker(item);

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                        ImGuiHoveredFlags.AnyWindow) &&
                    ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("RightClick" + item.ItemId);
                }

                if (ImGui.IsItemHovered())
                {
                    _tooltipService.DrawItemTooltip(item);
                }

                using (var popup = ImRaii.Popup("RightClick" + item.ItemId))
                {
                    if (popup)
                    {
                        _mediatorService.Publish(_menuService.DrawRightClickPopup(item.ItemRow));
                    }
                }

                var itemName = item.ItemRow.NameString;
                var sourceNames = string.Join(", ",
                    item.ItemRow.Sources.Select(c => c.Type).Distinct()
                        .Select(c => _itemInfoRenderService.GetSourceTypeName(c).Singular));
                var style = ImGui.GetStyle();
                var textHeight = ImGui.CalcTextSize(itemName).Y;
                var sourceNamesSize = ImGui.CalcTextSize(sourceNames).Y;
                var iconHeight = iconSize + style.FramePadding.Y * 2;

                if (sourceNames != string.Empty)
                {
                    textHeight += sourceNamesSize;
                }

                textHeight += ImGui.GetStyle().ItemSpacing.Y;

                float offsetY = Math.Max(0f, (iconHeight - textHeight) / 2.0f);

                ImGui.SameLine();

                var cursorPos = ImGui.GetCursorPos();
                ImGui.SetCursorPos(new Vector2(cursorPos.X, cursorPos.Y + offsetY));
                var isOwned = _options.IsOwned?.Invoke(item.ItemId);
                using (ImRaii.Group())
                {
                    using (ImRaii.PushColor(ImGuiCol.Text, isOwned == true ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed, isOwned != null))
                    {
                        if (item.Count == null)
                        {
                            ImGui.Text(itemName);
                        }
                        else
                        {
                            ImGui.Text(itemName + " x " + item.Count);
                        }
                    }

                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.TankBlue))
                    {
                        ImGui.Text(sourceNames);
                    }
                }
            }
        }

    }

    private void DrawOwnedMarker(ItemInfo item)
    {
        if (_options.IsOwned == null || !_options.IsOwned(item.ItemId))
        {
            return;
        }

        var tick = FontAwesomeIcon.Check.ToIconString();
        var fontSize = ImGui.GetFontSize() * 0.8f;
        var max = ImGui.GetItemRectMax();
        var position = new Vector2(max.X - fontSize - 2, max.Y - fontSize - 2);
        var drawList = ImGui.GetWindowDrawList();

        drawList.AddText(_font.IconFont, fontSize, position + Vector2.One, ImGui.GetColorU32(new Vector4(0, 0, 0, 1)), tick);
        drawList.AddText(_font.IconFont, fontSize, position, ImGui.GetColorU32(ImGuiColors.ParsedGreen), tick);
    }
}
