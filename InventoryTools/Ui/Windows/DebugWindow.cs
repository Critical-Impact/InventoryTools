#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Agents;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.GameStructs;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.UiModule;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using InventoryTools.Logic;
using LuminaSupplemental.Excel.Model;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Ui
{
    public class DebugWindow : Window
    {
        public override bool SaveState => true;

        public static string AsKey => "debug";
        public override string Key => AsKey;
        public override Vector2 DefaultSize { get; } = new(700, 700);
        public override Vector2 MaxSize { get; } = new(2000, 2000);
        public override Vector2 MinSize { get; } = new(200, 200);
        public override bool DestroyOnClose => false;
        private List<MobSpawnPosition> _spawnPositions = new List<MobSpawnPosition>();
        private InventoryType inventoryType;
        private FilterState? _filterState;
        private FilterResult? _filterResult;
        private float CurrentX;
        private float CurrentZ;

        public DebugWindow(string name = "Allagan Tools - Debug") : base(name)
        {
        }
        
        public DebugWindow() : base("Allagan Tools - Debug")
        {
        }
        
        public override unsafe void Draw()
        {
            ImGui.BeginChild("###ivDebugList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true);
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
            if (ImGui.Selectable("Inventory Debugging", ConfigurationManager.Config.SelectedDebugPage == 7))
            {
                ConfigurationManager.Config.SelectedDebugPage = 7;
            }
            if (ImGui.Selectable("Armoire Debugging", ConfigurationManager.Config.SelectedDebugPage == 8))
            {
                ConfigurationManager.Config.SelectedDebugPage = 8;
            }
            if (ImGui.Selectable("Glamour Chest Debugging", ConfigurationManager.Config.SelectedDebugPage == 9))
            {
                ConfigurationManager.Config.SelectedDebugPage = 9;
            }
            if (ImGui.Selectable("Inventory Scanner Cache", ConfigurationManager.Config.SelectedDebugPage == 10))
            {
                ConfigurationManager.Config.SelectedDebugPage = 10;
            }
            if (ImGui.Selectable("Inventory Container Tracking", ConfigurationManager.Config.SelectedDebugPage == 11))
            {
                ConfigurationManager.Config.SelectedDebugPage = 11;
            }
            if (ImGui.Selectable("Memory Sort Order", ConfigurationManager.Config.SelectedDebugPage == 12))
            {
                ConfigurationManager.Config.SelectedDebugPage = 12;
            }
            if (ImGui.Selectable("Filter State", ConfigurationManager.Config.SelectedDebugPage == 13))
            {
                ConfigurationManager.Config.SelectedDebugPage = 13;
            }
            if (ImGui.Selectable("Retainer Sort State", ConfigurationManager.Config.SelectedDebugPage == 14))
            {
                ConfigurationManager.Config.SelectedDebugPage = 14;
            }
            if (ImGui.Selectable("Retainer Manager", ConfigurationManager.Config.SelectedDebugPage == 15))
            {
                ConfigurationManager.Config.SelectedDebugPage = 15;
            }
            if (ImGui.Selectable("Crasher", ConfigurationManager.Config.SelectedDebugPage == 16))
            {
                ConfigurationManager.Config.SelectedDebugPage = 16;
            }
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("###ivDebugView", new Vector2(-1, -1), true);
            if (ConfigurationManager.Config.SelectedDebugPage == 0)
            {
                ImGui.Text("Character Information:");
                ImGui.Text(PluginService.CharacterMonitor.ActiveCharacter?.Name.ToString() ?? "Not Logged in Yet");
                ImGui.Text("Actual:" + PluginService.CharacterMonitor.LocalContentId.ToString());
                ImGui.Text("Reported:" + PluginService.CharacterMonitor.ActiveCharacterId.ToString());
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

            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 2)
            {
                float currentX = CurrentX;
                float currentZ = CurrentZ;
                ImGui.InputFloat("X:", ref currentX);
                ImGui.InputFloat("Z:", ref currentZ);
                if (currentX != CurrentX)
                {
                    CurrentX = currentX;
                }

                if (currentZ != CurrentZ)
                {
                    CurrentZ = currentZ;
                }

                if (_spawnPositions.Count != 0)
                {
                    for (var index = 0; index < _spawnPositions.Count; index++)
                    {
                        ImGui.PushID(index);
                        var spawnPosition = _spawnPositions[index];
                        ImGui.Text(Service.ExcelCache.GetBNpcNameExSheet().GetRow(spawnPosition.BNpcNameId)
                            ?.FormattedName ?? "Unknown Name");
                        if (ImGui.Button("Map"))
                        {
                            var territoryType = Service.ExcelCache.GetTerritoryTypeExSheet()
                                .GetRow(spawnPosition.TerritoryTypeId);
                            if (territoryType != null)
                            {
                                var agent = AgentMap.Instance();
                                agent->SetFlagMapMarker(spawnPosition.TerritoryTypeId, territoryType.MapEx.Row,
                                    new Vector3(spawnPosition.Position.X, 0f, spawnPosition.Position.Z));
                                agent->OpenMap(agent->CurrentMapId, agent->CurrentTerritoryId, "Testing");
                            }
                        }
                        ImGui.PopID();
                    }
                }
                
                if (ImGui.Button("Print Map Loc"))
                {
                    var agent = AgentMap.Instance();
                    agent->SetFlagMapMarker(agent->CurrentTerritoryId, agent->CurrentMapId, new Vector3(currentX, 0f, currentZ));
                    agent->OpenMap(agent->CurrentMapId, agent->CurrentTerritoryId, "Testing");
                }
                
                if (ImGui.Button("Get Saved Positions"))
                {
                    var entries = PluginService.MobTracker.GetEntries();
                    _spawnPositions = entries;
                    foreach (var entry in entries)
                    {
                        PluginLog.Log(entry.BNpcNameId.ToString());
                        PluginLog.Log(entry.Position.X.ToString());
                        PluginLog.Log(entry.Position.Z.ToString());
                    }
                }
                
                if (ImGui.Button("Save Positions File"))
                {
                    var entries = PluginService.MobTracker.GetEntries();
                     PluginService.MobTracker.SaveCsv(Service.Interface.GetPluginConfigDirectory() + Path.PathSeparator + "mobs.csv", entries);
                }
                if (ImGui.Button("Print 0,0 start"))
                {
                    var position = InventoryManager.Instance()->GetInventoryContainer(FFXIVClientStructs.FFXIV.Client
                        .Game
                        .InventoryType.Inventory1);
                    PluginLog.Log($"first item, first bag : {(ulong)position:X}", $"{(ulong)position:X}");
                }

                if (ImGui.Button("Convert Inventory Type"))
                {
                    var saddle1 = FFXIVClientStructs.FFXIV.Client.Game.InventoryType.SaddleBag1;
                    PluginLog.Log(saddle1.ToString());
                    PluginLog.Log(saddle1.Convert().ToString());
                }

                if (ImGui.Button("is loaded"))
                {
                    var retainer =
                        InventoryManager.Instance()->GetInventoryContainer(FFXIVClientStructs.FFXIV.Client.Game
                            .InventoryType.RetainerPage1);
                    PluginLog.Log(retainer->Loaded != 0 ? "True" : "False");
                }
                if (ImGui.Button("Check sort ordering"))
                {
                    PluginLog.Log($"item order module : {(ulong)ItemOrderModule.Instance()->SaddleBagPremium:X}",
                        $"{(ulong)ItemOrderModule.Instance()->SaddleBagPremium:X}");
                    PluginLog.Log($"item order module : {(ulong)ItemOrderModule.Instance():X}",
                        $"{(ulong)ItemOrderModule.Instance():X}");
                    PluginLog.Log($"slots per container : " +
                                  ItemOrderModule.Instance()->SaddleBagPremium->SlotPerContainer);
                    for (int i = 0; i < ItemOrderModule.Instance()->SaddleBagPremium->SlotPerContainer * 2; i++)
                    {
                        var slotIndex = ItemOrderModule.Instance()->SaddleBagPremium->Slots[i]->SlotIndex;
                        var containerIndex = ItemOrderModule.Instance()->SaddleBagPremium->Slots[i]->ContainerIndex;
                        PluginLog.Log(containerIndex.ToString() + ":" + slotIndex.ToString());
                    }

                    PluginLog.Log(ItemOrderModule.Instance()->SaddleBagPremium->SlotPerContainer.ToString());
                }

                if (ImGui.Button("Check retainer sort ordering"))
                {
                    PluginLog.Log($"item order module : {(ulong)ItemOrderModule.Instance()->Retainers:X}",
                        $"{(ulong)ItemOrderModule.Instance()->Retainers:X}");
                    PluginLog.Log($"item order module : {(ulong)ItemOrderModule.Instance():X}",
                        $"{(ulong)ItemOrderModule.Instance():X}");
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer1Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer2Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer3Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer4Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer5Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer6Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer7Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer8Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer9Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer10Id.ToString());
                    PluginLog.Log(ItemOrderModule.Instance()->Retainers->SortOrders->Retainer1Bag->SlotPerContainer
                        .ToString());
                    PluginLog.Log(
                        $"item order module : {(ulong)ItemOrderModule.Instance()->Retainers->SortOrders->Retainer1Bag:X}",
                        $"{(ulong)ItemOrderModule.Instance()->Retainers->SortOrders->Retainer1Bag:X}");
                    for (int i = 0;
                         i < ItemOrderModule.Instance()->Retainers->SortOrders->Retainer1Bag->SlotPerContainer * 5;
                         i++)
                    {
                        var slotIndex =
                            ItemOrderModule.Instance()->Retainers->SortOrders->Retainer1Bag->Slots[i]->SlotIndex;
                        var containerIndex =
                            ItemOrderModule.Instance()->Retainers->SortOrders->Retainer1Bag->Slots[i]->ContainerIndex;
                        PluginLog.Log(containerIndex.ToString() + ":" + slotIndex.ToString());
                    }
                }

                if (ImGui.Button("Memory sort check"))
                {
                    MemorySortScanner scanner = new MemorySortScanner();
                    var parsedItemOrder = scanner.ParseItemOrder();
                    if (parsedItemOrder.NormalInventories.ContainsKey("PlayerInventory"))
                    {
                        foreach (var slot in parsedItemOrder.NormalInventories["PlayerInventory"])
                        {
                            PluginLog.Log(
                                "Player Inventory: " + slot.containerIndex.ToString() + " : " + slot.slotIndex);
                        }
                    }

                    if (parsedItemOrder.NormalInventories.ContainsKey("ArmouryBody"))
                    {
                        foreach (var slot in parsedItemOrder.NormalInventories["ArmouryBody"])
                        {
                            PluginLog.Log("Armoury: " + slot.containerIndex.ToString() + " : " + slot.slotIndex);
                        }
                    }

                    if (parsedItemOrder.RetainerInventories.Count != 0)
                    {
                        foreach (var slot in parsedItemOrder.RetainerInventories.First().Value.InventoryCoords)
                        {
                            PluginLog.Log("Retainer: " + slot.containerIndex.ToString() + " : " + slot.slotIndex);
                        }
                    }
                }

                ImGui.Text("Inventory Information:");
                if (ImGui.Button("Try multi request"))
                {
                    PluginService.Universalis.RetrieveMarketBoardPrice(27757);
                    PluginService.Universalis.RetrieveMarketBoardPrice(12594);
                    PluginService.Universalis.RetrieveMarketBoardPrice(19984);
                }

                if (ImGui.Button("Item order module"))
                {
                    var clientInterfaceUiModule = (ItemOrderModule*)FFXIVClientStructs.FFXIV.Client.System.Framework
                        .Framework
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
                        var inventoryItem = (IntPtr)inv->GetInventorySlot(0);
                        PluginLog.Log($"first item pointer: {(ulong)inventoryItem:X}", $"{(ulong)inventoryItem:X}");
                        var inventoryItem1 = (IntPtr)inv->GetInventorySlot(1);
                        PluginLog.Log($"second item pointer: {(ulong)inventoryItem1:X}", $"{(ulong)inventoryItem1:X}");
                    }
                }

                if (ImGui.Button("Check armoury agent"))
                {

                    var agent = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework
                        .Instance()->UIModule->GetAgentModule()->GetAgentByInternalId(AgentId.ArmouryBoard);
                    if (agent->IsAgentActive())
                    {
                        var armouryAgent = (ArmouryBoard*)agent;
                        PluginLog.Log(armouryAgent->SelectedTab.ToString());
                    }

                    var inventoryLarge = PluginService.GameUi.GetWindow("InventoryLarge");
                    if (inventoryLarge != null)
                    {
                        var inventoryAddon = (InventoryLargeAddon*)inventoryLarge;
                        PluginLog.Log(inventoryAddon->CurrentTab.ToString());

                    }
                }

                if (ImGui.Button("Check saddle bag"))
                {

                    var saddleBag = InventoryManager.Instance();
                    var fcChest = InventoryManager.Instance()->GetInventoryContainer(InventoryType.FreeCompanyPage1);
                    PluginLog.Log($"saddle bag: {(ulong)saddleBag:X}", $"{(ulong)saddleBag:X}");
                    PluginLog.Log($"fcChest: {(ulong)fcChest:X}", $"{(ulong)fcChest:X}");

                }

                if (ImGui.Button("Check current company craft"))
                {
                    var subMarinePartsMenu = PluginService.GameUi.GetWindow("SubmarinePartsMenu");
                    if (subMarinePartsMenu != null)
                    {
                        var subAddon = (SubmarinePartsMenuAddon*)subMarinePartsMenu;
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
                        var inventoryAddon = (AddonSelectString*)inventoryLarge;
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

                    var inventoryLarge =
                        PluginService.GameUi.GetWindow(CriticalCommonLib.Services.Ui.WindowName.FreeCompanyChest
                            .ToString());
                    if (inventoryLarge != null)
                    {
                        var inventoryAddon = (InventoryFreeCompanyChestAddon*)inventoryLarge;
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

                if (ImGui.Button("Check prism box agent"))
                {
                    var agents =
                        FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
                            GetAgentModule();
                    InventoryMiragePrismBoxAgent* dresserAgent =
                        (InventoryMiragePrismBoxAgent*)agents->GetAgentByInternalId(AgentId.MiragePrismPrismBox);
                    PluginLog.Log(dresserAgent->SearchGender.ToString());
                    PluginLog.Log(dresserAgent->SearchLevel.ToString());
                    PluginLog.Log(dresserAgent->SearchText.ToString());
                    PluginLog.Log(dresserAgent->QuickSearchText.ToString());
                    PluginLog.Log(dresserAgent->SearchOrder.ToString());
                    PluginLog.Log($"Search Gender Pointer: {(ulong)dresserAgent->SearchGenderPtr:X}");

                    foreach (var glamourItem in dresserAgent->GlamourItems)
                    {
                        //PluginLog.Log(glamourItem.CorrectedItemId.ToString());
                    }
                }
            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 3)
            {
                unsafe
                {
                    var clientInterfaceUiModule = (ItemOrderModule*)FFXIVClientStructs.FFXIV.Client.System.Framework
                        .Framework
                        .Instance()->UIModule->GetItemOrderModule();
                    if (clientInterfaceUiModule != null)
                    {
                        ImGui.Text(clientInterfaceUiModule->RetainerID.ToString());
                        ImGui.Text($"Retainer Pointer: {(ulong)clientInterfaceUiModule->Retainers:X}");
                        var container =
                            InventoryManager.Instance()->GetInventoryContainer(FFXIVClientStructs.FFXIV.Client.Game
                                .InventoryType.RetainerPage1);
                        if (container != null)
                        {
                            ImGui.Text(container->Loaded.ToString());
                            for (int i = 0; i < container->Size; i++)
                            {
                                var item = container->Items[i];
                                var itemPointer = new IntPtr(&item);
                                ImGui.Text(item.ItemID.ToString());
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
                ImGui.Text("Current Items in Queue: " + PluginService.Universalis.QueuedCount);
            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 5)
            {
                var craftMonitorAgent = PluginService.CraftMonitor.Agent;
                var simpleCraftMonitorAgent = PluginService.CraftMonitor.SimpleAgent;
                if (craftMonitorAgent != null)
                {
                    ImGui.Text("Progress: " + craftMonitorAgent.Progress);
                    ImGui.Text("Total Progress Required: " +
                        PluginService.CraftMonitor.RecipeLevelTable?.ProgressRequired(PluginService.CraftMonitor
                            .CurrentRecipe) ?? "Unknown");
                    ImGui.Text("Quality: " + craftMonitorAgent.Quality);
                    ImGui.Text("Status: " + craftMonitorAgent.Status);
                    ImGui.Text("Step: " + craftMonitorAgent.Step);
                    ImGui.Text("Durability: " + craftMonitorAgent.Durability);
                    ImGui.Text("HQ Chance: " + craftMonitorAgent.HqChance);
                    ImGui.Text("Item: " +
                               (Service.ExcelCache.GetItemExSheet().GetRow(craftMonitorAgent.ResultItemId)
                                   ?.NameString ?? "Unknown"));
                    ImGui.Text("Current Recipe: " + PluginService.CraftMonitor.CurrentRecipe?.RowId ?? "Unknown");
                    ImGui.Text("Recipe Difficulty: " + PluginService.CraftMonitor.RecipeLevelTable?.Difficulty ??
                               "Unknown");
                    ImGui.Text(
                        "Recipe Difficulty Factor: " + PluginService.CraftMonitor.CurrentRecipe?.DifficultyFactor ??
                        "Unknown");
                    ImGui.Text("Recipe Durability: " + PluginService.CraftMonitor.RecipeLevelTable?.Durability ??
                               "Unknown");
                    ImGui.Text("Suggested Control: " + PluginService.CraftMonitor.RecipeLevelTable?.SuggestedControl ??
                               "Unknown");
                    ImGui.Text("Suggested Craftsmanship: " +
                        PluginService.CraftMonitor.RecipeLevelTable?.SuggestedCraftsmanship ?? "Unknown");
                    ImGui.Text("Current Craft Type: " + PluginService.CraftMonitor.Agent?.CraftType ?? "Unknown");
                }
                else if (simpleCraftMonitorAgent != null)
                {
                    ImGui.Text("NQ Complete: " + simpleCraftMonitorAgent.NqCompleted);
                    ImGui.Text("HQ Complete: " + simpleCraftMonitorAgent.HqCompleted);
                    ImGui.Text("Failed: " + simpleCraftMonitorAgent.TotalFailed);
                    ImGui.Text("Total Completed: " + simpleCraftMonitorAgent.TotalCompleted);
                    ImGui.Text("Total: " + simpleCraftMonitorAgent.Total);
                    ImGui.Text("Item: " + Service.ExcelCache.GetItemExSheet()
                        .GetRow(simpleCraftMonitorAgent.ResultItemId)?.NameString.ToString() ?? "Unknown");
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
            else if (ConfigurationManager.Config.SelectedDebugPage == 7)
            {
                Utils.ClickToCopyText($"{(ulong)InventoryManager.Instance():X}");
                if (ImGui.BeginTabBar("inventoryDebuggingTabs"))
                {
                    if (ImGui.BeginTabItem("Container/Slot"))
                    {
                        ImGui.PushItemWidth(200);
                        if (ImGui.BeginCombo("###containerSelect", $"{inventoryType} [{(int)inventoryType}]"))
                        {

                            foreach (var i in (InventoryType[])Enum.GetValues(typeof(InventoryType)))
                            {
                                if (ImGui.Selectable($"{i} [{(int)i}]##inventoryTypeSelect", i == inventoryType))
                                {
                                    inventoryType = i;
                                }
                            }

                            ImGui.EndCombo();
                        }

                        var container = InventoryManager.Instance()->GetInventoryContainer(inventoryType);

                        ImGui.PopItemWidth();


                        if (container != null)
                        {

                            ImGui.Text($"Container Address:");
                            ImGui.SameLine();
                            Utils.ClickToCopyText($"{(ulong)container:X}");

                            ImGui.SameLine();
                            Utils.PrintOutObject(*container, (ulong)container, new List<string>());

                            if (ImGui.TreeNode("Items##containerItems"))
                            {

                                for (var i = 0; i < container->Size; i++)
                                {
                                    var item = container->Items[i];
                                    var itemAddr = ((ulong)container->Items) + (ulong)sizeof(InventoryItem) * (ulong)i;
                                    Utils.ClickToCopyText($"{itemAddr:X}");
                                    ImGui.SameLine();
                                    var actualItem = Service.ExcelCache.GetItemExSheet().GetRow(item.ItemID);
                                    var actualItemName = actualItem?.Name ?? "<Not Found>";
                                    actualItemName += " - " + item.HashCode();
                                    Utils.PrintOutObject(item, (ulong)&item, new List<string> { $"Items[{i}]" }, false,
                                        $"[{i:00}] {actualItemName}");
                                }

                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.Text("Container not found.");
                        }

                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 8)
            {
                Utils.ClickToCopyText($"{(ulong)UIState.Instance():X}");
                if (UIState.Instance()->Cabinet.IsCabinetLoaded())
                {
                    int actualIndex = 0;
                    uint currentCategory = 0;
                    foreach (var row in Service.ExcelCache.GetCabinetSheet().OrderBy(c => c.Category.Row)
                                 .ThenBy(c => c.Order))
                    {
                        var itemId = row.Item.Row;
                        var index = row.RowId;
                        var isInArmoire = PluginService.GameInterface.IsInArmoire(itemId);
                        var memoryInventoryItem =
                            CriticalCommonLib.Models.InventoryItem.FromArmoireItem(isInArmoire ? itemId : 0,
                                (short)index);
                        memoryInventoryItem.SortedContainer = CriticalCommonLib.Enums.InventoryType.Armoire;
                        memoryInventoryItem.SortedCategory = InventoryCategory.Armoire;
                        memoryInventoryItem.RetainerId = PluginService.CharacterMonitor.LocalContentId;
                        if (memoryInventoryItem.Item.CabinetCategory != currentCategory)
                        {
                            actualIndex = 0;
                            currentCategory = memoryInventoryItem.Item.CabinetCategory;
                        }

                        memoryInventoryItem.SortedSlotIndex = actualIndex;
                        if (memoryInventoryItem.ItemId != 0)
                        {
                            actualIndex++;
                            Utils.PrintOutObject(memoryInventoryItem, index, new List<string>());
                        }
                    }
                }
                else
                {
                    ImGui.Text("Armoire not loaded.");
                }
            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 9)
            {

                var agents =
                    FFXIVClientStructs.FFXIV.Client.System.Framework.Framework
                        .Instance()->GetUiModule()->GetAgentModule();
                var dresserAgent = agents->GetAgentByInternalId(AgentId.MiragePrismPrismBox);
                if (dresserAgent->IsAgentActive())
                {

                    Utils.ClickToCopyText($"{(ulong)dresserAgent:X}");
                    var itemsStart = *(IntPtr*)((IntPtr)dresserAgent + 40) + 176;

                    if (itemsStart != IntPtr.Zero)
                    {
                        for (var i = 0; i < 800; i++)
                        {
                            var glamItem = (GlamourItem*)(itemsStart + i * 136);
                            var memoryInventoryItem =
                                CriticalCommonLib.Models.InventoryItem.FromGlamourItem(*glamItem);
                            memoryInventoryItem.SortedContainer =
                                CriticalCommonLib.Enums.InventoryType.GlamourChest;
                            memoryInventoryItem.SortedCategory = InventoryCategory.GlamourChest;
                            memoryInventoryItem.RetainerId = PluginService.CharacterMonitor.LocalContentId;
                            memoryInventoryItem.SortedSlotIndex = i;
                            Utils.PrintOutObject(memoryInventoryItem, (ulong)i, new List<string>());

                        }
                    }
                    else
                    {
                        ImGui.Text("Glamour Chest not loaded.");
                    }
                }
                else
                {
                    ImGui.Text("Glamour Chest not loaded.");

                }
            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 10)
            {
                if (ImGui.TreeNode("Character Bags 1##characterBags1"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.CharacterBag1.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.CharacterBag1[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Character Bags 2##characterBags2"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.CharacterBag2.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.CharacterBag2[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Character Bags 3##characterBags3"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.CharacterBag3.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.CharacterBag3[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Character Bags 4##characterBags4"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.CharacterBag4.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.CharacterBag4[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Character Equipped##characterEquipped"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.CharacterEquipped.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.CharacterEquipped[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Character Crystals##characterCrystals"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.CharacterCrystals.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.CharacterCrystals[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Character Currency##characterCurrency"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.CharacterCrystals.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.CharacterCrystals[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Saddlebag Left##saddlebagLeft"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.SaddleBag1.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.SaddleBag1[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Saddlebag Right##saddlebagRight"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.SaddleBag2.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.SaddleBag2[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Premium Saddlebag Left##premiumSaddleBagLeft"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.PremiumSaddleBag1.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.PremiumSaddleBag1[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Premium Saddlebag Right##premiumSaddleBagRight"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.PremiumSaddleBag2.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.PremiumSaddleBag2[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Head##armouryHead"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryHead.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryHead[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - MainHand##armouryMainHand"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryMainHand.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryMainHand[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Body##armouryBody"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryBody.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryBody[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Hands##armouryHands"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryHands.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryHands[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Legs##armouryLegs"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryLegs.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryLegs[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Feet##armouryFeet"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryFeet.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryFeet[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Off Hand##armouryOffHand"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryOffHand.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryOffHand[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Ears##armouryEars"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryEars.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryEars[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Neck##armouryNeck"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryNeck.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryNeck[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Wrists##armouryWrists"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryWrists.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryWrists[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - Rings##armouryRings"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmouryRings.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmouryRings[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoury - SoulCrystals##armourySoulCrystals"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.ArmourySoulCrystals.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.ArmourySoulCrystals[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Free Company Chest 1##freeCompanyBags1"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.FreeCompanyBag1.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.FreeCompanyBag1[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Free Company Chest 2##freeCompanyBags2"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.FreeCompanyBag2.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.FreeCompanyBag2[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Free Company Chest 3##freeCompanyBags3"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.FreeCompanyBag3.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.FreeCompanyBag3[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Free Company Chest 4##freeCompanyBags4"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.FreeCompanyBag4.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.FreeCompanyBag4[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Free Company Chest 5##freeCompanyBags5"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.FreeCompanyBag5.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.FreeCompanyBag5[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Armoire##armoire"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.Armoire.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.Armoire[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Glamour Chest##glamourChest"))
                {
                    for (int i = 0; i < PluginService.InventoryScanner.GlamourChest.Length; i++)
                    {
                        var item = PluginService.InventoryScanner.GlamourChest[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Bag 1##retainerBag1"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerBag1)
                    {
                        if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##1" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Bag 2##retainerBag2"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerBag2)
                    {
                        if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##2" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Bag 3##retainerBag3"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerBag3)
                    {
                        if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##3" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Bag 4##retainerBag4"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerBag4)
                    {
                        if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##4" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Bag 5##retainerBag5"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerBag5)
                    {
                        if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##5" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Equipped##retainerEquipped"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerEquipped)
                    {
                        if (ImGui.TreeNode("Retainer Equipped" + retainer.Key + "##equipped" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Market##retainerMarket"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerMarket)
                    {
                        if (ImGui.TreeNode("Retainer Market" + retainer.Key + "##market" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Market Prices##retainerMarketPrices"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerMarketPrices)
                    {
                        if (ImGui.TreeNode("Retainer Market" + retainer.Key + "##market" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Crystals##retainerCrystals"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerCrystals)
                    {
                        if (ImGui.TreeNode("Retainer Crystals" + retainer.Key + "##crystals" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Retainer Gil##retainerGil"))
                {
                    foreach (var retainer in PluginService.InventoryScanner.RetainerGil)
                    {
                        if (ImGui.TreeNode("Retainer Gil" + retainer.Key + "##gil" + retainer.Key))
                        {
                            for (int i = 0; i < retainer.Value.Length; i++)
                            {
                                var item = retainer.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Gearsets##gearsets"))
                {
                    foreach (var gearSet in PluginService.InventoryScanner.GearSets)
                    {
                        if (ImGui.TreeNode("Gearset " + PluginService.InventoryScanner.GearSetNames[gearSet.Key] +
                                           "##gil" + gearSet.Key))
                        {
                            for (int i = 0; i < gearSet.Value.Length; i++)
                            {
                                var item = gearSet.Value[i];
                                Utils.PrintOutObject(item, (ulong)i, new List<string>());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 11)
            {
                ImGui.Text("Inventories Seen via Network Traffic");
                foreach (var inventory in PluginService.InventoryScanner.InMemory)
                {
                    ImGui.Text(inventory.ToString());
                }

                ImGui.Text("Retainer Inventories Seen via Network Traffic");
                foreach (var inventory in PluginService.InventoryScanner.InMemoryRetainers)
                {
                    ImGui.Text(inventory.Key.ToString());
                    foreach (var hashSet in inventory.Value)
                    {
                        ImGui.Text(hashSet.ToString());
                    }
                }
            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 12)
            {
                if (ImGui.TreeNode("Memory Sort Order#memorySortScanner"))
                {
                    MemorySortScanner scanner = new MemorySortScanner();
                    var itemOrder = scanner.ParseItemOrder();
                    foreach (var retainer in itemOrder.RetainerInventories)
                    {
                        Utils.PrintOutObject(retainer.Key, (ulong)retainer.Key, new List<string>());

                        for (int i = 0; i < retainer.Value.InventoryCoords.Count; i++)
                        {
                            var item = retainer.Value.InventoryCoords[i];
                            Utils.PrintOutObject(item, (ulong)i, new List<string>());
                        }
                    }

                    ImGui.TreePop();
                }
            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 13)
            {
                if (ImGui.Button("Snapshot State"))
                {
                    _filterState = PluginService.OverlayService.LastState;
                    if (_filterState != null)
                    {
                        _filterResult = _filterState.FilterResult;
                    }
                }

                if (ImGui.TreeNode("Filter State##filterState"))
                {
                    if (_filterState != null && _filterResult != null)
                    {
                        Utils.PrintOutObject(_filterState, (ulong)0, new List<string>());
                        for (var index = 0; index < _filterResult.SortedItems.Count; index++)
                        {
                            var item = _filterResult.SortedItems[index];
                            if (ImGui.TreeNode("Sort Item##" + index))
                            {
                                Utils.PrintOutObject(item, (ulong)0, new List<string>());
                                ImGui.TreePop();
                            }
                        }
                    }
                    else
                    {
                        ImGui.Text("Filter state is not set.");
                    }

                    ImGui.TreePop();
                }
            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 14)
            {
                ImGui.Text($"{(ulong)ItemOrderModule.Instance():X}");
                //Utils.PrintOutObject(*ItemOrderModule.Instance(), (ulong)ItemOrderModule.Instance(), new List<string>());

            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 15)
            {
                ImGui.Text($"{(ulong)RetainerManager.Instance():X}");
                Utils.PrintOutObject(*RetainerManager.Instance(), (ulong)RetainerManager.Instance(),
                    new List<string>());

                ImGui.Text($"{(ulong)AgentRetainerList.Instance():X}");
                Utils.PrintOutObject(*AgentRetainerList.Instance(), (ulong)AgentRetainerList.Instance(),
                    new List<string>());
            }
            else if (ConfigurationManager.Config.SelectedDebugPage == 16)
            {
                if (ImGui.Button("Force Save"))
                {
                    ConfigurationManager.SaveAsync();
                    ConfigurationManager.SaveAsync();
                    ConfigurationManager.SaveAsync();
                }
            }

            ImGui.EndChild();

        }
        
        public override FilterConfiguration? SelectedConfiguration => null;

        public override void Invalidate()
        {
            
        }
    }
}
#endif