using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Services;

public class CompendiumMenuBuilder
{
    private readonly IChatUtilities _chatUtilities;
    private readonly MediatorService _mediatorService;
    private readonly ImGuiMenuService _imGuiMenuService;
    private readonly TryOn _tryOn;
    private readonly ItemSheet _itemSheet;

    public CompendiumMenuBuilder(IChatUtilities chatUtilities, MediatorService mediatorService, ImGuiMenuService imGuiMenuService, TryOn tryOn, ItemSheet itemSheet)
    {
        _chatUtilities = chatUtilities;
        _mediatorService = mediatorService;
        _imGuiMenuService = imGuiMenuService;
        _tryOn = tryOn;
        _itemSheet = itemSheet;
    }

    public void Header(string headerText)
    {
        ImGui.Text(headerText + ":");
        ImGui.Separator();
    }

    public void NewLine()
    {
        ImGui.NewLine();
    }

    public void Separator()
    {
        ImGui.Separator();
    }

    public void Locations(List<NamedLocation> locations)
    {
        if (locations.Count > 0)
        {
            using (var menu = ImRaii.Menu("Teleport"))
            {
                if (menu)
                {
                    foreach (var location in locations)
                    {
                        if (ImGui.MenuItem(location.Name + (location.MapLinkName == null ? "" : "(" + location.MapLinkName + ")" ) + " - Teleport(" + location.Location.FormattedName + ")"))
                        {
                            _mediatorService.Publish(
                                new RequestTeleportToMapMessage(location.Location.Map.RowId,
                                    new Vector2((float)location.Location.MapX,
                                        (float)location.Location.MapY)));
                            _chatUtilities.PrintFullMapLink(location.Location,
                                location.MapLinkName ?? location.Name);
                        }
                    }
                }
            }
        }
    }

    public void Item(ItemInfo item)
    {
        using (var menu = ImRaii.Menu(item.ItemRow.NameString))
        {
            if (menu)
            {
                _mediatorService.Publish(_imGuiMenuService.DrawRightClickPopup(item.ItemRow));
            }
        }
    }

    public void Items(List<ItemInfo> items)
    {
        foreach (var item in items)
        {
            using (var menu = ImRaii.Menu(item.ItemRow.NameString))
            {
                if (menu)
                {
                    _mediatorService.Publish(_imGuiMenuService.DrawRightClickPopup(item.ItemRow));
                }
            }
        }
    }

    public void Items(List<RowRef<Item>> items)
    {
        var actualItems = items.Where(c => c.IsValid && c.RowId != 0).Select(c => _itemSheet.GetRow(c.RowId)).ToList();
        foreach (var item in actualItems)
        {
            using (var menu = ImRaii.Menu(item.NameString))
            {
                if (menu)
                {
                    _mediatorService.Publish(_imGuiMenuService.DrawRightClickPopup(item));
                }
            }
        }
    }

    public void GroupedItems(List<RowRef<Item>> items, string text = "All Items")
    {
        var actualItems = items.Where(c => c.IsValid && c.RowId != 0).Select(c => _itemSheet.GetRow(c.RowId)).ToList();
        using (var menu = ImRaii.Menu(text))
        {
            if (menu)
            {

                _mediatorService.Publish(_imGuiMenuService.DrawRightClickPopup(actualItems));
            }
        }
    }

    public void TryOn(List<ItemInfo> items, string text = "Try On")
    {
        if (ImGui.Selectable(text))
        {
            _tryOn.TryOnItem(items.Select(c => c.ItemRow).ToList());
        }
    }


    public void TryOn(List<RowRef<Item>> items, string text = "Try On")
    {
        if (ImGui.Selectable(text))
        {
            _tryOn.TryOnItem(items);
        }
    }


}