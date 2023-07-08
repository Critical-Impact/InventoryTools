using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Extensions
{
    public static class ItemRightClickExtension
    {
        public static void DrawRightClickPopup(this ItemEx item)
        {
            DrawMenuItems(item);
            bool firstItem = true;
            
            var craftFilters =
                PluginService.FilterService.FiltersList.Where(c =>
                    c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault).ToArray();
            foreach (var filter in craftFilters)
            {
                if (item.CanBeCrafted && !Service.ExcelCache.IsCompanyCraft(item.RowId))
                {
                    if (firstItem)
                    {
                        ImGui.Separator();
                        firstItem = false;
                    }
                    if (ImGui.Selectable("Add to craft list - " + filter.Name))
                    {
                        filter.CraftList.AddCraftItem(item.RowId);
                        PluginService.WindowService.OpenCraftsWindow();
                        PluginService.WindowService.GetCraftsWindow().FocusFilter(filter);
                        filter.NeedsRefresh = true;
                        filter.StartRefresh();
                    }
                }
                if (item.CanBeCrafted && Service.ExcelCache.IsCompanyCraft(item.RowId))
                {
                    if (item.CompanyCraftSequenceEx != null)
                    {
                        for (var index = 0u; index < item.CompanyCraftSequenceEx.CompanyCraftPart.Length; index++)
                        {
                            var part = item.CompanyCraftSequenceEx.CompanyCraftPart[index];
                            if (part.Row == 0) continue;
                            if (firstItem)
                            {
                                ImGui.Separator();
                                firstItem = false;
                            }

                            if (ImGui.Selectable("Add " + (part.Value?.CompanyCraftType.Value?.Name ?? "Unknown") + " to craft list - " + filter.Name))
                            {
                                filter.CraftList.AddCraftItem(item.RowId, 1, InventoryItem.ItemFlags.None, index);
                                PluginService.WindowService.OpenCraftsWindow();
                                PluginService.WindowService.GetCraftsWindow().FocusFilter(filter);
                                filter.NeedsRefresh = true;
                                filter.StartRefresh();
                            }
                        }
                    }
                    ImGui.Separator();
                }
            }

            if (item.CanBeCrafted && !Service.ExcelCache.IsCompanyCraft(item.RowId))
            {
                if (ImGui.Selectable("Add to new craft list"))
                {
                    PluginService.FrameworkService.RunOnTick(() =>
                    {
                        var filter = PluginService.FilterService.AddNewCraftFilter();
                        filter.CraftList.AddCraftItem(item.RowId);
                        PluginService.WindowService.OpenCraftsWindow();
                        PluginService.WindowService.GetCraftsWindow().FocusFilter(filter);
                        filter.NeedsRefresh = true;
                        filter.StartRefresh();
                    });
                }
            }

            if (item.CanBeCrafted && Service.ExcelCache.IsCompanyCraft(item.RowId))
            {
                if (item.CompanyCraftSequenceEx != null)
                {
                    for (var index = 0u; index < item.CompanyCraftSequenceEx.CompanyCraftPart.Length; index++)
                    {
                        var part = item.CompanyCraftSequenceEx.CompanyCraftPart[index];
                        if (part.Row == 0) continue;
                        if (ImGui.Selectable("Add " + (part.Value?.CompanyCraftType.Value?.Name ?? "Unknown") + " to new craft list"))
                        {
                            var newPhase = index;
                            PluginService.FrameworkService.RunOnTick(() =>
                            {
                                var filter = PluginService.FilterService.AddNewCraftFilter();
                                filter.CraftList.AddCraftItem(item.RowId,1, InventoryItem.ItemFlags.None, newPhase);
                                PluginService.WindowService.OpenCraftsWindow();
                                PluginService.WindowService.GetCraftsWindow().FocusFilter(filter);
                                filter.NeedsRefresh = true;
                                filter.StartRefresh();
                            });
                        }
                    }
                }
            }
        }
        public static void DrawRightClickPopup(this CraftItem item, FilterConfiguration configuration)
        {
            DrawMenuItems(item.Item);
            bool firstItem = true;
            if (item.IsOutputItem)
            {
                if (firstItem)
                {
                    ImGui.Separator();
                    firstItem = false;
                }
                if (ImGui.Selectable("Remove from craft list"))
                {
                    configuration.CraftList.RemoveCraftItem(item.ItemId, item.Flags);
                    configuration.NeedsRefresh = true;
                    configuration.StartRefresh();
                }
            }

            if (item.Item.CanBeCrafted && item.IsOutputItem && Service.ExcelCache.IsCompanyCraft(item.ItemId))
            {
                if (item.Item.CompanyCraftSequenceEx != null)
                {
                    if (item.Phase != null && ImGui.Selectable("Switch to All Phases"))
                    {
                        configuration.CraftList.SetCraftPhase(item.ItemId, null);
                        configuration.StartRefresh();
                    }
                    for (var index = 0u; index < item.Item.CompanyCraftSequenceEx.CompanyCraftPart.Length; index++)
                    {
                        var part = item.Item.CompanyCraftSequenceEx.CompanyCraftPart[index];
                        if (part.Row == 0) continue;
                        if (item.Phase != index)
                        {
                            if (firstItem)
                            {
                                ImGui.Separator();
                                firstItem = false;
                            }
                            if (ImGui.Selectable("Switch to " + ((part.Value?.CompanyCraftType.Value?.Name ?? "") + " (Phase " + (index + 1) + ")")))
                            {
                                configuration.CraftList.SetCraftPhase(item.ItemId, index);
                                configuration.StartRefresh();
                            }
                        }
                    }
                }
            }

            if (!item.IsOutputItem)
            {
                var craftFilters =
                    PluginService.FilterService.FiltersList.Where(c =>
                        c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault);
                if (item.Item.CanBeCrafted && !Service.ExcelCache.IsCompanyCraft(item.Item.RowId))
                {
                    foreach (var filter in craftFilters)
                    {
                        if (firstItem)
                        {
                            ImGui.Separator();
                            firstItem = false;
                        }

                        if (ImGui.Selectable("Add " + item.QuantityNeeded + " item to craft list - " + filter.Name))
                        {
                            filter.CraftList.AddCraftItem(item.Item.RowId, item.QuantityNeeded,
                                InventoryItem.ItemFlags.None);
                            PluginService.WindowService.OpenCraftsWindow();
                            PluginService.WindowService.GetCraftsWindow().FocusFilter(filter);
                            configuration.NeedsRefresh = true;
                            configuration.StartRefresh();
                        }
                    }
                    if (ImGui.Selectable("Add " + item.QuantityNeeded + " item to new craft list"))
                    {
                        PluginService.FrameworkService.RunOnTick(() =>
                        {
                            var filter = PluginService.FilterService.AddNewCraftFilter();
                            filter.CraftList.AddCraftItem(item.Item.RowId, item.QuantityNeeded,
                                InventoryItem.ItemFlags.None);
                            PluginService.WindowService.OpenCraftsWindow();
                            PluginService.WindowService.GetCraftsWindow().FocusFilter(filter);
                            configuration.NeedsRefresh = true;
                            configuration.StartRefresh();
                        });
                    }
                }
            }

        }
        
        public static void DrawMenuItems(ItemEx item)
        {
            ImGui.Text(item.NameString);
            ImGui.Separator();
            if (ImGui.Selectable("Open in Garland Tools"))
            {
                $"https://www.garlandtools.org/db/#item/{item.GarlandToolsId}".OpenBrowser();
            }
            if (ImGui.Selectable("Open in Teamcraft"))
            {
                $"https://ffxivteamcraft.com/db/en/item/{item.RowId}".OpenBrowser();
            }
            if (ImGui.Selectable("Open in Universalis"))
            {
                $"https://universalis.app/market/{item.RowId}".OpenBrowser();
            }
            if (ImGui.Selectable("Copy Name"))
            {
                item.NameString.ToClipboard();
            }
            if (ImGui.Selectable("Link"))
            {
                PluginService.ChatUtilities.LinkItem(item);
            }
            if (item.CanTryOn && ImGui.Selectable("Try On"))
            {
                if (PluginService.TryOn.CanUseTryOn)
                {
                    PluginService.TryOn.TryOnItem(item);
                }
            }

            if (item.CanOpenCraftLog && ImGui.Selectable("Open Crafting Log"))
            {
                PluginService.GameInterface.OpenCraftingLog(item.RowId);
            }

            if (item.CanOpenGatheringLog && ImGui.Selectable("Open Gathering Log"))
            {
                PluginService.GameInterface.OpenGatheringLog(item.RowId);
            }

            if (item.ObtainedFishing && ImGui.Selectable("Open Fishing Log"))
            {
                PluginService.GameInterface.OpenFishingLog(item.RowId, item.IsSpearfishingItem());
            }

            if (item.CanOpenGatheringLog && ImGui.Selectable("Gather with Gatherbuddy"))
            {
                PluginService.CommandService.ProcessCommand("/gather " + item.NameString);
            }

            if (item.ObtainedFishing && ImGui.Selectable("Gather with Gatherbuddy"))
            {
                PluginService.CommandService.ProcessCommand("/gatherfish " + item.NameString);
            }

            if (ImGui.Selectable("More Information"))
            {
                PluginService.WindowService.OpenItemWindow(item.RowId);   
            }
            
        }
    }
}