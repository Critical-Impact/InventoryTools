using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Lumina.Excel;

namespace InventoryTools.Compendium.Sections;

public class CollectionRowRefSection : ViewSection
{
    private readonly CollectionRowRefSectionOptions _options;
    private readonly ITextureProvider _textureProvider;
    private readonly MediatorService _mediatorService;
    private readonly List<RowRefItem> _rowRefItems;
    private Dictionary<uint, string> _titles;
    private Dictionary<uint, string> _subtitles;
    private Dictionary<uint, (string?, uint?)> _icons;

    private sealed class RowRefItem
    {
        public RowRef RowRef { get; init; }
        public ICompendiumType? CompendiumType { get; init; }
        public Type? RefType { get; init; }
    }

    public delegate CollectionRowRefSection Factory(CollectionRowRefSectionOptions options);

    public CollectionRowRefSection(CollectionRowRefSectionOptions options, ICompendiumTypeFactory compendiumTypeFactory, ImGuiService imGuiService, ITextureProvider textureProvider, MediatorService mediatorService) : base(imGuiService)
    {
        _options = options;
        _textureProvider = textureProvider;
        _mediatorService = mediatorService;
        _rowRefItems = new List<RowRefItem>();
        _titles = new Dictionary<uint, string>();
        _subtitles = new Dictionary<uint, string>();
        _icons = new Dictionary<uint, (string?, uint?)>();

        foreach (var rowRef in _options.RelatedRefs.Where(c => options.Filter == null || c.RowType == options.Filter))
        {
            if (rowRef.RowId == 0)
            {
                continue;
            }
            var compendiumType = compendiumTypeFactory.GetByRowRef(rowRef, out var refType);
            _rowRefItems.Add(new RowRefItem
            {
                RowRef = rowRef,
                CompendiumType = compendiumType,
                RefType = refType
            });
        }
    }

    public override string SectionName => _options.SectionName;

    public override bool ShouldDraw(SectionState sectionState)
    {
        if (_options.HideIfEmpty)
        {
            foreach (var item in _rowRefItems)
            {
                if (item.CompendiumType != null && item.CompendiumType.HasRow(item.RowRef.RowId))
                {
                    return true;
                }
            }
            return false;
        }

        return true;
    }

    public override void DrawSection(SectionState sectionState)
    {
        bool hasDrawnAny = false;

        foreach (var item in _rowRefItems)
        {
            if (item.CompendiumType == null)
            {
                if (item.RefType == null)
                {
                    ImGui.Text("Unknown related row type.");
                }
                else
                {
                    ImGui.Text("No matching compendium type for " + item.RefType.Name);
                }
                hasDrawnAny = true;
            }
            else
            {
                if (!item.CompendiumType.HasRow(item.RowRef.RowId))
                {
                    continue;
                }

                using (ImRaii.PushId((int)item.RowRef.RowId))
                {
                    DrawRowRefItem(item);
                }

                hasDrawnAny = true;
            }
        }

        if (!hasDrawnAny && !_options.HideIfEmpty)
        {
            ImGui.Text("No related items found.");
        }
    }

    private void DrawRowRefItem(RowRefItem item)
    {
        var compendiumType = item.CompendiumType!;
        var rowRef = item.RowRef;

        if (!_icons.TryGetValue(rowRef.RowId, out var icon))
        {
            icon = compendiumType.GetIcon(rowRef.RowId);
            _icons.Add(rowRef.RowId, icon);
        }
        var iconSize = 32f * ImGui.GetIO().FontGlobalScale;

        if (icon.Item2 != null)
        {
            if (ImGui.ImageButton(_textureProvider.GetFromGameIcon(new GameIconLookup(icon.Item2.Value)).GetWrapOrEmpty().Handle, new(iconSize)))
            {
                _mediatorService.Publish(new OpenCompendiumViewMessage(compendiumType, rowRef.RowId));
            }
            ImGui.SameLine();
        }

        if (!_titles.TryGetValue(rowRef.RowId, out var title))
        {
            title = compendiumType.GetName(rowRef.RowId) ?? "";
            _titles.Add(rowRef.RowId, title);
        }

        if (!_subtitles.TryGetValue(rowRef.RowId, out var subTitle))
        {
            subTitle = compendiumType.GetSubtitle(rowRef.RowId) ?? "";
            _subtitles.Add(rowRef.RowId, subTitle);
        }

        var style = ImGui.GetStyle();
        var textHeight = ImGui.CalcTextSize(title).Y;

        if (!string.IsNullOrEmpty(subTitle))
            textHeight += ImGui.CalcTextSize(subTitle).Y;

        textHeight += style.ItemSpacing.Y;

        var iconHeight = iconSize + style.FramePadding.Y * 2;
        float offsetY = Math.Max(0f, (iconHeight - textHeight) / 2f);

        ImGui.SameLine();

        var cursorPos = ImGui.GetCursorPos();
        ImGui.SetCursorPos(new Vector2(cursorPos.X, cursorPos.Y + offsetY));

        ImGui.BeginGroup();

        ImGui.TextUnformatted(title);

        if (!string.IsNullOrEmpty(subTitle))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.TankBlue);
            ImGui.TextUnformatted(subTitle);
            ImGui.PopStyleColor();
        }

        ImGui.EndGroup();

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - offsetY);
    }
}