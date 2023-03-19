using Dalamud.Game.Command;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Services;

public class CommandService : ICommandService
{
    private CommandManager _commands;
    public CommandService(CommandManager commands)
    {
        _commands = commands;
    }
    public bool AddHandler(string command, CommandInfo info)
    {
        return _commands.AddHandler(command, info);
    }

    public bool RemoveHandler(string command)
    {
        return _commands.RemoveHandler(command);
    }

    public bool ProcessCommand(string content)
    {
        return _commands.ProcessCommand(content);
    }
}