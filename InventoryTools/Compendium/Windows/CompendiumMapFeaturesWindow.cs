using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.Shared.Extensions;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Types;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Compendium.Windows;

public class CompendiumMapFeaturesWindow : UintWindow
{
    private readonly IEnumerable<ICompendiumType> _compendiumTypes;
    private readonly TerritoryTypeCompendiumType _territoryTypeCompendiumType;
    private readonly ITextureProvider _textureProvider;

    private readonly Dictionary<ICompendiumType, List<uint>> _results = new();

    private ICompendiumType? _selectedType;

    private string _territoryName;

    public CompendiumMapFeaturesWindow(
        IEnumerable<ICompendiumType> compendiumTypes,
        TerritoryTypeCompendiumType territoryTypeCompendiumType,
        ITextureProvider textureProvider,
        ILogger<CompendiumMapFeaturesWindow> logger,
        MediatorService mediator,
        ImGuiService imGuiService,
        InventoryToolsConfiguration configuration)
        : base(logger, mediator, imGuiService, configuration, "Territory Compendium")
    {
        _compendiumTypes = compendiumTypes;
        _territoryTypeCompendiumType = territoryTypeCompendiumType;
        _textureProvider = textureProvider;
    }

    public override void Initialize(uint windowId)
    {
        base.Initialize(windowId);

        var relatedRows = _territoryTypeCompendiumType.GetRow(windowId);
        _territoryName = relatedRows?.FirstOrDefault()?.Base.PlaceName.ValueNullable?.Name.ToImGuiString() ?? "Unknown Territory";
        WindowName = _territoryName + " - " + "POIs";
        var relatedIds = relatedRows?.Select(c => c.RowId).ToHashSet() ?? [];

        _results.Clear();

        foreach (var compendiumType in _compendiumTypes)
        {
            if (!compendiumType.ShowInListing || !compendiumType.HasLocation)
                continue;

            foreach (var rowId in compendiumType)
            {
                var location = compendiumType.GetLocation(rowId);

                if (location == null)
                    continue;

                if (!relatedIds.Contains(location.TerritoryType.RowId))
                    continue;

                if (!_results.TryGetValue(compendiumType, out var list))
                {
                    list = new List<uint>();
                    _results[compendiumType] = list;
                }

                list.Add(rowId);
            }
        }

        _selectedType = _results
            .Where(r => r.Value.Count > 0)
            .OrderBy(r => r.Key.Plural)
            .Select(r => r.Key)
            .FirstOrDefault();
    }

    public override void DrawWindow()
    {
        if (_results.Count == 0)
        {
            ImGui.TextUnformatted("No entries found for this territory.");
            return;
        }

        var ordered = _results
            .Where(kvp => kvp.Value.Count > 0)
            .OrderBy(kvp => kvp.Key.Plural)
            .ToList();

        if (_selectedType == null || !ordered.Any(o => o.Key == _selectedType))
        {
            _selectedType = ordered.First().Key;
        }

        const float sidebarWidth = 220f;

        using (var sidebar = ImRaii.Child("##sidebar", new Vector2(sidebarWidth, 0), true))
        {
            if (sidebar)
            {
                ImGui.Text(this._territoryName);
                ImGui.Separator();
                foreach (var (type, rows) in ordered)
                {
                    using (ImRaii.PushId(type.Key))
                    {
                        var originalX = ImGui.GetCursorScreenPos().X;
                        var isSelected = _selectedType == type;

                        if (ImGui.Selectable("##selectable", isSelected, ImGuiSelectableFlags.None, new Vector2(0, 40)))
                        {
                            _selectedType = type;
                        }

                        var min = ImGui.GetItemRectMin();
                        var max = ImGui.GetItemRectMax();

                        ImGui.SetCursorScreenPos(min + new Vector2(6, 4));

                        var icon = type.Icon;

                        if (icon.Item2 != null)
                        {
                            var tex = _textureProvider
                                .GetFromGameIcon(new GameIconLookup(icon.Item2.Value))
                                .GetWrapOrEmpty();

                            ImGui.Image(tex.Handle, new Vector2(24, 24));
                            ImGui.SameLine();
                        }

                        ImGui.TextUnformatted($"{type.Plural} ({rows.Count})");

                        ImGui.SetCursorScreenPos(new Vector2(originalX, max.Y));
                    }
                }
            }
        }

        ImGui.SameLine();

        using (var content = ImRaii.Child("##content", new Vector2(0, 0), false))
        {
            if (!content || _selectedType == null)
                return;

            var rows = _results[_selectedType];

            ImGui.TextUnformatted($"{_selectedType.Plural} ({rows.Count})");
            ImGui.Separator();

            var clipper = new ImGuiListClipper();
            clipper.Begin(rows.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var rowId = rows[i];

                    var name = _selectedType.GetName(rowId) ?? "Unknown";
                    var subtitle = _selectedType.GetSubtitle(rowId);
                    var location = _selectedType.GetLocation(rowId);
                    var icon = _selectedType.GetIcon(rowId);

                    using (ImRaii.PushId(rowId.ToString()))
                    {
                        var originalX = ImGui.GetCursorScreenPos().X;

                        if (ImGui.Selectable("##" + rowId, false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, 50)))
                        {
                            MediatorService.Publish(new ToggleCompendiumViewMessage(_selectedType, rowId));
                        }

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        }

                        var min = ImGui.GetItemRectMin();
                        ImGui.SetCursorScreenPos(min);
                        var max = ImGui.GetItemRectMax();

                        using var group = ImRaii.Group();
                        if (group)
                        {
                            if (icon.Item2 != null)
                            {
                                var tex = _textureProvider
                                    .GetFromGameIcon(new GameIconLookup(icon.Item2.Value))
                                    .GetWrapOrEmpty();

                                ImGui.Image(tex.Handle, new Vector2(32, 32));
                                ImGui.SameLine();
                            }

                            using (ImRaii.Group())
                            {
                                ImGui.TextUnformatted(name);

                                if (!string.IsNullOrEmpty(subtitle))
                                {
                                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
                                    {
                                        ImGui.TextUnformatted(subtitle);
                                    }
                                }

                                if (location != null && location.HasCoordinates)
                                {
                                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
                                    {
                                        ImGui.TextUnformatted(
                                            $"{location.FormattedName} ({location.MapX:0.0}, {location.MapY:0.0})");
                                    }
                                }
                            }
                        }

                        ImGui.SetCursorScreenPos(new Vector2(originalX, max.Y));
                    }

                    ImGui.Separator();
                }
            }

            clipper.End();
        }
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;

    public override string GenericKey => "territory_compendium";
    public override string GenericName => "Territory Compendium";
    public override bool DestroyOnClose => true;
    public override bool SaveState => true;
    public override Vector2? DefaultSize => new Vector2(700, 600);
    public override Vector2? MaxSize => null;
    public override Vector2? MinSize => null;
}