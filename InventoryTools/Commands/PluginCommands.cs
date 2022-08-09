using CriticalCommonLib.Services;
using Dalamud.Logging;
using DalamudPluginProjectTemplate.Attributes;
using InventoryTools.Ui;

namespace InventoryTools.Commands
{
    public class PluginCommands
    {
        [Command("/inventorytools")]
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

        [Command("/invtools")]
        [HelpMessage("Shows the inventory tools filter window.")]
        public  void ShowHideInventoryToolsCommand3(string command, string args)
        {
            PluginService.WindowService.ToggleFiltersWindow();
        }

        [Command("/itfiltertoggle")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            PluginService.FilterService.ToggleActiveBackgroundFilter(args);
        }

        [Command("/invf")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand2(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            PluginService.FilterService.ToggleActiveBackgroundFilter(args);
        }

        [Command("/ifilter")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand3(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            PluginService.FilterService.ToggleActiveBackgroundFilter(args);
        }

        [Command("/openfilter")]
        [HelpMessage("Toggles the specified filter as it's own window.")]
        public  void OpenFilterCommand(string command, string args)
        {
            PluginService.PluginLogic.ToggleWindowFilterByName(args);
        }

        [Command("/crafts")]
        [HelpMessage("Opens the inventory tools crafts window")]
        public  void OpenCraftsWindow(string command, string args)
        {
            PluginService.WindowService.ToggleCraftsWindow();
        }

        [Command("/itconfig")]
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

        [Command("/ithelp")]
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

        [Command("/itlogging")]
        [HelpMessage("Turns on inventory tools chat logging")]
        public  void ToggleChatLogging(string command, string args)
        {
            ChatUtilities.LogsEnabled = !ChatUtilities.LogsEnabled;
        }
        
        #if DEBUG

        [Command("/itdebug")]
        [HelpMessage("Opens the inventory tools debug window")]
        public  void ToggleDebugWindow(string command, string args)
        {
            PluginService.WindowService.ToggleDebugWindow();
        }

        [Command("/itintro")]
        [HelpMessage("Opens the inventory tools debug window")]
        public void ToggleIntroWindow(string command, string args)
        {
            PluginService.WindowService.OpenWindow<IntroWindow>(IntroWindow.AsKey);
        }

        #endif
    }
}