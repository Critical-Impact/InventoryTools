using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Model;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Humanizer;
using InventoryTools.Compendium.Columns.Options;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using OtterGui.Extensions;
using Icons = AllaganLib.Shared.Misc.Icons;

namespace InventoryTools.Compendium.Types;

public class SharedModelCompendiumType : CompendiumType<SharedModelCache.SharedModelGroup>
{
    private readonly SharedModelCache _sharedModelCache;

    public SharedModelCompendiumType(CompendiumTable<SharedModelCache.SharedModelGroup>.Factory tableFactory, CompendiumColumnBuilder<SharedModelCache.SharedModelGroup>.Factory columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory, SharedModelCache sharedModelCache) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _sharedModelCache = sharedModelCache;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = "shared_models",
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
            BuildContextMenu = null
        });
    }

    public override string? GetName(SharedModelCache.SharedModelGroup row)
    {
        return "Shared Model #" + _sharedModelCache.IndexOf(row);
    }

    public override string? GetSubtitle(SharedModelCache.SharedModelGroup row)
    {
        return row.Items.Count + " items";
    }

    public override (string?, uint?) GetIcon(SharedModelCache.SharedModelGroup row)
    {
        return (null, row.Items.First().Icon);
    }

    public override uint GetRowId(SharedModelCache.SharedModelGroup row)
    {
        return (uint)GetRows().IndexOf(row);
    }

    public override SharedModelCache.SharedModelGroup GetRow(uint row)
    {
        return _sharedModelCache[(int)row];
    }

    public override bool HasRow(uint rowId)
    {
        return (int)rowId >= 0 && (int)rowId < _sharedModelCache.Count;
    }

    public override List<SharedModelCache.SharedModelGroup> GetRows()
    {
        return _sharedModelCache.ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<SharedModelCache.SharedModelGroup> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "##Icon", HelpText = "The icon of the shared model", Version = "14.0.3", ValueSelector = row => ("armor", null), CompendiumType = this, RowIdSelector = row => (uint)_sharedModelCache.IndexOf(row)});
        builder.AddStringColumn(new StringColumnOptions<SharedModelCache.SharedModelGroup>
        {
            ValueSelector = row => row.Items.First().ClassJobCategory?.Base.Name.ExtractText() ?? "Unknown",
            Name = "Class/Job",
            Key = "class_job",
            HelpText = "The class/job of the item",
            Version = "14.0.3"
        });
        builder.AddStringColumn(new StringColumnOptions<SharedModelCache.SharedModelGroup>
        {
            ValueSelector = row => string.Join(", ", row.Items.First().EquipSlotCategory?.PossibleSlots.Select(c => c.Humanize()) ?? []),
            Name = "Equip Slots",
            Key = "equip_slots",
            HelpText = "The equipment slots of the item",
            Version = "14.0.3"
        });
        builder.AddItemsColumn(new ItemsColumnOptions<SharedModelCache.SharedModelGroup>
        {
            ValueSelector = row => row.Items.ToList(),
            Name = "Items",
            Key = "items",
            HelpText = "The items that share this model",
            Version = "14.0.3",
            ColumnFlags = ImGuiTableColumnFlags.WidthStretch
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, SharedModelCache.SharedModelGroup row)
    {
        viewBuilder.Icon = row.Items.First().Icon;
        viewBuilder.Title = "Shared Model #" + _sharedModelCache.IndexOf(row);
        viewBuilder.Subtitle = row.Items.Count + " items";
        viewBuilder.AddItemListSection(new ItemListSectionOptions()
        {
            Items = row.Items.Select(c => new ItemInfo(c)),
            SectionName = "Items",
        });
    }

    public override string Singular => "Shared Model Set";
    public override string Plural => "Shared Model Sets";
    public override string Description => "Items that share the same model.";
    public override string Key => "shared_models";
    public override (string?, uint?) Icon => (null, Icons.CombinedClothingIcon);
}