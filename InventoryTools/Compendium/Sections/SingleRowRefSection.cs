using System;
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

namespace InventoryTools.Compendium.Sections;

public class SingleRowRefSection : ViewSection
{
    private readonly SingleRowRefSectionOptions _options;
    private readonly ITextureProvider _textureProvider;
    private readonly MediatorService _mediatorService;
    private readonly ICompendiumType? _relatedCompendiumType = null;
    private readonly Type? _refType;
    private string? _title;
    private string? _subTitle;
    private (string?, uint?)? _icon;
    private readonly uint relatedRefRowId;

    public delegate SingleRowRefSection Factory(SingleRowRefSectionOptions options);

    public SingleRowRefSection(SingleRowRefSectionOptions options, ICompendiumTypeFactory compendiumTypeFactory, ImGuiService imGuiService, ITextureProvider textureProvider, MediatorService mediatorService) : base(imGuiService)
    {
        _options = options;
        _textureProvider = textureProvider;
        _mediatorService = mediatorService;
        _relatedCompendiumType = compendiumTypeFactory.GetByRowRef(options.RelatedRef, out _refType);
        relatedRefRowId = _options.RelatedRef.RowId;
        if (_options.RelatedRef.RowType != null)
        {
            var newId = _relatedCompendiumType?.RemapType(_options.RelatedRef.RowType, relatedRefRowId);
            if (newId != null)
            {
                relatedRefRowId = newId.Value;
            }
        }
    }

    public override string SectionName => _options.SectionName ?? "Related " + (_relatedCompendiumType?.Singular ?? "Object");
    public override bool ShouldDraw(SectionState sectionState)
    {
        if (_relatedCompendiumType == null)
        {
            if (_refType == null)
            {
                return false;
            }
        }

        if (_relatedCompendiumType != null && !_relatedCompendiumType.HasRow(relatedRefRowId))
        {
            return false;
        }

        return true;
    }

    public override void DrawSection(SectionState sectionState)
    {
        if (_relatedCompendiumType == null)
        {
            if (_refType == null)
            {
                ImGui.Text("Unknown related row type.");
            }
            else
            {
                ImGui.Text("No matching compendium type for " + _refType.Name);
            }
        }
        else
        {
            if (!_relatedCompendiumType.HasRow(relatedRefRowId))
            {
                return;
            }

            var icon = _icon ??= _relatedCompendiumType.GetIcon(relatedRefRowId);
            var iconSize = 32f * ImGui.GetIO().FontGlobalScale;
            if (icon.Item2 != null)
            {
                if (ImGui.ImageButton(_textureProvider.GetFromGameIcon(new GameIconLookup(icon.Item2.Value)).GetWrapOrEmpty().Handle, new(iconSize)))
                {
                    _mediatorService.Publish(new OpenCompendiumViewMessage(_relatedCompendiumType, relatedRefRowId));
                }
                ImGui.SameLine();
            }

            var name = _title ??= _relatedCompendiumType.GetName(relatedRefRowId) ?? "";
            var subTitle = _subTitle ??= _relatedCompendiumType.GetSubtitle(relatedRefRowId) ?? "";

            var style = ImGui.GetStyle();
            var textHeight = ImGui.CalcTextSize(name).Y;

            if (!string.IsNullOrEmpty(subTitle))
                textHeight += ImGui.CalcTextSize(subTitle).Y;

            textHeight += style.ItemSpacing.Y;

            var iconHeight = iconSize + style.FramePadding.Y * 2;
            float offsetY = Math.Max(0f, (iconHeight - textHeight) / 2f);

            ImGui.SameLine();

            var cursorPos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cursorPos.X, cursorPos.Y + offsetY));

            using (var group = ImRaii.Group())
            {
                if (group)
                {
                    ImGui.TextUnformatted(_relatedCompendiumType.GetName(relatedRefRowId));

                    if (!string.IsNullOrEmpty(subTitle))
                    {
                        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.TankBlue))
                        {
                            ImGui.TextUnformatted(subTitle);
                        }
                    }
                }
            }

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - offsetY);

        }
    }
}