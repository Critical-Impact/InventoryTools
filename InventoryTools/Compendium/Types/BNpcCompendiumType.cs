using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Colors;
using Humanizer;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Compendium.Types;

public record BNpcZoneInfo(
    IReadOnlyList<TerritoryTypeRow> Zones,
    IReadOnlyList<string> ZoneNames,
    IReadOnlyList<string> ZoneTypes,
    IReadOnlyList<string> Expansions,
    bool IsInOpenWorld);

public class BNpcCompendiumType : CompendiumType<BNpcNameRow>
{
    private const string OpenWorldZoneType = "Open World";
    private const string OtherZoneType = "Other";
    private const string NotAHuntMark = "Not a Hunt Mark";

    private static readonly EquipSlot[] SharedModelSlots =
    [
        EquipSlot.MainHand,
        EquipSlot.OffHand,
        EquipSlot.Head,
        EquipSlot.Body,
        EquipSlot.Gloves,
        EquipSlot.Legs,
        EquipSlot.Feet,
        EquipSlot.FingerL,
        EquipSlot.FingerR,
    ];

    private readonly BNpcNameSheet _bNpcNameSheet;
    private readonly TerritoryTypeSheet _territoryTypeSheet;
    private readonly ItemSheet _itemSheet;
    private readonly List<MobSpawnPosition> _mobSpawnPositions;
    private readonly Lazy<List<BNpcNameRow>> _rows;
    private readonly Lazy<Dictionary<uint, BNpcZoneInfo>> _zoneInfo;
    private readonly Lazy<Dictionary<uint, List<uint>>> _mobsByTerritory;

    public BNpcCompendiumType(
        BNpcNameSheet bNpcNameSheet,
        TerritoryTypeSheet territoryTypeSheet,
        ItemSheet itemSheet,
        List<MobSpawnPosition> mobSpawnPositions,
        CompendiumTable<BNpcNameRow>.Factory tableFactory,
        CompendiumColumnBuilder<BNpcNameRow>.Factory columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _bNpcNameSheet = bNpcNameSheet;
        _territoryTypeSheet = territoryTypeSheet;
        _itemSheet = itemSheet;
        _mobSpawnPositions = mobSpawnPositions;
        _rows = new Lazy<List<BNpcNameRow>>(BuildRows);
        _zoneInfo = new Lazy<Dictionary<uint, BNpcZoneInfo>>(BuildZoneInfo);
        _mobsByTerritory = new Lazy<Dictionary<uint, List<uint>>>(BuildMobsByTerritory);
    }

    public override string Singular => "Mob";
    public override string Plural => "Mobs";
    public override string Description => "Monsters, where they spawn and what they drop.";
    public override string Key => "mobs";
    public override (string?, uint?) Icon => (null, Icons.MobIcon);

    public override List<Type>? RelatedTypes => [typeof(BNpcName)];

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<BNpcNameRow>()
        {
            Key = Key,
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
        });
    }

    public override string? GetName(BNpcNameRow row)
    {
        return AllaganLib.Shared.Extensions.StringExtensions.ToTitleCase(row.Base.Singular.ToImGuiString());
    }

    public override string? GetSubtitle(BNpcNameRow row)
    {
        return GetMobTypes(row);
    }

    public override (string?, uint?) GetIcon(BNpcNameRow row)
    {
        return (null, Icons.MobIcon);
    }

    public override uint GetRowId(BNpcNameRow row)
    {
        return row.RowId;
    }

    public override BNpcNameRow? GetRow(uint row)
    {
        return HasRow(row) ? _bNpcNameSheet.GetRowOrDefault(row) : null;
    }

    public override bool HasRow(uint rowId)
    {
        return _zoneInfo.Value.ContainsKey(rowId);
    }

    public override List<BNpcNameRow> GetRows()
    {
        return _rows.Value;
    }

    public override string GetDefaultGrouping()
    {
        return "zone_type";
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return
        [
            new CompendiumGrouping<BNpcNameRow>()
            {
                Key = "zone_type",
                Name = "Zone Type",
                GroupsFunc = row => GetZoneInfo(row).ZoneTypes,
                GroupMapping = zoneType => (string)zoneType,
            },
            new CompendiumGrouping<BNpcNameRow>()
            {
                Key = "zone",
                Name = "Zone",
                GroupsFunc = row => GetZoneInfo(row).ZoneNames,
                GroupMapping = zoneName => (string)zoneName,
            },
            new CompendiumGrouping<BNpcNameRow>()
            {
                Key = "expansion",
                Name = "Expansion",
                GroupsFunc = row => GetZoneInfo(row).Expansions,
                GroupMapping = expansion => (string)expansion,
            },
            new CompendiumGrouping<BNpcNameRow>()
            {
                Key = "hunt_rank",
                Name = "Hunt Rank",
                GroupFunc = GetHuntRank,
                GroupMapping = huntRank => (string)huntRank,
            },
        ];
    }

    public override void BuildColumns(CompendiumColumnBuilder<BNpcNameRow> builder)
    {
        var grouping = builder.CompendiumGrouping?.Key;

        builder.AddCompendiumOpenViewColumn(new()
        {
            Key = "icon",
            Name = "##Icon",
            HelpText = "Open the mob",
            Version = "15.0.14",
            ValueSelector = GetIcon,
            CompendiumType = this,
            RowIdSelector = row => row.RowId,
        });
        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "Name",
            HelpText = "The name of the mob",
            Version = "15.0.14",
            ValueSelector = GetName,
        });
        builder.AddStringColumn(new()
        {
            Key = "type",
            Name = "Type",
            HelpText = "The type of mob",
            Version = "15.0.14",
            ValueSelector = GetMobTypes,
        });

        if (grouping != "hunt_rank")
        {
            builder.AddStringColumn(new()
            {
                Key = "hunt_rank",
                Name = "Hunt Rank",
                HelpText = "The hunt rank of the mob, if it is a hunt mark",
                Version = "15.0.14",
                ValueSelector = row => GetHuntRank(row) != NotAHuntMark ? GetHuntRank(row) : null,
            });
        }

        builder.AddStringColumn(new()
        {
            Key = "zones",
            Name = "Zones",
            HelpText = "The zones the mob spawns in",
            Version = "15.0.14",
            ValueSelector = row => string.Join(", ", GetZoneInfo(row).ZoneNames),
        });

        if (grouping != "zone_type")
        {
            builder.AddStringColumn(new()
            {
                Key = "zone_type",
                Name = "Zone Type",
                HelpText = "The types of zone the mob spawns in, for example open world, dungeon or raid",
                Version = "15.0.14",
                ValueSelector = row => string.Join(", ", GetZoneInfo(row).ZoneTypes),
            });
        }

        builder.AddBooleanColumn(new()
        {
            Key = "open_world",
            Name = "Open World?",
            HelpText = "Does the mob spawn in an open world zone?",
            Version = "15.0.14",
            ValueSelector = row => GetZoneInfo(row).IsInOpenWorld,
        });

        if (grouping != "expansion")
        {
            builder.AddStringColumn(new()
            {
                Key = "expansion",
                Name = "Expansion",
                HelpText = "The expansions of the zones the mob spawns in",
                Version = "15.0.14",
                ValueSelector = row => string.Join(", ", GetZoneInfo(row).Expansions),
            });
        }

        builder.AddItemsColumn(new()
        {
            Key = "drops",
            Name = "Drops",
            HelpText = "The items the mob drops",
            Version = "15.0.14",
            ValueSelector = GetDrops,
        });
        builder.AddIntegerColumn(new()
        {
            Key = "id",
            Name = "ID",
            HelpText = "The ID of the mob (BNpcName)",
            Version = "15.0.14",
            ValueSelector = row => row.RowId.ToString(),
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, BNpcNameRow row)
    {
        viewBuilder.SetupDefaults(this, row);

        foreach (var mobType in row.MobTypes)
        {
            viewBuilder.AddTag(() => mobType.ToString(), () => "The type of mob");
        }

        var huntRank = GetHuntRank(row);
        if (huntRank != NotAHuntMark)
        {
            viewBuilder.AddTag(() => huntRank + " Rank Hunt", () => "The mob is a hunt mark", () => ImGuiColors.DalamudOrange);
        }

        var zoneInfo = GetZoneInfo(row);
        if (zoneInfo.IsInOpenWorld)
        {
            viewBuilder.AddTag(() => OpenWorldZoneType, () => "The mob spawns in an open world zone");
        }

        var garlandToolsId = row.GarlandToolsId;
        if (garlandToolsId != null)
        {
            viewBuilder.AddLink($"https://www.garlandtools.org/db/#mob/{garlandToolsId}", "Open in Garland Tools", "garlandtools");
        }

        viewBuilder.AddLink($"https://ffxivteamcraft.com/db/en/mob/{row.RowId}", "Open in Teamcraft", "teamcraft");

        viewBuilder.AddItemListSection(new ItemListSectionOptions()
        {
            SectionKey = "drops",
            SectionName = "Drops",
            Items = GetDrops(row).Select(c => new ItemInfo(c)).ToList(),
        });

        viewBuilder.AddMapLinksSectionSection(new MapLinksViewSectionOptions()
        {
            SectionKey = "spawn_locations",
            SectionName = "Spawn Locations",
            MapLinks = GetMapLinks(row),
        });

        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            SectionKey = "zones",
            SectionName = "Zones",
            RelatedRefs = zoneInfo.Zones.Select(c => c.Base.AsUntypedRowRef()).ToList(),
        });

        var firstBase = row.RelatedBases.FirstOrDefault();
        if (firstBase != null)
        {
            foreach (var slot in SharedModelSlots)
            {
                viewBuilder.AddItemListSection(new ItemListSectionOptions()
                {
                    SectionKey = "shared_models_" + slot,
                    SectionName = "Shared Models - " + slot.Humanize(),
                    Items = firstBase.GetRelatedItems(slot).Select(c => new ItemInfo(c)).ToList(),
                });
            }
        }
    }

    public override bool HasLocation => true;

    public override ILocation? GetLocation(BNpcNameRow row)
    {
        return GetMapLinks(row).FirstOrDefault()?.Location;
    }

    public BNpcZoneInfo GetZoneInfo(BNpcNameRow row)
    {
        return _zoneInfo.Value.TryGetValue(row.RowId, out var zoneInfo) ? zoneInfo : new BNpcZoneInfo([], [], [], [], false);
    }

    public List<uint> GetMobsInTerritory(uint territoryTypeId)
    {
        return _mobsByTerritory.Value.TryGetValue(territoryTypeId, out var mobs) ? mobs : [];
    }

    public string GetHuntRank(BNpcNameRow row)
    {
        var rank = row.NotoriousMonster?.RankFormatted();
        return rank is null or "N/A" ? NotAHuntMark : rank;
    }

    public string GetZoneTypeName(TerritoryTypeRow territoryType)
    {
        var contentFinderCondition = territoryType.Base.ContentFinderCondition;
        if (contentFinderCondition.RowId != 0)
        {
            var contentType = contentFinderCondition.ValueNullable?.ContentType.ValueNullable?.Name.ToImGuiString();
            if (!string.IsNullOrEmpty(contentType))
            {
                return contentType;
            }
        }

        return territoryType.Base.TerritoryIntendedUse.RowId == 1 ? OpenWorldZoneType : OtherZoneType;
    }

    private string GetMobTypes(BNpcNameRow row)
    {
        return string.Join(", ", row.MobTypes.Select(c => c.ToString()));
    }

    private List<ItemRow> GetDrops(BNpcNameRow row)
    {
        return row.MobDrops
            .Select(c => _itemSheet.GetRowOrDefault(c.ItemId))
            .Where(c => c != null)
            .Select(c => c!)
            .DistinctBy(c => c.RowId)
            .ToList();
    }

    private List<MapLinkEntry> GetMapLinks(BNpcNameRow row)
    {
        var mapLinks = new List<MapLinkEntry>();
        foreach (var spawn in row.MobSpawnPositions.DistinctBy(c => (c.TerritoryTypeId, MathF.Round(c.Position.X, 1), MathF.Round(c.Position.Y, 1))))
        {
            var territoryType = _territoryTypeSheet.GetRowOrDefault(spawn.TerritoryTypeId);
            if (territoryType == null || territoryType.Base.Map.RowId == 0)
            {
                continue;
            }

            var location = new GenericMapLocation(spawn.Position.X, spawn.Position.Y, territoryType.Base.Map, territoryType.Base.PlaceName, territoryType.RowRef);
            var zoneName = territoryType.Base.PlaceName.ValueNullable?.Name.ToImGuiString() ?? "Unknown";
            mapLinks.Add(new MapLinkEntry(Icons.FlagIcon, zoneName, $"X: {spawn.Position.X:0.0} Y: {spawn.Position.Y:0.0}", location));
        }

        return mapLinks;
    }

    private List<BNpcNameRow> BuildRows()
    {
        return _zoneInfo.Value.Keys
            .Select(c => _bNpcNameSheet.GetRowOrDefault(c))
            .Where(c => c != null)
            .Select(c => c!)
            .OrderBy(c => GetName(c))
            .ToList();
    }

    private Dictionary<uint, BNpcZoneInfo> BuildZoneInfo()
    {
        var zoneInfo = new Dictionary<uint, BNpcZoneInfo>();
        foreach (var spawns in _mobSpawnPositions.GroupBy(c => c.BNpcNameId))
        {
            var bNpcName = _bNpcNameSheet.GetRowOrDefault(spawns.Key);
            if (bNpcName == null || bNpcName.Base.Singular.IsEmpty)
            {
                continue;
            }

            var zones = spawns
                .Select(c => c.TerritoryTypeId)
                .Distinct()
                .Select(c => _territoryTypeSheet.GetRowOrDefault(c))
                .Where(c => c != null && c.Base.PlaceName.RowId != 0)
                .Select(c => c!)
                .ToList();
            if (zones.Count == 0)
            {
                continue;
            }

            var zoneTypes = zones.Select(GetZoneTypeName).Distinct().OrderBy(c => c).ToList();
            zoneInfo[spawns.Key] = new BNpcZoneInfo(
                zones,
                zones.Select(c => c.Base.PlaceName.ValueNullable?.Name.ToImGuiString() ?? "Unknown").Distinct().OrderBy(c => c).ToList(),
                zoneTypes,
                zones.Select(c => c.Base.ExVersion.ValueNullable?.Name.ToImGuiString() ?? "Unknown").Distinct().ToList(),
                zoneTypes.Contains(OpenWorldZoneType));
        }

        return zoneInfo;
    }

    private Dictionary<uint, List<uint>> BuildMobsByTerritory()
    {
        var mobsByTerritory = new Dictionary<uint, List<uint>>();
        foreach (var (bNpcNameId, zoneInfo) in _zoneInfo.Value)
        {
            foreach (var zone in zoneInfo.Zones)
            {
                if (!mobsByTerritory.TryGetValue(zone.RowId, out var mobs))
                {
                    mobs = [];
                    mobsByTerritory[zone.RowId] = mobs;
                }

                mobs.Add(bNpcNameId);
            }
        }

        return mobsByTerritory;
    }
}
