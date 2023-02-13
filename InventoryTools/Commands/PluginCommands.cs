using CriticalCommonLib.Services;
using Dalamud.Logging;
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
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
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
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
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
        [DoNotShowInHelp]
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
        [DoNotShowInHelp]
        [HelpMessage("Closes all filter windows.")]
        public void CloseFilterWindows(string command, string args)
        {
            PluginService.WindowService.CloseFilterWindows();
        }

        [Command("/atclearall")]
        [DoNotShowInHelp]
        [HelpMessage("Closes all filter windows and clears all active filters. Pass in background or ui to close just the background or ui filters respectively.")]
        public void ClearAll(string command, string args)
        {
            ClearFilter(command, args);
            CloseFilterWindows(command,args);
        }

        #endif
    }
}