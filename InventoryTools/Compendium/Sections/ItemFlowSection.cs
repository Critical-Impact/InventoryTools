using System;
using System.Linq;
using System.Numerics;
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
using OtterGui.Extensions;

namespace InventoryTools.Compendium.Sections;

public class ItemFlowSection : ViewSection
{
    private readonly ItemFlowSectionOptions _options;
    private readonly MediatorService _mediatorService;
    private readonly ImGuiService _imGuiService;
    private readonly ITextureProvider _textureProvider;
    private readonly ImGuiTooltipService _tooltipService;
    private readonly ImGuiMenuService _menuService;
    private readonly IFont _font;

    public delegate ItemFlowSection Factory(ItemFlowSectionOptions options);

    public ItemFlowSection(
        ItemFlowSectionOptions options,
        MediatorService mediatorService,
        IFont font,
        ImGuiService imGuiService,
        ITextureProvider textureProvider,
        ImGuiTooltipService tooltipService,
        ImGuiMenuService menuService
    ) : base(imGuiService)
    {
        _options = options;
        _mediatorService = mediatorService;
        _imGuiService = imGuiService;
        _textureProvider = textureProvider;
        _tooltipService = tooltipService;
        _menuService = menuService;
        _font = font;
    }

    public override string SectionName => _options.SectionName;

    public override void DrawSection(SectionState sectionState)
    {
        var iconSize = 32f * ImGui.GetIO().FontGlobalScale;

        var minElementWidth = 80f;
        foreach (var item in _options.Items)
        {
            var textSize = ImGui.CalcTextSize(item.Title).X;
            if (textSize > minElementWidth)
            {
                minElementWidth = textSize;
            }
        }

        var itemsPerColumn = Math.Max(1, _options.ItemsPerColumn);

        var totalItems = _options.Items.Count;
        var columnCount = (int)Math.Ceiling(totalItems / (float)itemsPerColumn);

        var maxTitleWidth = _options.Items
            .Select(i => ImGui.CalcTextSize(i.Title ?? string.Empty).X)
            .DefaultIfEmpty(0f)
            .Max();

        var columnWidth = Math.Max(iconSize, maxTitleWidth) + 12f;

        for (int col = 0; col < columnCount; col++)
        {
            using (ImRaii.Group())
            {
                for (int row = 0; row < itemsPerColumn; row++)
                {
                    int index = col * itemsPerColumn + row;
                    if (index >= totalItems)
                        break;

                    var entry = _options.Items[index];
                    DrawItemWithTitle(entry, columnWidth, iconSize);

                    var isLastItemInColumn = row == itemsPerColumn - 1 || index == totalItems - 1;

                    if (!isLastItemInColumn)
                    {
                        DrawDownArrow(columnWidth);
                    }
                    else if (col < columnCount - 1)
                    {
                        DrawRightArrow(columnWidth);
                    }
                }
            }

            if (col < columnCount - 1)
            {
                ImGui.SameLine();
            }
        }
    }

    private void DrawDownArrow(float columnWidth)
    {
        DrawCenteredIcon(FontAwesomeIcon.ArrowDown.ToIconString(), columnWidth);
    }

    private void DrawRightArrow(float columnWidth)
    {
        DrawCenteredIcon(FontAwesomeIcon.ArrowRight.ToIconString(), columnWidth);
    }

    private void DrawCenteredIcon(string icon, float width)
    {
        var size = ImGui.CalcTextSize(icon);

        ImGui.Dummy(new Vector2(0, 4));

        var cursor = ImGui.GetCursorPos();
        ImGui.SetCursorPosX(cursor.X + (width - size.X) / 2f);

        using (ImRaii.PushFont(_font.IconFont))
        {
            ImGui.Text(icon);
        }

        ImGui.Dummy(new Vector2(0, 4));
    }

    private void DrawItemWithTitle(ItemFlowEntry entry, float columnWidth, float iconSize)
    {
        var item = entry.Item;
        var item2 = entry.Item2;

        using (ImRaii.Group())
        {
            var start = ImGui.GetCursorPos();

            ImGui.Dummy(new Vector2(columnWidth, 0));
            ImGui.SetCursorPos(start);

            var title = entry.Title ?? string.Empty;

            var titleSize = ImGui.CalcTextSize(title);
            var contentWidth = columnWidth;

            var cursor = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(cursor.X + (contentWidth - titleSize.X) / 2f);
            ImGui.Text(title);

            ImGui.Dummy(new Vector2(0, 2));

            var cursorAfterTitle = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(cursorAfterTitle.X + (contentWidth - iconSize - (item2 == null ? 0 : iconSize)) / 2f);

            if (ImGui.ImageButton(
                    _textureProvider
                        .GetFromGameIcon(new GameIconLookup(item.Icon))
                        .GetWrapOrEmpty()
                        .Handle,
                    new Vector2(iconSize, iconSize)))
            {
                _mediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.RowId));
            }

            if (ImGui.IsItemHovered() &&
                ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("RightClick" + item.RowId);
            }


            if (item2 != null)
            {
                ImGui.SameLine();
                if (ImGui.ImageButton(
                        _textureProvider
                            .GetFromGameIcon(new GameIconLookup(item2.Icon))
                            .GetWrapOrEmpty()
                            .Handle,
                        new Vector2(iconSize, iconSize)))
                {
                    _mediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item2.RowId));
                }

                if (ImGui.IsItemHovered() &&
                    ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("RightClick" + item2.RowId);
                }
            }

            _tooltipService.DrawItemTooltip(item);

            using (var popup = ImRaii.Popup("RightClick" + item.RowId))
            {
                if (popup)
                {
                    _mediatorService.Publish(
                        _menuService.DrawRightClickPopup(item));
                }
            }
        }
    }
}