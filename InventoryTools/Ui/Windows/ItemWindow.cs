using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using OtterGui;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Ui
{
    class ItemWindow : Window
    {
        public override bool SaveState => false;
        public static string AsKey(uint itemId)
        {
            return "item_" + itemId;
        }
        private uint _itemId;
        private ItemEx? Item => Service.ExcelCache.GetItemExSheet().GetRow(_itemId);
        private CraftItem? _craftItem = null;
        public ItemWindow(uint itemId, string name = "Allagan Tools - Invalid Item") : base(name)
        {
            _itemId = itemId;
            if (Item != null)
            {
                WindowName = "Allagan Tools - " + Item.Name;
                RetainerTasks = Item.RetainerTasks.ToArray();
                RecipesResult = Item.RecipesAsResult.ToArray();
                RecipesAsRequirement = Item.RecipesAsRequirement.ToArray();
                Vendors = Item.Vendors.SelectMany(shop => shop.ENpcs.SelectMany(npc => npc.Locations.Select(location => (shop, npc, location)))).ToList();
                GatheringSources = Item.GetGatheringSources().ToList();
                SharedModels = Item.GetSharedModels();
            }
            else
            {
                RetainerTasks = Array.Empty<RetainerTaskNormalEx>();
                RecipesResult = Array.Empty<RecipeEx>();
                RecipesAsRequirement = Array.Empty<RecipeEx>();
                GatheringSources = new();
                Vendors = new();
                SharedModels = new();
            }
        }

        public List<ItemEx> SharedModels { get; }

        private List<GatheringSource> GatheringSources { get; }

        private List<(IShop shop, ENpc npc, ILocation location)> Vendors { get; }

        private RecipeEx[] RecipesAsRequirement { get;  }

        private RecipeEx[] RecipesResult { get; }

        private RetainerTaskNormalEx[] RetainerTasks { get; }

        public override string Key => AsKey(_itemId);
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (Item == null)
            {
                ImGui.Text("Item with the ID " + _itemId + " could not be found.");   
            }
            else
            {
                ImGui.Text("Item Level " + Item.LevelItem.Row.ToString());
                ImGui.TextWrapped(Item.Description.ToDalamudString().ToString());
                if (Item.CanBeAcquired)
                {
                    ImGui.Text("Acquired:" + (PluginService.GameInterface.HasAcquired(Item) ? "Yes" : "No"));
                }
                var itemIcon = PluginService.IconStorage[Item.Icon];
                if (itemIcon != null)
                {
                    ImGui.Image(itemIcon.ImGuiHandle, new Vector2(100, 100) * ImGui.GetIO().FontGlobalScale);
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right)) 
                    {
                        ImGui.OpenPopup("RightClick" + _itemId);
                    }
                    
                    if (ImGui.BeginPopup("RightClick" + _itemId))
                    {
                        Item.DrawRightClickPopup();
                        ImGui.EndPopup();
                    }
                }
                
                var garlandIcon = PluginService.IconStorage[65090];
                if (ImGui.ImageButton(garlandIcon.ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://www.garlandtools.org/db/#item/{_itemId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Garland Tools");
                ImGui.SameLine();
                var tcIcon = PluginService.IconStorage[60046];
                if (ImGui.ImageButton(tcIcon.ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://ffxivteamcraft.com/db/en/item/{_itemId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Teamcraft");
                if (Item.CanOpenCraftLog)
                {
                    ImGui.SameLine();
                    var craftableIcon = PluginService.IconStorage[66456];
                    if (ImGui.ImageButton(craftableIcon.ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        PluginService.GameInterface.OpenCraftingLog(_itemId);
                    }

                    ImGuiUtil.HoverTooltip("Craftable - Open in Craft Log");
                }
                if (Item.CanBeCrafted)
                {
                    ImGui.SameLine();
                    var craftableIcon = PluginService.IconStorage[60858];
                    if (ImGui.ImageButton(craftableIcon.ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        ImGui.OpenPopup("AddCraftList" + _itemId);
                    }
                    
                    if (ImGui.BeginPopup("AddCraftList" + _itemId))
                    {
                        var craftFilters =
                            PluginService.FilterService.FiltersList.Where(c =>
                                c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault);
                        foreach (var filter in craftFilters)
                        {
                            if (ImGui.Selectable("Add item to craft list - " + filter.Name))
                            {
                                PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                {
                                    filter.CraftList.AddCraftItem(_itemId, 1, InventoryItem.ItemFlags.None);
                                    PluginService.WindowService.OpenCraftsWindow();
                                    PluginService.WindowService.GetCraftsWindow().FocusFilter(filter);
                                });
                            }
                        }
                        ImGui.EndPopup();
                    }

                    ImGuiUtil.HoverTooltip("Craftable - Add to Craft List");
                }
                if (Item.CanOpenGatheringLog)
                {
                    ImGui.SameLine();
                    var gatherableIcon = PluginService.IconStorage[66457];
                    if (ImGui.ImageButton(gatherableIcon.ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        PluginService.GameInterface.OpenGatheringLog(_itemId);
                    }

                    ImGuiUtil.HoverTooltip("Gatherable - Open in Gathering Log");
                    
                    ImGui.SameLine();
                    var gbIcon = PluginService.IconStorage[63900];
                    if (ImGui.ImageButton(gbIcon.ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        PluginService.CommandService.ProcessCommand("/gather " + Item.NameString);
                    }

                    ImGuiUtil.HoverTooltip("Gatherable - Gather with Gatherbuddy");
                }

                ImGui.Separator();
                if (ImGui.CollapsingHeader("Sources (" + Item.Sources.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var sources = Item.Sources;
                    for (var index = 0; index < sources.Count; index++)
                    {
                        ImGui.PushID("Source"+index);
                        var source = sources[index];
                        var sourceIcon = PluginService.IconStorage[source.Icon];
                        if (sourceIcon != null)
                        {
                            if (source.CanOpen)
                            {
                                if (source is ItemSource itemSource && itemSource.ItemId != null)
                                {
                                    if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1),
                                            0))
                                    {
                                        PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                        {
                                            PluginService.WindowService.OpenItemWindow(itemSource.ItemId.Value);
                                        });
                                    }
                                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                            ImGuiHoveredFlags.AllowWhenOverlapped &
                                                            ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                            ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                            ImGuiHoveredFlags.AnyWindow) &&
                                        ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                    {
                                        ImGui.OpenPopup("RightClickSource" + itemSource.ItemId);
                                    }
                                    if (ImGui.BeginPopup("RightClickSource" + itemSource.ItemId))
                                    {
                                        ImGui.OpenPopup("RightClickSource" + itemSource.ItemId);
                                        var itemEx = Service.ExcelCache.GetItemExSheet()
                                            .GetRow(itemSource.ItemId.Value);
                                        if (itemEx != null)
                                        {
                                            itemEx.DrawRightClickPopup();
                                        }
                                        ImGui.EndPopup();
                                    }
                                }
                                else if (source is DutySource dutySource)
                                {
                                    if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1),
                                            0))
                                    {

                                        PluginService.WindowService.OpenDutyWindow(dutySource.ContentFinderConditionId);

                                    }
                                }
                                else
                                {
                                    ImGui.Image(sourceIcon.ImGuiHandle,
                                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale);
                                }
                            }
                            else
                            {
                                ImGui.Image(sourceIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale);
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(source.FormattedName);
                            if (index + 1 < sources.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }
                        }

                        ImGui.PopID();
                    }
                }
                if (ImGui.CollapsingHeader("Uses/Rewards (" + Item.Uses.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var uses = Item.Uses;
                    for (var index = 0; index < uses.Count; index++)
                    {
                        ImGui.PushID("Use"+index);
                        var use = uses[index];
                        var useIcon = PluginService.IconStorage[use.Icon];
                        if (useIcon != null)
                        {
                            if (use.ItemId != null)
                            {
                                if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                                {
                                    PluginService.WindowService.OpenItemWindow(use.ItemId.Value);
                                }
                                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right)) 
                                {
                                    ImGui.OpenPopup("RightClickUse" + use.ItemId);
                                }
                    
                                if (ImGui.BeginPopup("RightClickUse"+ use.ItemId))
                                {
                                    var itemEx = Service.ExcelCache.GetItemExSheet().GetRow(use.ItemId.Value);
                                    if (itemEx != null)
                                    {
                                        itemEx.DrawRightClickPopup();
                                    }

                                    ImGui.EndPopup();
                                }
                            }
                            else
                            {
                                ImGui.Image(useIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale);
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(use.FormattedName);
                            if (index + 1 < uses.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }
                        }

                        ImGui.PopID();
                    }
                }

                void DrawSupplierRow((IShop shop, ENpc npc, ILocation location) tuple)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text( tuple.npc.Resident?.Singular ?? "Unknown");
                    ImGui.TableNextColumn();
                    ImGui.Text(tuple.shop.Name);     
                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(tuple.location + " ( " + Math.Round(tuple.location.MapX,2) + "/" + Math.Round(tuple.location.MapY,2) + ")");
                    ImGui.TableNextColumn();
                    if (ImGui.Button("Open Map Link##" + tuple.shop.RowId + "_" + tuple.npc.Key + "_" + tuple.location.MapEx.Row))
                    {
                        PluginService.ChatUtilities.PrintFullMapLink(tuple.location, Item.NameString);
                    }


                }

                bool hasInformation = false;
                if (Vendors.Count != 0)
                {
                    hasInformation = true;
                    if (ImGui.CollapsingHeader("Shops (" + Vendors.Count + ")"))
                    {
                        ImGui.Text("Shops: ");
                        ImGuiTable.DrawTable("VendorsText", Vendors, DrawSupplierRow, ImGuiTableFlags.None,
                            new[] { "NPC", "Shop Name", "Location", "" });
                    }
                }
                if (RetainerTasks.Length != 0)
                {
                    hasInformation = true;
                    if (ImGui.CollapsingHeader("Ventures (" + RetainerTasks.Count() + ")"))
                    {
                        ImGuiTable.DrawTable("Ventures", RetainerTasks, DrawRetainerRow, ImGuiTableFlags.None,
                            new[] { "Name", "Time", "Quantities" });
                    }
                }
                if (GatheringSources.Count != 0)
                {
                    hasInformation = true;
                    if (ImGui.CollapsingHeader("Gathering (" + GatheringSources.Count + ")"))
                    {
                        ImGuiTable.DrawTable("Gathering", GatheringSources, DrawGatheringRow,
                            ImGuiTableFlags.None, new[] { "", "Level", "Location", "" });
                    }
                }
                if (RecipesAsRequirement.Length != 0)
                {
                    hasInformation = true;
                    if (ImGui.CollapsingHeader("Recipes (" + RecipesAsRequirement.Length + ")"))
                    {
                        ImGuiStylePtr style = ImGui.GetStyle();
                        float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                        for (var index = 0; index < RecipesAsRequirement.Length; index++)
                        {
                            ImGui.PushID(index);
                            var recipe = RecipesAsRequirement[index];
                            if (recipe.ItemResultEx.Value != null)
                            {
                                var icon = PluginService.IconStorage.LoadIcon(recipe.ItemResultEx.Value.Icon);
                                if (ImGui.ImageButton(icon.ImGuiHandle,
                                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                                {
                                    PluginService.WindowService.OpenItemWindow(recipe.ItemResultEx.Row);
                                }
                                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right)) 
                                {
                                    ImGui.OpenPopup("RightClick" + recipe.RowId);
                                }
                    
                                if (ImGui.BeginPopup("RightClick"+ recipe.RowId))
                                {
                                    if (recipe.ItemResultEx.Value != null)
                                    {
                                        recipe.ItemResultEx.Value.DrawRightClickPopup();
                                    }

                                    ImGui.EndPopup();
                                }

                                float lastButtonX2 = ImGui.GetItemRectMax().X;
                                float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                                ImGuiUtil.HoverTooltip(recipe.ItemResultEx.Value!.NameString + " - " +
                                                       (recipe.CraftType.Value?.Name ?? "Unknown"));
                                if (index + 1 < RecipesAsRequirement.Length && nextButtonX2 < windowVisibleX2)
                                {
                                    ImGui.SameLine();
                                }
                            }

                            ImGui.PopID();
                        }
                    }
                }

                if (SharedModels.Count != 0)
                {
                    hasInformation = true;
                    if (ImGui.CollapsingHeader("Shared Models (" + SharedModels.Count + ")"))
                    {
                        ImGuiStylePtr style = ImGui.GetStyle();
                        float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                        for (var index = 0; index < SharedModels.Count; index++)
                        {
                            ImGui.PushID(index);
                            var sharedModel = SharedModels[index];
                            var icon = PluginService.IconStorage.LoadIcon(sharedModel.Icon);
                            if (ImGui.ImageButton(icon.ImGuiHandle, new(32, 32)))
                            {
                                PluginService.WindowService.OpenItemWindow(sharedModel.RowId);
                            }
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right)) 
                            {
                                ImGui.OpenPopup("RightClick" + sharedModel.RowId);
                            }
                
                            if (ImGui.BeginPopup("RightClick"+ sharedModel.RowId))
                            {
                                sharedModel.DrawRightClickPopup();
                                ImGui.EndPopup();
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(sharedModel.NameString);
                            if (index + 1 < SharedModels.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }

                            ImGui.PopID();
                        }
                    }
                }

                if (Item.IsCompanyCraft)
                {
                    hasInformation = true;
                    if (_craftItem == null)
                    {
                        _craftItem = new CraftItem(Item.RowId, InventoryItem.ItemFlags.None, 1, true);
                        _craftItem.GenerateRequiredMaterials();
                    }
                    if (ImGui.CollapsingHeader("Company Craft Recipe (" + _craftItem.ChildCrafts.Count + ")"))
                    {
                        ImGuiStylePtr style = ImGui.GetStyle();
                        float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                        var index = 0;
                        foreach(var craftItem in _craftItem.ChildCrafts)
                        {
                            var item = Service.ExcelCache.GetItemExSheet().GetRow(craftItem.ItemId);
                            ImGui.PushID(index);
                            var icon = PluginService.IconStorage.LoadIcon(item.Icon);
                            if (ImGui.ImageButton(icon.ImGuiHandle, new(32, 32)))
                            {
                                PluginService.WindowService.OpenItemWindow(item.RowId);
                            }
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right)) 
                            {
                                ImGui.OpenPopup("RightClick" + item.RowId);
                            }
                
                            if (ImGui.BeginPopup("RightClick"+ item.RowId))
                            {
                                item.DrawRightClickPopup();
                                ImGui.EndPopup();
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(item.NameString + " - " + craftItem.QuantityRequired);
                            if (index + 1 < _craftItem.ChildCrafts.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }

                            ImGui.PopID();
                            index++;
                        }
                    }
                }
                if (!hasInformation)
                {
                    ImGui.Text("No information available.");
                }
                
                #if DEBUG
                if (ImGui.CollapsingHeader("Debug"))
                {
                    ImGui.Text("Item ID: " + _itemId);
                    Utils.PrintOutObject(Item, 0, new List<string>());
                }
                #endif

            }
        }

        private void DrawGatheringRow(GatheringSource obj)
        {
            ImGui.TableNextColumn();
            ImGui.PushID(obj.GetHashCode());
            var source = obj.Source;
            var icon = PluginService.IconStorage[source.Icon];
            if (ImGui.ImageButton(icon.ImGuiHandle, new(32, 32)))
            {
                PluginService.GameInterface.OpenGatheringLog(_itemId);
            }
            ImGuiUtil.HoverTooltip(source.Name + " - Open in Gathering Log");
            ImGui.TableNextColumn();
            ImGui.Text(obj.Level.GatheringItemLevel.ToString());     
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.PlaceName.Name + " - " + (obj.TerritoryType.PlaceName.Value?.Name ?? "Unknown"));
            ImGui.PopID();
        }

        private void DrawRecipeResultRow(RecipeEx obj)
        {

        }

        private void DrawRetainerRow(RetainerTaskNormalEx obj)
        {
            ImGui.TableNextColumn();
            ImGui.Text( obj.TaskName);
            ImGui.TableNextColumn();
            ImGui.Text(obj.TaskTime + " minutes");     
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.Quantities);
        }

        public override void Invalidate()
        {
            
        }
        public override FilterConfiguration? SelectedConfiguration => null;
        public override Vector2 DefaultSize { get; } = new Vector2(500, 800);
        public override Vector2 MaxSize => new (800, 1500);
        public override Vector2 MinSize => new (100, 100);
    }
}