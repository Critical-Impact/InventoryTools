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
        sb.AppendLine($"[{_name}]{{{(int)logLevel}}} {state}: {exception?.Message}");
        sb.AppendLine(exception?.StackTrace);
        var innerException = exception?.InnerException;
        while (innerException != null)
        {
            sb.AppendLine($"InnerException {innerException}: {innerException.Message}");
            sb.AppendLine(innerException.StackTrace);
            innerException = innerException.InnerException;
        }
        if (logLevel == LogLevel.Trace)
            _pluginLog.Verbose($"[{_name}]{{{(int)logLevel}}} {state}");
        else if (logLevel == LogLevel.Debug)
            _pluginLog.Debug($"[{_name}]{{{(int)logLevel}}} {state}");
        else if (logLevel == LogLevel.Information)
            _pluginLog.Information($"[{_name}]{{{(int)logLevel}}} {state}");
        else if (logLevel == LogLevel.Warning)
            _pluginLog.Warning($"[{_name}]{{{(int)logLevel}}} {state}");
        else if (logLevel == LogLevel.Error)
            _pluginLog.Error($"[{_name}]{{{(int)logLevel}}} {state}");
        else if (logLevel == LogLevel.Critical)
            _pluginLog.Fatal($"[{_name}]{{{(int)logLevel}}} {state}");
    }
}