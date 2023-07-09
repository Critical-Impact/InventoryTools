using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using CriticalCommonLib.Time;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Ui.Widgets;
using Lumina.Excel.GeneratedSheets;
using OtterGui;
using OtterGui.Raii;

namespace InventoryTools.Logic.Columns
{
    public class CraftGatherColumn : CheckboxColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Tools;

        public override bool? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return item.CanBeGathered || item.ObtainedFishing;
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override bool? CurrentValue(CraftItem currentValue)
        {
            return true;
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
        
        void DrawSupplierRow(ItemEx item,(IShop shop, ENpc? npc, ILocation? location) tuple)
        {
            ImGui.TableNextColumn();
            ImGui.TextWrapped(tuple.shop.Name);
            if (tuple.npc != null)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped(tuple.npc?.Resident?.Singular ?? "");
            }
            if (tuple.npc != null && tuple.location != null)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped(tuple.location + " ( " + Math.Round(tuple.location.MapX, 2) + "/" +
                                  Math.Round(tuple.location.MapY, 2) + ")");
                ImGui.TableNextColumn();
                if (ImGui.Button("Teleport##" + tuple.shop.RowId + "_" + tuple.npc.Key + "_" +
                                 tuple.location.MapEx.Row))
                {
                    var nearestAetheryte = tuple.location.GetNearestAetheryte();
                    if (nearestAetheryte != null)
                    {
                        PluginService.TeleporterIpc.Teleport(nearestAetheryte.RowId);
                    }
                    PluginService.ChatUtilities.PrintFullMapLink(tuple.location, item.NameString);
                    ImGui.CloseCurrentPopup();
                }
            }
            else
            {
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
            }

        }

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(item) == true)
            {
                bool hasVendors;
                bool hasGather;
                if (item.IngredientPreference.Type is IngredientPreferenceType.Buy or IngredientPreferenceType.HouseVendor)
                {
                    hasVendors = DrawVendorButton(item, rowIndex);
                    if (hasVendors)
                    {
                        ImGui.SameLine();
                    }
                    hasGather = DrawGatherButtons(item, rowIndex);
                }
                else
                {
                    hasGather = DrawGatherButtons(item, rowIndex);
                    if (hasGather)
                    {
                        ImGui.SameLine();
                    }
                    hasVendors = DrawVendorButton(item, rowIndex);
                }

                if (item.UpTime != null)
                {
                    if (hasGather || hasVendors)
                    {
                        ImGui.SameLine();
                    }
                    var nextUptime = item.UpTime.Value.NextUptime(Service.SeTime.ServerTime);
                    if (nextUptime.Equals(TimeInterval.Always)
                        || nextUptime.Equals(TimeInterval.Invalid)
                        || nextUptime.Equals(TimeInterval.Never)) return;
                    if (nextUptime.Start > TimeStamp.UtcNow)
                    {
                        ImGui.Text(" (Up in " +
                                          TimeInterval.DurationString(nextUptime.Start, TimeStamp.UtcNow,
                                              true) + ")");
                    }
                    else
                    {
                        ImGui.Text(" (Up for " +
                                   TimeInterval.DurationString( nextUptime.End,TimeStamp.UtcNow,
                                       true) + ")");
                    }
                }
            }
        }

        private static bool DrawGatherButtons(CraftItem item, int rowIndex)
        {
            if (item.Item.ObtainedGathering)
            {
                if (ImGui.Button("Gather##Gather" + rowIndex))
                {
                    PluginService.CommandService.ProcessCommand("/gather " + item.Name);
                }

                return true;
            }
            else if (item.Item.ObtainedFishing)
            {
                if (ImGui.Button("Gather##Gather" + rowIndex))
                {
                    PluginService.CommandService.ProcessCommand("/gatherfish " + item.Name);
                }
                return true;
            }

            return false;
        }

        private bool DrawVendorButton(CraftItem item, int rowIndex)
        {
            if (item.Item.Vendors.Any())
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0.0f);
                if (ImGui.Button("Buy##Buy" + rowIndex))
                {
                    int mapId = item.MapId == null ? -1 : (int) item.MapId;
                    var vendor = GetLocations(item.Item).OrderBy(c => (c.location?.MapEx.Row ?? 0) == mapId ? 0 : 1).FirstOrDefault();
                    if (vendor.location != null)
                    {
                        var nearestAetheryte = vendor.location.GetNearestAetheryte();
                        if (nearestAetheryte != null)
                        {
                            PluginService.TeleporterIpc.Teleport(nearestAetheryte.RowId);
                        }

                        PluginService.ChatUtilities.PrintFullMapLink(vendor.location, vendor.location + " - Buy " + item.QuantityMissingOverall + " " + item.Item.NameString);
                    }
                    else
                    {
                        var shopName = vendor.shop.Name;
                        PluginService.ChatUtilities.Print("No location available. Shop is called " + shopName);
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
                                ImGuiTable.DrawTable("VendorsText", GetLocations(item.Item),
                                    tuple => DrawSupplierRow(item.Item, tuple), ImGuiTableFlags.None,
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

        public override void Draw(FilterConfiguration configuration, InventoryItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(item) == true)
            {
                if (item.Item.ObtainedGathering)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gather " + item.Item.NameString);
                    }
                }
                else if (item.Item.ObtainedFishing)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gatherfish " + item.Item.NameString);
                    }
                }
            }
        }

        public override void Draw(FilterConfiguration configuration, ItemEx item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(item) == true)
            {
                if (item.ObtainedGathering)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gather " + item.NameString);
                    }
                }
                else if (item.ObtainedFishing)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gatherfish " + item.NameString);
                    }
                }
            }
        }

        public override string Name { get; set; } = "Gather";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Shows a button that links to gatherbuddy's /gather function.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}