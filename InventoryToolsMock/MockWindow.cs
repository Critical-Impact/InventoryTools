using System.Numerics;
using ImGuiNET;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Ui;

namespace InventoryToolsMock;

public class MockWindow : Window
{
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
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
}