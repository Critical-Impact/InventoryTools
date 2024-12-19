using System;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using InventoryTools.Attributes;
using InventoryTools.Mediator;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Commands
{
    public class PluginCommands
    {
        public ILogger<PluginCommands> Logger { get; }
        private readonly MediatorService _mediatorService;
        private readonly IChatUtilities _chatUtilities;
        private readonly ItemSheet _itemSheet;
        private readonly IListService _listService;

        public PluginCommands(MediatorService mediatorService, IChatUtilities chatUtilities, ItemSheet itemSheet, IListService listService, ILogger<PluginCommands> logger)
        {
            Logger = logger;
            _mediatorService = mediatorService;
            _chatUtilities = chatUtilities;
            _itemSheet = itemSheet;
            _listService = listService;
        }

        [Command("/allagantools")]
        [Aliases("/atools")]
        [HelpMessage("Shows the allagan tools items list window.")]
        public void ShowHideInventoryToolsCommand(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(FiltersWindow)));
        }
        [Command("/duties")]
        [Aliases("/atduties")]
        [HelpMessage("Shows the allagan tools duties window.")]
        public void ShowHideDutiesWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(DutiesWindow)));
        }
        [Command("/mobs")]
        [Aliases("/atmobs")]
        [HelpMessage("Shows the allagan tools mobs window.")]
        public void ShowHideMobsWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(BNpcsWindow)));
        }
        [Command("/atnpcs")]
        [HelpMessage("Shows the allagan tools npcs window.")]
        public void ShowHideNpcsWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(ENpcsWindow)));
        }


        [Command("/athighlight")]
        [Aliases("/atf")]
        [HelpMessage("Toggles the specified list's highlight on/off, turning off any other highlighting in the process.")]
        public  void FilterToggleCommand(string command, string args)
        {
            Logger.LogTrace(command);
            Logger.LogTrace(args);
            if (args.Trim() == "")
            {
                Service.Chat.PrintError("You must enter the name of an list.");
            }
            else
            {
                _listService.ToggleActiveBackgroundList(args);
            }
        }

        [Command("/openlist")]
        [HelpMessage("Open/closes a window displaying the contents of a single list.")]
        public  void OpenFilterCommand(string command, string args)
        {
            if (args.Trim() == "")
            {
                Service.Chat.PrintError("You must enter the name of a list.");
            }
            else
            {
                var list = _listService.GetListByKeyOrName(args.Trim());
                if (list != null)
                {
                    _mediatorService.Publish(new ToggleStringWindowMessage(typeof(FilterWindow), list.Key));
                }
                else
                {
                    Service.Chat.PrintError("Could not find a list with that name.");
                }
            }
        }

        [Command("/crafts")]
        [HelpMessage("Opens the allagan tools crafts window")]
        public  void OpenCraftsWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(CraftsWindow)));
        }

        [Command("/airships")]
        [HelpMessage("Opens the allagan tools airships window")]
        public  void ToggleAirshipsWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(AirshipsWindow)));
        }

        [Command("/submarines")]
        [HelpMessage("Opens the allagan tools submarines window")]
        public  void ToggleSubmarinesWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(SubmarinesWindow)));
        }

        [Command("/retainerventures")]
        [HelpMessage("Opens the allagan tools retainer ventures window")]
        public  void ToggleToggleRetainerTasksWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(RetainerTasksWindow)));
        }

        [Command("/atconfig")]
        [HelpMessage("Opens the allagan tools configuration window")]
        public  void OpenConfigurationWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(ConfigurationWindow)));
        }

        [Command("/athelp")]
        [HelpMessage("Opens the allagan tools help window")]
        public  void OpenHelpWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(HelpWindow)));
        }

        #if DEBUG

        [Command("/atdebug")]
        [HelpMessage("Opens the allagan tools debug window")]
        public  void ToggleDebugWindow(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(DebugWindow)));
        }
#endif

        [Command("/atclearhighlights", "/atclearfilter")]
        [HelpMessage("Clears the currently active highlighting. Pass in background or ui to turn off highlighting for the background and ui highlighting respectively.")]
        public void ClearFilter(string command, string args)
        {
            args = args.Trim();
            if (args == "")
            {
                _listService.ClearActiveBackgroundList();
                _listService.ClearActiveUiList();
            }
            else if (args == "background")
            {
                _listService.ClearActiveBackgroundList();
            }
            else if (args == "ui")
            {
                _listService.ClearActiveUiList();
            }
        }

        [Command("/atcloselists", "/atclosefilters")]
        [HelpMessage("Closes all list windows.")]
        public void CloseFilterWindows(string command, string args)
        {
            _mediatorService.Publish(new CloseWindowsByTypeMessage(typeof(FilterWindow)));
        }

        [Command("/atclearall")]
        [HelpMessage("Closes all list windows and clears all active highlighting. Pass in background or ui to close just the background or ui highlighting respectively.")]
        public void ClearAll(string command, string args)
        {
            ClearFilter(command, args);
            CloseFilterWindows(command,args);
        }

        [Command("/craftoverlay")]
        [HelpMessage("Toggles the crafting overlay window.")]
        public void CraftOverlay(string command, string args)
        {
            _mediatorService.Publish(new ToggleGenericWindowMessage(typeof(CraftOverlayWindow)));
        }

        [Command("/moreinfo")]
        [Aliases("/itemwindow")]
        [HelpMessage("Opens the more information window for a specific item. Provide the name of the item or the ID of the item.")]
        public void MoreInformation(string command, string args)
        {
            args = args.Trim();
            if(args == "")
            {
                return;
            }

            ItemRow? item = null;
            if (UInt32.TryParse(args, out uint itemId))
            {
                item = _itemSheet.GetRowOrDefault(itemId);
            }
            else
            {
                if (_itemSheet.ItemsBySearchString.TryGetValue(args.ToParseable(), out itemId))
                {
                    item = _itemSheet.GetRowOrDefault(itemId);
                }
            }
            if (item != null && item.RowId != 0)
            {
                _mediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.RowId));
            }
            else
            {
                _chatUtilities.PrintError("The item " + args + " could not be found.");
            }
        }


    }
}