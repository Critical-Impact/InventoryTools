#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using InventoryTools.Logic;
using LuminaSupplemental.Excel.Model;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui.DebugWindows;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    using CriticalCommonLib.Addons;
    using Dalamud.Plugin.Services;

    public enum DebugMenu
    {
        Characters = 0,
        Inventories = 1,
        InventoryScanner = 2,
        InventoryMonitor = 3,
        Random = 4,
        CraftAgents = 5,
        DebugWindows = 6,
        Addons = 7,
        GameInventory = 8,
        Unlocks = 9,
        LayerDebugger = 10,
    }
    public class DebugWindow : GenericWindow, IMenuWindow
    {
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly IInventoryScanner _inventoryScanner;
        private readonly ICraftMonitor _craftMonitor;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IGameGui gameGui;
        private readonly ItemSheet _itemSheet;
        private readonly InventoryToolsConfiguration _configuration;
        private InventoryType? _inventoryType;

        public DebugWindow(ILogger<DebugWindow> logger,
            MediatorService mediator,
            ImGuiService imGuiService,
            InventoryToolsConfiguration configuration,
            IInventoryMonitor inventoryMonitor,
            IInventoryScanner inventoryScanner,
            ICraftMonitor craftMonitor,
            ICharacterMonitor characterMonitor,
            IGameGui gameGui,
            ItemSheet itemSheet,
            string name = "Debug Window") : base(logger,
            mediator,
            imGuiService,
            configuration,
            name)
        {
            _inventoryMonitor = inventoryMonitor;
            _inventoryScanner = inventoryScanner;
            _craftMonitor = craftMonitor;
            _characterMonitor = characterMonitor;
            this.gameGui = gameGui;
            _itemSheet = itemSheet;
            _configuration = configuration;
        }
        public override void Initialize()
        {
            Key = "debug";
            WindowName = "Debug";
        }

        public override bool SaveState => true;

        public override Vector2? DefaultSize { get; } = new(700, 700);
        public override Vector2? MaxSize { get; } = new(2000, 2000);
        public override Vector2? MinSize { get; } = new(200, 200);
        public override string GenericKey => "debug";
        public override string GenericName => "Debug";
        public override bool DestroyOnClose => false;
        private List<MobSpawnPosition> _spawnPositions = new List<MobSpawnPosition>();

        public override unsafe void Draw()
        {
            using (var sideBar = ImRaii.Child("###ivDebugList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale,
                       true))
            {
                if (sideBar.Success)
                {
                    if (ImGui.Selectable("Characters", _configuration.SelectedDebugPage == (int)DebugMenu.Characters))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.Characters;
                    }

                    if (ImGui.Selectable("Inventories", _configuration.SelectedDebugPage == (int)DebugMenu.Inventories))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.Inventories;
                    }

                    if (ImGui.Selectable("Inventory Scanner", _configuration.SelectedDebugPage == (int)DebugMenu.InventoryScanner))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.InventoryScanner;
                    }

                    if (ImGui.Selectable("Inventory Monitor", _configuration.SelectedDebugPage == (int)DebugMenu.InventoryMonitor))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.InventoryMonitor;
                    }

                    if (ImGui.Selectable("Game Inventory", _configuration.SelectedDebugPage == (int)DebugMenu.GameInventory))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.GameInventory;
                    }

                    if (ImGui.Selectable("Random", _configuration.SelectedDebugPage == (int)DebugMenu.Random))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.Random;
                    }

                    if (ImGui.Selectable("Craft Agents", _configuration.SelectedDebugPage == (int)DebugMenu.CraftAgents))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.CraftAgents;
                    }

                    if (ImGui.Selectable("Debug Windows", _configuration.SelectedDebugPage == (int)DebugMenu.DebugWindows))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.DebugWindows;
                    }

                    if (ImGui.Selectable("Addons", _configuration.SelectedDebugPage == (int)DebugMenu.Addons))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.Addons;
                    }

                    if (ImGui.Selectable("Unlocks", _configuration.SelectedDebugPage == (int)DebugMenu.Unlocks))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.Unlocks;
                    }

                    if (ImGui.Selectable("Layer Debugger", _configuration.SelectedDebugPage == (int)DebugMenu.LayerDebugger))
                    {
                        _configuration.SelectedDebugPage = (int)DebugMenu.LayerDebugger;
                    }

                }
            }
            ImGui.SameLine();

            using (var mainChild = ImRaii.Child("Main", new Vector2(-1, -1), true))
            {
                if (mainChild.Success)
                {
                    if (_configuration.SelectedDebugPage == (int)DebugMenu.Characters)
                    {
                        DrawCharacterDebugTab();
                    }
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.Inventories)
                    {
                        DrawInventoriesDebugTab();
                    }
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.Random)
                    {
                        DrawRandomTab();
                    }
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.CraftAgents)
                    {
                        DrawCraftAgentTab();
                    }
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.DebugWindows)
                    {
                        DrawDebugWindows();
                    }
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.Addons)
                    {
                        DrawAddons();
                    }
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.LayerDebugger)
                    {
                        DrawLayerDebugger();
                    }
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.Unlocks)
                    {
                        DrawUnlocks();
                    }
/*
                    else if (_configuration.SelectedDebugPage == 2)
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
                                ImGui.TextUnformatted(_excelCache.GetBNpcNameExSheet().GetRow(spawnPosition.BNpcNameId)
                                    ?.FormattedName ?? "Unknown Name");
                                if (ImGui.Button("Map"))
                                {
                                    var territoryType = _excelCache.GetTerritoryTypeExSheet()
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
                                _logger.Info(freeCompanyInfoProxy->ID.ToString());
                                _logger.Info(SeString.Parse(freeCompanyInfoProxy->Name, 22).TextValue);
                            }
                        }

                        if (ImGui.Button("Check Shop List"))
                        {
                            var addon = Service.Gui.GetAddonByName("Shop");
                            var actualAddon = ((AtkUnitBase*) addon);
                            if (actualAddon != null)
                            {
                                var listNode = (AtkComponentNode*)actualAddon->GetNodeById(16);
                                if (listNode == null || (ushort)listNode->AtkResNode.Type < 1000) return;
                                var actualListNode = (AtkComponentList*)listNode->Component;
                                if (actualListNode != null)
                                {
                                    for (var i = 0; i < actualListNode->ListLength - 1; i++)
                                    {
                                        if (actualListNode->ItemRendererList[i].AtkComponentListItemRenderer != null)
                                        {
                                            var listItem = actualListNode->ItemRendererList[i]
                                                .AtkComponentListItemRenderer;

                                            var uldManager = listItem->AtkComponentButton.AtkComponentBase.UldManager;
                                            if (uldManager.NodeListCount < 4) continue;

                                            var textNode = (AtkTextNode*)uldManager.SearchNodeById(3);
                                            if (textNode != null)
                                            {
                                                var seString = MemoryHelper.ReadSeString(&textNode->NodeText);
                                                _logger.Info(seString.ToString());
                                            }
                                        }
                                    }
                                }
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
                            var entries = _mobTracker.GetEntries();
                            _spawnPositions = entries;
                            foreach (var entry in entries)
                            {
                                _logger.Info(entry.BNpcNameId.ToString());
                                _logger.Info(entry.Position.X.ToString());
                                _logger.Info(entry.Position.Z.ToString());
                            }
                        }

                        if (ImGui.Button("Save Positions File"))
                        {
                            var entries = _mobTracker.GetEntries();
                            _mobTracker.SaveCsv(
                                Service.Interface.GetPluginConfigDirectory() + Path.PathSeparator + "mobs.csv",
                                entries);
                        }

                        if (ImGui.Button("Print 0,0 start"))
                        {
                            var position = InventoryManager.Instance()->GetInventoryContainer(FFXIVClientStructs.FFXIV
                                .Client
                                .Game
                                .InventoryType.Inventory1);
                            _logger.Info($"first item, first bag : {(ulong)position:X}", $"{(ulong)position:X}");
                        }

                        if (ImGui.Button("Convert Inventory Type"))
                        {
                            var saddle1 = FFXIVClientStructs.FFXIV.Client.Game.InventoryType.SaddleBag1;
                            _logger.Info(saddle1.ToString());
                            _logger.Info(saddle1.Convert().ToString());
                        }

                        if (ImGui.Button("is loaded"))
                        {
                            var retainer =
                                InventoryManager.Instance()->GetInventoryContainer(FFXIVClientStructs.FFXIV.Client.Game
                                    .InventoryType.RetainerPage1);
                            _logger.Info(retainer->Loaded != 0 ? "True" : "False");
                        }

                        ImGui.TextUnformatted("Inventory Information:");
                        if (ImGui.Button("Try multi request"))
                        {
                            _universalis.RetrieveMarketBoardPrice(27757);
                            _universalis.RetrieveMarketBoardPrice(12594);
                            _universalis.RetrieveMarketBoardPrice(19984);
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
                                _logger.Info($"item order module : {(ulong)module:X}", $"{(ulong)module:X}");
                            }
                        }

                        if (ImGui.Button("Check inventory manager pointer"))
                        {
                            var instance = InventoryManager.Instance();
                            if (instance != null)
                            {
                                _logger.Info($"Manager pointer: {(ulong)instance:X}", $"{(ulong)instance:X}");
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
                                _logger.Info($"first item pointer: {(ulong)inventoryItem:X}",
                                    $"{(ulong)inventoryItem:X}");
                                var inventoryItem1 = (IntPtr)inv->GetInventorySlot(1);
                                _logger.Info($"second item pointer: {(ulong)inventoryItem1:X}",
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
                                _logger.Info(armouryAgent->SelectedTab.ToString());
                            }

                            var inventoryLarge = _gameUiManager.GetWindow("InventoryLarge");
                            if (inventoryLarge != null)
                            {
                                var inventoryAddon = (InventoryLargeAddon*)inventoryLarge;
                                _logger.Info(inventoryAddon->CurrentTab.ToString());

                            }
                        }

                        if (ImGui.Button("Check saddle bag"))
                        {

                            var saddleBag = InventoryManager.Instance();
                            var fcChest =
                                InventoryManager.Instance()->GetInventoryContainer(InventoryType.FreeCompanyPage1);
                            _logger.Info($"saddle bag: {(ulong)saddleBag:X}", $"{(ulong)saddleBag:X}");
                            _logger.Info($"fcChest: {(ulong)fcChest:X}", $"{(ulong)fcChest:X}");

                        }

                        if (ImGui.Button("Check current company craft"))
                        {
                            var subMarinePartsMenu = _gameUiManager.GetWindow("SubmarinePartsMenu");
                            if (subMarinePartsMenu != null)
                            {
                                var subAddon = (SubmarinePartsMenuAddon*)subMarinePartsMenu;
                                _logger.Info("Current Phase: " + subAddon->Phase.ToString());
                                _logger.Info("Item 1: " + subAddon->AmountHandedIn(0).ToString());
                                _logger.Info("Item 2: " + subAddon->AmountHandedIn(1).ToString());
                                _logger.Info("Item 3: " + subAddon->AmountHandedIn(2).ToString());
                                _logger.Info("Item 4: " + subAddon->AmountHandedIn(3).ToString());
                                _logger.Info("Item 5: " + subAddon->AmountHandedIn(4).ToString());
                                _logger.Info("Item 6: " + subAddon->AmountHandedIn(5).ToString());
                                _logger.Info("Item 1: " + subAddon->AmountNeeded(0).ToString());
                                _logger.Info("Item 2: " + subAddon->AmountNeeded(1).ToString());
                                _logger.Info("Item 3: " + subAddon->AmountNeeded(2).ToString());
                                _logger.Info("Item 4: " + subAddon->AmountNeeded(3).ToString());
                                _logger.Info("Item 5: " + subAddon->AmountNeeded(4).ToString());
                                _logger.Info("Item 6: " + subAddon->AmountNeeded(5).ToString());
                                _logger.Info("Crafting: " + subAddon->ResultItemId.ToString());
                                _logger.Info("Item Required: " + subAddon->RequiredItemId(0).ToString());
                                _logger.Info("Item Required: " + subAddon->RequiredItemId(1).ToString());
                                _logger.Info("Item Required: " + subAddon->RequiredItemId(2).ToString());
                                _logger.Info("Item Required: " + subAddon->RequiredItemId(3).ToString());
                                _logger.Info("Item Required: " + subAddon->RequiredItemId(4).ToString());
                                _logger.Info("Item Required: " + subAddon->RequiredItemId(5).ToString());

                            }
                        }

                        if (ImGui.Button("Check select string"))
                        {

                            var inventoryLarge = _gameUiManager.GetWindow("SelectString");
                            if (inventoryLarge != null)
                            {
                                var inventoryAddon = (AddonSelectString*)inventoryLarge;
                                _logger.Info(inventoryAddon->PopupMenu.PopupMenu.EntryCount.ToString());
                                for (int i = 0; i < inventoryAddon->PopupMenu.PopupMenu.EntryCount; i++)
                                {
                                    var popupMenuEntryName = inventoryAddon->PopupMenu.PopupMenu.EntryNames[i];
                                    _logger.Info(popupMenuEntryName->ToString());
                                }

                            }
                        }

                        if (ImGui.Button("Check free company tab"))
                        {

                            var inventoryLarge =
                                _gameUiManager.GetWindow(CriticalCommonLib.Services.Ui.WindowName.FreeCompanyChest
                                    .ToString());
                            if (inventoryLarge != null)
                            {
                                var inventoryAddon = (InventoryFreeCompanyChestAddon*)inventoryLarge;
                                _logger.Info(inventoryAddon->CurrentTab.ToString());

                            }
                        }

                        if (ImGui.Button("Check prism box"))
                        {
                            var prismBox = new AtkInventoryMiragePrismBox();
                            _logger.Info(prismBox.CurrentPage.ToString());
                            _logger.Info(prismBox.CurrentTab.ToString());
                            _logger.Info(prismBox.ClassJobSelected.ToString());
                            _logger.Info(prismBox.OnlyDisplayRaceGenderItems.ToString());
                        }

                        if (ImGui.Button("Check prism box agent"))
                        {
                            var agents =
                                FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
                                    GetAgentModule();
                            InventoryMiragePrismBoxAgent* dresserAgent =
                                (InventoryMiragePrismBoxAgent*)agents->GetAgentByInternalId(
                                    AgentId.MiragePrismPrismBox);
                            _logger.Info(dresserAgent->SearchGender.ToString());
                            _logger.Info(dresserAgent->SearchLevel.ToString());
                            _logger.Info(dresserAgent->SearchText.ToString());
                            _logger.Info(dresserAgent->QuickSearchText.ToString());
                            _logger.Info(dresserAgent->SearchOrder.ToString());
                            _logger.Info($"Search Gender Pointer: {(ulong)dresserAgent->SearchGenderPtr:X}");

                            foreach (var glamourItem in dresserAgent->GlamourItems)
                            {
                                //_logger.Info(glamourItem.CorrectedItemId.ToString());
                            }
                        }
                    }
                    else if (_configuration.SelectedDebugPage == 3)
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
                    else if (_configuration.SelectedDebugPage == 4)
                    {
                        ImGui.TextUnformatted("Current Items in Queue: " + _universalis.QueuedCount);
                    }
                    else if (_configuration.SelectedDebugPage == 5)
                    {
                        var craftMonitorAgent = _craftMonitor.Agent;
                        var simpleCraftMonitorAgent = _craftMonitor.SimpleAgent;
                        if (craftMonitorAgent != null)
                        {
                            ImGui.TextUnformatted("Progress: " + craftMonitorAgent.Progress);
                            ImGui.TextUnformatted("Total Progress Required: " +
                                _craftMonitor.RecipeLevelTable?.ProgressRequired(_craftMonitor
                                    .CurrentRecipe) ?? "Unknown");
                            ImGui.TextUnformatted("Quality: " + craftMonitorAgent.Quality);
                            ImGui.TextUnformatted("Status: " + craftMonitorAgent.Status);
                            ImGui.TextUnformatted("Step: " + craftMonitorAgent.Step);
                            ImGui.TextUnformatted("Durability: " + craftMonitorAgent.Durability);
                            ImGui.TextUnformatted("HQ Chance: " + craftMonitorAgent.HqChance);
                            ImGui.TextUnformatted("Item: " +
                                       (_excelCache.GetItemSheet().GetRow(craftMonitorAgent.ResultItemId)
                                           ?.NameString ?? "Unknown"));
                            ImGui.TextUnformatted(
                                "Current Recipe: " + _craftMonitor.CurrentRecipe?.RowId ?? "Unknown");
                            ImGui.TextUnformatted(
                                "Recipe Difficulty: " + _craftMonitor.RecipeLevelTable?.Difficulty ??
                                "Unknown");
                            ImGui.TextUnformatted(
                                "Recipe Difficulty Factor: " +
                                _craftMonitor.CurrentRecipe?.DifficultyFactor ??
                                "Unknown");
                            ImGui.TextUnformatted(
                                "Recipe Durability: " + _craftMonitor.RecipeLevelTable?.Durability ??
                                "Unknown");
                            ImGui.TextUnformatted("Suggested Control: " +
                                       _craftMonitor.RecipeLevelTable?.SuggestedControl ??
                                       "Unknown");
                            ImGui.TextUnformatted("Suggested Craftsmanship: " +
                                _craftMonitor.RecipeLevelTable?.SuggestedCraftsmanship ?? "Unknown");
                            ImGui.TextUnformatted(
                                "Current Craft Type: " + _craftMonitor.Agent?.CraftType ?? "Unknown");
                        }
                        else if (simpleCraftMonitorAgent != null)
                        {
                            ImGui.TextUnformatted("NQ Complete: " + simpleCraftMonitorAgent.NqCompleted);
                            ImGui.TextUnformatted("HQ Complete: " + simpleCraftMonitorAgent.HqCompleted);
                            ImGui.TextUnformatted("Failed: " + simpleCraftMonitorAgent.TotalFailed);
                            ImGui.TextUnformatted("Total Completed: " + simpleCraftMonitorAgent.TotalCompleted);
                            ImGui.TextUnformatted("Total: " + simpleCraftMonitorAgent.Total);
                            ImGui.TextUnformatted("Item: " + _excelCache.GetItemSheet()
                                .GetRow(simpleCraftMonitorAgent.ResultItemId)?.NameString.ToString() ?? "Unknown");
                            ImGui.TextUnformatted(
                                "Current Recipe: " + _craftMonitor.CurrentRecipe?.RowId ?? "Unknown");
                            ImGui.TextUnformatted(
                                "Current Craft Type: " + _craftMonitor.Agent?.CraftType ?? "Unknown");
                        }
                        else
                        {
                            ImGui.TextUnformatted("Not crafting.");
                        }
                    }
                    else if (_configuration.SelectedDebugPage == 6)
                    {
                        //ImGui.TextUnformatted("Running: " + (_funTimeService.IsRunning ? "Yes" : "No"));
                        //if (ImGui.Button(_funTimeService.IsRunning ? "Stop" : "Start"))
                        //{
                        //    _funTimeService.Toggle();
                        //}
                    }
                    else if (_configuration.SelectedDebugPage == 7)
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
                                            var actualItem = _excelCache.GetItemSheet().GetRow(item.ItemID);
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
                    else if (_configuration.SelectedDebugPage == 8)
                    {
                        Utils.ClickToCopyText($"{(ulong)UIState.Instance():X}");
                        if (UIState.Instance()->Cabinet.IsCabinetLoaded())
                        {
                            int actualIndex = 0;
                            uint currentCategory = 0;
                            foreach (var row in _excelCache.GetCabinetSheet().OrderBy(c => c.Category.Row)
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
                                memoryInventoryItem.RetainerId = _characterMonitor.LocalContentId;
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
                    else if (_configuration.SelectedDebugPage == 9)
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
                                    memoryInventoryItem.RetainerId = _characterMonitor.LocalContentId;
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
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.InventoryScanner)
                    {
                        DrawInventoryScannerDebugTab();
                    }
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.GameInventory)
                    {
                        var inventoryTypes = Enum.GetValues<InventoryType>();
                        using (var combo = ImRaii.Combo("Inventory Type", _inventoryType?.ToString() ?? "None"))
                        {
                            if (combo)
                            {
                                foreach (var inventoryType in inventoryTypes)
                                {
                                    if (ImGui.Selectable(inventoryType.ToString()))
                                    {
                                        _inventoryType = inventoryType;
                                    }
                                }
                            }
                        }

                        if (_inventoryType != null)
                        {
                            var container = InventoryManager.Instance()->GetInventoryContainer(_inventoryType.Value);
                            ImGui.Text(container->Loaded != 0 ? "Container Loaded" : "Container Unloaded");
                            ImGui.Text(container->Loaded.ToString());

                        }
                    }
                    else if (_configuration.SelectedDebugPage == (int)DebugMenu.InventoryMonitor)
                    {
                        foreach (var character in _inventoryMonitor.Inventories)
                        {
                            using (var characterNode = ImRaii.TreeNode(character.Key + "##" + character.Key))
                            {
                                if (characterNode.Success)
                                {
                                    using (ImRaii.PushId(character.Key.ToString()))
                                    {
                                        var possibleValues = Enum.GetValues<CriticalCommonLib.Enums.InventoryType>();
                                        foreach (var possibleValue in possibleValues)
                                        {
                                            var bag = character.Value.GetInventoryByType(possibleValue);
                                            var bagName = possibleValue.ToString();
                                            if (bag != null)
                                            {
                                                using (var bagNode = ImRaii.TreeNode(bagName + "##" + bagName))
                                                {
                                                    if (bagNode.Success)
                                                    {
                                                        for (int i = 0; i < bag.Length; i++)
                                                        {
                                                            var item = bag[i];
                                                            if (item != null)
                                                            {
                                                                Utils.PrintOutObject(item, (ulong)i,
                                                                    new List<string>());
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    /*
                    else if (_configuration.SelectedDebugPage == 11)
                    {
                        ImGui.TextUnformatted("Inventories Seen via Network Traffic");
                        foreach (var inventory in _inventoryScanner.InMemory)
                        {
                            ImGui.TextUnformatted(inventory.ToString());
                        }

                        ImGui.TextUnformatted("Retainer Inventories Seen via Network Traffic");
                        foreach (var inventory in _inventoryScanner.InMemoryRetainers)
                        {
                            ImGui.TextUnformatted(inventory.Key.ToString());
                            foreach (var hashSet in inventory.Value)
                            {
                                ImGui.TextUnformatted(hashSet.ToString());
                            }
                        }
                    }
                    else if (_configuration.SelectedDebugPage == 12)
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
                    else if (_configuration.SelectedDebugPage == 13)
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
                    else if (_configuration.SelectedDebugPage == 14)
                    {
                        ImGui.TextUnformatted($"{(ulong)ItemOrderModule.Instance():X}");
                        //Utils.PrintOutObject(*ItemOrderModule.Instance(), (ulong)ItemOrderModule.Instance(), new List<string>());

                    }
                    else if (_configuration.SelectedDebugPage == 15)
                    {
                        ImGui.TextUnformatted($"{(ulong)RetainerManager.Instance():X}");
                        Utils.PrintOutObject(*RetainerManager.Instance(), (ulong)RetainerManager.Instance(),
                            new List<string>());

                        ImGui.TextUnformatted($"{(ulong)AgentRetainerList.Instance():X}");
                        Utils.PrintOutObject(*AgentRetainerList.Instance(), (ulong)AgentRetainerList.Instance(),
                            new List<string>());
                    }
                    else if (_configuration.SelectedDebugPage == 16)
                    {
                        if (ImGui.Button("Force Save"))
                        {
                            ConfigurationManagerService.SaveAsync();
                            ConfigurationManagerService.SaveAsync();
                            ConfigurationManagerService.SaveAsync();
                        }
                    }
                    else if (_configuration.SelectedDebugPage == 16)
                    {

                    }*/

                }
            }
        }

        private unsafe void DrawLayerDebugger()
        {
            var activeLayout = LayoutWorld.Instance()->ActiveLayout;
            if (activeLayout != null)
            {
                ImGui.TextUnformatted($"Level ID: {activeLayout->LevelId}");
                ImGui.TextUnformatted($"ID: {activeLayout->Id}");
                ImGui.TextUnformatted($"Type: {activeLayout->Type}");
                ImGui.TextUnformatted($"Resource Strings: {activeLayout->Type}");
                foreach (var resourcePath in activeLayout->ResourcePaths.Strings)
                {
                    if (resourcePath.Value != null)
                    {
                        ImGui.TextUnformatted($"{resourcePath.Value->DataString}");
                    }
                }
                ImGui.TextUnformatted($"Layers:");
                foreach (var layer in activeLayout->Layers)
                {
                    ImGui.TextUnformatted($"{layer.Item1}");
                    var pointer = layer.Item2.Value;
                    if (pointer != null)
                    {
                        ImGui.TextUnformatted($"Layer ID: " + pointer->Id);
                        ImGui.TextUnformatted($"Layer Group ID: " + pointer->LayerGroupId);
                        ImGui.TextUnformatted($"Festival ID: " + pointer->FestivalId);
                    }
                }
            }
        }

        private void DrawInventoryScannerDebugTab()
        {

            ImGui.TextUnformatted("Inventories Seen via Network Traffic");
            foreach (var inventory in _inventoryScanner.LoadedInventories)
            {
                ImGui.TextUnformatted(inventory.ToString());
            }

            ImGui.TextUnformatted("Retainer Inventories Seen via Network Traffic");
            foreach (var inventory in _inventoryScanner.InMemoryRetainers)
            {
                ImGui.TextUnformatted(inventory.Key.ToString());
                foreach (var hashSet in inventory.Value)
                {
                    ImGui.TextUnformatted(hashSet.ToString());
                }
            }
            if (ImGui.TreeNode("Character Bags 1##characterBags1"))
            {
                for (int i = 0; i < _inventoryScanner.CharacterBag1.Length; i++)
                {
                    var item = _inventoryScanner.CharacterBag1[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Character Bags 2##characterBags2"))
            {
                for (int i = 0; i < _inventoryScanner.CharacterBag2.Length; i++)
                {
                    var item = _inventoryScanner.CharacterBag2[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Character Bags 3##characterBags3"))
            {
                for (int i = 0; i < _inventoryScanner.CharacterBag3.Length; i++)
                {
                    var item = _inventoryScanner.CharacterBag3[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Character Bags 4##characterBags4"))
            {
                for (int i = 0; i < _inventoryScanner.CharacterBag4.Length; i++)
                {
                    var item = _inventoryScanner.CharacterBag4[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Character Equipped##characterEquipped"))
            {
                for (int i = 0; i < _inventoryScanner.CharacterEquipped.Length; i++)
                {
                    var item = _inventoryScanner.CharacterEquipped[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Character Crystals##characterCrystals"))
            {
                for (int i = 0; i < _inventoryScanner.CharacterCrystals.Length; i++)
                {
                    var item = _inventoryScanner.CharacterCrystals[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Character Currency##characterCurrency"))
            {
                for (int i = 0; i < _inventoryScanner.CharacterCurrency.Length; i++)
                {
                    var item = _inventoryScanner.CharacterCurrency[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Saddlebag Left##saddlebagLeft"))
            {
                for (int i = 0; i < _inventoryScanner.SaddleBag1.Length; i++)
                {
                    var item = _inventoryScanner.SaddleBag1[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Saddlebag Right##saddlebagRight"))
            {
                for (int i = 0; i < _inventoryScanner.SaddleBag2.Length; i++)
                {
                    var item = _inventoryScanner.SaddleBag2[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Premium Saddlebag Left##premiumSaddleBagLeft"))
            {
                for (int i = 0; i < _inventoryScanner.PremiumSaddleBag1.Length; i++)
                {
                    var item = _inventoryScanner.PremiumSaddleBag1[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Premium Saddlebag Right##premiumSaddleBagRight"))
            {
                for (int i = 0; i < _inventoryScanner.PremiumSaddleBag2.Length; i++)
                {
                    var item = _inventoryScanner.PremiumSaddleBag2[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Head##armouryHead"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryHead.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryHead[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - MainHand##armouryMainHand"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryMainHand.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryMainHand[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Body##armouryBody"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryBody.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryBody[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Hands##armouryHands"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryHands.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryHands[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Legs##armouryLegs"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryLegs.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryLegs[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Feet##armouryFeet"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryFeet.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryFeet[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Off Hand##armouryOffHand"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryOffHand.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryOffHand[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Ears##armouryEars"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryEars.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryEars[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Neck##armouryNeck"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryNeck.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryNeck[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Wrists##armouryWrists"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryWrists.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryWrists[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - Rings##armouryRings"))
            {
                for (int i = 0; i < _inventoryScanner.ArmouryRings.Length; i++)
                {
                    var item = _inventoryScanner.ArmouryRings[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoury - SoulCrystals##armourySoulCrystals"))
            {
                for (int i = 0; i < _inventoryScanner.ArmourySoulCrystals.Length; i++)
                {
                    var item = _inventoryScanner.ArmourySoulCrystals[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Free Company Chest 1##freeCompanyBags1"))
            {
                for (int i = 0; i < _inventoryScanner.FreeCompanyBag1.Length; i++)
                {
                    var item = _inventoryScanner.FreeCompanyBag1[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Free Company Chest 2##freeCompanyBags2"))
            {
                for (int i = 0; i < _inventoryScanner.FreeCompanyBag2.Length; i++)
                {
                    var item = _inventoryScanner.FreeCompanyBag2[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Free Company Chest 3##freeCompanyBags3"))
            {
                for (int i = 0; i < _inventoryScanner.FreeCompanyBag3.Length; i++)
                {
                    var item = _inventoryScanner.FreeCompanyBag3[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Free Company Chest 4##freeCompanyBags4"))
            {
                for (int i = 0; i < _inventoryScanner.FreeCompanyBag4.Length; i++)
                {
                    var item = _inventoryScanner.FreeCompanyBag4[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Free Company Chest 5##freeCompanyBags5"))
            {
                for (int i = 0; i < _inventoryScanner.FreeCompanyBag5.Length; i++)
                {
                    var item = _inventoryScanner.FreeCompanyBag5[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Free Company Currency##freeCompanyCurrency"))
            {
                var bagType = (InventoryType)CriticalCommonLib.Enums.InventoryType.FreeCompanyCurrency;
                var bag = _inventoryScanner.GetInventoryByType(bagType);
                var bagLoaded = _inventoryScanner.IsBagLoaded(bagType);
                if (ImGui.TreeNode(bagType.ToString() + (bagLoaded ? " (Loaded)" : " (Not Loaded)")))
                {
                    var itemCount = bag.Count(c => c.ItemId != 0);
                    ImGui.Text(itemCount + "/" + bag.Length);
                    for (int i = 0; i < bag.Length; i++)
                    {
                        var item = bag[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Armoire##armoire"))
            {
                for (int i = 0; i < _inventoryScanner.Armoire.Length; i++)
                {
                    var item = _inventoryScanner.Armoire[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Glamour Chest##glamourChest"))
            {
                for (int i = 0; i < _inventoryScanner.GlamourChest.Length; i++)
                {
                    var item = _inventoryScanner.GlamourChest[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Retainer Bag 1##retainerBag1"))
            {
                foreach (var retainer in _inventoryScanner.RetainerBag1)
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
                foreach (var retainer in _inventoryScanner.RetainerBag2)
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
                foreach (var retainer in _inventoryScanner.RetainerBag3)
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
                foreach (var retainer in _inventoryScanner.RetainerBag4)
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
                foreach (var retainer in _inventoryScanner.RetainerBag5)
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
                foreach (var retainer in _inventoryScanner.RetainerEquipped)
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
                foreach (var retainer in _inventoryScanner.RetainerMarket)
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
                foreach (var retainer in _inventoryScanner.RetainerMarketPrices)
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
                foreach (var retainer in _inventoryScanner.RetainerCrystals)
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
                foreach (var retainer in _inventoryScanner.RetainerGil)
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
                foreach (var gearSet in _inventoryScanner.GetGearSets())
                {
                    ImGui.Text(gearSet.Key + ":");
                    foreach (var actualset in gearSet.Value)
                    {
                        ImGui.Text(actualset.Item1 + " : " + actualset.Item2);
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
                    var bag = _inventoryScanner.GetInventoryByType(bagType);
                    var bagLoaded = _inventoryScanner.IsBagLoaded(bagType);
                    if (ImGui.TreeNode(bagType.ToString() + (bagLoaded ? " (Loaded)" : " (Not Loaded)")))
                    {
                        var itemCount = bag.Count(c => c.ItemId != 0);
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

        public void DrawRandomTab()
        {
            if (ImGui.Button("Clear notices"))
            {
                _configuration.NotificationsSeen.Clear();
            }

            if (ImGui.Button("Print Inventory Types"))
            {
                unsafe
                {
                    var containers = InventoryManager.Instance()->Inventories;
                    for (int i = 0; i < containers->Size; i++)
                    {
                        var items = containers[i].Items;
                        for (int j = 0; j < containers[i].Size; j++)
                        {
                            if (items[j].ItemId == 8564)
                            {
                                Logger.LogDebug(((uint)containers[i].Type).ToString() + (containers[i].Type.ToString()));
                            }
                        }
                    }
                }
            }
        }

        public void DrawDebugWindows()
        {
            if (ImGui.Button("Overlay Service"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(DebugOverlayServiceWindow)));
            }
            if (ImGui.Button("Window Service"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(DebugWindowServiceWindow)));
            }
            if (ImGui.Button("List Service"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(DebugListServiceWindow)));
            }
        }

        public void DrawUnlocks()
        {
            var acquiredItems = _configuration.AcquiredItems;
            foreach (var characterPair in acquiredItems)
            {
                var character = _characterMonitor.GetCharacterById(characterPair.Key);
                ImGui.TextUnformatted(character?.FormattedName ?? "Unknown Character");
                ImGui.Text($"{characterPair.Value.Count} unlocked items");
            }
        }
        public unsafe void DrawAddons()
        {
            if (ImGui.CollapsingHeader("Free Company Chest"))
            {
                var freeCompanyChest = this.gameGui.GetAddonByName("FreeCompanyChest");
                if (freeCompanyChest != IntPtr.Zero)
                {
                    var freeCompanyChestAddon = (InventoryFreeCompanyChestAddon*)freeCompanyChest;
                    if (freeCompanyChestAddon != null)
                    {
                        ImGui.Text($"Current Tab: { freeCompanyChestAddon->CurrentTab }");
                    }
                }
            }
            if (ImGui.CollapsingHeader("Glamour Chest(MiragePrismPrismBox)"))
            {
                var addon = this.gameGui.GetAddonByName("MiragePrismPrismBox");
                if (addon != IntPtr.Zero)
                {
                    var mirageAddon = (InventoryMiragePrismBoxAddon*)addon;
                    if (mirageAddon != null)
                    {
                        ImGui.Text($"Current Tab: { mirageAddon->SelectedTab }");
                        ImGui.Text($"Class/Job Selected: { mirageAddon->ClassJobSelected }");
                    }
                }
            }
            if (ImGui.CollapsingHeader("Armoire(CabinetWithdraw)"))
            {
                var uiState = UIState.Instance();
                if (uiState == null)
                {
                    ImGui.Text("UIState not found.");
                }
                else
                {
                    ImGui.Text(uiState->Cabinet.IsCabinetLoaded() ? "Cabinet Loaded" : "Cabinet Not Loaded");
                }

                var addon = this.gameGui.GetAddonByName("CabinetWithdraw");
                if (addon != IntPtr.Zero)
                {
                    var cabinetWithdraw = (AddonCabinetWithdraw*)addon;
                    if (cabinetWithdraw != null)
                    {
                        ImGui.Text($"Artifact Armor Selected: { (cabinetWithdraw->ArtifactArmorRadioButton->IsChecked ? "yes" : "no") }");
                        ImGui.Text($"Seasonal Gear 1 Selected: { (cabinetWithdraw->SeasonalGear1RadioButton->IsChecked ? "yes" : "no") }");
                        ImGui.Text($"Seasonal Gear 2 Selected: { (cabinetWithdraw->SeasonalGear2RadioButton->IsChecked ? "yes" : "no") }");
                        ImGui.Text($"Seasonal Gear 3 Selected: { (cabinetWithdraw->SeasonalGear3RadioButton->IsChecked ? "yes" : "no") }");
                        ImGui.Text($"Seasonal Gear 4 Selected: { (cabinetWithdraw->SeasonalGear4RadioButton->IsChecked ? "yes" : "no") }");
                        ImGui.Text($"Seasonal Gear 5 Selected: { (cabinetWithdraw->SeasonalGear5RadioButton->IsChecked ? "yes" : "no") }");
                        ImGui.Text($"Achievements Selected: { (cabinetWithdraw->AchievementsRadioButton->IsChecked ? "yes" : "no") }");
                        ImGui.Text($"Exclusive Extras Selected: { (cabinetWithdraw->ExclusiveExtrasRadioButton->IsChecked ? "yes" : "no") }");
                        ImGui.Text($"Search Selected: { (cabinetWithdraw->SearchRadioButton->IsChecked ? "yes" : "no") }");
                    }
                }
            }

            if (ImGui.CollapsingHeader("Housing Goods"))
            {
                var addon = this.gameGui.GetAddonByName("HousingGoods");
                if (addon != IntPtr.Zero)
                {
                    var housingGoods = (AddonHousingGoods*)addon;
                    if (housingGoods != null)
                    {
                        ImGui.Text($"Current Tab: { (housingGoods->CurrentTab) }");
                    }
                }

            }
        }

        public unsafe void DrawCraftAgentTab()
        {
            var craftMonitorAgent = _craftMonitor.Agent;
            var simpleCraftMonitorAgent = _craftMonitor.SimpleAgent;
            if (craftMonitorAgent != null)
            {
                ImGui.Text($"Craft Monitor Pointer: {(ulong)craftMonitorAgent.Agent:X}");
                ImGui.TextUnformatted("Is Trial Synthesis: " + craftMonitorAgent.IsTrialSynthesis);
                ImGui.TextUnformatted("Progress: " + craftMonitorAgent.Progress);
                ImGui.TextUnformatted("Total Progress Required: " +
                    _craftMonitor.RecipeLevelTable?.ProgressRequired(_craftMonitor
                        .CurrentRecipe) ?? "Unknown");
                ImGui.TextUnformatted("Quality: " + craftMonitorAgent.Quality);
                ImGui.TextUnformatted("Status: " + craftMonitorAgent.Status);
                ImGui.TextUnformatted("Step: " + craftMonitorAgent.Step);
                ImGui.TextUnformatted("Durability: " + craftMonitorAgent.Durability);
                ImGui.TextUnformatted("HQ Chance: " + craftMonitorAgent.HqChance);
                ImGui.TextUnformatted("Item: " +
                           (_itemSheet.GetRow(craftMonitorAgent.ResultItemId)
                               ?.NameString ?? "Unknown"));
                ImGui.TextUnformatted(
                    "Current Recipe: " + _craftMonitor.CurrentRecipe?.RowId ?? "Unknown");
                ImGui.TextUnformatted(
                    "Recipe Difficulty: " + _craftMonitor.RecipeLevelTable?.Base.Difficulty ??
                    "Unknown");
                ImGui.TextUnformatted(
                    "Recipe Difficulty Factor: " +
                    _craftMonitor.CurrentRecipe?.Base.DifficultyFactor ??
                    "Unknown");
                ImGui.TextUnformatted(
                    "Recipe Durability: " + _craftMonitor.RecipeLevelTable?.Base.Durability ??
                    "Unknown");
                ImGui.TextUnformatted("Suggested Craftsmanship: " +
                    _craftMonitor.RecipeLevelTable?.Base.SuggestedCraftsmanship ?? "Unknown");
                ImGui.TextUnformatted(
                    "Current Craft Type: " + _craftMonitor.Agent?.CraftType ?? "Unknown");
            }
            else if (simpleCraftMonitorAgent != null)
            {
                ImGui.Text($"Simple Craft Monitor Pointer: {(ulong)simpleCraftMonitorAgent.Agent:X}");
                ImGui.TextUnformatted("NQ Complete: " + simpleCraftMonitorAgent.NqCompleted);
                ImGui.TextUnformatted("HQ Complete: " + simpleCraftMonitorAgent.HqCompleted);
                ImGui.TextUnformatted("Failed: " + simpleCraftMonitorAgent.TotalFailed);
                ImGui.TextUnformatted("Total Completed: " + simpleCraftMonitorAgent.TotalCompleted);
                ImGui.TextUnformatted("Total: " + simpleCraftMonitorAgent.Total);
                ImGui.TextUnformatted("Item: " + _itemSheet
                    .GetRowOrDefault(simpleCraftMonitorAgent.ResultItemId)?.NameString.ToString() ?? "Unknown");
                ImGui.TextUnformatted(
                    "Current Recipe: " + _craftMonitor.CurrentRecipe?.RowId ?? "Unknown");
                ImGui.TextUnformatted(
                    "Current Craft Type: " + _craftMonitor.Agent?.CraftType ?? "Unknown");
            }
            else
            {
                ImGui.TextUnformatted("Not crafting.");
            }
        }
        private void DrawInventoriesDebugTab()
        {
            // ImGui.TextUnformatted("Inventory Information:");
            // ImGui.Separator();
            // foreach (var inventory in _inventoryMonitor.Inventories)
            // {
            //     var character = _characterMonitor.GetCharacterById(inventory.Key);
            //     var characterName = "Unknown";
            //     if (character != null)
            //     {
            //         characterName = character.FormattedName;
            //     }
            //
            //     if (ImGui.TreeNode(characterName + "##char" + inventory.Key))
            //     {
            //         foreach (var item in inventory.Value)
            //         {
            //             ImGui.TextUnformatted(item.Key.FormattedName());
            //             ImGui.Text(item.Value.Count(c => !c.IsEmpty) + "/" + item.Value.Count);
            //             ImGui.Separator();
            //         }
            //
            //         ImGui.TreePop();
            //     }
            // }
            // ImGui.TextUnformatted("Inventory List:");
            // ImGui.Separator();
            // ImGui.BeginTable("retainerTable", 6);
            // ImGui.TableSetupColumn("Inventory ID");
            // ImGui.TableSetupColumn("Category");
            // ImGui.TableSetupColumn("Name");
            // ImGui.TableSetupColumn("Sorted Slot Index");
            // ImGui.TableSetupColumn("Item ID");
            // ImGui.TableSetupColumn("Unsorted Slot ID");
            // ImGui.TableHeadersRow();
            // var inventories = _inventoryMonitor.Inventories;
            // foreach (var inventory in inventories)
            // {
            //     foreach (var itemSet in inventory.Value.GetAllInventories())
            //     {
            //         foreach (var item in itemSet.Value)
            //         {
            //             ImGui.TableNextColumn();
            //             ImGui.TextUnformatted((inventory.Key).ToString());
            //             ImGui.TableNextColumn();
            //             ImGui.TextUnformatted(itemSet.Key.ToString());
            //             ImGui.TableNextColumn();
            //             ImGui.TextUnformatted(item.FormattedName);
            //             ImGui.TableNextColumn();
            //             ImGui.TextUnformatted(item.SortedSlotIndex.ToString());
            //             ImGui.TableNextColumn();
            //             ImGui.TextUnformatted(item.ItemId.ToString());
            //             ImGui.TableNextColumn();
            //             ImGui.TextUnformatted(item.Slot.ToString());
            //         }
            //     }
            // }
            //
            // ImGui.EndTable();
        }

        private unsafe void DrawCharacterDebugTab()
        {
            ImGui.TextUnformatted("Character Information:");
            ImGui.TextUnformatted(_characterMonitor.ActiveCharacter?.Name.ToString() ??
                                  "Not Logged in Yet");
            ImGui.TextUnformatted("Local Character ID:" + _characterMonitor.LocalContentId.ToString());
            ImGui.TextUnformatted("Free Company ID:" + _characterMonitor.ActiveFreeCompanyId.ToString());
            ImGui.TextUnformatted("Current Territory Id:" + Service.ClientState.TerritoryType.ToString());
            ImGui.Separator();
            ImGui.TextUnformatted("Cached Character ID:" + _characterMonitor.ActiveCharacterId.ToString());
            ImGui.TextUnformatted("Cached House Id:" + _characterMonitor.ActiveHouseId.ToString());
            ImGui.TextUnformatted("Cached Ward Id:" + _characterMonitor.InternalWardId.ToString());
            ImGui.TextUnformatted("Cached Plot Id:" + _characterMonitor.InternalPlotId.ToString());
            ImGui.TextUnformatted("Cached Division Id:" + _characterMonitor.InternalDivisionId.ToString());
            ImGui.TextUnformatted("Cached Room Id:" + _characterMonitor.InternalRoomId.ToString());
            ImGui.TextUnformatted("Cached House Id:" + _characterMonitor.InternalHouseId.ToString());

            var ot = HousingManager.Instance()->OutdoorTerritory;
            if(ot != null)
            {
                ImGui.TextUnformatted(ot->HouseId.ToString());
            }
            var it = HousingManager.Instance()->IndoorTerritory;
            if(it != null)
            {
                ImGui.TextUnformatted(it->HouseId.ToString());
            }
            var ct = HousingManager.Instance()->CurrentTerritory;
            if(ct != null)
            {
                ImGui.TextUnformatted($"{(ulong)ct:X}");
            }
            ImGui.TextUnformatted("Owned House IDS:");
            foreach (var id in _characterMonitor.GetOwnedHouseIds())
            {
                ImGui.TextUnformatted(id.ToString());
            }
            ImGui.TextUnformatted("Has Housing Permission:" +
                                  (_characterMonitor.InternalHasHousePermission || _characterMonitor.GetOwnedHouseIds().Contains(_characterMonitor.InternalHouseId) ? "Yes" : "No"));
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
            var retainers = _characterMonitor.Characters;
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