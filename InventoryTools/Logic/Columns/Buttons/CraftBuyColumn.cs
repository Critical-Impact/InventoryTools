using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Mediator;

namespace InventoryTools.Logic.Columns.Buttons;

public class CraftBuyColumn : ButtonColumn
{
    private readonly IGameInterface _gameInterface;
    private readonly IChatUtilities _chatUtilities;

    public CraftBuyColumn(IGameInterface gameInterface, IChatUtilities chatUtilities)
    {
        _gameInterface = gameInterface;
        _chatUtilities = chatUtilities;
    }
    public override string Name { get; set; } = "Buy Button";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "A button/list to show you where you can buy an item";
    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SearchResult searchResult, int rowIndex, int columnIndex)
    {
        var messages = new List<MessageBase>();
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            DrawVendorButton(searchResult.Item, rowIndex, messages);
        }

        return messages;
    }
    
    List<(IShop shop, ENpc? npc, ILocation? location)> GetLocations(ItemEx item)
    {
        var vendors = new List<(IShop shop, ENpc? npc, ILocation? location)>();
        foreach (var vendor in item.Vendors)
        {
            if (vendor.Name == "")
            {
                continue;
            }
            if (!vendor.ENpcs.Any())
            {
                vendors.Add(new (vendor, null, null));
            }
            else
            {
                foreach (var npc in vendor.ENpcs)
                {
                    if (!npc.Locations.Any())
                    {
                        vendors.Add(new (vendor, npc, null));
                    }
                    else
                    {
                        foreach (var location in npc.Locations)
                        {
                            vendors.Add(new (vendor, npc, location));
                        }
                    }
                }
            }
        }

        vendors = vendors.OrderByDescending(c => c.npc != null && c.location != null).ToList();
        return vendors;
    }
    
    void DrawSupplierRow(ItemEx item,(IShop shop, ENpc? npc, ILocation? location) tuple, List<MessageBase> messages)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            ImGui.TextWrapped(tuple.shop.Name);
        }

        if (tuple.npc != null)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                ImGui.TextWrapped(tuple.npc?.Resident?.Singular ?? "");
            }
        }
        if (tuple.npc != null && tuple.location != null)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                ImGui.TextWrapped(tuple.location + " ( " + Math.Round(tuple.location.MapX, 2) + "/" +
                                  Math.Round(tuple.location.MapY, 2) + ")");
            }

            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (ImGui.Button("Teleport##" + tuple.shop.RowId + "_" + tuple.npc.Key + "_" +
                                 tuple.location.MapEx.Row))
                {
                    var nearestAetheryte = tuple.location.GetNearestAetheryte();
                    if (nearestAetheryte != null)
                    {
                        messages.Add(new RequestTeleportMessage(nearestAetheryte.RowId));
                    }

                    _chatUtilities.PrintFullMapLink(tuple.location, item.NameString);
                    ImGui.CloseCurrentPopup();
                }
            }
        }
        else
        {
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
        }
    }
    
    private bool DrawVendorButton(ItemEx item, int rowIndex, List<MessageBase> messages)
    {
        if (item.Item.Vendors.Any())
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0.0f);
            if (ImGui.Button("Buy##Buy" + rowIndex))
            {
                var vendor = GetLocations(item.Item).FirstOrDefault();
                if (vendor.location != null)
                {
                    var nearestAetheryte = vendor.location.GetNearestAetheryte();
                    if (nearestAetheryte != null)
                    {
                        messages.Add(new RequestTeleportMessage(nearestAetheryte.RowId));
                    }

                    _chatUtilities.PrintFullMapLink(vendor.location, vendor.location.ToString());
                }
                else
                {
                    var shopName = vendor.shop.Name;
                    _chatUtilities.Print("No location available. Shop is called " + shopName);
                }
            }

            ImGui.SameLine(0, 0);
            if (ImGui.ArrowButton("select##" + rowIndex, ImGuiDir.Down))
            {
                ImGui.OpenPopup("buyLocations" + rowIndex);
            }

            using (var popup = ImRaii.Popup("buyLocations" + rowIndex))
            {
                if (popup.Success)
                {
                    using (var scroller = ImRaii.Child("buyLocationsScroll" + rowIndex, new(400, 200)))
                    {
                        if (scroller.Success)
                        {
                            ImGuiTable.DrawTable("VendorsText", GetLocations(item.Item), tuple =>
                                {
                                    DrawSupplierRow(item.Item, tuple, messages);
                                }, ImGuiTableFlags.None,
                                new[] { "Shop Name", "NPC", "Location", "" });
                        }
                    }
                }
            }

            ImGui.PopStyleVar();
            return true;
        }

        return false;
    }
}