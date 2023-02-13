using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;

namespace InventoryToolsMock;

public class MockChatUtilities : IChatUtilities
{
    public bool LogsEnabled { get; set; }
    public void PrintLog(string message)
    {
        PluginLog.Log("Chat Log Output: " + message);
    }

    public void Print(SeString message)
    {
        PrintLog(message.ToString());
    }

    public void Print(string message)
    {
        PrintLog(message);
    }

    public void Print(string left, string center, int color, string right)
    {
        PrintLog(left + "<" + color + ">" + center + "</" + color + ">" + right);
    }

    public void PrintError(SeString message)
    {
        PrintError(message.ToString());
    }

    public void PrintError(string message)
    {
        PluginLog.Log(message);
    }

    public void PrintError(string left, string center, int color, string right)
    {
        PrintError(left + "<" + color + ">" + center + "</" + color + ">" + right);
    }

    public void PrintClipboardMessage(string objectType, string name, Exception? e = null)
    {
        PluginLog.Log("Clipboard Message:");
        PluginLog.Log(name);
        PluginLog.Log(objectType);
    }

    public void PrintGeneralMessage(string objectType, string name)
    {
        PluginLog.Log("General Message:");
        PluginLog.Log(name);
        PluginLog.Log(objectType);
    }

    public void PrintFullMapLink(ILocation location, string? textOverride = null)
    {
        if (location.MapEx.Value != null && location.MapEx.Value.TerritoryType.Value != null)
        {
            var name = location.ToString();
            if (name != null)
            {
                PluginLog.Log(textOverride ?? name, location.MapEx.Value.TerritoryType.Value);
                PluginLog.Log((float)(location.MapX) + ":" + (float)(location.MapY));
            }
        }
    }

    public void LinkItem(ItemEx item)
    {
        PluginLog.Log("Item Link:");
        PluginLog.Log(item.NameString);
    }
}