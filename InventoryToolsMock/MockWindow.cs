using System.Numerics;
using System.Text;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Ui;
using LuminaSupplemental.Excel.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OtterGui.Raii;
using QoLBar;

namespace InventoryToolsMock;

public class MockWindow : Window
{
    private List<InventoryItem> _items;

    public MockWindow(string name = "Mock Tools", ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
    }
    
    public MockWindow() : base("Mock Tools")
    {
    }

    public override void OnClose()
    {
        IsOpen = true;
    }

    public static string AsKey => "mock";
    public override string Key => AsKey;
    public override bool DestroyOnClose { get; } = false;
    public override bool SaveState { get; } = true;
    public override Vector2 DefaultSize => new Vector2(200, 200);
    public override Vector2 MaxSize => new Vector2(2000, 2000);
    public override Vector2 MinSize => new Vector2(200, 200);

    public override void Draw()
    {
        if (ImGui.BeginTabBar("MockTabs"))
        {
            DrawWindowTab();
            DrawDataTab();
            DrawCharacterTab();
            ImGui.EndTabBar();
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

            ImGui.EndTabItem();
        }
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
}