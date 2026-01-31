using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Mediator;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Services;

public class CompendiumMenuRenderer
{
    private readonly IChatUtilities _chatUtilities;
    private readonly ImGuiMenuService _imGuiMenuService;

    public CompendiumMenuRenderer(IChatUtilities chatUtilities, ImGuiMenuService imGuiMenuService)
    {
        _chatUtilities = chatUtilities;
        _imGuiMenuService = imGuiMenuService;
    }
    public List<MessageBase> RenderMenu(uint rowId, ICompendiumType compendiumType)
    {
        var messages = new List<MessageBase>();
        if (compendiumType is ILocations locations)
        {
            var actualLocations = locations.GetLocations(rowId);
            if (actualLocations != null && actualLocations.Count > 0)
            {
                using (var menu = ImRaii.Menu("Teleport"))
                {
                    if (menu)
                    {
                        foreach (var location in actualLocations)
                        {
                            if (ImGui.MenuItem(location.Name + (location.MapLinkName == null ? "" : "(" + location.MapLinkName + ")" ) + " - Teleport(" + location.Location.FormattedName + ")"))
                            {
                                messages.Add(
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
        if (compendiumType is IItems items)
        {
            var actualItems = items.GetItems(rowId) ?? [];
            foreach (var item in actualItems)
            {
                using (var menu = ImRaii.Menu(item.ItemRow.NameString))
                {
                    if (menu)
                    {
                        messages.AddRange(_imGuiMenuService.DrawRightClickPopup(item.ItemRow));
                    }
                }
            }
        }

        return messages;
    }
}