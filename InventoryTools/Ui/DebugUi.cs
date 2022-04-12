using System;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using ImGuiNET;

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
                    
                }
                else if (Configuration.SelectedDebugPage == 3)
                {
                    unsafe
                    {
                        var clientInterfaceUiModule = PluginService.ClientInterface.UiModule;
                        var module = clientInterfaceUiModule?.ItemOrderModule;
                        if (module != null)
                        {
                            var moduleData = module.Data;
                            if (moduleData != null)
                            {
                                ImGui.Text(moduleData->RetainerID.ToString());
                                ImGui.Text(new IntPtr(moduleData->RetainerPtr).ToString());
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