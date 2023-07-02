using System.Numerics;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using OtterGui.Raii;

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
            using (var sideBarChild =
                   ImRaii.Child("SideBar", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (sideBarChild.Success)
                {
                    if (ImGui.Selectable("General", ConfigurationManager.Config.SelectedHelpPage == 0))
                    {
                        ConfigurationManager.Config.SelectedHelpPage = 0;
                    }

                    if (ImGui.Selectable("Filter Basics", ConfigurationManager.Config.SelectedHelpPage == 1))
                    {
                        ConfigurationManager.Config.SelectedHelpPage = 1;
                    }

                    if (ImGui.Selectable("Filtering", ConfigurationManager.Config.SelectedHelpPage == 2))
                    {
                        ConfigurationManager.Config.SelectedHelpPage = 2;
                    }

                    if (ImGui.Selectable("About", ConfigurationManager.Config.SelectedHelpPage == 3))
                    {
                        ConfigurationManager.Config.SelectedHelpPage = 3;
                    }
                }
            }

            ImGui.SameLine();

            using (var mainChild = ImRaii.Child("###ivHelpView", new Vector2(-1, -1), true))
            {
                if (mainChild.Success)
                {
                    if (ConfigurationManager.Config.SelectedHelpPage == 0)
                    {
                        ImGui.TextUnformatted("This is a very basic guide, for more information please see the wiki.");
                        if (ImGui.Button("Open Wiki"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview".OpenBrowser();
                        }

                        ImGui.TextUnformatted("Basic Plugin Information:");
                        ImGui.Separator();
                        ImGui.TextWrapped(
                            "Allagan Tools will track the multitude of inventories in the game. It also gives you the ability to highlight where items are within said inventories.");
                        ImGui.TextWrapped(
                            "I've taken a small amount of inspiration from Teamcraft and full credit to them for the idea of the inventory optimisations that their application provides.");
                        ImGui.TextWrapped(
                            "The plugin has been built for speed and such it can't quite do every inventory optimisation that Teamcraft can do but it's getting there.");
                        ImGui.NewLine();
                        ImGui.TextUnformatted("Concepts:");
                        ImGui.Separator();
                        ImGui.TextWrapped(
                            "Filters: At present you can only have 1 filter enabled at a time. There are 2 filters available, one is the window filter and one is the background filter. When a filter is active, it enables highlighting and lets you see the relevant items.");
                        ImGui.TextWrapped(
                            "Window Filter: When the allagan Tools window is visible, this is the filter that will be used to determine what to highlight.");
                        ImGui.TextWrapped(
                            "Background Filter: When the allagan Tools window is closed, this is the filter that will be used to determine what to highlight. On top of this, it can be toggled on/off with the commands listed below. The intention is that you could have macros setup to toggle the filters on/off.");
                        ImGui.NewLine();
                        ImGui.TextUnformatted("Commands:");
                        ImGui.Separator();
                        ImGui.TextWrapped("The below commands will open/close the main allagan Tools window.");
                        ImGui.TextUnformatted("/allagantools,/inventorytools, /inv, /invtools");
                        ImGui.TextWrapped(
                            "The below commands will toggle the background filter specified with <name>.");
                        ImGui.TextUnformatted("/itfiltertoggle <name>, /invf <name>, /ifilter <name>");
                    }
                    else if (ConfigurationManager.Config.SelectedHelpPage == 1)
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.Text("Filters are the core way the plugin provides a way for you to view the items you are looking for or are attempting to sort.");
                        ImGui.Text("There are currently 3 types of filter that can be created.");
                        ImGui.PopTextWrapPos();
                        ImGui.NewLine();

                        ImGui.Text("Search Filter");
                        ImGui.PushTextWrapPos();

                        ImGui.TextUnformatted("This type of filter allows you to filter all the items you currently have stored across your characters and retainers. If you want to be able to find an item but don't really care about where the item ends up this is the filter type to use.");
                        ImGui.TextUnformatted("Example Usages:");
                        ImGui.BulletText("Finding materials for a craft");
                        ImGui.BulletText("Finding a housing item you put somewhere");
                        ImGui.BulletText("Seeing how much an item you just picked up is worth");
                        ImGui.BulletText("Seeing if a specific item is already in your glamour chest or armoire");
                        ImGui.BulletText("Checking your retainers equipment without actually going to a retainer bell");
                        ImGui.BulletText("Checking if any items you have can go into the armoire");
                        ImGui.PopTextWrapPos();
                        ImGui.NewLine();

                        ImGui.Text("Sort Filter");
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("This type of filter does exactly what the search filter does but once it has found the items, it takes the entire list of items and then calculates a list of where those items should go based on the filter's configuration. If you want to be able to store your items on your retainers without doubling up this is the filter type to use.");
                        ImGui.TextUnformatted("Example Usages:");
                        ImGui.BulletText("Putting away materials after a craft and not having them double up");
                        ImGui.BulletText("Store items above a certain item level within your chocobo saddlebag for later");
                        ImGui.PopTextWrapPos();

                        ImGui.NewLine();
                        ImGui.Text("Game Item Filter");
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("This filter allows you search across all the items that exist within the game's catalogue of items.");
                        ImGui.TextUnformatted("Example Usages:");
                        ImGui.BulletText("Searching for glamours");
                        ImGui.BulletText("Seeing what mounts/minions you haven't obtained");
                        ImGui.BulletText("Tracking the prices of all the items within the game");
                        ImGui.PopTextWrapPos();
                    }
                    else if (ConfigurationManager.Config.SelectedHelpPage == 2)
                    {
                        ImGui.TextUnformatted("Advanced Filtering:");
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
                        ImGui.TextWrapped(
                            "&& and || AND and OR respectively - Can be used to chain operators together.");
                    }
                    else if (ConfigurationManager.Config.SelectedHelpPage == 3)
                    {
                        ImGui.TextUnformatted("About:");
                        ImGui.TextUnformatted(
                            "This plugin is written in some of the free time that I have, it's a labour of love and I will hopefully be actively releasing updates for a while.");
                        ImGui.TextUnformatted(
                            "If you run into any issues please submit feedback via the plugin installer feedback button.");
                        ImGui.TextUnformatted("Plugin Wiki: ");
                        ImGui.SameLine();
                        if (ImGui.Button("Open##WikiBtn"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview".OpenBrowser();
                        }

                        ImGui.TextUnformatted("Found a bug?");
                        ImGui.SameLine();
                        if (ImGui.Button("Open##BugBtn"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/issues".OpenBrowser();
                        }
                    }
                }
            }
        }
        
        public override FilterConfiguration? SelectedConfiguration => null;

        public override void Invalidate()
        {
            
        }
    }
}