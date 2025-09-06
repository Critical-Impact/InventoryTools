using System.Collections.Generic;
using AllaganLib.Shared.Interfaces;
using AllaganLib.Shared.Services;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Debuggers;

public class QueueDebuggerPane : IDebugPane
{
    private readonly BackgroundTaskCollector _backgroundTaskCollector;

    public QueueDebuggerPane(BackgroundTaskCollector backgroundTaskCollector)
    {
        _backgroundTaskCollector = backgroundTaskCollector;
    }

    public void Draw()
    {
        foreach (var taskQueue in _backgroundTaskCollector.BackgroundTaskQueues)
        {
            using (var group = ImRaii.Group())
            {
                if (group)
                {
                    ImGui.Text(taskQueue.QueueName);
                    ImGui.SameLine();
                    ImGui.Text(taskQueue.QueueCount.ToString());
                }
            }
        }
    }

    public string Name => "Queue Debugger";
}