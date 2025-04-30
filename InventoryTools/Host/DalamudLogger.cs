using System;
using System.Text;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Host;

internal sealed class DalamudLogger : ILogger
{
    private readonly string _name;
    private readonly IPluginLog _pluginLog;

    public DalamudLogger(string name, IPluginLog pluginLog)
    {
        _name = name;
        _pluginLog = pluginLog;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel)
    {
        //return (int)_configuration.LogLevel <= (int)logLevel;
        return true;
    }

    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
        return BeginScope(state);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;


        StringBuilder sb = new();
        sb.Append($"[{_name}]{{{(int)logLevel}}} {state}: {exception?.Message}");
        if (exception != null)
        {
            sb.AppendLine(exception.StackTrace);
            var innerException = exception?.InnerException;
            while (innerException != null)
            {
                sb.AppendLine($"InnerException {innerException}: {innerException.Message}");
                sb.AppendLine(innerException.StackTrace);
                innerException = innerException.InnerException;
            }
        }

        switch (logLevel)
        {
            case LogLevel.Trace:
                _pluginLog.Verbose(sb.ToString());
                break;
            case LogLevel.Debug:
                _pluginLog.Debug(sb.ToString());
                break;
            case LogLevel.Information:
                _pluginLog.Information(sb.ToString());
                break;
            case LogLevel.Warning:
                _pluginLog.Warning(sb.ToString());
                break;
            case LogLevel.Error:
                _pluginLog.Error(sb.ToString());
                break;
            case LogLevel.Critical:
                _pluginLog.Fatal(sb.ToString());
                break;
        }

    }
}