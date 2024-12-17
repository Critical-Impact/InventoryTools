using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets.Helpers;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Plugin.Services;
using InventoryTools.Mediator;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class TeleporterService : DisposableMediatorSubscriberBase, IHostedService
{
    private readonly ITeleporterIpc _teleporterIpc;
    private readonly ExcelSheet<Aetheryte> _aetheryteSheet;
    private readonly ExcelSheet<Level> _levelSheet;
    private readonly SubrowExcelSheet<MapMarker> _mapMarkerSheet;
    private readonly ExcelSheet<Map> _mapSheet;
    private readonly IDataManager _dataManager;
    private Dictionary<uint, List<AetheryteLookup>>? _aetheryteMap = null;

    public TeleporterService(ILogger<TeleporterService> logger, MediatorService mediatorService, ITeleporterIpc teleporterIpc, ExcelSheet<Aetheryte> aetheryteSheet, ExcelSheet<Level> levelSheet, SubrowExcelSheet<MapMarker> mapMarkerSheet, ExcelSheet<Map> mapSheet, IDataManager dataManager) : base(logger, mediatorService)
    {
        _teleporterIpc = teleporterIpc;
        _aetheryteSheet = aetheryteSheet;
        _levelSheet = levelSheet;
        _mapMarkerSheet = mapMarkerSheet;
        _mapSheet = mapSheet;
        _dataManager = dataManager;
    }

    public List<AetheryteLookup>? GetAetherytes(uint mapId)
    {
        if (_aetheryteMap == null)
        {
            _aetheryteMap = new Dictionary<uint, List<AetheryteLookup>>();
            foreach (var mapMarkerRow in _mapMarkerSheet)
            {
                for (var subRowIndex = 0; subRowIndex < mapMarkerRow.Count; subRowIndex++)
                {
                    var subRow = mapMarkerRow[subRowIndex];
                    if (subRow.DataType == 3)
                    {
                        var aetheryte = _aetheryteSheet.GetRowOrDefault(subRow.DataKey.RowId);
                        if (aetheryte != null)
                        {
                            _aetheryteMap.TryAdd(aetheryte.Value.Map.RowId,
                                []);

                            _aetheryteMap[aetheryte.Value.Map.RowId]
                                .Add(new AetheryteLookup(new RowRef<Map>(_dataManager.Excel,
                                        aetheryte.Value.Map.RowId,
                                        _dataManager.GameData.Options.DefaultExcelLanguage),
                                new RowRef<Aetheryte>(_dataManager.Excel, aetheryte.Value.RowId,
                                    _dataManager.GameData.Options.DefaultExcelLanguage),
                                new SubrowRef<MapMarker>(_dataManager.Excel, subRow.RowId,
                                    _dataManager.GameData.Options.DefaultExcelLanguage), subRowIndex));
                        }
                    }
                }
            }
        }

        return _aetheryteMap.GetValueOrDefault(mapId);
    }

    private void TeleportRequested(RequestTeleportMessage obj)
    {
        _teleporterIpc.Teleport(obj.aetheryteId);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
        MediatorService.Subscribe<RequestTeleportMessage>(this, TeleportRequested);
        MediatorService.Subscribe<RequestTeleportToTerritoryMessage>(this, TeleportRequested);
        MediatorService.Subscribe<RequestTeleportToMapMessage>(this, TeleportRequested);
        MediatorService.Subscribe<RequestTeleportToFishingSpotRowMessage>(this, TeleportRequested);
        MediatorService.Subscribe<RequestTeleportToGatheringPointRowMessage>(this, TeleportRequested);
        MediatorService.Subscribe<RequestTeleportToSpearFishingSpotRowMessage>(this, TeleportRequested);
        return Task.CompletedTask;
    }

    private void TeleportRequested(RequestTeleportToSpearFishingSpotRowMessage message)
    {
        var map = message.spearfishingNotebook.Base.TerritoryType.Value.Map.Value;
        var coords = MapUtility.WorldToMap(new Vector2(message.spearfishingNotebook.Base.X, message.spearfishingNotebook.Base.Y),
            map);
        TeleportRequested(new RequestTeleportToMapMessage(map.RowId, coords));
    }

    private void TeleportRequested(RequestTeleportToGatheringPointRowMessage message)
    {
        TeleportRequested(message.gatheringPoint);
    }

    private void TeleportRequested(ILocation location)
    {
        TeleportRequested(new RequestTeleportToMapMessage(location.Map.RowId, new Vector2((float)location.MapX, (float)location.MapY)));
    }

    private void TeleportRequested(RequestTeleportToFishingSpotRowMessage message)
    {
        var map = message.fishingSpot.Base.TerritoryType.Value.Map.Value;
        var coords = MapUtility.WorldToMap(new Vector2(message.fishingSpot.Base.X, message.fishingSpot.Base.Z),
            map);
        TeleportRequested(new RequestTeleportToMapMessage(map.RowId, coords));
    }

    private void TeleportRequested(RequestTeleportToMapMessage obj)
    {
        var aetherytes = this.GetAetherytes(obj.mapId);
        if (aetherytes is { Count: > 0 })
        {
            var closestAetheryte = aetherytes.MinBy(c => c.DistanceToAetheryte(obj.mapCoordinates));
            _teleporterIpc.Teleport(closestAetheryte!.Aetheryte.RowId);
        }
    }

    private void TeleportRequested(RequestTeleportToTerritoryMessage message)
    {
        var map = _mapSheet.FirstOrNull(c => c.TerritoryType.RowId == message.territoryTypeId);
        if (map != null)
        {
            TeleportRequested(new RequestTeleportToMapMessage(map.Value.RowId, message.mapCoordinates));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Stopping service {type} ({this})", GetType().Name, this);
        return Task.CompletedTask;
    }
}