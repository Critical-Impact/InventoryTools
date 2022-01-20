using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools
{
    public partial class InventoryToolsUi
    {
        private unsafe void DrawDebugUi()
        {

            if (ImGui.BeginChild("###ivDebugList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (ImGui.Selectable("Retainers", _configuration.SelectedDebugPage == 0))
                {
                    _configuration.SelectedDebugPage = 0;
                }
                if (ImGui.Selectable("Inventories", _configuration.SelectedDebugPage == 1))
                {
                    _configuration.SelectedDebugPage = 1;
                }
                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("###ivDebugView", new Vector2(-1, -1), true))
            {
                if (_configuration.SelectedDebugPage == 0)
                {
                    ImGui.Text("Character Information:");
                    ImGui.Text(_clientState.LocalPlayer?.Name.ToString() ?? "Not Logged in Yet");
                    ImGui.Text("Actual:" + _clientState.LocalContentId.ToString());
                    ImGui.Text("Reported:" + _characterMonitor.ActiveCharacter.ToString());
                    ImGui.Text("Retainers:");
                    ImGui.BeginTable("retainerTable", 5);
                    ImGui.TableSetupColumn("Hire Order");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Gil");
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("Owner ID");
                    ImGui.TableHeadersRow();
                    var retainers = _characterMonitor.Characters;
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
                else if (_configuration.SelectedDebugPage == 1)
                {
                    ImGui.Text("Inventory Information:");
                    ImGui.BeginTable("retainerTable", 4);
                    ImGui.TableSetupColumn("Inventory ID");
                    ImGui.TableSetupColumn("Category");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Sorted Slot Index");
                    ImGui.TableHeadersRow();
                    var inventories = _inventoryMonitor.Inventories;
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
                            }
                        }

                    }
                    ImGui.EndTable();
                    
                }
                ImGui.EndChild();
            }
        }
    }
}