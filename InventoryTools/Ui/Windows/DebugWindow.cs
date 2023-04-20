#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Agents;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.GameStructs;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using InventoryTools.Logic;
using LuminaSupplemental.Excel.Model;
using OtterGui.Raii;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Ui
{
    public enum DebugMenu
    {
        Characters = 0,
        Inventories = 1,
        InventoryScanner = 2,
        InventoryMonitor = 3
    }
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
            using (var sideBar = ImRaii.Child("###ivDebugList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale,
                       true))
            {
                if (sideBar.Success)
                {
                    if (ImGui.Selectable("Characters", ConfigurationManager.Config.SelectedDebugPage == (int)DebugMenu.Characters))
                    {
                        ConfigurationManager.Config.SelectedDebugPage = (int)DebugMenu.Characters;
                    }

                    if (ImGui.Selectable("Inventories", ConfigurationManager.Config.SelectedDebugPage == (int)DebugMenu.Inventories))
                    {
                        ConfigurationManager.Config.SelectedDebugPage = (int)DebugMenu.Inventories;
                    }

                    if (ImGui.Selectable("Inventory Scanner", ConfigurationManager.Config.SelectedDebugPage == (int)DebugMenu.InventoryScanner))
                    {
                        ConfigurationManager.Config.SelectedDebugPage = (int)DebugMenu.InventoryScanner;
                    }


                    if (ImGui.Selectable("Inventory Monitor", ConfigurationManager.Config.SelectedDebugPage == (int)DebugMenu.InventoryMonitor))
                    {
                        ConfigurationManager.Config.SelectedDebugPage = (int)DebugMenu.InventoryScanner;
                    }

                }
            }
            ImGui.SameLine();

            using (var mainChild = ImRaii.Child("Main", new Vector2(-1, -1), true))
            {
                if (mainChild.Success)
                {
                    if (ConfigurationManager.Config.SelectedDebugPage == (int)DebugMenu.Characters)
                    {
                        DrawCharacterDebugTab();
                    }
                    else if (ConfigurationManager.Config.SelectedDebugPage == (int)DebugMenu.Inventories)
                    {
                        DrawInventoriesDebugTab();
                    }
/*
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
                                ImGui.TextUnformatted(Service.ExcelCache.GetBNpcNameExSheet().GetRow(spawnPosition.BNpcNameId)
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

                        if (ImGui.Button("Free Company"))
                        {
                            var infoProxy =
                                Framework.Instance()->UIModule->GetInfoModule()->GetInfoProxyById(InfoProxyId
                                    .FreeCompany);
                            if (infoProxy != null)
                            {
                                var freeCompanyInfoProxy = (InfoProxyFreeCompany*)infoProxy;
                                PluginLog.Log(freeCompanyInfoProxy->ID.ToString());
                                PluginLog.Log(SeString.Parse(freeCompanyInfoProxy->Name, 22).TextValue);
                            }
                        }


                        if (ImGui.Button("Print Map Loc"))
                        {
                            var agent = AgentMap.Instance();
                            agent->SetFlagMapMarker(agent->CurrentTerritoryId, agent->CurrentMapId,
                                new Vector3(currentX, 0f, currentZ));
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
                            PluginService.MobTracker.SaveCsv(
                                Service.Interface.GetPluginConfigDirectory() + Path.PathSeparator + "mobs.csv",
                                entries);
                        }

                        if (ImGui.Button("Print 0,0 start"))
                        {
                            var position = InventoryManager.Instance()->GetInventoryContainer(FFXIVClientStructs.FFXIV
                                .Client
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

                        ImGui.TextUnformatted("Inventory Information:");
                        if (ImGui.Button("Try multi request"))
                        {
                            PluginService.Universalis.RetrieveMarketBoardPrice(27757);
                            PluginService.Universalis.RetrieveMarketBoardPrice(12594);
                            PluginService.Universalis.RetrieveMarketBoardPrice(19984);
                        }

                        if (ImGui.Button("Item order module"))
                        {
                            var clientInterfaceUiModule = (ItemOrderModule*)FFXIVClientStructs.FFXIV.Client.System
                                .Framework
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
                                var inv = instance->GetInventoryContainer(FFXIVClientStructs.FFXIV.Client.Game
                                    .InventoryType
                                    .Inventory1);
                                var inventoryItem = (IntPtr)inv->GetInventorySlot(0);
                                PluginLog.Log($"first item pointer: {(ulong)inventoryItem:X}",
                                    $"{(ulong)inventoryItem:X}");
                                var inventoryItem1 = (IntPtr)inv->GetInventorySlot(1);
                                PluginLog.Log($"second item pointer: {(ulong)inventoryItem1:X}",
                                    $"{(ulong)inventoryItem1:X}");
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
                            var fcChest =
                                InventoryManager.Instance()->GetInventoryContainer(InventoryType.FreeCompanyPage1);
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
                                (InventoryMiragePrismBoxAgent*)agents->GetAgentByInternalId(
                                    AgentId.MiragePrismPrismBox);
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
                            var clientInterfaceUiModule = (ItemOrderModule*)FFXIVClientStructs.FFXIV.Client.System
                                .Framework
                                .Framework
                                .Instance()->UIModule->GetItemOrderModule();
                            if (clientInterfaceUiModule != null)
                            {
                                ImGui.TextUnformatted(clientInterfaceUiModule->ActiveRetainerId.ToString());
                                // ImGui.TextUnformatted($"Retainer Pointer: {(ulong)clientInterfaceUiModule->Retainers:X}");
                                // var container =
                                //     InventoryManager.Instance()->GetInventoryContainer(FFXIVClientStructs.FFXIV.Client.Game
                                //         .InventoryType.RetainerPage1);
                                // if (container != null)
                                // {
                                //     ImGui.TextUnformatted(container->Loaded.ToString());
                                //     for (int i = 0; i < container->Size; i++)
                                //     {
                                //         var item = container->Items[i];
                                //         var itemPointer = new IntPtr(&item);
                                //         ImGui.TextUnformatted(item.ItemID.ToString());
                                //         ImGui.TextUnformatted(itemPointer.ToString());
                                //     }
                                // }
                            }
                            else
                            {
                                ImGui.TextUnformatted("Module not loaded");
                            }
                        }
                    }
                    else if (ConfigurationManager.Config.SelectedDebugPage == 4)
                    {
                        ImGui.TextUnformatted("Current Items in Queue: " + PluginService.Universalis.QueuedCount);
                    }
                    else if (ConfigurationManager.Config.SelectedDebugPage == 5)
                    {
                        var craftMonitorAgent = PluginService.CraftMonitor.Agent;
                        var simpleCraftMonitorAgent = PluginService.CraftMonitor.SimpleAgent;
                        if (craftMonitorAgent != null)
                        {
                            ImGui.TextUnformatted("Progress: " + craftMonitorAgent.Progress);
                            ImGui.TextUnformatted("Total Progress Required: " +
                                PluginService.CraftMonitor.RecipeLevelTable?.ProgressRequired(PluginService.CraftMonitor
                                    .CurrentRecipe) ?? "Unknown");
                            ImGui.TextUnformatted("Quality: " + craftMonitorAgent.Quality);
                            ImGui.TextUnformatted("Status: " + craftMonitorAgent.Status);
                            ImGui.TextUnformatted("Step: " + craftMonitorAgent.Step);
                            ImGui.TextUnformatted("Durability: " + craftMonitorAgent.Durability);
                            ImGui.TextUnformatted("HQ Chance: " + craftMonitorAgent.HqChance);
                            ImGui.TextUnformatted("Item: " +
                                       (Service.ExcelCache.GetItemExSheet().GetRow(craftMonitorAgent.ResultItemId)
                                           ?.NameString ?? "Unknown"));
                            ImGui.TextUnformatted(
                                "Current Recipe: " + PluginService.CraftMonitor.CurrentRecipe?.RowId ?? "Unknown");
                            ImGui.TextUnformatted(
                                "Recipe Difficulty: " + PluginService.CraftMonitor.RecipeLevelTable?.Difficulty ??
                                "Unknown");
                            ImGui.TextUnformatted(
                                "Recipe Difficulty Factor: " +
                                PluginService.CraftMonitor.CurrentRecipe?.DifficultyFactor ??
                                "Unknown");
                            ImGui.TextUnformatted(
                                "Recipe Durability: " + PluginService.CraftMonitor.RecipeLevelTable?.Durability ??
                                "Unknown");
                            ImGui.TextUnformatted("Suggested Control: " +
                                       PluginService.CraftMonitor.RecipeLevelTable?.SuggestedControl ??
                                       "Unknown");
                            ImGui.TextUnformatted("Suggested Craftsmanship: " +
                                PluginService.CraftMonitor.RecipeLevelTable?.SuggestedCraftsmanship ?? "Unknown");
                            ImGui.TextUnformatted(
                                "Current Craft Type: " + PluginService.CraftMonitor.Agent?.CraftType ?? "Unknown");
                        }
                        else if (simpleCraftMonitorAgent != null)
                        {
                            ImGui.TextUnformatted("NQ Complete: " + simpleCraftMonitorAgent.NqCompleted);
                            ImGui.TextUnformatted("HQ Complete: " + simpleCraftMonitorAgent.HqCompleted);
                            ImGui.TextUnformatted("Failed: " + simpleCraftMonitorAgent.TotalFailed);
                            ImGui.TextUnformatted("Total Completed: " + simpleCraftMonitorAgent.TotalCompleted);
                            ImGui.TextUnformatted("Total: " + simpleCraftMonitorAgent.Total);
                            ImGui.TextUnformatted("Item: " + Service.ExcelCache.GetItemExSheet()
                                .GetRow(simpleCraftMonitorAgent.ResultItemId)?.NameString.ToString() ?? "Unknown");
                            ImGui.TextUnformatted(
                                "Current Recipe: " + PluginService.CraftMonitor.CurrentRecipe?.RowId ?? "Unknown");
                            ImGui.TextUnformatted(
                                "Current Craft Type: " + PluginService.CraftMonitor.Agent?.CraftType ?? "Unknown");
                        }
                        else
                        {
                            ImGui.TextUnformatted("Not crafting.");
                        }
                    }
                    else if (ConfigurationManager.Config.SelectedDebugPage == 6)
                    {
                        //ImGui.TextUnformatted("Running: " + (PluginService.FunTimeService.IsRunning ? "Yes" : "No"));
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
                                        if (ImGui.Selectable($"{i} [{(int)i}]##inventoryTypeSelect",
                                                i == inventoryType))
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

                                    ImGui.TextUnformatted($"Container Address:");
                                    ImGui.SameLine();
                                    Utils.ClickToCopyText($"{(ulong)container:X}");

                                    ImGui.SameLine();
                                    Utils.PrintOutObject(*container, (ulong)container, new List<string>());

                                    if (ImGui.TreeNode("Items##containerItems"))
                                    {

                                        for (var i = 0; i < container->Size; i++)
                                        {
                                            var item = container->Items[i];
                                            var itemAddr = ((ulong)container->Items) +
                                                           (ulong)sizeof(InventoryItem) * (ulong)i;
                                            Utils.ClickToCopyText($"{itemAddr:X}");
                                            ImGui.SameLine();
                                            var actualItem = Service.ExcelCache.GetItemExSheet().GetRow(item.ItemID);
                                            var actualItemName = actualItem?.Name ?? "<Not Found>";
                                            actualItemName += " - " + item.HashCode();
                                            Utils.PrintOutObject(item, (ulong)&item, new List<string> { $"Items[{i}]" },
                                                false,
                                                $"[{i:00}] {actualItemName}");
                                        }

                                        ImGui.TreePop();
                                    }
                                }
                                else
                                {
                                    ImGui.TextUnformatted("Container not found.");
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
                            ImGui.TextUnformatted("Armoire not loaded.");
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
                                ImGui.TextUnformatted("Glamour Chest not loaded.");
                            }
                        }
                        else
                        {
                            ImGui.TextUnformatted("Glamour Chest not loaded.");

                        }
                    }
                    */
                    else if (ConfigurationManager.Config.SelectedDebugPage == (int)DebugMenu.InventoryScanner)
                    {
                        DrawInventoryScannerDebugTab();
                    }
                    else if (ConfigurationManager.Config.SelectedDebugPage == (int)DebugMenu.InventoryMonitor)
                    {
                        
                    }
                    /*
                    else if (ConfigurationManager.Config.SelectedDebugPage == 11)
                    {
                        ImGui.TextUnformatted("Inventories Seen via Network Traffic");
                        foreach (var inventory in PluginService.InventoryScanner.InMemory)
                        {
                            ImGui.TextUnformatted(inventory.ToString());
                        }

                        ImGui.TextUnformatted("Retainer Inventories Seen via Network Traffic");
                        foreach (var inventory in PluginService.InventoryScanner.InMemoryRetainers)
                        {
                            ImGui.TextUnformatted(inventory.Key.ToString());
                            foreach (var hashSet in inventory.Value)
                            {
                                ImGui.TextUnformatted(hashSet.ToString());
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
                                ImGui.TextUnformatted("Filter state is not set.");
                            }

                            ImGui.TreePop();
                        }
                    }
                    else if (ConfigurationManager.Config.SelectedDebugPage == 14)
                    {
                        ImGui.TextUnformatted($"{(ulong)ItemOrderModule.Instance():X}");
                        //Utils.PrintOutObject(*ItemOrderModule.Instance(), (ulong)ItemOrderModule.Instance(), new List<string>());

                    }
                    else if (ConfigurationManager.Config.SelectedDebugPage == 15)
                    {
                        ImGui.TextUnformatted($"{(ulong)RetainerManager.Instance():X}");
                        Utils.PrintOutObject(*RetainerManager.Instance(), (ulong)RetainerManager.Instance(),
                            new List<string>());

                        ImGui.TextUnformatted($"{(ulong)AgentRetainerList.Instance():X}");
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
                    else if (ConfigurationManager.Config.SelectedDebugPage == 16)
                    {

                    }*/
                    
                }
            }
        }
        private void DrawInventoryScannerDebugTab()
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
            var bags = new[]
            {
                InventoryType.HousingInteriorPlacedItems1,
                InventoryType.HousingInteriorPlacedItems2,
                InventoryType.HousingInteriorPlacedItems3,
                InventoryType.HousingInteriorPlacedItems4,
                InventoryType.HousingInteriorPlacedItems5,
                InventoryType.HousingInteriorPlacedItems6,
                InventoryType.HousingInteriorPlacedItems7,
                InventoryType.HousingInteriorPlacedItems8,
                InventoryType.HousingInteriorStoreroom1,
                InventoryType.HousingInteriorStoreroom2,
                InventoryType.HousingInteriorStoreroom3,
                InventoryType.HousingInteriorStoreroom4,
                InventoryType.HousingInteriorStoreroom5,
                InventoryType.HousingInteriorStoreroom6,
                InventoryType.HousingInteriorStoreroom7,
                InventoryType.HousingInteriorStoreroom8,
                InventoryType.HousingExteriorAppearance,
                InventoryType.HousingInteriorAppearance,
                InventoryType.HousingExteriorPlacedItems,
                InventoryType.HousingExteriorStoreroom,
            };

            if (ImGui.TreeNode("Housing Inventories"))
            {
                foreach (var bagType in bags)
                {
                    var bag = PluginService.InventoryScanner.GetInventoryByType(bagType);
                    var bagLoaded = PluginService.InventoryScanner.IsBagLoaded(bagType);
                    if (ImGui.TreeNode(bagType.ToString() + (bagLoaded ? " (Loaded)" : " (Not Loaded)")))
                    {
                        var itemCount = bag.Count(c => c.ItemID != 0);
                        ImGui.Text(itemCount + "/" + bag.Length);
                        for (int i = 0; i < bag.Length; i++)
                        {
                            var item = bag[i];
                            Utils.PrintOutObject(item, (ulong)i, new List<string>());
                        }

                        ImGui.TreePop();
                    }

                }
                ImGui.TreePop();
            }
        }
        private void DrawInventoriesDebugTab()
        {
            ImGui.TextUnformatted("Inventory Information:");
            ImGui.Separator();
            foreach (var inventory in PluginService.InventoryMonitor.Inventories)
            {
                var character = PluginService.CharacterMonitor.GetCharacterById(inventory.Key);
                var characterName = "Unknown";
                if (character != null)
                {
                    characterName = character.FormattedName;
                }

                if (ImGui.TreeNode(characterName + "##char" + inventory.Key))
                {
                    foreach (var item in inventory.Value)
                    {
                        ImGui.TextUnformatted(item.Key.FormattedName());
                        ImGui.Text(item.Value.Count(c => !c.IsEmpty) + "/" + item.Value.Count);
                        ImGui.Separator();
                    }

                    ImGui.TreePop();
                }
            }
            ImGui.TextUnformatted("Inventory List:");
            ImGui.Separator();
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
                        ImGui.TextUnformatted((inventory.Key).ToString());
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(itemSet.Key.ToString());
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(item.FormattedName);
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(item.SortedSlotIndex.ToString());
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(item.ItemId.ToString());
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(item.Slot.ToString());
                    }
                }
            }

            ImGui.EndTable();
        }

        private void DrawCharacterDebugTab()
        {
            ImGui.TextUnformatted("Character Information:");
            ImGui.TextUnformatted(PluginService.CharacterMonitor.ActiveCharacter?.Name.ToString() ??
                                  "Not Logged in Yet");
            ImGui.TextUnformatted("Local Character ID:" + PluginService.CharacterMonitor.LocalContentId.ToString());
            ImGui.TextUnformatted("Current Territory Id:" + Service.ClientState.TerritoryType.ToString());
            ImGui.Separator();
            ImGui.TextUnformatted("Cached Character ID:" + PluginService.CharacterMonitor.ActiveCharacterId.ToString());
            ImGui.TextUnformatted("Cached House Id:" + PluginService.CharacterMonitor.ActiveHouseId.ToString());
            ImGui.TextUnformatted("Cached Ward Id:" + PluginService.CharacterMonitor.InternalWardId.ToString());
            ImGui.TextUnformatted("Cached Plot Id:" + PluginService.CharacterMonitor.InternalPlotId.ToString());
            ImGui.TextUnformatted("Cached Division Id:" + PluginService.CharacterMonitor.InternalDivisionId.ToString());
            ImGui.TextUnformatted("Cached Room Id:" + PluginService.CharacterMonitor.InternalRoomId.ToString());
            ImGui.TextUnformatted("Cached House Id:" + PluginService.CharacterMonitor.InternalHouseId.ToString());
            ImGui.TextUnformatted("Has Housing Permission:" +
                                  (PluginService.CharacterMonitor.InternalHasHousePermission ? "Yes" : "No"));
            ImGui.NewLine();
            ImGui.TextUnformatted("Retainers:");
            ImGui.BeginTable("retainerTable", 6);
            ImGui.TableSetupColumn("Hire Order");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Type");
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
                    ImGui.TextUnformatted((retainer.Value.HireOrder + 1).ToString());
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(retainer.Value.CharacterType == CharacterType.Housing
                        ? retainer.Value.HousingName
                        : retainer.Value.Name);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(retainer.Value.CharacterType.ToString());
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(retainer.Value.Gil.ToString());
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(retainer.Value.CharacterId.ToString());
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(retainer.Value.OwnerId.ToString());
                }
            }

            ImGui.EndTable();
            ImGui.Separator();
            ImGui.TextUnformatted("Character Objects:");
            foreach (var retainer in retainers)
            {
                if (ImGui.TreeNode(retainer.Value.CharacterType == CharacterType.Housing
                        ? retainer.Value.HousingName
                        : retainer.Value.Name + "##" + retainer.Key))
                {
                    Utils.PrintOutObject(retainer.Value, 0, new List<string>());

                    ImGui.TreePop();
                }
            }
        }

        public override FilterConfiguration? SelectedConfiguration => null;

        public override void Invalidate()
        {
            
        }
    }
}
#endif