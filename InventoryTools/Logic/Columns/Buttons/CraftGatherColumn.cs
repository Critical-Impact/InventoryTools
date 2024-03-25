using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using CriticalCommonLib.Time;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class CraftGatherColumn : CheckboxColumn
    {
        private readonly IChatUtilities _chatUtilities;

        public CraftGatherColumn(ILogger<CraftGatherColumn> logger, ImGuiService imGuiService, IChatUtilities chatUtilities) : base(logger, imGuiService)
        {
            _chatUtilities = chatUtilities;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Buttons;

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item.Item);
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return item.CanBeGathered || item.ObtainedFishing;
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
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
        
        void DrawSupplierRow(ItemEx item,(IShop shop, ENpc? npc, ILocation? location) tuple, List<MessageBase> messages)
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
                        messages.Add(new RequestTeleportMessage(nearestAetheryte.RowId));
                    }
                    _chatUtilities.PrintFullMapLink(tuple.location, item.NameString);
                    ImGui.CloseCurrentPopup();
                }
            }
            else
            {
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
            }
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            var messages = new List<MessageBase>();
            ImGui.TableNextColumn();
            if (CurrentValue(columnConfiguration, item) == true)
            {
                bool hasVendors;
                bool hasGather;
                if (item.IngredientPreference.Type is IngredientPreferenceType.Buy or IngredientPreferenceType.HouseVendor)
                {
                    hasVendors = DrawVendorButton(item, rowIndex, messages);
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
                    hasVendors = DrawVendorButton(item, rowIndex, messages);
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
                        || nextUptime.Equals(TimeInterval.Never)) return null;
                    if (nextUptime.Start > TimeStamp.UtcNow)
                    {
                        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed))
                        {
                            ImGui.Text(" (Up in " +
                                       TimeInterval.DurationString(nextUptime.Start, TimeStamp.UtcNow,
                                           true) + ")");
                        }
                    }
                    else
                    {
                        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen))
                        {
                            ImGui.Text(" (Up for " +
                                       TimeInterval.DurationString(nextUptime.End, TimeStamp.UtcNow,
                                           true) + ")");
                        }
                    }
                }
            }
            return null;
        }

        private bool DrawGatherButtons(CraftItem item, int rowIndex)
        {
            if (item.Item.ObtainedGathering)
            {
                if (ImGui.Button("Gather##Gather" + rowIndex))
                {
                    Service.Commands.ProcessCommand("/gather " + item.Name);
                }

                return true;
            }
            else if (item.Item.ObtainedFishing)
            {
                if (ImGui.Button("Gather##Gather" + rowIndex))
                {
                    Service.Commands.ProcessCommand("/gatherfish " + item.Name);
                }
                return true;
            }

            return false;
        }

        private bool DrawVendorButton(CraftItem item, int rowIndex, List<MessageBase> messages)
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
                            messages.Add(new RequestTeleportMessage(nearestAetheryte.RowId));
                        }

                        _chatUtilities.PrintFullMapLink(vendor.location, vendor.location + " - Buy " + item.QuantityMissingOverall + " " + item.Item.NameString);
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

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(columnConfiguration, item) == true)
            {
                if (item.Item.ObtainedGathering)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        Service.Commands.ProcessCommand("/gather " + item.Item.NameString);
                    }
                }
                else if (item.Item.ObtainedFishing)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        Service.Commands.ProcessCommand("/gatherfish " + item.Item.NameString);
                    }
                }
            }
            return null;
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            ItemEx item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(columnConfiguration, item) == true)
            {
                if (item.ObtainedGathering)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        Service.Commands.ProcessCommand("/gather " + item.NameString);
                    }
                }
                else if (item.ObtainedFishing)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        Service.Commands.ProcessCommand("/gatherfish " + item.NameString);
                    }
                }
            }
            return null;
        }

        public override string RenderName { get; } = "Gather/Purchase";
        public override string Name { get; set; } = "Gather/Purchase/Buy";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Shows a button that links to gatherbuddy's /gather function.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}