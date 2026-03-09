using System;
using System.Collections.Generic;
using System.Numerics;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Compendium.Windows;

public class CompendiumTypesWindow : GenericWindow
{
    private readonly IEnumerable<ICompendiumType> _compendiumTypes;
    private readonly ITextureProvider _textureProvider;
    private string _search = string.Empty;

    public CompendiumTypesWindow(IEnumerable<ICompendiumType> compendiumTypes, ITextureProvider textureProvider, ILogger<CompendiumTypesWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration) : base(logger, mediator, imGuiService, configuration, "Compendium")
    {
        _compendiumTypes = compendiumTypes;
        _textureProvider = textureProvider;
    }

    public override void DrawWindow()
    {
        const float cardWidth = 260f;
        const float cardHeight = 72f;
        const float iconSize = 48f;
        const float padding = 10f;

        var style = ImGui.GetStyle();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##compendium_search", "Search compendium...", ref _search, 100);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var regionWidth = ImGui.GetContentRegionAvail().X;

        var columns = Math.Max(
            1,
            (int)((regionWidth + style.ItemSpacing.X) / (cardWidth + style.ItemSpacing.X))
        );

        var index = 0;

        foreach (var compendiumType in _compendiumTypes)
        {
            if (!string.IsNullOrWhiteSpace(_search) && !(compendiumType.Plural.Contains(_search, StringComparison.OrdinalIgnoreCase) || compendiumType.Description.Contains(_search, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            using (ImRaii.PushId(compendiumType.Key))
            {
                if (index % columns != 0)
                {
                    ImGui.SameLine();
                }

                using var pushedStyle = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 8f).Push(ImGuiStyleVar.ChildBorderSize, 1f);

                using (var child = ImRaii.Child(
                           "card",
                           new Vector2(cardWidth, cardHeight),
                           true,
                           ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
                       ))
                {
                    if (child)
                    {
                        ImGui.SetCursorPos(new Vector2(padding, padding));

                        var icon = compendiumType.Icon;

                        if (icon.Item2 != null)
                        {
                            var tex = _textureProvider
                                .GetFromGameIcon(new GameIconLookup(icon.Item2.Value))
                                .GetWrapOrEmpty();

                            ImGui.Image(tex.Handle, new Vector2(iconSize, iconSize));
                            ImGui.SameLine();
                        }

                        using (var group = ImRaii.Group())
                        {
                            if (group)
                            {
                                ImGui.TextUnformatted(compendiumType.Plural);
                                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
                                ImGui.TextWrapped(compendiumType.Description);
                                ImGui.PopStyleColor();
                            }
                        }
                    }
                }


                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }

                if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    MediatorService.Publish(new ToggleCompendiumListMessage(compendiumType));
                }
            }

            index++;
        }
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
    public override string GenericKey => "compendium_types";
    public override string GenericName => "Compendium";
    public override bool DestroyOnClose => true;
    public override bool SaveState => true;
    public override Vector2? DefaultSize => new Vector2(600, 600);
    public override Vector2? MaxSize => null;
    public override Vector2? MinSize => null;

    public override void Initialize()
    {
    }
}