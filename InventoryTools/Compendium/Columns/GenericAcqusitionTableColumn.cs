using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.Interface.Grid;
using AllaganLib.Interface.Grid.ColumnFilters;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Logic.Settings;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Columns;

public abstract class GenericAcquisitionTableColumnOld<TData> : StringColumn<WindowState, TData, MessageBase>
{
    private readonly CompendiumRowHeightSetting _rowHeightSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly ItemInfoRenderService _itemInfoRenderService;

    protected GenericAcquisitionTableColumnOld(CompendiumRowHeightSetting rowHeightSetting, InventoryToolsConfiguration configuration, ItemInfoRenderService itemInfoRenderService, ImGuiService imGuiService, StringColumnFilter stringColumnFilter) : base(imGuiService, stringColumnFilter)
    {
        _rowHeightSetting = rowHeightSetting;
        _configuration = configuration;
        _itemInfoRenderService = itemInfoRenderService;
    }

    public override IEnumerable<MessageBase>? Draw(WindowState config, TData item, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var messages = new  List<MessageBase>();
            var sources = this.GetItemSources(item);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().CellPadding.Y);
            var rowHeight = _rowHeightSetting.CurrentValue(_configuration);
            messages.AddRange(_itemInfoRenderService.DrawItemSourceIconsContainer("ItemSources" + rowIndex, rowHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, new Vector2(rowHeight, rowHeight), sources.ToList()));
            return messages;
        }
        return null;
    }

    public abstract IEnumerable<ItemSource> GetItemSources(TData item);

    public override string? CurrentValue(TData item)
    {
        var sources = string.Join(", ", GetItemSources(item)
            .Select(c => _itemInfoRenderService.GetSourceTypeName(c.Type).Singular).Distinct());
        return sources;
    }
}