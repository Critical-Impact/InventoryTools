using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.Interface.Grid;
using AllaganLib.Interface.Grid.ColumnFilters;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Compendium.Columns.Options;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Logic.Settings;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Columns;

/// <summary>
/// Shows one flag button for each map link. A left click prints the map link to chat. A right click opens the map link menu.
/// </summary>
public class GenericMapLinksTableColumn<TData> : StringColumn<WindowState, TData, MessageBase>
{
    public delegate GenericMapLinksTableColumn<TData> Factory(MapLinksColumnOptions<TData> columnOptions);

    private readonly ImGuiService _imGuiService;
    private readonly CompendiumRowHeightSetting _rowHeightSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IChatUtilities _chatUtilities;
    private readonly IMenuProvider<MapLinkEntry> _menuProvider;
    private readonly Func<TData, List<MapLinkEntry>> _valueSelector;
    private readonly CompendiumDataCacher<TData, List<MapLinkEntry>> _cache = new(50);

    public GenericMapLinksTableColumn(ImGuiService imGuiService,
        StringColumnFilter stringColumnFilter,
        CompendiumRowHeightSetting rowHeightSetting,
        InventoryToolsConfiguration configuration,
        IChatUtilities chatUtilities,
        IMenuProvider<MapLinkEntry> menuProvider,
        MapLinksColumnOptions<TData> columnOptions) : base(imGuiService, stringColumnFilter)
    {
        Key = columnOptions.Key;
        Name = columnOptions.Name;
        RenderName = columnOptions.RenderName;
        Width = columnOptions.Width;
        HideFilter = columnOptions.HideFilter;
        ColumnFlags = columnOptions.ColumnFlags;
        EmptyText = columnOptions.EmptyText;
        HelpText = columnOptions.HelpText;
        Version = columnOptions.Version;
        _valueSelector = columnOptions.ValueSelector;
        _imGuiService = imGuiService;
        _rowHeightSetting = rowHeightSetting;
        _configuration = configuration;
        _chatUtilities = chatUtilities;
        _menuProvider = menuProvider;
    }

    public override string DefaultValue { get; set; }
    public override string Key { get; set; }
    public override string Name { get; set; }
    public override string? RenderName { get; set; }
    public override int Width { get; set; }
    public override bool HideFilter { get; set; }
    public override ImGuiTableColumnFlags ColumnFlags { get; set; }
    public override string EmptyText { get; set; }
    public override string HelpText { get; set; }
    public override string Version { get; set; }

    public override string? CurrentValue(TData item)
    {
        return string.Join(", ", GetEntries(item).Select(c => c.Name).Distinct());
    }

    private List<MapLinkEntry> GetEntries(TData item)
    {
        if (_cache.TryGet(item, out var entries))
            return entries;

        entries = _valueSelector.Invoke(item);
        _cache.Add(item, entries);
        return entries;
    }

    public override IEnumerable<MessageBase>? Draw(WindowState config, TData item, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            return null;
        }

        var entries = GetEntries(item);
        var rowHeight = _rowHeightSetting.CurrentValue(_configuration);
        var iconSize = new Vector2(rowHeight) * ImGui.GetIO().FontGlobalScale;
        var index = 0;
        _imGuiService.WrapTableColumnElements("MapLinks" + Key + rowIndex, entries,
            rowHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X,
            entry =>
            {
                using var id = ImRaii.PushId(index++);
                if (ImGui.ImageButton(_imGuiService.GetIconTexture(entry.Icon).Handle, iconSize, Vector2.Zero, Vector2.One, 0))
                {
                    _chatUtilities.PrintFullMapLink(entry.Location, entry.LinkText);
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    _menuProvider.Open(entry);
                }

                if (ImGui.IsItemHovered())
                {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.TextUnformatted(entry.Name);
                    if (!string.IsNullOrEmpty(entry.Subtitle))
                    {
                        ImGui.TextUnformatted(entry.Subtitle);
                    }
                }

                _menuProvider.Draw(entry);
                return true;
            });
        return null;
    }
}
