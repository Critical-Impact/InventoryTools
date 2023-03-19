using Dalamud.Game.Command;
using Dalamud.Logging;
using InventoryTools.Services.Interfaces;

namespace InventoryToolsMock;

public class MockCommandService : ICommandService
{
    public bool AddHandler(string command, CommandInfo info)
    {
        return true;
    }

    public bool RemoveHandler(string command)
    {
        return false;
    }

    public bool ProcessCommand(string content)
    {
        PluginLog.Verbose("Would have fired off the command: '" + content + "'");
        return true;
    }
}