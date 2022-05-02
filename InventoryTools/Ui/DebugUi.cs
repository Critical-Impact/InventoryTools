using System;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Agents;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.UiModule;
using Dalamud.Interface;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using InventoryTools.GameUi;
using InventoryType = CriticalCommonLib.Enums.InventoryType;

namespace InventoryTools
{
    public partial class InventoryToolsUi
    {
        #if DEBUG

        private unsafe void DrawDebugUi()
        {
            if (ImGui.BeginChild("###ivDebugList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (ImGui.Selectable("Retainers", Configuration.SelectedDebugPage == 0))
                {
                    Configuration.SelectedDebugPage = 0;
                }
                if (ImGui.Selectable("Inventories", Configuration.SelectedDebugPage == 1))
                {
                    Configuration.SelectedDebugPage = 1;
                }
                if (ImGui.Selectable("Stuff", Configuration.SelectedDebugPage == 2))
                {
                    Configuration.SelectedDebugPage = 2;
                }
                if (ImGui.Selectable("Retainer Debugger", Configuration.SelectedDebugPage == 3))
                {
                    Configuration.SelectedDebugPage = 3;
                }
                if (ImGui.Selectable("Universalis", Configuration.SelectedDebugPage == 4))
                {
                    Configuration.SelectedDebugPage = 4;
                }
                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("###ivDebugView", new Vector2(-1, -1), true))
            {
                if (Configuration.SelectedDebugPage == 0)
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
                else if (Configuration.SelectedDebugPage == 1)
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
                else if (Configuration.SelectedDebugPage == 2)
                {
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
                else if (Configuration.SelectedDebugPage == 3)
                {
                    unsafe
                    {
                        var clientInterfaceUiModule = (ItemOrderModule*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework
                            .Instance()->UIModule->GetItemOrderModule();
                        if (clientInterfaceUiModule != null)
                        {
                            ImGui.Text(clientInterfaceUiModule->RetainerID.ToString());
                            ImGui.Text(new IntPtr(clientInterfaceUiModule->RetainerPtr).ToString());
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
                else if (Configuration.SelectedDebugPage == 4)
                {
                    ImGui.Text("Current Items in Queue: " + Universalis.QueuedCount);
                }
                ImGui.EndChild();
            }
        }
        #endif
    }
}