using Dalamud.Logging;
using DalamudPluginProjectTemplate.Attributes;
using InventoryTools.Logic;

namespace InventoryTools
{
    public class PluginCommands
    {
        [Command("/inventorytools")]
        [HelpMessage("Shows the inventory tools window.")]
        public void ShowHideInventoryToolsCommand(string command, string args)
        {
            ConfigurationManager.Config.IsVisible = !ConfigurationManager.Config.IsVisible;
        }

        [Command("/inv")]
        [HelpMessage("Shows the inventory tools window.")]
        public  void ShowHideInventoryToolsCommand2(string command, string args)
        {
            ConfigurationManager.Config.IsVisible = !ConfigurationManager.Config.IsVisible;
        }

        [Command("/invtools")]
        [HelpMessage("Shows the inventory tools window.")]
        public  void ShowHideInventoryToolsCommand3(string command, string args)
        {
            ConfigurationManager.Config.IsVisible = !ConfigurationManager.Config.IsVisible;
        }

        [Command("/itfiltertoggle")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            PluginService.PluginLogic.ToggleActiveBackgroundFilterByName(args);
        }

        [Command("/invf")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand2(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            PluginService.PluginLogic.ToggleActiveBackgroundFilterByName(args);
        }

        [Command("/ifilter")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public  void FilterToggleCommand3(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            PluginService.PluginLogic.ToggleActiveBackgroundFilterByName(args);
        }

        [Command("/openfilter")]
        [HelpMessage("Toggles the specified filter as it's own window.")]
        public  void OpenFilterCommand(string command, string args)
        {
            PluginService.PluginLogic.ToggleWindowFilterByName(args);
        }
    }
}