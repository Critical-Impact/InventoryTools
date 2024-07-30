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
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Logic.Columns.Buttons
{
    public class CraftGatherColumn : CheckboxColumn
    {
        private readonly IChatUtilities _chatUtilities;
        private readonly ISeTime _seTime;

        public CraftGatherColumn(ILogger<CraftGatherColumn> logger, ImGuiService imGuiService, IChatUtilities chatUtilities, ISeTime seTime) : base(logger, imGuiService)
        {
            _chatUtilities = chatUtilities;
            _seTime = seTime;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Buttons;

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.CraftItem != null)
            {
                return true;
            }
            return searchResult.Item.CanBeGathered || searchResult.Item.ObtainedFishing;
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

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex)
        {
            ImGui.TableNextColumn();
            if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
            
            var messages = new List<MessageBase>();
            if (CurrentValue(columnConfiguration, searchResult) == true)
            {
                bool hasVendors;
                bool hasGather;
                if (searchResult.CraftItem != null && searchResult.CraftItem.IngredientPreference.Type is IngredientPreferenceType.Buy or IngredientPreferenceType.HouseVendor)
                {
                    hasVendors = DrawVendorButton(searchResult, rowIndex, messages, false);
                    hasGather = DrawGatherButtons(searchResult, rowIndex, hasVendors);
                }
                else
                {
                    hasGather = DrawGatherButtons(searchResult, rowIndex, false);
                    hasVendors = DrawVendorButton(searchResult, rowIndex, messages, hasGather);
                }

                var gatheringUptime = searchResult.Item.GetGatheringUptime();
                if (gatheringUptime != null)
                {
                    if (hasGather || hasVendors)
                    {
                        ImGui.SameLine();
                    }
                    var nextUptime = gatheringUptime.Value.NextUptime(_seTime.ServerTime);
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
            return messages;
        }

        private bool DrawGatherButtons(SearchResult searchResult, int rowIndex, bool needsSameLine)
        {
            if (searchResult.Item.ObtainedGathering)
            {
                if (needsSameLine)
                {
                    ImGui.SameLine();
                }
                if (ImGui.Button("Gather##Gather" + rowIndex))
                {
                    Service.Commands.ProcessCommand("/gather " + searchResult.Item.Name);
                }

                return true;
            }
            else if (searchResult.Item.ObtainedFishing)
            {
                if (needsSameLine)
                {
                    ImGui.SameLine();
                }
                if (ImGui.Button("Gather##Gather" + rowIndex))
                {
                    Service.Commands.ProcessCommand("/gatherfish " + searchResult.Item.Name);
                }
                return true;
            }

            return false;
        }

        private bool DrawVendorButton(SearchResult item, int rowIndex, List<MessageBase> messages, bool needsSameLine)
        {
            if (item.Item.Vendors.Any())
            {
                if (needsSameLine)
                {
                    ImGui.SameLine();
                }
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0.0f);
                if (ImGui.Button("Buy##Buy" + rowIndex))
                {
                    uint? umapId = item.CraftItem?.MapId ?? null;
                    int mapId = umapId == null ? -1 : (int)umapId;
                    var vendor = GetLocations(item.Item).OrderBy(c => (c.location?.MapEx.Row ?? 0) == mapId ? 0 : 1).FirstOrDefault();
                    if (vendor.location != null)
                    {
                        var nearestAetheryte = vendor.location.GetNearestAetheryte();
                        if (nearestAetheryte != null)
                        {
                            messages.Add(new RequestTeleportMessage(nearestAetheryte.RowId));
                        }

                        var npcName = vendor.npc?.Resident?.Singular.AsReadOnly().ToString() ?? null;

                        List<string?> stringParts = new()
                        {
                            vendor.location + " - Buy ",
                            item.CraftItem?.QuantityMissingOverall.ToString() ?? null,
                            item.Item.NameString,
                            npcName != null ? $" from {npcName}" : null
                        };

                        var mapLinkText = String.Join("", stringParts.Where(c => c != null).Select(c => c!).ToList());
                        
                        _chatUtilities.PrintFullMapLink(vendor.location, mapLinkText);
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

        public override string RenderName { get; } = "Gather/Purchase";
        public override string Name { get; set; } = "Gather/Purchase/Buy";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Shows a button that links to gatherbuddy's /gather function.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.CraftFilter;
    }
}