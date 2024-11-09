using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using InventoryTools.Attributes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ReSharper disable ForCanBeConvertedToForeach

namespace InventoryTools.Commands
{
    public class PluginCommandManager<T> : IHostedService where T : notnull
    {
        public ILogger<PluginCommandManager<T>> Logger { get; }
        private (string, CommandInfo)[] _pluginCommands;
        private readonly T _host;
        private readonly ICommandManager _commandManager;

        public PluginCommandManager(ILogger<PluginCommandManager<T>> logger, T host, ICommandManager commandManager)
        {
            Logger = logger;
            this._host = host;
            _commandManager = commandManager;
        }

        private void AddCommandHandlers()
        {
            for (var i = 0; i < this._pluginCommands.Length; i++)
            {
                var (command, commandInfo) = this._pluginCommands[i];
                _commandManager.AddHandler(command, commandInfo);
            }
        }

        private void RemoveCommandHandlers()
        {
            for (var i = 0; i < this._pluginCommands.Length; i++)
            {
                var (command, _) = this._pluginCommands[i];
                _commandManager.RemoveHandler(command);
            }
        }

        private IEnumerable<(string, CommandInfo)> GetCommandInfoTuple(MethodInfo method)
        {
            var handlerDelegate = (IReadOnlyCommandInfo.HandlerDelegate) Delegate.CreateDelegate(typeof(IReadOnlyCommandInfo.HandlerDelegate), this._host, method);

            var command = handlerDelegate.Method.GetCustomAttribute<CommandAttribute>();
            var aliases = handlerDelegate.Method.GetCustomAttribute<AliasesAttribute>();
            var helpMessage = handlerDelegate.Method.GetCustomAttribute<HelpMessageAttribute>();
            var doNotShowInHelp = handlerDelegate.Method.GetCustomAttribute<DoNotShowInHelpAttribute>();

            var commandInfo = new CommandInfo(handlerDelegate)
            {
                HelpMessage = helpMessage?.HelpMessage ?? string.Empty,
                ShowInHelp = doNotShowInHelp == null,
            };

            // Create list of tuples that will be filled with one tuple per alias, in addition to the base command tuple.
            var commandInfoTuples = new List<(string, CommandInfo)>();
            if (command != null)
            {
                commandInfoTuples.Add((command.Command, commandInfo));
                if (command.SecondaryCommand != null)
                {
                    commandInfoTuples.Add((command.SecondaryCommand, commandInfo));
                }
            }
            if (aliases != null)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < aliases.Aliases.Length; i++)
                {
                    commandInfoTuples.Add((aliases.Aliases[i], commandInfo));
                }
            }

            return commandInfoTuples;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);

            this._pluginCommands = _host.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public |
                                                             BindingFlags.Static | BindingFlags.Instance)
                .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
                .SelectMany(GetCommandInfoTuple)
                .ToArray();

            AddCommandHandlers();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Stopping service {type} ({this})", GetType().Name, this);
            RemoveCommandHandlers();
            return Task.CompletedTask;
        }
    }
}