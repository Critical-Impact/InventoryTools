using System.Numerics;
using System.Text;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface.ImGuiFileDialog;
using ImGuiNET;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Ui;
using LuminaSupplemental.Excel.Model;
using Newtonsoft.Json;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Log;

namespace InventoryToolsMock;

public class MockWindow : GenericWindow
{
    private readonly Logger _otterLogger;
    private readonly ICraftMonitor _craftMonitor;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IMobTracker _mobTracker;
    private readonly IFileDialogManager _fileDialogManager;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly IOverlayService _overlayService;
    private readonly InventoryHistory _inventoryHistory;
    private readonly ExcelCache _excelCache;
    private readonly ExcelSheet<World> _worldSheet;

    public MockWindow(ILogger<MockWindow> logger,
        MediatorService mediator,
        ImGuiService imGuiService,
        InventoryToolsConfiguration configuration,
        Logger otterLogger,
        ICraftMonitor craftMonitor,
        ICharacterMonitor characterMonitor,
        IMobTracker mobTracker,
        IFileDialogManager fileDialogManager,
        IInventoryMonitor inventoryMonitor,
        IOverlayService overlayService,
        HostedInventoryHistory inventoryHistory,
        ExcelCache excelCache,
        ExcelSheet<World> worldSheet,
        string name = "Mock Tools") : base(logger,
        mediator,
        imGuiService,
        configuration,
        name)
    {
        _otterLogger = otterLogger;
        _craftMonitor = craftMonitor;
        _characterMonitor = characterMonitor;
        _mobTracker = mobTracker;
        _fileDialogManager = fileDialogManager;
        _inventoryMonitor = inventoryMonitor;
        _overlayService = overlayService;
        _inventoryHistory = inventoryHistory;
        _excelCache = excelCache;
        _worldSheet = worldSheet;
    }
    private List<InventoryItem> _items;

    public override void Initialize()
    {
        WindowName = "Mock Tools";
        Key = "mock";
        _rng = new Random();
        _activeWorldPicker = new WorldPicker(_worldSheet.Where(c => c.IsPublic).ToList(), false, _otterLogger);
        _homeWorldPicker = new WorldPicker(_worldSheet.Where(c => c.IsPublic).ToList(), false, _otterLogger);
    }


    public override void OnClose()
    {
        IsOpen = true;
    }

    public override string GenericKey { get; } = "mockwindow";
    public override string GenericName { get; } = "Mock Window";
    public override bool DestroyOnClose { get; } = false;
    public override bool SaveState { get; } = true;
    public override Vector2? DefaultSize => new Vector2(200, 200);
    public override Vector2? MaxSize => new Vector2(2000, 2000);
    public override Vector2? MinSize => new Vector2(200, 200);
    private Random _rng;
    private WorldPicker _activeWorldPicker;
    private WorldPicker _homeWorldPicker;

    public override void Draw()
    {
        ImGui.ShowStackToolWindow();
        if (ImGui.BeginTabBar("MockTabs"))
        {
            DrawWindowTab();
            DrawDataTab();
            DrawCharacterTab();
            DrawGameUiTab();
            DrawImGuiTab();
            DrawCraftMonitor();
            ImGui.EndTabBar();
        }
    }

    private string _windowName = "";

    private void DrawImGuiTab()
    {
        using (var gameUiTab = ImRaii.TabItem("ImGui UI"))
        {
            if (gameUiTab.Success)
            {
                if (ImGui.Button("9.6pt##DalamudSettingsGlobalUiScaleReset96"))
                {
                    ImGui.GetIO().FontGlobalScale = 9.6f / 12.0f;
                }

                ImGui.SameLine();
                if (ImGui.Button("12pt##DalamudSettingsGlobalUiScaleReset12"))
                {
                    ImGui.GetIO().FontGlobalScale = 1;
                }

                ImGui.SameLine();
                if (ImGui.Button("14pt##DalamudSettingsGlobalUiScaleReset14"))
                {
                    ImGui.GetIO().FontGlobalScale = 14.0f / 12.0f;
                }

                ImGui.SameLine();
                if (ImGui.Button("18pt##DalamudSettingsGlobalUiScaleReset18"))
                {
                    ImGui.GetIO().FontGlobalScale = 18.0f / 12.0f;
                }

                ImGui.SameLine();
                if (ImGui.Button("36pt##DalamudSettingsGlobalUiScaleReset36"))
                {
                    ImGui.GetIO().FontGlobalScale = 36.0f / 12.0f;
                }
            }
        }

    }

    private int _selectedItemId = 0;
    private int _quantity = 1;
    private bool _isHq = false;
    private void DrawCraftMonitor()
    {
        using (var tab = ImRaii.TabItem("Craft Monitor"))
        {
            if (tab.Success)
            {
                var selectedItemId = _selectedItemId;
                if (ImGui.InputInt("Item ID", ref selectedItemId))
                {
                    _selectedItemId = selectedItemId;
                }
                var quantity = _quantity;
                if (ImGui.InputInt("Quantity", ref quantity))
                {
                    _quantity = quantity;
                }
                var isHq = _isHq;
                if (ImGui.Checkbox("Is HQ?", ref isHq))
                {
                    _isHq = isHq;
                }

                if (ImGui.Button("Send Event"))
                {
                    if (_craftMonitor is MockCraftMonitor craftMonitor)
                    {
                        craftMonitor.CompleteCraft((uint)_selectedItemId, isHq, (uint)quantity);
                    }
                }
            }
        }

    }

    private void DrawGameUiTab()
    {
        using (var gameUiTab = ImRaii.TabItem("Game UI"))
        {
            if (gameUiTab.Success)
            {
                using (var combo = ImRaii.Combo("Window Name", _windowName))
                {
                    if (combo.Success)
                    {
                        foreach (var windowName in Enum.GetNames(typeof(WindowName)))
                        {
                            if (ImGui.Selectable(windowName, _windowName == windowName))
                            {
                                _windowName = windowName;
                            }
                        }
                    }
                }

                if (ImGui.Button("Show"))
                {
                    if (Enum.TryParse(_windowName, out WindowName actualWindowName))
                    {
                        //Program.MockPlugin._gameUiManager.ManualInvokeUiVisibilityChanged(actualWindowName, true);
                    }
                }

                if (ImGui.Button("Hide"))
                {
                    if (Enum.TryParse(_windowName, out WindowName actualWindowName))
                    {
                        //Program.MockPlugin._gameUiManager.ManualInvokeUiVisibilityChanged(actualWindowName, false);
                    }
                }
            }

            if (ImGui.Button("Test Context Menu"))
            {
                MediatorService.Publish(new AddToNewCraftListMessage(1, 1, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None, false));
            }
        }
    }

    private void DrawCharacterTab()
    {
        using (var characterTab = ImRaii.TabItem("Characters"))
        {
            if (characterTab.Success)
            {
                using (var combo = ImRaii.Combo("Active Character", _characterMonitor.ActiveCharacter?.FormattedName ?? "N/A"))
                {
                    if (combo.Success)
                    {
                        foreach (var character in _characterMonitor.GetPlayerCharacters())
                        {
                            if (ImGui.Selectable(character.Value.FormattedName + "##" + character.Key, _characterMonitor.ActiveCharacterId == character.Key))
                            {
                                _characterMonitor.OverrideActiveCharacter(character.Key);
                                if (_characterMonitor is MockCharacterMonitor mockCharacterMonitor)
                                {
                                    mockCharacterMonitor.OverrideIsLoggedIn(true);
                                }
                            }
                        }
                    }
                }
                using (var combo = ImRaii.Combo("Active Retainer", _characterMonitor.ActiveRetainer?.FormattedName ?? "N/A"))
                {
                    if (combo.Success)
                    {
                        if(ImGui.Selectable("None", _characterMonitor.ActiveRetainerId == 0))
                        {
                            _characterMonitor.OverrideActiveRetainer(0);
                        }
                        foreach (var character in _characterMonitor.GetRetainerCharacters())
                        {
                            if (ImGui.Selectable(character.Value.FormattedName + "##" + character.Key, _characterMonitor.ActiveRetainerId == character.Key))
                            {
                                _characterMonitor.OverrideActiveRetainer(character.Key);
                            }
                        }
                    }
                }

                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    int homeWorldId = (int)(activeCharacter.WorldId);

                    if (_homeWorldPicker.Draw("Home World",
                            _characterMonitor.ActiveCharacter?.World?.Name.ExtractText() ?? "Not Set",
                            "Set the home world of the active character", ref homeWorldId, 100, 32,
                            ImGuiComboFlags.None))
                    {
                            var newWorld = _homeWorldPicker.Items[homeWorldId];
                            activeCharacter.WorldId = newWorld.RowId;
                            _characterMonitor.UpdateCharacter(activeCharacter);
                    }

                    int activeWorldId = (int)(activeCharacter.ActiveWorldId);

                    if (_activeWorldPicker.Draw("Active World",
                            _characterMonitor.ActiveCharacter?.ActiveWorld?.Name.ExtractText() ?? "Not Set",
                            "Set the active world of the active character", ref activeWorldId, 100, 32,
                            ImGuiComboFlags.None))
                    {
                        var newWorld = _activeWorldPicker.Items[activeWorldId];
                        activeCharacter.ActiveWorldId = newWorld.RowId;
                        _characterMonitor.UpdateCharacter(activeCharacter);
                    }

                }

            }
        }
    }

    private void DrawDataTab()
    {
        if (ImGui.BeginTabItem("Data"))
        {
            if (ImGui.Button("Add fake spawn data"))
            {
                _mobTracker.AddEntry(new MobSpawnPosition(1,1,1,new Vector3(1,1,1), 1));
            }
            if (ImGui.Button("Load existing inventories.json"))
            {
                _fileDialogManager.OpenFileDialog("Pick a file", "*.*", ConvertFile);
            }
            if (ImGui.Button("Save loaded json to csv"))
            {
                _fileDialogManager.SaveFileDialog("Pick a file", "*.csv", "inventories", ".csv",
                    (b, s) =>
                    {
                        if (b)
                        {
                            CsvLoader.ToCsvRaw<InventoryItem>(_items, s);
                        }
                    });
            }
            if (ImGui.Button("Refresh overlay states"))
            {
                _characterMonitor.OverrideActiveCharacter(_characterMonitor.GetPlayerCharacters().First().Key);
                _overlayService.RefreshOverlayStates();
            }
            if (ImGui.Button("Refresh item counts for inventory"))
            {
                _inventoryMonitor.GenerateItemCounts();
            }
            if (ImGui.Button("Push random item to player bag"))
            {
                var currentHistory = _inventoryHistory.GetHistory();
                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    var fromItem = new InventoryItem();
                    var slot = (short)_rng.Next(0,35);
                    fromItem.Slot = slot;
                    fromItem.ItemId = 0;
                    fromItem.SortedSlotIndex = slot;
                    fromItem.SortedContainer = InventoryType.Bag0;
                    fromItem.SortedCategory = InventoryCategory.CharacterBags;
                    fromItem.RetainerId = activeCharacter.CharacterId;
                    fromItem.Quantity = 0;
                    var toItem = new InventoryItem();
                    toItem.Slot = slot;
                    toItem.ItemId = (uint)_rng.Next(1000,10000);
                    toItem.SortedSlotIndex = slot;
                    toItem.SortedContainer = InventoryType.Bag0;
                    toItem.SortedCategory = InventoryCategory.CharacterBags;
                    toItem.RetainerId = activeCharacter.CharacterId;
                    toItem.Quantity = 1;

                    currentHistory.Add(new InventoryChange(fromItem, toItem, InventoryChangeReason.Added, (uint)currentHistory.Count + 1));
                }
                _inventoryHistory.LoadExistingHistory(currentHistory);
            }

            ImGui.EndTabItem();
        }
    }

    private void ConvertFile(bool success, string fileName)
    {
        if (success)
        {
            try
            {
                Logger.LogDebug("Loading inventories from " + fileName);
                var cacheFile = new FileInfo(fileName);
                string json = File.ReadAllText(cacheFile.FullName, Encoding.UTF8);
                MinifyResolver minifyResolver = new();
                var parsedInventories = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>>(json, new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    ContractResolver = minifyResolver
                });
                _items = parsedInventories.SelectMany(c => c.Value.SelectMany(d => d.Value)).ToList();
            }
            catch (Exception e)
            {
                Logger.LogError("Error while parsing saved saved inventory data, " + e.Message);
            }
        }
    }

    private void DrawWindowTab()
    {
        if (ImGui.BeginTabItem("Windows"))
        {
            if (ImGui.Button("Craft Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
            }

            if (ImGui.Button("Items Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
            }

            if (ImGui.Button("Help Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(HelpWindow)));
            }

            #if DEBUG
            if (ImGui.Button("Debug Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(DebugWindow)));
            }
            #endif

            if (ImGui.Button("Configuration Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
            }

            if (ImGui.Button("Duties Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(DutiesWindow)));
            }

            if (ImGui.Button("Mobs Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(BNpcsWindow)));
            }

            if (ImGui.Button("Airships Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(AirshipsWindow)));
            }

            if (ImGui.Button("Submarines Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(SubmarinesWindow)));
            }

            if (ImGui.Button("Retainer Ventures Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(RetainerTasksWindow)));
            }


            if (ImGui.Button("NPCs Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ENpcsWindow)));
            }

            if (ImGui.Button("Icons Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(IconBrowserWindow)));
            }

            if (ImGui.Button("Intro Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(IntroWindow)));
            }

            if (ImGui.Button("Mock Items Window"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(MockGameItemsWindow)));
            }

            if (ImGui.Button("Configuration Wizard"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
            }

            if (ImGui.Button("ListDebugWindow"))
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ListDebugWindow)));
            }

            if (ImGui.Button("Stack Tool"))
            {
                ImGui.ShowStackToolWindow();
            }

            ImGui.EndTabItem();
        }
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;

}