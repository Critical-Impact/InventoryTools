using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using CriticalCommonLib.Time;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

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

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(item) == true)
            {
                var hasButton = false;
                // if (item.IngredientPreference.Type == IngredientPreferenceType.Buy)
                // {
                //     //TODO: Rework this
                //     if (item.Item.Vendors.Any() && ImGui.SmallButton("Buy##Buy" + rowIndex))
                //     {
                //         var vendor = item.Item.Vendors.FirstOrDefault(c => c.ENpcs.Any());
                //         if (vendor != null)
                //         {
                //             var shopListing = vendor.ENpcs.First();
                //             if (shopListing.Locations.Any())
                //             {
                //                 var location = shopListing.Locations.First();
                //                 location.TeleportToNearestAetheryte();
                //             }
                //         }
                //     }
                //     
                // }
                if (item.Item.ObtainedGathering)
                {
                    hasButton = true;
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gather " + item.Name);
                    }
                }
                else if(item.Item.ObtainedFishing)
                {
                    hasButton = true;
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gatherfish " + item.Name);
                    }
                }
                if (item.UpTime != null)
                {
                    if (hasButton)
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