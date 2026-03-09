using System;
using System.Numerics;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Services;
using InventoryTools.Mediator;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Sections;

public class SingleRowRefSection : CompendiumViewSection
{
    private readonly SingleRowRefSectionOptions _options;
    private readonly ICompendiumTypeFactory _compendiumTypeFactory;
    private readonly ITextureProvider _textureProvider;
    private readonly MediatorService _mediatorService;
    private ICompendiumType? _relatedCompendiumType = null;
    private Type? refType;

    public delegate SingleRowRefSection Factory(SingleRowRefSectionOptions options);

    public SingleRowRefSection(SingleRowRefSectionOptions options, ICompendiumTypeFactory compendiumTypeFactory, ImGuiService imGuiService, ITextureProvider textureProvider, MediatorService mediatorService) : base(imGuiService)
    {
        _options = options;
        _compendiumTypeFactory = compendiumTypeFactory;
        _textureProvider = textureProvider;
        _mediatorService = mediatorService;
        _relatedCompendiumType = _compendiumTypeFactory.GetByRowRef(options.RelatedRef, out refType);
    }

    public override string SectionName => _options.SectionName ?? "Related " + (_relatedCompendiumType?.Singular ?? "Object");
    public override bool ShouldDraw(SectionState sectionState)
    {
        if (_relatedCompendiumType == null)
        {
            if (refType == null)
            {
                return false;
            }
        }

        if (_relatedCompendiumType != null && !_relatedCompendiumType.HasRow(_options.RelatedRef.RowId))
        {
            return false;
        }

        return true;
    }

    public override void DrawSection(SectionState sectionState)
    {
        if (_relatedCompendiumType == null)
        {
            if (refType == null)
            {
                ImGui.Text("Unknown related row type.");
            }
            else
            {
                ImGui.Text("No matching compendium type for " + refType.Name);
            }
        }
        else
        {
            if (!_relatedCompendiumType.HasRow(_options.RelatedRef.RowId))
            {
                return;
            }

            var icon = _relatedCompendiumType.GetIcon(_options.RelatedRef.RowId);
            var iconSize = 32f * ImGui.GetIO().FontGlobalScale;
            if (icon.Item2 != null)
            {
                if (ImGui.ImageButton(_textureProvider.GetFromGameIcon(new GameIconLookup(icon.Item2.Value)).GetWrapOrEmpty().Handle, new(iconSize)))
                {
                    _mediatorService.Publish(new OpenCompendiumViewMessage(_relatedCompendiumType, _options.RelatedRef.RowId));
                }
                ImGui.SameLine();
            }

            var name = _relatedCompendiumType.GetName(_options.RelatedRef.RowId) ?? "";
            var subTitle = _relatedCompendiumType.GetSubtitle(_options.RelatedRef.RowId);

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

            ImGui.BeginGroup();

            ImGui.TextUnformatted(_relatedCompendiumType.GetName(_options.RelatedRef.RowId));

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
}