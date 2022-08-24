using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
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
                    c.FilterType == Logic.FilterType.CraftFilter).ToArray();
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
                    //TODO: replace with a dynamic way of determining number of steps
                    for (uint i = 0; i < 3; i++)
                    {
                        if (firstItem)
                        {
                            ImGui.Separator();
                            firstItem = false;
                        }
                        if (ImGui.Selectable("Add phase " + i + " to craft list - " + filter.Name))
                        {
                            filter.CraftList.AddCraftItem(item.RowId, 1, InventoryItem.ItemFlags.None, i);
                            PluginService.WindowService.OpenCraftsWindow();
                            PluginService.WindowService.GetCraftsWindow().FocusFilter(filter);
                            filter.NeedsRefresh = true;
                            filter.StartRefresh();
                        }
                    }
                }
            }
        }
        public static void DrawRightClickPopup(this CraftItem item, FilterConfiguration configuration)
        {
            DrawMenuItems(item.Item);
            bool firstItem = true;
            if (item.Item.CanOpenCraftLog)
            {
                if (firstItem)
                {
                    ImGui.Separator();
                    firstItem = false;
                }
                if (ImGui.Selectable("Open in Crafting Log"))
                {
                    GameInterface.OpenCraftingLog(item.ItemId, item.RecipeId);
                }
            }

            if (item.Item.CanBeCrafted && item.IsOutputItem)
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

            if (item.Item.CanBeCrafted && item.IsOutputItem && item.Phase != null && Service.ExcelCache.IsCompanyCraft(item.ItemId))
            {
                for (uint i = 0; i < 3; i++)
                {
                    if (item.Phase != i)
                    {
                        if (firstItem)
                        {
                            ImGui.Separator();
                            firstItem = false;
                        }
                        if (ImGui.Selectable("Switch to Phase " + (i + 1)))
                        {
                            item.SwitchPhase(i);
                            configuration.StartRefresh();
                        }
                    }
                }
            }

            if (!item.IsOutputItem)
            {
                var craftFilters =
                    PluginService.FilterService.FiltersList.Where(c =>
                        c.FilterType == Logic.FilterType.CraftFilter);
                foreach (var filter in craftFilters)
                {
                    if (item.Item.CanBeCrafted && !Service.ExcelCache.IsCompanyCraft(item.Item.RowId))
                    {
                        //TODO: replace with a dynamic way of determining number of steps
                        if (firstItem)
                        {
                            ImGui.Separator();
                            firstItem = false;
                        }
                        if (ImGui.Selectable("Add " + item.QuantityNeeded + " item to craft list - " + filter.Name))
                        {
                            filter.CraftList.AddCraftItem(item.Item.RowId, item.QuantityNeeded, InventoryItem.ItemFlags.None);
                            PluginService.WindowService.OpenCraftsWindow();
                            PluginService.WindowService.GetCraftsWindow().FocusFilter(filter);
                            configuration.NeedsRefresh = true;
                            configuration.StartRefresh();
                        }
                    }
                }
            }

        }
        
        public static void DrawMenuItems(ItemEx item)
        {
            ImGui.Text(item.Name);
            ImGui.Separator();
            if (ImGui.Selectable("Open in Garland Tools"))
            {
                $"https://www.garlandtools.org/db/#item/{item.RowId}".OpenBrowser();
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
                item.Name.ToDalamudString().ToString().ToClipboard();
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
                GameInterface.OpenCraftingLog(item.RowId);
            }

            if (item.CanOpenGatheringLog && ImGui.Selectable("Open Gathering Log"))
            {
                GameInterface.OpenGatheringLog(item.RowId);
            }

            if (item.CanOpenGatheringLog && ImGui.Selectable("Gather with Gatherbuddy"))
            {
                Service.Commands.ProcessCommand("/gather " + item.Name);
            }

            if (ImGui.Selectable("More Information"))
            {
                PluginService.WindowService.OpenItemWindow(item.RowId);   
            }
            
        }
    }
}