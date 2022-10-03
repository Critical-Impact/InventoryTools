using System.Numerics;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Logic;

namespace InventoryTools.Ui
{
    public class IntroWindow : Window
    {
        private TextureWrap _allaganToolsIcon;
        public IntroWindow(string name = "Allagan Tools") : base(name)
        {
            SetupWindow();
        }
        
        public IntroWindow() : base("Allagan Tools")
        {
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
            ImGui.BeginChild("LogoLeft", new Vector2(200, 0));
            ImGui.SetCursorPosY(40);
            ImGui.Image(_allaganToolsIcon.ImGuiHandle, new Vector2(200, 200) * ImGui.GetIO().FontGlobalScale);
            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.BeginChild("IntroRight", new Vector2(0, 0));
            ImGui.TextWrapped("Welcome to Allagan Tools.");
            ImGui.TextWrapped("Allagan Tools is a plugin for Final Fantasy XIV that was formerly called Inventory Tools. The addition of crafting and item windows has made the plugin larger than its original scope.");
            ImGui.TextWrapped("There are now various new windows that can be opened via command shortcuts and from the main window.");
            ImGui.TextWrapped("A large amount of the functionality of the plugin can be accessed by right clicking on items within the various windows. This includes information on ways to obtain items, gathering locations, store locations, recipes and more. ");
            ImGui.TextWrapped("The data parsing for items is still being worked on and as such it will not be as comprehensive as Garland Tools and Teamcraft. However, Allagan Tools is constantly being updated and improved to provide you with the best experience possible.");
            ImGui.NewLine();
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
            ImGui.EndChild();
        }

        public override Vector2 DefaultSize { get; } = new Vector2(600, 350);
        public override Vector2 MaxSize { get; } = new Vector2(600, 360);
        public override Vector2 MinSize { get; } = new Vector2(600, 360);
        public override bool SaveState => false;

        public override ImGuiWindowFlags? WindowFlags { get; } =
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar;
    }
}