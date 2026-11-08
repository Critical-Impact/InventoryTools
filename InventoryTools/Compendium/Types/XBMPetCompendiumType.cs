#pragma warning disable PendingExcelSchema
using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.Shared.Extensions;
using DalaMock.Host.Mediator;
using Dalamud.Utility;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Ui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace InventoryTools.Compendium.Types;

public enum XBMPetLocationType
{
    None,
    Zone,
    Duty,
}

public class XBMPetCompendiumType : CompendiumType<AllaganLib.GameSheets.LuminaSheets.XBMPet>
{
    private const int WeaponskillAbilityIndex = 0;
    private const int NatureAbilityIndex = 1;

    private readonly ExcelSheet<AllaganLib.GameSheets.LuminaSheets.XBMPet> _xbmPetSheet;
    private readonly ExcelSheet<Pet> _petSheet;
    private readonly ExcelSheet<LuminaAction> _actionSheet;
    private readonly ExcelSheet<ActionTransient> _actionTransientSheet;
    private readonly ExcelSheet<PlaceName> _placeNameSheet;
    private readonly ExcelSheet<ContentFinderCondition> _contentFinderConditionSheet;
    private readonly ExcelSheet<TerritoryType> _territoryTypeSheet;
    private readonly ItemInfoCache _itemInfoCache;
    private List<AllaganLib.GameSheets.LuminaSheets.XBMPet>? _rows;
    private Dictionary<uint, uint>? _placeNameToTerritoryType;
    private Dictionary<uint, ItemBeastmasterFamiliarUse>? _familiarToUnlockItem;

    public XBMPetCompendiumType(
        ExcelSheet<AllaganLib.GameSheets.LuminaSheets.XBMPet> xbmPetSheet,
        ExcelSheet<Pet> petSheet,
        ExcelSheet<LuminaAction> actionSheet,
        ExcelSheet<ActionTransient> actionTransientSheet,
        ExcelSheet<PlaceName> placeNameSheet,
        ExcelSheet<ContentFinderCondition> contentFinderConditionSheet,
        ExcelSheet<TerritoryType> territoryTypeSheet,
        ItemInfoCache itemInfoCache,
        CompendiumTable<AllaganLib.GameSheets.LuminaSheets.XBMPet>.Factory tableFactory,
        CompendiumColumnBuilder<AllaganLib.GameSheets.LuminaSheets.XBMPet>.Factory columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _xbmPetSheet = xbmPetSheet;
        _petSheet = petSheet;
        _actionSheet = actionSheet;
        _actionTransientSheet = actionTransientSheet;
        _placeNameSheet = placeNameSheet;
        _contentFinderConditionSheet = contentFinderConditionSheet;
        _territoryTypeSheet = territoryTypeSheet;
        _itemInfoCache = itemInfoCache;
    }

    public override List<Type>? RelatedTypes => [typeof(AllaganLib.GameSheets.LuminaSheets.XBMPet)];

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<AllaganLib.GameSheets.LuminaSheets.XBMPet>()
        {
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
            Key = Key,
        });
    }

    public override string? GetName(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        return _petSheet.GetRowOrDefault(row.Pet.RowId)?.Name.ToImGuiString().FirstCharToUpper();
    }

    public override string? GetSubtitle(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        return GetFamilyName(row.Classification);
    }

    public override (string?, uint?) GetIcon(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        return (null, row.Icon);
    }

    public override uint GetRowId(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        return row.RowId;
    }

    public override AllaganLib.GameSheets.LuminaSheets.XBMPet GetRow(uint row)
    {
        return _xbmPetSheet.GetRowOrDefault(row) ?? default;
    }

    public override List<AllaganLib.GameSheets.LuminaSheets.XBMPet> GetRows()
    {
        return _rows ??= _xbmPetSheet.Where(c => c.Pet.RowId != 0).ToList();
    }

    public override bool HasRow(uint rowId)
    {
        if (rowId == 0)
        {
            return false;
        }

        var row = _xbmPetSheet.GetRowOrDefault(rowId);
        return row != null && row.Value.Pet.RowId != 0;
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return
        [
            new CompendiumGrouping<AllaganLib.GameSheets.LuminaSheets.XBMPet>()
            {
                Key = "family",
                Name = "Family",
                GroupFunc = row => row.Classification,
                GroupMapping = family => GetFamilyName((byte)family),
            },
            new CompendiumGrouping<AllaganLib.GameSheets.LuminaSheets.XBMPet>()
            {
                Key = "location_type",
                Name = "Location Type",
                GroupFunc = row => GetLocationType(row),
                GroupMapping = locationType => GetLocationTypeName((XBMPetLocationType)locationType),
            },
        ];
    }

    public override void BuildColumns(CompendiumColumnBuilder<AllaganLib.GameSheets.LuminaSheets.XBMPet> builder)
    {
        builder.AddCompendiumOpenViewColumn(new()
        {
            Key = "icon",
            Name = "##Icon",
            HelpText = "The icon of the familiar",
            Version = "15.0.14",
            ValueSelector = GetIcon,
            CompendiumType = this,
            RowIdSelector = row => row.RowId,
        });
        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "Name",
            HelpText = "The name of the familiar",
            Version = "15.0.14",
            ValueSelector = GetName,
        });

        var grouping = builder.CompendiumGrouping?.Key;
        if (grouping != "family")
        {
            builder.AddStringColumn(new()
            {
                Key = "family",
                Name = "Family",
                HelpText = "The monster family of the familiar",
                Version = "15.0.14",
                ValueSelector = row => GetFamilyName(row.Classification),
            });
        }

        builder.AddCompendiumLinkColumn(new()
        {
            Key = "location",
            Name = "Location",
            HelpText = "The zone or duty where the familiar is found. Click to open the zone or duty.",
            Version = "15.0.14",
            ValueSelector = GetLocationName,
            RowRefSelector = GetLocationRowRef,
        });

        if (grouping != "location_type")
        {
            builder.AddStringColumn(new()
            {
                Key = "location_type",
                Name = "Location Type",
                HelpText = "Is the familiar found in a zone or in a duty?",
                Version = "15.0.14",
                ValueSelector = row => GetLocationTypeName(GetLocationType(row)),
            });
        }

        builder.AddStringColumn(new()
        {
            Key = "weaponskill",
            Name = "Weaponskill",
            HelpText = "The weaponskill the familiar can be ordered to use",
            Version = "15.0.14",
            ValueSelector = row => FormatSkillSummary(GetAbility(row, WeaponskillAbilityIndex), row.Description.ToImGuiString()),
        });
        builder.AddStringColumn(new()
        {
            Key = "ability",
            Name = "Ability",
            HelpText = "The ability the familiar can use under the effect of One with Nature",
            Version = "15.0.14",
            ValueSelector = row => FormatSkillSummary(GetAbility(row, NatureAbilityIndex), row.TrickDescription.ToImGuiString()),
        });
        builder.AddStringColumn(new()
        {
            Key = "auto_attack",
            Name = "Auto-attack",
            HelpText = "Is the familiar's auto-attack melee or ranged?",
            Version = "15.0.14",
            ValueSelector = GetAutoAttackType,
        });
        builder.AddItemColumn(new()
        {
            Key = "unlock_item",
            Name = "Unlock Item",
            HelpText = "The item that unlocks the familiar, if one exists",
            Version = "15.0.14",
            ValueSelector = row => GetUnlockItem(row)?.Item.RowId,
        });
        builder.AddItemSourcesColumn(new()
        {
            Key = "unlock_item_sources",
            Name = "Unlock Item Sources",
            HelpText = "The sources of the item that unlocks the familiar",
            Version = "15.0.14",
            ValueSelector = GetUnlockItemSources,
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        viewBuilder.SetupDefaults(this, row);
        viewBuilder.Description = row.Description.ToImGuiString();

        var locationType = GetLocationType(row);
        viewBuilder.AddTag(() => GetFamilyName(row.Classification), () => "The monster family of the familiar.");
        viewBuilder.AddTag(() => "Auto-attack: " + GetAutoAttackType(row), () => "The range of the familiar's auto-attack.");

        viewBuilder.AddMetadataSection(new MetadataSectionOptions()
        {
            SectionKey = "details",
            SectionName = "Details",
            Rows =
            [
                new MetadataSectionOptions.Row { Label = "Family", Value = () => GetFamilyName(row.Classification) },
                new MetadataSectionOptions.Row { Label = "Location", Value = () => GetLocationName(row) },
                new MetadataSectionOptions.Row { Label = "Location Type", Value = () => GetLocationTypeName(locationType) },
                new MetadataSectionOptions.Row { Label = "Auto-attack", Value = () => GetAutoAttackType(row) },
            ],
        });

        var weaponskill = GetAbility(row, WeaponskillAbilityIndex);
        var natureAbility = GetAbility(row, NatureAbilityIndex);
        viewBuilder.AddMetadataSection(new MetadataSectionOptions()
        {
            SectionKey = "skills",
            SectionName = "Skills",
            Rows =
            [
                new MetadataSectionOptions.Row
                {
                    Label = weaponskill?.Name.ToImGuiString() ?? "Weaponskill",
                    Value = () => GetSkillDescription(weaponskill, row.TrickDescription.ToImGuiString()),
                },
                new MetadataSectionOptions.Row
                {
                    Label = natureAbility?.Name.ToImGuiString() ?? "Ability",
                    Value = () => GetSkillDescription(natureAbility, row.TemperedDescription.ToImGuiString()),
                },
            ],
        });

        var locationRef = GetLocationRowRef(row);
        if (locationRef != null)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionKey = "location",
                SectionName = locationType == XBMPetLocationType.Duty ? "Related Duty" : "Related Zone",
                RelatedRef = locationRef.Value,
            });
        }

        var unlockItem = GetUnlockItem(row);
        if (unlockItem != null)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionKey = "unlock_item",
                SectionName = "Unlock Item",
                RelatedRef = unlockItem.Item.Base.AsUntypedRowRef(),
            });
            viewBuilder.AddItemSourcesSection(new ItemSourcesSectionOptions()
            {
                SectionKey = "unlock_item_sources",
                SectionName = "Unlock Item Sources",
                Sources = GetUnlockItemSources(row),
                SourceType = SourceType.Source,
            });
        }
    }

    public ItemBeastmasterFamiliarUse? GetUnlockItem(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        _familiarToUnlockItem ??= _itemInfoCache
            .GetItemUsesByType<ItemBeastmasterFamiliarUse>(ItemInfoType.BeastmasterFamiliar)
            .GroupBy(c => c.Familiar.RowId)
            .ToDictionary(c => c.Key, c => c.First());
        return _familiarToUnlockItem.GetValueOrDefault(row.RowId);
    }

    private List<ItemSource> GetUnlockItemSources(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        var unlockItem = GetUnlockItem(row);
        if (unlockItem == null)
        {
            return [];
        }

        return _itemInfoCache.GetItemSources(unlockItem.Item.RowId) ?? [];
    }

    public override bool HasLocation => true;

    public override ILocation? GetLocation(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        var territoryType = GetTerritoryType(row);
        if (territoryType == null)
        {
            return null;
        }

        return new TerritoryLocation(territoryType.Value);
    }

    public XBMPetLocationType GetLocationType(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        if (row.Location.RowId == 0)
        {
            return XBMPetLocationType.None;
        }

        return row.LocationKey switch
        {
            1 => XBMPetLocationType.Zone,
            2 => XBMPetLocationType.Duty,
            _ => XBMPetLocationType.None,
        };
    }

    public string GetLocationName(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        var locationName = GetLocationType(row) switch
        {
            XBMPetLocationType.Zone => _placeNameSheet.GetRowOrDefault(row.Location.RowId)?.Name.ToImGuiString(),
            XBMPetLocationType.Duty => _contentFinderConditionSheet.GetRowOrDefault(row.Location.RowId)?.Name.ToImGuiString().FirstCharToUpper(),
            _ => null,
        };

        return string.IsNullOrEmpty(locationName) ? "Unknown" : locationName;
    }

    public TerritoryType? GetTerritoryType(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        switch (GetLocationType(row))
        {
            case XBMPetLocationType.Zone:
                _placeNameToTerritoryType ??= BuildPlaceNameToTerritoryType();
                if (_placeNameToTerritoryType.TryGetValue(row.Location.RowId, out var territoryTypeId))
                {
                    return _territoryTypeSheet.GetRowOrDefault(territoryTypeId);
                }

                return null;
            case XBMPetLocationType.Duty:
                var contentFinderCondition = _contentFinderConditionSheet.GetRowOrDefault(row.Location.RowId);
                if (contentFinderCondition == null || contentFinderCondition.Value.TerritoryType.RowId == 0)
                {
                    return null;
                }

                return contentFinderCondition.Value.TerritoryType.ValueNullable;
            default:
                return null;
        }
    }

    public RowRef? GetLocationRowRef(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        switch (GetLocationType(row))
        {
            case XBMPetLocationType.Zone:
                return GetTerritoryType(row)?.AsUntypedRowRef();
            case XBMPetLocationType.Duty:
                var contentFinderCondition = _contentFinderConditionSheet.GetRowOrDefault(row.Location.RowId);
                if (contentFinderCondition == null || !contentFinderCondition.Value.Content.Is<InstanceContent>())
                {
                    return null;
                }

                return contentFinderCondition.Value.Content;
            default:
                return null;
        }
    }

    private Dictionary<uint, uint> BuildPlaceNameToTerritoryType()
    {
        var placeNameToTerritoryType = new Dictionary<uint, uint>();
        foreach (var territoryType in _territoryTypeSheet)
        {
            if (territoryType.Map.RowId == 0 || territoryType.ContentFinderCondition.RowId != 0 || territoryType.TerritoryIntendedUse.RowId != 1)
            {
                continue;
            }

            placeNameToTerritoryType.TryAdd(territoryType.PlaceName.RowId, territoryType.RowId);
        }

        return placeNameToTerritoryType;
    }

    private LuminaAction? GetAbility(AllaganLib.GameSheets.LuminaSheets.XBMPet row, int abilityIndex)
    {
        var pet = _petSheet.GetRowOrDefault(row.Pet.RowId);
        if (pet == null)
        {
            return null;
        }

        var abilityId = pet.Value.Abilities[abilityIndex].RowId;
        if (abilityId == 0)
        {
            return null;
        }

        return _actionSheet.GetRowOrDefault(abilityId);
    }

    private string GetSkillDescription(LuminaAction? action, string summary)
    {
        if (action != null)
        {
            var description = _actionTransientSheet.GetRowOrDefault(action.Value.RowId)?.Description.ToImGuiString();
            if (!string.IsNullOrEmpty(description))
            {
                return description;
            }
        }

        return summary;
    }

    private string FormatSkillSummary(LuminaAction? action, string summary)
    {
        var name = action?.Name.ToImGuiString();
        if (string.IsNullOrEmpty(name))
        {
            return summary;
        }

        if (string.IsNullOrEmpty(summary))
        {
            return name;
        }

        return name + ": " + summary;
    }

    private string GetAutoAttackType(AllaganLib.GameSheets.LuminaSheets.XBMPet row)
    {
        var action = _actionSheet.GetRowOrDefault(row.Action.RowId);
        if (action == null || row.Action.RowId == 0)
        {
            return "Unknown";
        }

        return action.Value.Range > 5 ? "Ranged" : "Melee";
    }

    private string GetFamilyName(byte family)
    {
        return family switch
        {
            1 => "Beastkin",
            2 => "Vilekin",
            3 => "Cloudkin",
            4 => "Seedkin",
            5 => "Wavekin",
            6 => "Scalekin",
            7 => "Soulkin",
            8 => "Ashkin",
            _ => "Unknown",
        };
    }

    private string GetLocationTypeName(XBMPetLocationType locationType)
    {
        return locationType switch
        {
            XBMPetLocationType.Zone => "Zone",
            XBMPetLocationType.Duty => "Duty",
            _ => "None",
        };
    }

    public override string Singular => "Familiar";
    public override string Plural => "Familiars";
    public override string Description => "Familiars that can be tamed by the beastmaster.";
    public override string Key => "familiars";
    public override (string?, uint?) Icon => (null, 242001);
}
