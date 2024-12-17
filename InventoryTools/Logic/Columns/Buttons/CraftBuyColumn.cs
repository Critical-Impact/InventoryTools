using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

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

    List<(IShop shop, ENpcBaseRow? npc, ILocation? location)> GetLocations(ItemRow item)
    {
        var vendors = new List<(IShop shop, ENpcBaseRow? npc, ILocation? location)>();
        var shops = item.GetSourcesByCategory<ItemShopSource>(ItemInfoCategory.Shop).Select(c => c.Shop);
        foreach (var vendor in shops)
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

    void DrawSupplierRow(ItemRow item,(IShop shop, ENpcBaseRow? npc, ILocation? location) tuple, List<MessageBase> messages)
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
                ImGui.TextWrapped(tuple.npc?.Resident.ValueNullable?.Singular.ExtractText() ?? "");
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
                if (ImGui.Button("Teleport##" + tuple.shop.RowId + "_" + tuple.npc.RowId + "_" +
                                 tuple.location.Map.RowId))
                {
                    var nearestAetheryte = tuple.location.GetNearestAetheryte();
                    if (nearestAetheryte != null)
                    {
                        messages.Add(new RequestTeleportMessage(nearestAetheryte.Value.RowId));
                    }

                    _chatUtilities.PrintFullMapLink(tuple.location, tuple.npc.ENpcResidentRow.Base.Singular.ExtractText());
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

    private bool DrawVendorButton(ItemRow item, int rowIndex, List<MessageBase> messages)
    {
        var shops = item.GetSourcesByCategory<ItemShopSource>(ItemInfoCategory.Shop).Select(c => c.Shop);
        if (shops.Any())
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0.0f);
            if (ImGui.Button("Buy##Buy" + rowIndex))
            {
                var vendor = GetLocations(item).FirstOrDefault();
                if (vendor.location != null)
                {
                    var nearestAetheryte = vendor.location.GetNearestAetheryte();
                    if (nearestAetheryte != null)
                    {
                        messages.Add(new RequestTeleportMessage(nearestAetheryte.Value.RowId));
                    }

                    _chatUtilities.PrintFullMapLink(vendor.location, vendor.npc?.ENpcResidentRow.Base.Singular.ExtractText() ?? vendor.location.ToString());
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
                            ImGuiTable.DrawTable("VendorsText", GetLocations(item), tuple =>
                                {
                                    DrawSupplierRow(item, tuple, messages);
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