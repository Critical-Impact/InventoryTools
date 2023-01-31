using System.Numerics;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;

namespace InventoryTools.Ui
{
    public class HelpWindow : Window
    {
        public override bool SaveState => false;
        public static string AsKey => "help";
        public override  string Key => AsKey;
        public override Vector2 DefaultSize { get; } = new Vector2(700, 700);
        public override  Vector2 MaxSize { get; } = new Vector2(2000, 2000);
        public override  Vector2 MinSize { get; } = new Vector2(200, 200);
        public override bool DestroyOnClose => true;

        public HelpWindow(string name = "Allagan Tools - Help") : base(name)
        {
            
        }
        public HelpWindow() : base("Allagan Tools - Help")
        {
            
        }
        
        public override void Draw()
        {
            ImGui.BeginChild("###ivHelpList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true);
            if (ImGui.Selectable("General", ConfigurationManager.Config.SelectedHelpPage == 0))
            {
                ConfigurationManager.Config.SelectedHelpPage = 0;
            }

            if (ImGui.Selectable("Filtering", ConfigurationManager.Config.SelectedHelpPage == 1))
            {
                ConfigurationManager.Config.SelectedHelpPage = 1;
            }

            if (ImGui.Selectable("About", ConfigurationManager.Config.SelectedHelpPage == 2))
            {
                ConfigurationManager.Config.SelectedHelpPage = 2;
            }

            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("###ivHelpView", new Vector2(-1, -1), true);
            if (ConfigurationManager.Config.SelectedHelpPage == 0)
            {
                ImGui.Text("This is a very basic guide, for more information please see the wiki.");
                if (ImGui.Button("Open Wiki"))
                {
                    "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview".OpenBrowser();
                }

                ImGui.Text("Basic Plugin Information:");
                ImGui.Separator();
                ImGui.TextWrapped(
                    "Allagan Tools will track the multitude of inventories in the game. It also gives you the ability to highlight where items are within said inventories.");
                ImGui.TextWrapped(
                    "I've taken a small amount of inspiration from Teamcraft and full credit to them for the idea of the inventory optimisations that their application provides.");
                ImGui.TextWrapped(
                    "The plugin has been built for speed and such it can't quite do every inventory optimisation that Teamcraft can do but it's getting there.");
                ImGui.NewLine();
                ImGui.Text("Concepts:");
                ImGui.Separator();
                ImGui.TextWrapped(
                    "Filters: At present you can only have 1 filter enabled at a time. There are 2 filters available, one is the window filter and one is the background filter. When a filter is active, it enables highlighting and lets you see the relevant items.");
                ImGui.TextWrapped(
                    "Window Filter: When the allagan Tools window is visible, this is the filter that will be used to determine what to highlight.");
                ImGui.TextWrapped(
                    "Background Filter: When the allagan Tools window is closed, this is the filter that will be used to determine what to highlight. On top of this, it can be toggled on/off with the commands listed below. The intention is that you could have macros setup to toggle the filters on/off.");
                ImGui.NewLine();
                ImGui.Text("Commands:");
                ImGui.Separator();
                ImGui.TextWrapped("The below commands will open/close the main allagan Tools window.");
                ImGui.Text("/allagantools,/inventorytools, /inv, /invtools");
                ImGui.TextWrapped("The below commands will toggle the background filter specified with <name>.");
                ImGui.Text("/itfiltertoggle <name>, /invf <name>, /ifilter <name>");
            }
            else if (ConfigurationManager.Config.SelectedHelpPage == 1)
            {
                ImGui.Text("Advanced Filtering:");
                ImGui.Separator();
                ImGui.TextWrapped(
                    "When creating a filter or when searching through the results of a filter it is possible to use a series of operators to make your search more specific. The available operators are dependant on what you searching against but at present support for !, <, >, >=, <=, = is present.");
                ImGui.TextWrapped(
                    "! - Show any results that do not contain what is entered - available for text and numbers.");
                ImGui.TextWrapped(
                    "< - Show any results that have a value less than what is entered - available for numbers.");
                ImGui.TextWrapped(
                    "> - Show any results that have a value greater than what is entered - available for numbers.");
                ImGui.TextWrapped(
                    ">= - Show any results that have a value greater or equal to what is entered - available for numbers.");
                ImGui.TextWrapped(
                    "<= - Show any results that have a value less than or equal to what is entered - available for numbers.");
                ImGui.TextWrapped(
                    "= - Show any results that have a value equal to exactly what is entered - available for text and numbers.");
                ImGui.TextWrapped("&& and || AND and OR respectively - Can be used to chain operators together.");
            }
            else if (ConfigurationManager.Config.SelectedHelpPage == 2)
            {
                ImGui.Text("About:");
                ImGui.Text(
                    "This plugin is written in some of the free time that I have, it's a labour of love and I will hopefully be actively releasing updates for a while.");
                ImGui.Text(
                    "If you run into any issues please submit feedback via the plugin installer feedback button.");
                ImGui.Text("Plugin Wiki: ");
                ImGui.SameLine();
                if (ImGui.Button("Open##WikiBtn"))
                {
                    "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview".OpenBrowser();
                }

                ImGui.Text("Found a bug?");
                ImGui.SameLine();
                if (ImGui.Button("Open##BugBtn"))
                {
                    "https://github.com/Critical-Impact/InventoryTools/issues".OpenBrowser();
                }

                ImGui.Separator();
                if (ConfigurationManager.Config.TetrisEnabled)
                {
                    if (ImGui.Button("I do not like tetris"))
                    {
                        ConfigurationManager.Config.TetrisEnabled = false;
                    }
                }
                else
                {
                    if (ImGui.Button("I like tetris"))
                    {
                        ConfigurationManager.Config.TetrisEnabled = true;
                    }
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