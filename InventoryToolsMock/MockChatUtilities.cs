using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services;

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using LuminaSupplemental.Excel.Model;

namespace InventoryToolsMock;

public class MockChatUtilities : IChatUtilities
{
    private readonly IPluginLog _pluginLog;

    public MockChatUtilities(IPluginLog pluginLog)
    {
        _pluginLog = pluginLog;
    }
    public bool LogsEnabled { get; set; }
    public void PrintLog(string message)
    {
        _pluginLog.Info("Chat Log Output: " + message);
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
        _pluginLog.Info(message);
    }

    public void PrintError(string left, string center, int color, string right)
    {
        PrintError(left + "<" + color + ">" + center + "</" + color + ">" + right);
    }

    public void PrintClipboardMessage(string objectType, string name, Exception? e = null)
    {
        _pluginLog.Info("Clipboard Message:");
        _pluginLog.Info(name);
        _pluginLog.Info(objectType);
    }

    public void PrintGeneralMessage(string objectType, string name)
    {
        _pluginLog.Info("General Message:");
        _pluginLog.Info(name);
        _pluginLog.Info(objectType);
    }

    public void PrintFullMapLink(ILocation location, string? textOverride = null)
    {
        if (location.Map.IsValid && location.Map.Value.TerritoryType.IsValid)
        {
            var name = location.ToString();
            if (name != null)
            {
                _pluginLog.Info(textOverride ?? name, location.Map.Value.TerritoryType.Value);
                _pluginLog.Info("Map ID: " + location.Map.RowId);
                _pluginLog.Info("Territory Type ID: " + location.TerritoryType.RowId);
                _pluginLog.Info("Place Name ID: " + location.PlaceName.RowId);
                _pluginLog.Info("Map X & Y: " + (float)(location.MapX) + ":" + (float)(location.MapY));
            }
        }
    }

    public void PrintFullMapLink(MobSpawnPosition mobSpawnPosition, string text)
    {
        if (mobSpawnPosition.TerritoryType.IsValid && mobSpawnPosition.TerritoryType.Value.Map.IsValid)
        {
            _pluginLog.Info(text);
            _pluginLog.Info("Map ID: " + mobSpawnPosition.TerritoryType.Value.Map.RowId);
            _pluginLog.Info("Territory Type ID: " + mobSpawnPosition.TerritoryType.RowId);
            _pluginLog.Info("Place Name ID: " + mobSpawnPosition.TerritoryType.Value.PlaceName.RowId);
            _pluginLog.Info("Map X & Y: " + (float)(mobSpawnPosition.Position.X) + ":" + (float)(mobSpawnPosition.Position.Y));
        }
    }

    public void PrintGatheringMapLink(GatheringPointRow gatheringPoint)
    {
        _pluginLog.Info("Map ID: " + gatheringPoint.TerritoryType.Value.Map.RowId);
        _pluginLog.Info("Territory Type ID: " + gatheringPoint.TerritoryType.RowId);
        _pluginLog.Info("Place Name ID: " + gatheringPoint.TerritoryType.Value.PlaceName.RowId);
        _pluginLog.Info("Map X & Y: " + (float)(gatheringPoint.MapX) + ":" + (float)(gatheringPoint.MapY));
    }

    public void PrintGatheringMapLink(FishingSpotRow fishingSpotRow, FishParameterRow fishParameterRow)
    {
        _pluginLog.Info("Record type: " + fishParameterRow.Base.FishingRecordType.Value.Addon.Value.Text.ExtractText());
        _pluginLog.Info("Map ID: " + fishingSpotRow.TerritoryType.Value.Map.RowId);
        _pluginLog.Info("Territory Type ID: " + fishingSpotRow.TerritoryType.RowId);
        _pluginLog.Info("Place Name ID: " + fishingSpotRow.TerritoryType.Value.PlaceName.RowId);
        _pluginLog.Info("Map X & Y: " + (float)(fishingSpotRow.MapX) + ":" + (float)(fishingSpotRow.MapY));
    }

    public void PrintGatheringMapLink(SpearfishingNotebookRow spearfishingNotebookRow, SpearfishingItemRow spearfishingItemRow)
    {
        _pluginLog.Info("Record type: " + spearfishingItemRow.FishRecordType);
        _pluginLog.Info("Map ID: " + spearfishingNotebookRow.TerritoryType.Value.Map.RowId);
        _pluginLog.Info("Territory Type ID: " + spearfishingNotebookRow.TerritoryType.RowId);
        _pluginLog.Info("Place Name ID: " + spearfishingNotebookRow.TerritoryType.Value.PlaceName.RowId);
        _pluginLog.Info("Map X & Y: " + (float)(spearfishingNotebookRow.MapX) + ":" + (float)(spearfishingNotebookRow.MapY));
    }

    public void LinkItem(ItemRow item)
    {
        _pluginLog.Info("Item Link:");
        _pluginLog.Info(item.NameString);
    }
}