using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;

namespace InventoryToolsMock;

public class MockSettingsWindow : Window
{
    public MockSettingsWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
    }

    public MockSettingsWindow() : base("Mock Settings", ImGuiWindowFlags.None, true)
    {
        
    }

    public override void Draw()
    {
        var gamePath = AppSettings.Default.GamePath ?? "";
        var pluginConfigPath = AppSettings.Default.PluginConfigPath ?? "";
        var autoStart = AppSettings.Default.AutoStart;
        
        ImGui.SetNextWindowSize(new Vector2(450,150));
        if (ImGui.Begin("Mock Settings", ImGuiWindowFlags.None))
        {
            if (ImGui.InputTextWithHint("Game Path##gp", "Please enter your game path", ref gamePath, 999))
            {
                if (gamePath != AppSettings.Default.GamePath)
                {
                    AppSettings.Default.GamePath = gamePath;
                }
            }

            ImGuiUtil.HoverTooltip("Must be the game/sqpack directory");

            if (gamePath != "" && !Directory.Exists(gamePath))
            {
                ImGui.Text("The configured path does not exist.");
            }
            
            if (ImGui.InputTextWithHint("Plugin Config Path##pcp", "Please enter your plugin config path", ref pluginConfigPath, 999))
            {
                if (pluginConfigPath != AppSettings.Default.PluginConfigPath)
                {
                    AppSettings.Default.PluginConfigPath = pluginConfigPath;
                }
            }
            
            if (pluginConfigPath != "" && !Directory.Exists(pluginConfigPath))
            {
                ImGui.Text("The configured path does not exist.");
            }
            
                        
            if (ImGui.Checkbox("Auto-start?", ref autoStart))
            {
                if (autoStart != AppSettings.Default.AutoStart)
                {
                    AppSettings.Default.AutoStart = autoStart;
                }
            }
            
            if(AppSettings.Dirty && ImGui.Button("Save"))
            {
                AppSettings.Default.Save();
            }
            
            if (Directory.Exists(gamePath) && Directory.Exists(pluginConfigPath))
            {
                if (Program._mockPlugin == null && ImGui.Button("Start Plugin"))
                {
                    Program.StartPlugin();
                }
                else if (Program._mockPlugin != null && ImGui.Button("Stop Plugin"))
                {
                    Program.StopPlugin();
                }
            }
        }
        
        ImGui.End();
    }
}