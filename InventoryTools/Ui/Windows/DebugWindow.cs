#if DEBUG
using System;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Agents;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.Sheets;
using CriticalCommonLib.UiModule;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryType = CriticalCommonLib.Enums.InventoryType;

namespace InventoryTools.Ui
{
    public class DebugWindow : Window
    {
        public override bool SaveState => true;

        public static string AsKey => "debug";
        public override string Name { get; } = "Inventory Tools - Debug";
        public override string Key => AsKey;
        public override Vector2 Size { get; } = new(700, 700);
        public override Vector2 MaxSize { get; } = new(2000, 2000);
        public override Vector2 MinSize { get; } = new(200, 200);
        public override bool DestroyOnClose => false;
        public override unsafe void Draw()
        {
            if (ImGui.BeginChild("###ivDebugList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (ImGui.Selectable("Retainers", ConfigurationManager.Config.SelectedDebugPage == 0))
                {
                    ConfigurationManager.Config.SelectedDebugPage = 0;
                }
                if (ImGui.Selectable("Inventories", ConfigurationManager.Config.SelectedDebugPage == 1))
                {
                    ConfigurationManager.Config.SelectedDebugPage = 1;
                }
                if (ImGui.Selectable("Stuff", ConfigurationManager.Config.SelectedDebugPage == 2))
                {
                    ConfigurationManager.Config.SelectedDebugPage = 2;
                }
                if (ImGui.Selectable("Retainer Debugger", ConfigurationManager.Config.SelectedDebugPage == 3))
                {
                    ConfigurationManager.Config.SelectedDebugPage = 3;
                }
                if (ImGui.Selectable("Universalis", ConfigurationManager.Config.SelectedDebugPage == 4))
                {
                    ConfigurationManager.Config.SelectedDebugPage = 4;
                }
                if (ImGui.Selectable("Crafting", ConfigurationManager.Config.SelectedDebugPage == 5))
                {
                    ConfigurationManager.Config.SelectedDebugPage = 5;
                }
                if (ImGui.Selectable("Fun Times", ConfigurationManager.Config.SelectedDebugPage == 6))
                {
                    ConfigurationManager.Config.SelectedDebugPage = 6;
                }
                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("###ivDebugView", new Vector2(-1, -1), true))
            {
                if (ConfigurationManager.Config.SelectedDebugPage == 0)
                {
                    ImGui.Text("Character Information:");
                    ImGui.Text(Service.ClientState.LocalPlayer?.Name.ToString() ?? "Not Logged in Yet");
                    ImGui.Text("Actual:" + Service.ClientState.LocalContentId.ToString());
                    ImGui.Text("Reported:" + PluginService.CharacterMonitor.ActiveCharacter.ToString());
                    ImGui.Text("Retainers:");
                    ImGui.BeginTable("retainerTable", 5);
                    ImGui.TableSetupColumn("Hire Order");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Gil");
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("Owner ID");
                    ImGui.TableHeadersRow();
                    var retainers = PluginService.CharacterMonitor.Characters;
                    foreach (var retainer in retainers)
                    {
                        if (retainer.Value.Name != "Unhired")
                        {
                            ImGui.TableNextColumn();
                            ImGui.Text((retainer.Value.HireOrder + 1).ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(retainer.Value.Name);
                            ImGui.TableNextColumn();
                            ImGui.Text(retainer.Value.Gil.ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(retainer.Value.CharacterId.ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(retainer.Value.OwnerId.ToString());
                        }
                    }
                    ImGui.EndTable();
                }
                else if (ConfigurationManager.Config.SelectedDebugPage == 1)
                {
                    ImGui.Text("Inventory Information:");
                    ImGui.BeginTable("retainerTable", 6);
                    ImGui.TableSetupColumn("Inventory ID");
                    ImGui.TableSetupColumn("Category");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Sorted Slot Index");
                    ImGui.TableSetupColumn("Item ID");
                    ImGui.TableSetupColumn("Unsorted Slot ID");
                    ImGui.TableHeadersRow();
                    var inventories = PluginService.InventoryMonitor.Inventories;
                    foreach (var inventory in inventories)
                    {
                        foreach (var itemSet in inventory.Value)
                        {
                            foreach (var item in itemSet.Value)
                            {
                                ImGui.TableNextColumn();
                                ImGui.Text((inventory.Key).ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(itemSet.Key.ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(item.FormattedName);
                                ImGui.TableNextColumn();
                                ImGui.Text(item.SortedSlotIndex.ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(item.ItemId.ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(item.Slot.ToString());
                            }
                        }

                    }
                    ImGui.EndTable();
                    ImGui.Text("Inventories Seen via Network Traffic");
                    foreach (var inventory in PluginService.InventoryMonitor.LoadedInventories)
                    {
                        ImGui.Text(inventory.Key.ToString());
                    }
                }
                else if (ConfigurationManager.Config.SelectedDebugPage == 2)
                {
                    if (ImGui.Button("Check sort ordering"))
                    {
                        PluginLog.Log($"item order module : {(ulong)ItemOrderModule.Instance()->PlayerInventory:X}", $"{(ulong)ItemOrderModule.Instance()->PlayerInventory:X}");
                        PluginLog.Log(ItemOrderModule.Instance()->PlayerInventory->containerId.ToString());
                        PluginLog.Log(ItemOrderModule.Instance()->PlayerInventory->SlotPerContainer.ToString());
                        PluginLog.Log(ItemOrderModule.Instance()->PlayerInventory->Unk04.ToString());
                    }
                    ImGui.Text("Inventory Information:");
                    if (ImGui.Button("Try multi request"))
                    {
                        Universalis.RetrieveMarketBoardPrice(27757);
                        Universalis.RetrieveMarketBoardPrice(12594);
                        Universalis.RetrieveMarketBoardPrice(19984);
                    }

                    if (ImGui.Button("Item order module"))
                    {
                        var clientInterfaceUiModule = (ItemOrderModule*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework
                            .Instance()->UIModule->GetItemOrderModule();
                        var module = clientInterfaceUiModule;
                        if (module != null)
                        {
                            PluginLog.Log($"item order module : {(ulong)module:X}", $"{(ulong)module:X}");
                        }
                    }

                    if (ImGui.Button("Check inventory manager pointer"))
                    {
                        var instance = InventoryManager.Instance();
                        if (instance != null)
                        {
                            PluginLog.Log($"Manager pointer: {(ulong)instance:X}", $"{(ulong)instance:X}");
                        }
                    }
                    if (ImGui.Button("Check inventory item pointer"))
                    {
                        var instance = InventoryManager.Instance();
                        if (instance != null)
                        {
                            var inv = instance->GetInventoryContainer(FFXIVClientStructs.FFXIV.Client.Game.InventoryType
                                .Inventory1);
                            var inventoryItem = (IntPtr)inv->GetInventorySlot(32);
                            PluginLog.Log($"first item pointer: {(ulong)inventoryItem:X}", $"{(ulong)inventoryItem:X}");
                            PluginLog.Log($"first item pointer qty: {(ulong)inventoryItem + 12:X}", $"{(ulong)inventoryItem + 12:X}");
                        }
                    }
                    if (ImGui.Button("Check retainer pointer"))
                    {
                        var retainerBag0 = GameInterface.GetContainer(InventoryType.RetainerBag0);
                        if (retainerBag0 != null)
                        {
                            PluginLog.Log($"Retainer Bag 0 Pointer: {(ulong)retainerBag0:X}", $"{(ulong)retainerBag0:X}");
                            if (retainerBag0->Loaded == 0)
                            {
                                PluginLog.Log("Retainer bag not loaded");
                            }
                            else
                            {
                                var slot1 = &retainerBag0->Items[0];
                                if (slot1 != null)
                                {
                                    PluginLog.Log($"Retainer Bag 0 Slot 0 Pointer: {(ulong)slot1:X}", $"{(ulong)slot1:X}");
                                }
                                else
                                {
                                    PluginLog.Log("slot 1 is 0");
                                }
                            }
                            
                        }
                        else
                        {
                            PluginLog.Log("Bag not found");
                        }
                    }
                    if (ImGui.Button("Check armoury agent"))
                    {
                        
                        var agent = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework
                            .Instance()->UIModule->GetAgentModule()->GetAgentByInternalId(AgentId.ArmouryBoard);
                        if (agent->IsAgentActive())
                        {
                            var armouryAgent = (ArmouryBoard*) agent;
                            PluginLog.Log(armouryAgent->SelectedTab.ToString());
                        }

                        var inventoryLarge = PluginService.GameUi.GetWindow("InventoryLarge");
                        if (inventoryLarge != null)
                        {
                            var inventoryAddon = (InventoryLargeAddon*) inventoryLarge;
                            PluginLog.Log(inventoryAddon->CurrentTab.ToString());

                        }
                    }
                    if (ImGui.Button("Check current company craft"))
                    {
                        var subMarinePartsMenu = PluginService.GameUi.GetWindow("SubmarinePartsMenu");
                        if (subMarinePartsMenu != null)
                        {
                            var subAddon = (SubmarinePartsMenuAddon*) subMarinePartsMenu;
                            PluginLog.Log("Current Phase: " + subAddon->Phase.ToString());
                            PluginLog.Log("Item 1: " + subAddon->AmountHandedIn(0).ToString());
                            PluginLog.Log("Item 2: " + subAddon->AmountHandedIn(1).ToString());
                            PluginLog.Log("Item 3: " + subAddon->AmountHandedIn(2).ToString());
                            PluginLog.Log("Item 4: " + subAddon->AmountHandedIn(3).ToString());
                            PluginLog.Log("Item 5: " + subAddon->AmountHandedIn(4).ToString());
                            PluginLog.Log("Item 6: " + subAddon->AmountHandedIn(5).ToString());
                            PluginLog.Log("Item 1: " + subAddon->AmountNeeded(0).ToString());
                            PluginLog.Log("Item 2: " + subAddon->AmountNeeded(1).ToString());
                            PluginLog.Log("Item 3: " + subAddon->AmountNeeded(2).ToString());
                            PluginLog.Log("Item 4: " + subAddon->AmountNeeded(3).ToString());
                            PluginLog.Log("Item 5: " + subAddon->AmountNeeded(4).ToString());
                            PluginLog.Log("Item 6: " + subAddon->AmountNeeded(5).ToString());
                            PluginLog.Log("Crafting: " + subAddon->ResultItemId.ToString());
                            PluginLog.Log("Item Required: " + subAddon->RequiredItemId(0).ToString());
                            PluginLog.Log("Item Required: " + subAddon->RequiredItemId(1).ToString());
                            PluginLog.Log("Item Required: " + subAddon->RequiredItemId(2).ToString());
                            PluginLog.Log("Item Required: " + subAddon->RequiredItemId(3).ToString());
                            PluginLog.Log("Item Required: " + subAddon->RequiredItemId(4).ToString());
                            PluginLog.Log("Item Required: " + subAddon->RequiredItemId(5).ToString());

                        }
                    }
                    if (ImGui.Button("Check select string"))
                    {
                        
                        var inventoryLarge = PluginService.GameUi.GetWindow("SelectString");
                        if (inventoryLarge != null)
                        {
                            var inventoryAddon = (AddonSelectString*) inventoryLarge;
                            PluginLog.Log(inventoryAddon->PopupMenu.PopupMenu.EntryCount.ToString());
                            for (int i = 0; i < inventoryAddon->PopupMenu.PopupMenu.EntryCount; i++)
                            {
                                var popupMenuEntryName = inventoryAddon->PopupMenu.PopupMenu.EntryNames[i];
                                PluginLog.Log(popupMenuEntryName->ToString());
                            }

                        }
                    }
                    if (ImGui.Button("Check free company tab"))
                    {
                        
                        var inventoryLarge = PluginService.GameUi.GetWindow(WindowName.FreeCompanyChest.ToString());
                        if (inventoryLarge != null)
                        {
                            var inventoryAddon = (InventoryFreeCompanyChestAddon*) inventoryLarge;
                            PluginLog.Log(inventoryAddon->CurrentTab.ToString());

                        }
                    }

                    if (ImGui.Button("Check prism box"))
                    {
                        var prismBox = new AtkInventoryMiragePrismBox();
                        PluginLog.Log(prismBox.CurrentPage.ToString());
                        PluginLog.Log(prismBox.CurrentTab.ToString());
                        PluginLog.Log(prismBox.ClassJobSelected.ToString());
                        PluginLog.Log(prismBox.OnlyDisplayRaceGenderItems.ToString());
                    }
                }
                else if (ConfigurationManager.Config.SelectedDebugPage == 3)
                {
                    unsafe
                    {
                        var clientInterfaceUiModule = (ItemOrderModule*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework
                            .Instance()->UIModule->GetItemOrderModule();
                        if (clientInterfaceUiModule != null)
                        {
                            ImGui.Text(clientInterfaceUiModule->RetainerID.ToString());
                            ImGui.Text($"Retainer Pointer: {(ulong)clientInterfaceUiModule->RetainerPtr:X}");
                            var container = GameInterface.GetContainer(InventoryType.RetainerBag0);
                            if (container != null)
                            {
                                ImGui.Text(container->Loaded.ToString());
                                for (int i = 0; i < container->SlotCount; i++)
                                {
                                    var item = container->Items[i];
                                    var itemPointer = new IntPtr(&item);
                                    ImGui.Text(item.ItemId.ToString());
                                    ImGui.Text(itemPointer.ToString());
                                }
                            }
                        }
                        else
                        {
                            ImGui.Text("Module not loaded");
                        }
                    }
                }
                else if (ConfigurationManager.Config.SelectedDebugPage == 4)
                {
                    ImGui.Text("Current Items in Queue: " + Universalis.QueuedCount);
                }
                else if (ConfigurationManager.Config.SelectedDebugPage == 5)
                {
                    var craftMonitorAgent = PluginService.CraftMonitor.Agent;
                    var simpleCraftMonitorAgent = PluginService.CraftMonitor.SimpleAgent;
                    if (craftMonitorAgent != null)
                    {
                        ImGui.Text("Progress: " + craftMonitorAgent.Progress);
                        ImGui.Text("Total Progress Required: " + PluginService.CraftMonitor.RecipeLevelTable?.ProgressRequired(PluginService.CraftMonitor.CurrentRecipe) ?? "Unknown");
                        ImGui.Text("Quality: " + craftMonitorAgent.Quality);
                        ImGui.Text("Status: " + craftMonitorAgent.Status);
                        ImGui.Text("Step: " + craftMonitorAgent.Step);
                        ImGui.Text("Durability: " + craftMonitorAgent.Durability);
                        ImGui.Text("HQ Chance: " + craftMonitorAgent.HqChance);
                        ImGui.Text("Item: " + (Service.ExcelCache.GetSheet<ItemEx>().GetRow(craftMonitorAgent.ResultItemId)?.Name ?? "Unknown"));
                        ImGui.Text("Current Recipe: " + PluginService.CraftMonitor.CurrentRecipe?.RowId ?? "Unknown");
                        ImGui.Text("Recipe Difficulty: " + PluginService.CraftMonitor.RecipeLevelTable?.Difficulty ?? "Unknown");
                        ImGui.Text("Recipe Difficulty Factor: " + PluginService.CraftMonitor.CurrentRecipe?.DifficultyFactor ?? "Unknown");
                        ImGui.Text("Recipe Durability: " + PluginService.CraftMonitor.RecipeLevelTable?.Durability ?? "Unknown");
                        ImGui.Text("Suggested Control: " + PluginService.CraftMonitor.RecipeLevelTable?.SuggestedControl ?? "Unknown");
                        ImGui.Text("Suggested Craftsmanship: " + PluginService.CraftMonitor.RecipeLevelTable?.SuggestedCraftsmanship ?? "Unknown");
                        ImGui.Text("Current Craft Type: " + PluginService.CraftMonitor.Agent?.CraftType ?? "Unknown");
                    }
                    else if (simpleCraftMonitorAgent != null)
                    {
                        ImGui.Text("NQ Complete: " + simpleCraftMonitorAgent.NqCompleted);
                        ImGui.Text("HQ Complete: " + simpleCraftMonitorAgent.HqCompleted);
                        ImGui.Text("Failed: " + simpleCraftMonitorAgent.TotalFailed);
                        ImGui.Text("Total Completed: " + simpleCraftMonitorAgent.TotalCompleted);
                        ImGui.Text("Total: " + simpleCraftMonitorAgent.Total);
                        ImGui.Text("Item: " + Service.ExcelCache.GetSheet<ItemEx>().GetRow(simpleCraftMonitorAgent.ResultItemId)?.Name.ToString() ?? "Unknown");
                        ImGui.Text("Current Recipe: " + PluginService.CraftMonitor.CurrentRecipe?.RowId ?? "Unknown");
                        ImGui.Text("Current Craft Type: " + PluginService.CraftMonitor.Agent?.CraftType ?? "Unknown");
                    }
                    else
                    {
                        ImGui.Text("Not crafting.");
                    }
                }
                else if (ConfigurationManager.Config.SelectedDebugPage == 6)
                {
                    //ImGui.Text("Running: " + (PluginService.FunTimeService.IsRunning ? "Yes" : "No"));
                    //if (ImGui.Button(PluginService.FunTimeService.IsRunning ? "Stop" : "Start"))
                    //{
                    //    PluginService.FunTimeService.Toggle();
                    //}
                }
                ImGui.EndChild();
            }
        }
        
        public override FilterConfiguration? SelectedConfiguration => null;

        public override void Invalidate()
        {
            
        }
    }
}
#endif