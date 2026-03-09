using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Sections;

public sealed class MapLinksViewSection : CompendiumViewSection
{
    private readonly MapLinksViewSectionOptions _options;
    private readonly ITextureProvider _textureProvider;
    private readonly IMenuProvider<MapLinkEntry> _menuProvider;

    public delegate MapLinksViewSection Factory(MapLinksViewSectionOptions options);

    public MapLinksViewSection(
        MapLinksViewSectionOptions options,
        ITextureProvider textureProvider,
        IMenuProvider<MapLinkEntry> menuProvider,
        ImGuiService imGuiService) : base(imGuiService)
    {
        _options = options;
        _textureProvider = textureProvider;
        _menuProvider = menuProvider;
    }

    public override string SectionName => _options.SectionName;

    public override void DrawSection(SectionState sectionState)
    {
        if (_options.MapLinks == null)
            return;

        var iconSize = 32f * ImGui.GetIO().FontGlobalScale;
        var style = ImGui.GetStyle();

        int i = 0;

        foreach (var entry in _options.MapLinks)
        {
            ImGui.PushID(i++);

            if (ImGui.ImageButton(
                    _textureProvider
                        .GetFromGameIcon(new GameIconLookup(entry.Icon))
                        .GetWrapOrEmpty()
                        .Handle,
                    new Vector2(iconSize, iconSize)))
            {
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                _menuProvider.Open(entry);
            }

            _menuProvider.Draw(entry);

            var textHeight = ImGui.CalcTextSize(entry.Name).Y;

            if (!string.IsNullOrEmpty(entry.Subtitle))
                textHeight += ImGui.CalcTextSize(entry.Subtitle).Y;

            textHeight += style.ItemSpacing.Y;

            var iconHeight = iconSize + style.FramePadding.Y * 2;
            float offsetY = Math.Max(0f, (iconHeight - textHeight) / 2f);

            ImGui.SameLine();

            var cursorPos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cursorPos.X, cursorPos.Y + offsetY));

            ImGui.BeginGroup();

            ImGui.TextUnformatted(entry.Name);

            if (!string.IsNullOrEmpty(entry.Subtitle))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.TankBlue);
                ImGui.TextUnformatted(entry.Subtitle);
                ImGui.PopStyleColor();
            }

            if (ImGui.IsItemHovered() || ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenOverlapped))
            {
                var loc = entry.Location;
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(loc.FormattedName);
                ImGui.Separator();
                ImGui.Text($"X: {loc.MapX:0.0}");
                ImGui.Text($"Y: {loc.MapY:0.0}");
                ImGui.EndTooltip();
            }

            ImGui.EndGroup();

            ImGui.PopID();
        }
    }
}