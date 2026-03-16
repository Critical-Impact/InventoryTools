using System;
using System.Numerics;
using CriticalCommonLib.Services;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Sections;

public sealed class MapLinkViewSection : ViewSection
{
    private readonly MapLinkViewSectionOptions _options;
    private readonly ITextureProvider _textureProvider;
    private readonly IMenuProvider<MapLinkEntry> _menuProvider;
    private readonly IChatUtilities _chatUtilities;

    public delegate MapLinkViewSection Factory(MapLinkViewSectionOptions options);

    public MapLinkViewSection(
        MapLinkViewSectionOptions options,
        ITextureProvider textureProvider,
        IMenuProvider<MapLinkEntry> menuProvider,
        ImGuiService imGuiService,
        IChatUtilities chatUtilities) : base(imGuiService)
    {
        _options = options;
        _textureProvider = textureProvider;
        _menuProvider = menuProvider;
        _chatUtilities = chatUtilities;
    }

    public override string SectionName => _options.SectionName;

    public override void DrawSection(SectionState sectionState)
    {
        if (_options.MapLink == null)
            return;

        var entry = _options.MapLink;

        var iconSize = 32f * ImGui.GetIO().FontGlobalScale;

        if (ImGui.ImageButton(
                _textureProvider
                    .GetFromGameIcon(new GameIconLookup(entry.Icon))
                    .GetWrapOrEmpty()
                    .Handle,
                new Vector2(iconSize, iconSize)))
        {
                _chatUtilities.PrintFullMapLink(entry.Location);
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            _menuProvider.Open(entry);
        }
        _menuProvider.Draw(entry);

        var style = ImGui.GetStyle();
        var textHeight = ImGui.CalcTextSize(entry.Name).Y;

        if (!string.IsNullOrEmpty(entry.Subtitle))
            textHeight += ImGui.CalcTextSize(entry.Subtitle).Y;

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
                ImGui.TextUnformatted(entry.Name);

                if (!string.IsNullOrEmpty(entry.Subtitle))
                {
                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.TankBlue))
                    {
                        ImGui.TextUnformatted(entry.Subtitle);
                    }
                }

                if (ImGui.IsItemHovered() || ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenOverlapped))
                {
                    var loc = entry.Location;
                    using (var tooltip = ImRaii.Tooltip())
                    {
                        if (tooltip)
                        {
                            ImGui.TextUnformatted(loc.FormattedName);
                            ImGui.Separator();
                            ImGui.Text($"X: {loc.MapX:0.0}");
                            ImGui.Text($"Y: {loc.MapY:0.0}");
                        }
                    }
                }

            }
        }

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - offsetY);
    }
}