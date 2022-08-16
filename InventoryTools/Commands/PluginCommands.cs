using CriticalCommonLib.Services;
using Dalamud.Logging;
using DalamudPluginProjectTemplate.Attributes;
using InventoryTools.Ui;

namespace InventoryTools.Commands
{
    public class PluginCommands
    {
        [Command("/allagantools","/inventorytools")]
        [HelpMessage("Shows the inventory tools filter window.")]
        public void ShowHideInventoryToolsCommand(string command, string args)
        {
            PluginService.WindowService.ToggleFiltersWindow();
        }

        [Command("/inv")]
        [HelpMessage("Shows the inventory tools filter window.")]
        public  void ShowHideInventoryToolsCommand2(string command, string args)
        {
            PluginService.WindowService.ToggleFiltersWindow();
        }

        [Command("/atools","/invtools")]
        [HelpMessage("Shows the inventory tools filter window.")]
        public  void ShowHideInventoryToolsCommand3(string command, string args)
        {
            PluginService.WindowService.ToggleFiltersWindow();
        }

        [Command("/atfiltertoggle","/itfiltertoggle")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            if (args.Trim() == "")
            {
                ChatUtilities.PrintError("You must enter the name of a filter.");
            }
            else
            {
                PluginService.FilterService.ToggleActiveBackgroundFilter(args);
            }
        }

        [Command("/atf","/invf")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand2(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            if (args.Trim() == "")
            {
                ChatUtilities.PrintError("You must enter the name of a filter.");
            }
            else
            {
                PluginService.FilterService.ToggleActiveBackgroundFilter(args);
            }
        }

        [Command("/ifilter")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand3(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            if (args.Trim() == "")
            {
                ChatUtilities.PrintError("You must enter the name of a filter.");
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
                ChatUtilities.PrintError("You must enter the name of a filter.");
            }
            else
            {
                PluginService.PluginLogic.ToggleWindowFilterByName(args);
            }
        }

        [Command("/crafts")]
        [HelpMessage("Opens the inventory tools crafts window")]
        public  void OpenCraftsWindow(string command, string args)
        {
            PluginService.WindowService.ToggleCraftsWindow();
        }

        [Command("/atconfig","/itconfig")]
        [HelpMessage("Opens the inventory tools configuration window")]
        public  void OpenConfigurationWindow(string command, string args)
        {
            PluginService.WindowService.ToggleConfigurationWindow();
        }

        [Command("/invconfig")]
        [HelpMessage("Opens the inventory tools configuration window")]
        public  void OpenConfigurationWindow2(string command, string args)
        {
            PluginService.WindowService.ToggleConfigurationWindow();
        }

        [Command("/athelp","/ithelp")]
        [HelpMessage("Opens the inventory tools help window")]
        public  void OpenHelpWindow(string command, string args)
        {
            PluginService.WindowService.ToggleHelpWindow();
        }

        [Command("/invhelp")]
        [HelpMessage("Opens the inventory tools help window")]
        public  void OpenHelpWindow2(string command, string args)
        {
            PluginService.WindowService.ToggleHelpWindow();
        }
        
        #if DEBUG

        [Command("/atdebug","/itdebug")]
        [HelpMessage("Opens the inventory tools debug window")]
        public  void ToggleDebugWindow(string command, string args)
        {
            PluginService.WindowService.ToggleDebugWindow();
        }

        [Command("/atintro","/itintro")]
        [HelpMessage("Opens the inventory tools debug window")]
        public void ToggleIntroWindow(string command, string args)
        {
            PluginService.WindowService.OpenWindow<IntroWindow>(IntroWindow.AsKey);
        }

        #endif
    }
}