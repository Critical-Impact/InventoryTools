using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Logging;
using DalamudPluginProjectTemplate;
using DalamudPluginProjectTemplate.Attributes;
using static Dalamud.Game.Command.CommandInfo;

// ReSharper disable ForCanBeConvertedToForeach

namespace InventoryTools.Commands
{
    public class PluginCommandManager<T> : IDisposable where T : notnull
    {
        private readonly (string, CommandInfo)[] _pluginCommands;
        private readonly T _host;

        public PluginCommandManager(T host)
        {
            this._host = host;

            this._pluginCommands = host.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public |
                                                            BindingFlags.Static | BindingFlags.Instance)
                .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
                .SelectMany(GetCommandInfoTuple)
                .ToArray();

            AddCommandHandlers();
        }

        private void AddCommandHandlers()
        {
            for (var i = 0; i < this._pluginCommands.Length; i++)
            {
                var (command, commandInfo) = this._pluginCommands[i];
                PluginService.CommandService.AddHandler(command, commandInfo);
            }
        }

        private void RemoveCommandHandlers()
        {
            for (var i = 0; i < this._pluginCommands.Length; i++)
            {
                var (command, _) = this._pluginCommands[i];
                PluginService.CommandService.RemoveHandler(command);
            }
        }

        private IEnumerable<(string, CommandInfo)> GetCommandInfoTuple(MethodInfo method)
        {
            var handlerDelegate = (HandlerDelegate) Delegate.CreateDelegate(typeof(HandlerDelegate), this._host, method);

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
        
        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed && disposing)
            {
                RemoveCommandHandlers();
            }
            _disposed = true;         
        }
        
        ~PluginCommandManager()
        {
#if DEBUG
            // In debug-builds, make sure that a warning is displayed when the Disposable object hasn't been
            // disposed by the programmer.

            if( _disposed == false )
            {
                PluginLog.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
            }
#endif
            Dispose (true);
        }
    }
}