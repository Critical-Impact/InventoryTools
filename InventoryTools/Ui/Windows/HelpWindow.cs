using System.Numerics;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    public class HelpWindow : GenericWindow
    {
        private readonly InventoryToolsConfiguration _configuration;

        public HelpWindow(ILogger<HelpWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "Help Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _configuration = configuration;
        }
        public override void Initialize()
        {
            WindowName = "Help";
            Key = "help";
        }

        public override bool SaveState => false;
        public override Vector2? DefaultSize { get; } = new Vector2(700, 700);
        public override  Vector2? MaxSize { get; } = new Vector2(2000, 2000);
        public override  Vector2? MinSize { get; } = new Vector2(200, 200);
        public override string GenericKey { get; } = "help";
        public override string GenericName { get; } = "Help";
        public override bool DestroyOnClose => true;


        public override void Draw()
        {
            using (var sideBarChild =
                   ImRaii.Child("SideBar", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (sideBarChild.Success)
                {
                    if (ImGui.Selectable("1. General", _configuration.SelectedHelpPage == 0))
                    {
                        _configuration.SelectedHelpPage = 0;
                    }

                    if (ImGui.Selectable("2. Filter Basics", _configuration.SelectedHelpPage == 1))
                    {
                        _configuration.SelectedHelpPage = 1;
                    }

                    if (ImGui.Selectable("3. Filtering", _configuration.SelectedHelpPage == 2))
                    {
                        _configuration.SelectedHelpPage = 2;
                    }

                    if (ImGui.Selectable("4. About", _configuration.SelectedHelpPage == 3))
                    {
                        _configuration.SelectedHelpPage = 3;
                    }
                }
            }

            ImGui.SameLine();

            using (var mainChild = ImRaii.Child("###ivHelpView", new Vector2(-1, -1), true))
            {
                if (mainChild.Success)
                {
                    if (_configuration.SelectedHelpPage == 0)
                    {
                        ImGui.TextWrapped(
                            "Allagan Tools is a mult-purpose plugin providing 3 primary features, tracking/displaying your inventory data, helping you plan crafts and providing information about items. There are other features, and they are covered in 'Features'");
                        ImGui.TextWrapped(
                            "If you've used Teamcraft or Garland Tools, it takes some inspiration from both.");
                        ImGui.NewLine();
                        ImGui.TextUnformatted("Inventory Tracking:");
                        ImGui.Separator();
                        ImGui.TextWrapped("The plugin will do it's best to keep track of your inventories. Some inventories are only cached when they are first accessed. If you aren't seeing your retainer/free company/glamour chest/etc then please be sure to view them first otherwise the plugin will not be able to cache them.");
                        ImGui.TextWrapped("Once the plugin knows about the items, you can create lists to narrow down searches for specific items, help you sort the items and a myriad of other things.");
                        ImGui.NewLine();

                        ImGui.TextUnformatted("Craft Planning:");
                        ImGui.Separator();
                        ImGui.TextWrapped("The plugin has a dedicated crafts window that lets you create lists of items you want to craft. It'll create a plan that breaks each item down into it's individual parts and will tell you what you're missing. It'll tell you where everything you need is and if you are missing anything, it'll direct you to the place to find/buy the missing items.");
                        ImGui.TextWrapped("If you've ever used Teamcraft, you should be right at home.");
                        ImGui.NewLine();

                        ImGui.TextUnformatted("Item Information:");
                        ImGui.Separator();
                        ImGui.TextWrapped("The plugin has a fairly comprehensive database of information about each item. If you've used garland tools, the information provided is very similar. Clicking on an item's icon within the plugin will always open the item's information window.");
                        ImGui.NewLine();

                        ImGui.TextUnformatted("Highlighting:");
                        ImGui.Separator();
                        ImGui.TextWrapped("When using either an item list or a craft list, you can toggle highlighting. This will highlight the items in game so that you can see exactly where the items are. When the plugins windows are active, you can hit the 'Highlight' checkbox to activate highlighting for that list. If you want to trigger this with a macro, please have a look at the commands section of help, you can toggle 'background' highlighting.");
                        ImGui.NewLine();

                        ImGui.TextUnformatted("This is a very basic guide, for more information please see the wiki.");
                        if (ImGui.Button("Open Wiki"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview".OpenBrowser();
                        }
                    }
                    else if (_configuration.SelectedHelpPage == 1)
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.Text("Lists are the core way the plugin provides a way for you to view the items you are looking for or are attempting to sort.");
                        ImGui.Text("There are currently 3 types of list that can be created.");
                        ImGui.PopTextWrapPos();
                        ImGui.NewLine();

                        ImGui.Text("Search List");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();

                        ImGui.TextUnformatted("This type of list allows you search for specific items across all your inventories. If you just need to find an item, but don't want help sorting it, this is the list type you want.");
                        ImGui.TextUnformatted("Example Usages:");
                        ImGui.BulletText("Finding materials for a craft.");
                        ImGui.BulletText("Finding a housing item you put somewhere.");
                        ImGui.BulletText("Seeing how much an item you just picked up is worth.");
                        ImGui.BulletText("Seeing if a specific item is already in your glamour chest or armoire.");
                        ImGui.BulletText("Checking your retainers equipment without actually going to a retainer bell.");
                        ImGui.BulletText("Checking if any items you have can go into the armoire.");
                        ImGui.PopTextWrapPos();
                        ImGui.NewLine();

                        ImGui.Text("Sort Filter");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("This type of list builds on top of the 'Search List' but also lets you pick where you want the items to be sorted. It'll attempt to show you the most optimized plan for storing the items in the destinations you pick.");
                        ImGui.TextUnformatted("Example Usages:");
                        ImGui.BulletText("Putting away materials after a craft and not having them double up.");
                        ImGui.BulletText("Store items above a certain item level within your chocobo saddlebag for later.");
                        ImGui.BulletText("Find items that are unique to your free company chest and put them there.");
                        ImGui.PopTextWrapPos();

                        ImGui.NewLine();
                        ImGui.Text("Game Item Filter");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("This filter allows you search across all the items that exist within the game's catalogue of items.");
                        ImGui.TextUnformatted("Example Usages:");
                        ImGui.BulletText("Searching for glamours");
                        ImGui.BulletText("Seeing what mounts/minions you haven't obtained");
                        ImGui.BulletText("Tracking the prices of all the items within the game");
                        ImGui.PopTextWrapPos();
                    }
                    else if (_configuration.SelectedHelpPage == 2)
                    {
                        ImGui.TextUnformatted("Advanced Search/Filter Syntax:");
                        ImGui.Separator();
                        ImGui.TextWrapped(
                            "When creating a list or when searching through the results of a list it is possible to use a series of operators to make your search more specific. The available operators are dependant on what you searching against but at present support for !, <, >, >=, <=, = is present.");
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
                    else if (_configuration.SelectedHelpPage == 3)
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