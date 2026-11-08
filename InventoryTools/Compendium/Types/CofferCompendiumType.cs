using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Colors;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Compendium.Types.Extra;
using InventoryTools.Ui;

namespace InventoryTools.Compendium.Types;

public class CofferCompendiumType : CompendiumType<CofferContents>, IDisposable
{
    private const string ColumnVersion = "15.0.14";

    private const string ScopeHelpText = "Owned means unlocked, or in the inventories of the current character, their retainers, free company or housing.";

    private const byte TreasureMapFilterGroup = 18;

    private const uint TalesOfAdventureAction = 25174;

    private const uint UnobtainableItemUICategory = 39;

    private static readonly ItemInfoType[] ContainerTypes =
    [
        ItemInfoType.Loot,
        ItemInfoType.Coffer,
        ItemInfoType.CardPack,
        ItemInfoType.Logogram,
        ItemInfoType.Oizys,
        ItemInfoType.Auxesia,
    ];

    private readonly ItemInfoCache _itemInfoCache;
    private readonly ItemSheet _itemSheet;
    private readonly IUnlockTrackerService _unlockTrackerService;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly Lazy<Dictionary<uint, CofferContents>> _rowsById;
    private readonly Lazy<List<CofferContents>> _rows;
    private readonly List<WeakReference<CompendiumTable<CofferContents>>> _tables = [];
    private readonly object _ownedLock = new();
    private HashSet<uint>? _ownedInventoryItemIds;

    public CofferCompendiumType(
        ItemInfoCache itemInfoCache,
        ItemSheet itemSheet,
        IUnlockTrackerService unlockTrackerService,
        IInventoryMonitor inventoryMonitor,
        ICharacterMonitor characterMonitor,
        CompendiumTable<CofferContents>.Factory tableFactory,
        CompendiumColumnBuilder<CofferContents>.Factory columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _itemInfoCache = itemInfoCache;
        _itemSheet = itemSheet;
        _unlockTrackerService = unlockTrackerService;
        _inventoryMonitor = inventoryMonitor;
        _characterMonitor = characterMonitor;
        _rowsById = new Lazy<Dictionary<uint, CofferContents>>(BuildRows);
        _rows = new Lazy<List<CofferContents>>(() => _rowsById.Value.Values.OrderBy(c => c.Item.NameString).ToList());

        _inventoryMonitor.OnInventoryChanged += InventoryChanged;
        _unlockTrackerService.ItemUnlockStatusChanged += UnlockStatusChanged;
        _characterMonitor.OnCharacterLoggedIn += CharacterLoggedInOrOut;
        _characterMonitor.OnCharacterLoggedOut += CharacterLoggedInOrOut;
        _characterMonitor.OnCharacterUpdated += CharacterUpdated;
        _characterMonitor.OnCharacterRemoved += CharacterRemoved;
    }

    public override string Singular => "Coffer";
    public override string Plural => "Coffers";
    public override string Description => "What drops from coffers, card packs, etc";
    public override string Key => "coffers";
    public override (string?, uint?) Icon => (null, 26531);

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        var table = Factory.Invoke(new CompendiumTableOptions<CofferContents>()
        {
            Key = Key,
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
        });

        lock (_tables)
        {
            _tables.RemoveAll(c => !c.TryGetTarget(out _));
            _tables.Add(new WeakReference<CompendiumTable<CofferContents>>(table));
        }

        return table;
    }

    public override string? GetName(CofferContents row)
    {
        return row.Item.NameString;
    }

    public override string? GetSubtitle(CofferContents row)
    {
        return GetCategoryName(row.Category);
    }

    public override (string?, uint?) GetIcon(CofferContents row)
    {
        return (null, row.Item.Icon);
    }

    public override uint GetRowId(CofferContents row)
    {
        return row.RowId;
    }

    public override CofferContents? GetRow(uint row)
    {
        return _rowsById.Value.GetValueOrDefault(row);
    }

    public override bool HasRow(uint rowId)
    {
        return _rowsById.Value.ContainsKey(rowId);
    }

    public override List<CofferContents> GetRows()
    {
        return _rows.Value;
    }

    public override string GetDefaultGrouping()
    {
        return "category";
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return
        [
            new CompendiumGrouping<CofferContents>()
            {
                Key = "category",
                Name = "Category",
                GroupFunc = row => row.Category,
                GroupMapping = category => GetCategoryName((CofferCategory)category),
            },
            new CompendiumGrouping<CofferContents>()
            {
                Key = "type",
                Name = "Type",
                GroupFunc = row => row.SourceType,
                GroupMapping = sourceType => GetSourceTypeName((ItemInfoType)sourceType),
            },
        ];
    }

    public override void BuildColumns(CompendiumColumnBuilder<CofferContents> builder)
    {
        builder.AddCompendiumOpenViewColumn(new()
        {
            Key = "icon",
            Name = "##Icon",
            HelpText = "The icon of the coffer",
            Version = ColumnVersion,
            ValueSelector = GetIcon,
            CompendiumType = this,
            RowIdSelector = row => row.RowId,
        });
        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "Name",
            HelpText = "The name of the coffer",
            Version = ColumnVersion,
            ValueSelector = GetName,
        });

        if (builder.CompendiumGrouping?.Key != "category")
        {
            builder.AddStringColumn(new()
            {
                Key = "category",
                Name = "Category",
                HelpText = "The category of container. Loot containers are sorted by the items they give.",
                Version = ColumnVersion,
                ValueSelector = row => GetCategoryName(row.Category),
            });
        }

        if (builder.CompendiumGrouping?.Key != "type")
        {
            builder.AddStringColumn(new()
            {
                Key = "type",
                Name = "Type",
                HelpText = "The type of container",
                Version = ColumnVersion,
                ValueSelector = row => GetSourceTypeName(row.SourceType),
            });
        }

        builder.AddIntegerColumn(new()
        {
            Key = "contents",
            Name = "Contents",
            HelpText = "The number of different items the coffer can give",
            Version = ColumnVersion,
            ValueSelector = row => row.Contents.Count.ToString(),
        });
        builder.AddIntegerColumn(new()
        {
            Key = "owned",
            Name = "Owned",
            HelpText = "The number of items from the coffer that you own. " + ScopeHelpText,
            Version = ColumnVersion,
            ValueSelector = row => GetOwnedCount(row).ToString(),
        });
        builder.AddStringColumn(new()
        {
            Key = "completion",
            Name = "Completion",
            HelpText = "Owned items out of all items the coffer can give. " + ScopeHelpText,
            Version = ColumnVersion,
            ValueSelector = GetCompletionText,
        });
        builder.AddBooleanColumn(new()
        {
            Key = "complete",
            Name = "Complete?",
            HelpText = "Do you own every item the coffer can give? " + ScopeHelpText,
            Version = ColumnVersion,
            ValueSelector = row => IsComplete(row),
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, CofferContents row)
    {
        viewBuilder.SetupDefaults(this, row);
        viewBuilder.Description = row.Item.Base.Description.ToImGuiString();

        viewBuilder.AddTag(
            () => GetCompletionText(row) + " owned",
            () => ScopeHelpText,
            () => IsComplete(row) ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudYellow);

        viewBuilder.AddMetadataSection(new MetadataSectionOptions()
        {
            SectionKey = "details",
            SectionName = "Details",
            Rows =
            [
                new MetadataSectionOptions.Row { Label = "Category", Value = () => GetCategoryName(row.Category) },
                new MetadataSectionOptions.Row { Label = "Type", Value = () => GetSourceTypeName(row.SourceType) },
                new MetadataSectionOptions.Row { Label = "Contents", Value = () => row.Contents.Count.ToString() },
                new MetadataSectionOptions.Row { Label = "Owned", Value = () => GetCompletionText(row) },
            ],
        });

        viewBuilder.AddItemListSection(new ItemListSectionOptions()
        {
            SectionKey = "contents",
            SectionName = "Contents",
            Items = row.Contents,
            IsOwned = IsOwned,
        });

        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            SectionKey = "coffer_item",
            SectionName = "Coffer Item",
            RelatedRef = row.Item.Base.AsUntypedRowRef(),
        });

        viewBuilder.AddItemSourcesSection(new ItemSourcesSectionOptions()
        {
            SectionKey = "coffer_sources",
            SectionName = "Coffer Sources",
            Sources = row.Item.Sources,
            SourceType = SourceType.Source,
        });
    }

    public bool IsOwned(uint itemId)
    {
        if (GetOwnedInventoryItemIds().Contains(itemId))
        {
            return true;
        }

        var itemRow = _itemSheet.GetRowOrDefault(itemId);
        return itemRow != null && _unlockTrackerService.IsUnlocked(itemRow, false) == true;
    }

    public int GetOwnedCount(CofferContents row)
    {
        return row.Contents.Count(c => IsOwnedItem(c.ItemRow));
    }

    public bool IsComplete(CofferContents row)
    {
        return row.Contents.Count != 0 && GetOwnedCount(row) == row.Contents.Count;
    }

    public void Dispose()
    {
        _inventoryMonitor.OnInventoryChanged -= InventoryChanged;
        _unlockTrackerService.ItemUnlockStatusChanged -= UnlockStatusChanged;
        _characterMonitor.OnCharacterLoggedIn -= CharacterLoggedInOrOut;
        _characterMonitor.OnCharacterLoggedOut -= CharacterLoggedInOrOut;
        _characterMonitor.OnCharacterUpdated -= CharacterUpdated;
        _characterMonitor.OnCharacterRemoved -= CharacterRemoved;
    }

    private bool IsOwnedItem(ItemRow itemRow)
    {
        return GetOwnedInventoryItemIds().Contains(itemRow.RowId) || _unlockTrackerService.IsUnlocked(itemRow, false) == true;
    }

    private string GetCompletionText(CofferContents row)
    {
        return GetOwnedCount(row) + "/" + row.Contents.Count;
    }

    private Dictionary<uint, CofferContents> BuildRows()
    {
        var usesByContainer = ContainerTypes
            .SelectMany(type => _itemInfoCache.GetItemUsesByType<ItemSupplementSource>(type))
            .Where(c => c.CostItem != null)
            .GroupBy(c => c.CostItem!.RowId);

        var rows = new Dictionary<uint, CofferContents>();
        foreach (var uses in usesByContainer)
        {
            var container = uses.First().CostItem!;
            if (container.NameString == string.Empty)
            {
                continue;
            }

            var contents = uses
                .Where(c => c.Item.NameString != string.Empty && c.Item.Base.ItemUICategory.RowId != UnobtainableItemUICategory)
                .GroupBy(c => c.Item.RowId)
                .Select(c => c.First())
                .OrderBy(c => c.Item.NameString)
                .Select(c => ItemInfo.Create(c.Item, min: c.Supplement.Min, max: c.Supplement.Max, probability: c.Supplement.Probability))
                .ToList();

            if (contents.Count == 0)
            {
                continue;
            }

            var sourceType = GetSourceType(uses.ToList());
            rows[container.RowId] = new CofferContents()
            {
                RowId = container.RowId,
                Item = container,
                SourceType = sourceType,
                Category = GetCategory(container, sourceType, contents),
                Contents = contents,
            };
        }

        return rows;
    }

    private ItemInfoType GetSourceType(List<ItemSupplementSource> uses)
    {
        var specificUses = uses.Where(c => c.Type != ItemInfoType.Loot).ToList();
        if (specificUses.Count == 0)
        {
            return ItemInfoType.Loot;
        }

        return specificUses.GroupBy(c => c.Type).OrderByDescending(c => c.Count()).ThenBy(c => c.Key).First().Key;
    }

    private CofferCategory GetCategory(ItemRow container, ItemInfoType sourceType, List<ItemInfo> contents)
    {
        switch (sourceType)
        {
            case ItemInfoType.Coffer:
                return CofferCategory.Coffers;
            case ItemInfoType.CardPack:
                return CofferCategory.CardPacks;
            case ItemInfoType.Logogram:
                return CofferCategory.Logograms;
            case ItemInfoType.Oizys:
            case ItemInfoType.Auxesia:
                return CofferCategory.AncientRecords;
        }

        if (container.Base.FilterGroup == TreasureMapFilterGroup)
        {
            return CofferCategory.TreasureMaps;
        }

        if (container.Base.ItemAction.ValueNullable?.Action.RowId == TalesOfAdventureAction)
        {
            return CofferCategory.TalesOfAdventure;
        }

        var gear = contents
            .Select(c => (Item: c.ItemRow, Slots: c.ItemRow.Base.EquipSlotCategory.ValueNullable))
            .Where(c => c.Item.Base.EquipSlotCategory.RowId != 0 && c.Slots != null)
            .Select(c => (c.Item, Slots: c.Slots!.Value))
            .ToList();

        if (gear.Count * 2 < contents.Count)
        {
            return CofferCategory.Miscellaneous;
        }

        var weapons = gear.Where(c => c.Slots.MainHand > 0 || c.Slots.OffHand > 0).ToList();
        if (weapons.Count * 2 >= gear.Count)
        {
            var isSingleJob = weapons.Select(c => c.Item.Base.ClassJobCategory.RowId).Distinct().Count() == 1;
            return isSingleJob ? CofferCategory.JobArms : CofferCategory.WeaponCoffers;
        }

        var armourSlots = new HashSet<string>();
        var hasAccessories = false;
        foreach (var (_, slots) in gear)
        {
            AddSlot(armourSlots, "Head", slots.Head);
            AddSlot(armourSlots, "Body", slots.Body);
            AddSlot(armourSlots, "Gloves", slots.Gloves);
            AddSlot(armourSlots, "Waist", slots.Waist);
            AddSlot(armourSlots, "Legs", slots.Legs);
            AddSlot(armourSlots, "Feet", slots.Feet);
            hasAccessories |= slots.Ears > 0 || slots.Neck > 0 || slots.Wrists > 0 || slots.FingerL > 0 || slots.FingerR > 0;
        }

        if (armourSlots.Count == 0 && weapons.Count == 0)
        {
            return CofferCategory.AccessoryCoffers;
        }

        if (armourSlots.Count == 1 && !hasAccessories && weapons.Count == 0)
        {
            return CofferCategory.ArmourCoffers;
        }

        return CofferCategory.GearSets;
    }

    private void AddSlot(HashSet<string> slots, string slotName, sbyte slotValue)
    {
        if (slotValue > 0)
        {
            slots.Add(slotName);
        }
    }

    private HashSet<uint> GetOwnedInventoryItemIds()
    {
        lock (_ownedLock)
        {
            return _ownedInventoryItemIds ??= _inventoryMonitor.AllItems
                .Where(c => _characterMonitor.BelongsToActiveCharacter(c.RetainerId))
                .Select(c => c.ItemId)
                .ToHashSet();
        }
    }

    private void InvalidateOwnership(bool clearInventory)
    {
        if (clearInventory)
        {
            lock (_ownedLock)
            {
                _ownedInventoryItemIds = null;
            }
        }

        lock (_tables)
        {
            _tables.RemoveAll(c => !c.TryGetTarget(out _));
            foreach (var reference in _tables)
            {
                if (reference.TryGetTarget(out var table))
                {
                    table.IsDirty = true;
                }
            }
        }
    }

    private void InventoryChanged(List<InventoryChange> inventoryChanges, InventoryMonitor.ItemChanges? itemChanges)
    {
        InvalidateOwnership(true);
    }

    private void UnlockStatusChanged()
    {
        InvalidateOwnership(false);
    }

    private void CharacterLoggedInOrOut(ulong characterId)
    {
        InvalidateOwnership(true);
    }

    private void CharacterUpdated(Character? character)
    {
        InvalidateOwnership(true);
    }

    private void CharacterRemoved(ulong characterId)
    {
        InvalidateOwnership(true);
    }

    private string GetCategoryName(CofferCategory category)
    {
        return category switch
        {
            CofferCategory.WeaponCoffers => "Weapon Coffers",
            CofferCategory.JobArms => "Job Arms",
            CofferCategory.ArmourCoffers => "Armour Coffers",
            CofferCategory.AccessoryCoffers => "Accessory Coffers",
            CofferCategory.GearSets => "Gear Sets",
            CofferCategory.TreasureMaps => "Treasure Maps",
            CofferCategory.TalesOfAdventure => "Tales of Adventure",
            CofferCategory.Miscellaneous => "Miscellaneous",
            CofferCategory.Coffers => "Coffers",
            CofferCategory.CardPacks => "Card Packs",
            CofferCategory.Logograms => "Logograms",
            CofferCategory.AncientRecords => "Ancient Records",
            _ => "Unknown",
        };
    }

    private string GetSourceTypeName(ItemInfoType sourceType)
    {
        return sourceType switch
        {
            ItemInfoType.Loot => "Loot Container",
            ItemInfoType.Coffer => "Coffer",
            ItemInfoType.CardPack => "Card Pack",
            ItemInfoType.Logogram => "Logogram",
            ItemInfoType.Oizys => "Ancient Record (Oizys)",
            ItemInfoType.Auxesia => "Ancient Record (Auxesia)",
            _ => "Unknown",
        };
    }
}
