using System;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Sheets;
using DalamudPluginProjectTemplate;
using DalamudPluginProjectTemplate.Attributes;
using InventoryTools.Ui;

namespace InventoryTools.Commands
{
    public class PluginCommands
    {
        [Command("/allagantools")]
        [Aliases("/atools")]
        [HelpMessage("Shows the allagan tools filter window.")]
        public void ShowHideInventoryToolsCommand(string command, string args)
        {
            PluginService.WindowService.ToggleFiltersWindow();
        }
        [Command("/duties")]
        [Aliases("/atduties")]
        [HelpMessage("Shows the allagan tools duties window.")]
        public void ShowHideDutiesWindow(string command, string args)
        {
            PluginService.WindowService.ToggleDutiesWindow();
        }
        [Command("/mobs")]
        [Aliases("/atmobs")]
        [HelpMessage("Shows the allagan tools mobs window.")]
        public void ShowHideMobsWindow(string command, string args)
        {
            PluginService.WindowService.ToggleMobWindow();
        }
        [Command("/atnpcs")]
        [HelpMessage("Shows the allagan tools npcs window.")]
        public void ShowHideNpcsWindow(string command, string args)
        {
            PluginService.WindowService.ToggleENpcsWindow();
        }
 
        [Command("/inv")]
        [Aliases("/inventorytools")]
        [DoNotShowInHelp]
        [HelpMessage("Shows the allagan tools filter window.")]
        public  void ShowHideInventoryToolsCommand2(string command, string args)
        {
            PluginService.WindowService.ToggleFiltersWindow();
        }


        [Command("/atfiltertoggle")]
        [Aliases("/atf")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand(string command, string args)
        {
            Service.Log.Verbose(command);
            Service.Log.Verbose(args);
            if (args.Trim() == "")
            {
                PluginService.ChatUtilities.PrintError("You must enter the name of a filter.");
            }
            else
            {
                PluginService.FilterService.ToggleActiveBackgroundFilter(args);
            }
        }

        [Command("/invf")]
        [DoNotShowInHelp]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommandIT(string command, string args)
        {
            FilterToggleCommand(command, args);
        }

        [Command("/ifilter")]
        [DoNotShowInHelp]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand3(string command, string args)
        {
            Service.Log.Verbose(command);
            Service.Log.Verbose(args);
            if (args.Trim() == "")
            {
                PluginService.ChatUtilities.PrintError("You must enter the name of a filter.");
            }
            else
            {
                PluginService.FilterService.ToggleActiveBackgroundFilter(args);
            }
        }

        [Command("/openfilter")]
        [HelpMessage("Toggles the specified filter as it's own window.")]
        public  void OpenFilterCommand(string command, string args)
        {
            if (args.Trim() == "")
            {
                PluginService.ChatUtilities.PrintError("You must enter the name of a filter.");
            }
            else
            {
                PluginService.PluginLogic.ToggleWindowFilterByName(args);
            }
        }

        [Command("/crafts")]
        [HelpMessage("Opens the allagan tools crafts window")]
        public  void OpenCraftsWindow(string command, string args)
        {
            PluginService.WindowService.ToggleCraftsWindow();
        }

        [Command("/airships")]
        [HelpMessage("Opens the allagan tools airships window")]
        public  void ToggleAirshipsWindow(string command, string args)
        {
            PluginService.WindowService.ToggleAirshipsWindow();
        }

        [Command("/submarines")]
        [HelpMessage("Opens the allagan tools submarines window")]
        public  void ToggleSubmarinesWindow(string command, string args)
        {
            PluginService.WindowService.ToggleSubmarinesWindow();
        }

        [Command("/retainerventures")]
        [HelpMessage("Opens the allagan tools retainer ventures window")]
        public  void ToggleToggleRetainerTasksWindow(string command, string args)
        {
            PluginService.WindowService.ToggleRetainerTasksWindow();
        }

        [Command("/atconfig")]
        [HelpMessage("Opens the allagan tools configuration window")]
        public  void OpenConfigurationWindow(string command, string args)
        {
            PluginService.WindowService.ToggleConfigurationWindow();
        }

        [Command("/itconfig")]
        [DoNotShowInHelp]
        [HelpMessage("Opens the allagan tools configuration window")]
        public void OpenConfigurationWindowIT(string command, string args)
        {
            OpenConfigurationWindow(command, args);
        }

        [Command("/invconfig")]
        [DoNotShowInHelp]
        [HelpMessage("Opens the allagan tools configuration window")]
        public  void OpenConfigurationWindow2(string command, string args)
        {
            PluginService.WindowService.ToggleConfigurationWindow();
        }

        [Command("/athelp")]
        [HelpMessage("Opens the allagan tools help window")]
        public  void OpenHelpWindow(string command, string args)
        {
            PluginService.WindowService.ToggleHelpWindow();
        }

        [Command("/invhelp")]
        [Aliases("/ithelp")]
        [DoNotShowInHelp]
        [HelpMessage("Opens the allagan tools help window")]
        public  void OpenHelpWindow2(string command, string args)
        {
            PluginService.WindowService.ToggleHelpWindow();
        }
        
        #if DEBUG

        [Command("/atdebug")]
        [HelpMessage("Opens the allagan tools debug window")]
        public  void ToggleDebugWindow(string command, string args)
        {
            PluginService.WindowService.ToggleDebugWindow();
        }

        [Command("/itdebug")]
        [DoNotShowInHelp]
        [HelpMessage("Opens the allagan tools debug window")]
        public  void ToggleDebugWindowIT(string command, string args)
        {
            PluginService.WindowService.ToggleDebugWindow();
        }

        [Command("/atintro")]
        [HelpMessage("Opens the allagan tools debug window")]
        public void ToggleIntroWindow(string command, string args)
        {
            PluginService.WindowService.OpenWindow<IntroWindow>(IntroWindow.AsKey);
        }

        [Command("/itintro")]
        [DoNotShowInHelp]
        [HelpMessage("Opens the allagan tools debug window")]
        public void ToggleIntroWindowIT(string command, string args)
        {
            ToggleIntroWindow(command,args);
        }

        [Command("/atclearfilter")]
        [HelpMessage("Clears the active filter. Pass in background or ui to close just the background or ui filters respectively.")]
        public void ClearFilter(string command, string args)
        {
            args = args.Trim();
            if (args == "")
            {
                PluginService.FilterService.ClearActiveBackgroundFilter();
                PluginService.FilterService.ClearActiveUiFilter();
            }
            else if (args == "background")
            {
                PluginService.FilterService.ClearActiveBackgroundFilter();
            }
            else if (args == "ui")
            {
                PluginService.FilterService.ClearActiveUiFilter();
            }
        }

        [Command("/atclosefilters")]
        [HelpMessage("Closes all filter windows.")]
        public void CloseFilterWindows(string command, string args)
        {
            PluginService.WindowService.CloseFilterWindows();
        }

        [Command("/atclearall")]
        [HelpMessage("Closes all filter windows and clears all active filters. Pass in background or ui to close just the background or ui filters respectively.")]
        public void ClearAll(string command, string args)
        {
            ClearFilter(command, args);
            CloseFilterWindows(command,args);
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

            ItemEx? item;
            if (UInt32.TryParse(args, out uint itemId))
            {
                item = Service.ExcelCache.GetItemExSheet().GetRow(itemId);
            }
            else
            {
                item = Service.ExcelCache.GetItemExSheet().FirstOrDefault(c => c!.SearchString == args.ToParseable(), null);
            }
            if (item != null && item.RowId != 0)
            {
                PluginService.WindowService.OpenItemWindow(item.RowId);
            }
            else
            {
                PluginService.ChatUtilities.PrintError("The item " + args + " could not be found.");
            }
        }

        #endif
    }
}