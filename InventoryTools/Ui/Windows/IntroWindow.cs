using System.Numerics;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Logic;
using OtterGui.Raii;

namespace InventoryTools.Ui
{
    public class IntroWindow : Window
    {
        private TextureWrap _allaganToolsIcon;
        public IntroWindow(string name = "Allagan Tools") : base(name)
        {
            Flags =
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar;
            SetupWindow();
        }
        
        public IntroWindow() : base("Allagan Tools")
        {
            Flags =
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar;
            SetupWindow();
        }
        
        private void SetupWindow()
        {
            _allaganToolsIcon = PluginService.PluginLogic.LoadImage("icon-hor");
        }
        
        public override void Invalidate()
        {
        }

        public static string AsKey => "Intro";
        public override FilterConfiguration? SelectedConfiguration => null;
        public override string Key => AsKey;
        public override bool DestroyOnClose { get; } = true;
        public override void Draw()
        {
            using (var leftChild = ImRaii.Child("Left", new Vector2(200, 0)))
            {
                if (leftChild.Success)
                {
                    ImGui.SetCursorPosY(40);
                    ImGui.Image(_allaganToolsIcon.ImGuiHandle, new Vector2(200, 200) * ImGui.GetIO().FontGlobalScale);
                }
            }
            ImGui.SameLine();
            using (var rightChild = ImRaii.Child("Right", new Vector2(0, 0), false, ImGuiWindowFlags.NoScrollbar))
            {
                if (rightChild.Success)
                {
                    using (var textChild = ImRaii.Child("Text", new Vector2(0, -32)))
                    {
                        if (textChild.Success)
                        {
                            ImGui.TextWrapped("Welcome to Allagan Tools.");
                            ImGui.TextWrapped(
                                "Allagan Tools is a plugin for Final Fantasy XIV that provides the following features:");
                            using (ImRaii.PushIndent())
                            {
                                ImGui.Bullet();
                                ImGui.Text("Track your inventories");
                                ImGui.Text("Plan your crafts");
                                ImGui.Text("Provide information about items, monsters, duties and much more");
                            }
                            
                            ImGui.TextWrapped(
                                "You can open various new windows using command shortcuts(the main filter  or from the main window.");
                            ImGui.TextWrapped(
                                "If you're unsure, right-click on an item or a table row for more options!");
                            ImGui.TextWrapped(
                                "To learn about the different features, I recommend going to the settings section and reading the information provided by the ? icons.");
                        }
                    }

                    using (var buttonsChild = ImRaii.Child("Buttons", new Vector2(0, 32)))
                    {
                        if (buttonsChild.Success)
                        {
                            if (ImGui.Button("Close"))
                            {
                                Close();
                            }

                            ImGui.SameLine(0, 4);
                            if (ImGui.Button("Close & Open Main Window"))
                            {
                                Close();
                                PluginService.WindowService.OpenWindow<FiltersWindow>(FiltersWindow.AsKey);
                            }
                        }
                    }
                }
            }
        }

        public override Vector2 DefaultSize { get; } = new Vector2(600, 350);
        public override Vector2 MaxSize { get; } = new Vector2(600, 360);
        public override Vector2 MinSize { get; } = new Vector2(600, 360);
        public override bool SaveState => false;
    }
}