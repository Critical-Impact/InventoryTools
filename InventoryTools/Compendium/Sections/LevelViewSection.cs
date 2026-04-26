using System;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Services;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Sections;

public class LevelViewSection : ViewSection
{
    private readonly LevelViewSectionOptions _options;
    private readonly LevelSheet _levelSheet;
    private readonly ITextureProvider _textureProvider;
    private readonly ICompendiumTypeFactory _compendiumTypeFactory;
    private ICompendiumType? _relatedObjectType = null;
    private LevelRow? _levelRow;
    private bool _relatedTypeCreated;

    public delegate LevelViewSection Factory(LevelViewSectionOptions options);

    public LevelViewSection(LevelViewSectionOptions options, LevelSheet levelSheet, ITextureProvider textureProvider, ICompendiumTypeFactory compendiumTypeFactory, ImGuiService imGuiService) : base(imGuiService)
    {
        _options = options;
        _levelSheet = levelSheet;
        _textureProvider = textureProvider;
        _compendiumTypeFactory = compendiumTypeFactory;
    }

    public override string SectionName => _options.SectionName;
    public override void DrawSection(SectionState sectionState)
    {
        var entry = _options.Level;
        _levelRow ??= _levelSheet.GetRow(_options.Level.RowId);
        if (!_relatedTypeCreated)
        {
            _relatedObjectType = _compendiumTypeFactory.GetByRowRef(entry.Value.Object, out var type);
            _relatedTypeCreated = true;
        }

        var icon = _relatedObjectType?.GetIcon(entry.Value.Object.RowId).Item2 ?? Icons.FlagIcon;
        var name = _relatedObjectType?.GetName(entry.Value.Object.RowId) ?? "Location";

        var iconSize = 32f * ImGui.GetIO().FontGlobalScale;

        if (ImGui.ImageButton(
                _textureProvider
                    .GetFromGameIcon(new GameIconLookup(icon))
                    .GetWrapOrEmpty()
                    .Handle,
                new Vector2(iconSize, iconSize)))
        {
        }

        var style = ImGui.GetStyle();
        var textHeight = ImGui.CalcTextSize(name).Y;
        textHeight += style.ItemSpacing.Y;

        textHeight += ImGui.CalcTextSize(_levelRow.FormattedName).Y;

        var iconHeight = iconSize + style.FramePadding.Y * 2;
        float offsetY = Math.Max(0f, (iconHeight / 2f) - (textHeight / 2f));

        ImGui.SameLine();

        var cursorPos = ImGui.GetCursorPos();
        ImGui.SetCursorPos(new Vector2(cursorPos.X, cursorPos.Y + offsetY));

        using (var group = ImRaii.Group())
        {
            if (group)
            {
                ImGui.TextUnformatted(name);
                using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.TankBlue))
                {
                    ImGui.TextUnformatted(_levelRow.FormattedName);
                }

                if (ImGui.IsItemHovered() || ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenOverlapped))
                {
                    using (var tooltip = ImRaii.Tooltip())
                    {
                        if (tooltip)
                        {
                            ImGui.TextUnformatted(_levelRow.FormattedName);
                            ImGui.Separator();
                            ImGui.Text($"X: {_levelRow.MapX:0.0}");
                            ImGui.Text($"Y: {_levelRow.MapY:0.0}");
                        }
                    }
                }
            }
        }

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - offsetY);
    }
}

public class LevelViewSectionOptions
{
    public string SectionName { get; init; } = "Location";
    public RowRef<Level> Level { get; init; }
}