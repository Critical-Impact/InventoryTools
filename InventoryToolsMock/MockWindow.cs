using System.Numerics;
using System.Text;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using CriticalCommonLib.Services.Ui;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Ui;
using LuminaSupplemental.Excel.Model;
using Newtonsoft.Json;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using OtterGui.Widgets;
using QoLBar;

namespace InventoryToolsMock;

public class MockWindow : Window
{
    private List<InventoryItem> _items;

    public MockWindow(string name = "Mock Tools", ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
        _rng = new Random();
    }
    
    public MockWindow() : base("Mock Tools")
    {
        _rng = new Random();
    }

    public override void OnClose()
    {
        IsOpen = true;
    }

    public static string AsKey => "mock";
    public override string Key => AsKey;
    public override bool DestroyOnClose { get; } = false;
    public override bool SaveState { get; } = true;
    public override Vector2? DefaultSize => new Vector2(200, 200);
    public override Vector2? MaxSize => new Vector2(2000, 2000);
    public override Vector2? MinSize => new Vector2(200, 200);
    private Random _rng;

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
                    if (PluginService.CraftMonitor is MockCraftMonitor craftMonitor)
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
        }
    }

    private void DrawCharacterTab()
    {
        using (var characterTab = ImRaii.TabItem("Characters"))
        {
            if (characterTab.Success)
            {
                using (var combo = ImRaii.Combo("Active Character", PluginService.CharacterMonitor.ActiveCharacter?.FormattedName ?? "N/A"))
                {
                    if (combo.Success)
                    {
                        foreach (var character in PluginService.CharacterMonitor.GetPlayerCharacters())
                        {
                            if (ImGui.Selectable(character.Value.FormattedName + "##" + character.Key, PluginService.CharacterMonitor.ActiveCharacterId == character.Key))
                            {
                                PluginService.CharacterMonitor.OverrideActiveCharacter(character.Key);
                            }
                        }
                    }
                }
                using (var combo = ImRaii.Combo("Active Retainer", PluginService.CharacterMonitor.ActiveRetainer?.FormattedName ?? "N/A"))
                {
                    if (combo.Success)
                    {
                        foreach (var character in PluginService.CharacterMonitor.GetRetainerCharacters())
                        {
                            if (ImGui.Selectable(character.Value.FormattedName + "##" + character.Key, PluginService.CharacterMonitor.ActiveRetainerId == character.Key))
                            {
                                PluginService.CharacterMonitor.OverrideActiveRetainer(character.Key);
                            }
                        }
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
                PluginService.MobTracker.AddEntry(new MobSpawnPosition(1,1,1,new Vector3(1,1,1), 1));
            }
            if (ImGui.Button("Load existing inventories.json"))
            {
                PluginService.FileDialogManager.OpenFileDialog("Pick a file", "*.*", ConvertFile);
            }
            if (ImGui.Button("Save loaded json to csv"))
            {
                PluginService.FileDialogManager.SaveFileDialog("Pick a file", "*.csv", "inventories", ".csv",
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
                PluginService.CharacterMonitor.OverrideActiveCharacter(PluginService.CharacterMonitor.GetPlayerCharacters().First().Key);
                PluginService.OverlayService.RefreshOverlayStates();
            }
            if (ImGui.Button("Refresh item counts for inventory"))
            {
                PluginService.InventoryMonitor.GenerateItemCounts();
            }
            if (ImGui.Button("Push random item to player bag"))
            {
                var currentHistory = PluginService.InventoryHistory.GetHistory();
                var activeCharacter = PluginService.CharacterMonitor.ActiveCharacter;
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
                PluginService.InventoryHistory.LoadExistingHistory(currentHistory);
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
                PluginLog.Verbose("Loading inventories from " + fileName);
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
                PluginLog.Error("Error while parsing saved saved inventory data, " + e.Message);
            }
        }
    }

    private static void DrawWindowTab()
    {
        if (ImGui.BeginTabItem("Windows"))
        {
            if (ImGui.Button("Craft Window"))
            {
                PluginService.WindowService.ToggleCraftsWindow();
            }

            if (ImGui.Button("Filters Window"))
            {
                PluginService.WindowService.ToggleFiltersWindow();
            }

            if (ImGui.Button("Help Window"))
            {
                PluginService.WindowService.ToggleHelpWindow();
            }

            if (ImGui.Button("Debug Window"))
            {
                PluginService.WindowService.ToggleDebugWindow();
            }

            if (ImGui.Button("Configuration Window"))
            {
                PluginService.WindowService.ToggleConfigurationWindow();
            }

            if (ImGui.Button("Duties Window"))
            {
                PluginService.WindowService.ToggleDutiesWindow();
            }

            if (ImGui.Button("Mobs Window"))
            {
                PluginService.WindowService.ToggleMobWindow();
            }

            if (ImGui.Button("Airships Window"))
            {
                PluginService.WindowService.ToggleAirshipsWindow();
            }

            if (ImGui.Button("Submarines Window"))
            {
                PluginService.WindowService.ToggleSubmarinesWindow();
            }

            if (ImGui.Button("Retainer Ventures Window"))
            {
                PluginService.WindowService.ToggleSubmarinesWindow();
            }


            if (ImGui.Button("NPCs Window"))
            {
                PluginService.WindowService.ToggleWindow<ENpcsWindow>(ENpcsWindow.AsKey);
            }

            if (ImGui.Button("Icons Window"))
            {
                PluginService.WindowService.ToggleWindow<IconBrowserWindow>(IconBrowserWindow.AsKey);
            }

            if (ImGui.Button("Intro Window"))
            {
                PluginService.WindowService.ToggleWindow<IntroWindow>(IntroWindow.AsKey);
            }

            if (ImGui.Button("Mock Items Window"))
            {
                PluginService.WindowService.ToggleWindow<MockGameItemsWindow>(MockGameItemsWindow.AsKey);
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