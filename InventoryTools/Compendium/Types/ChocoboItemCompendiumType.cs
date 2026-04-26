using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.Shared.Extensions;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Compendium.Types.Extra;
using InventoryTools.Localizers;

public class ChocoboItemCompendiumType : CompendiumType<ChocoboItem>
{
    private readonly ChocoboItemIterator _iterator;
    private readonly ILocalizer<ChocoboItemSourceType> _itemSourceTypeLocalizer;

    private List<ChocoboItem>? _rows;

    public ChocoboItemCompendiumType(
        CompendiumTable<ChocoboItem>.Factory tableFactory,
        CompendiumColumnBuilder<ChocoboItem>.Factory columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory,
        ChocoboItemIterator iterator,
        ILocalizer<ChocoboItemSourceType> itemSourceTypeLocalizer
    ) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _iterator = iterator;
        _itemSourceTypeLocalizer = itemSourceTypeLocalizer;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = Key,
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
        });
    }

    public override string Singular => "Chocobo Item";
    public override string Plural => "Chocobo Items";
    public override string Description => "Items consumed or equipped by chocobo companions.";
    public override string Key => "chocoboItems";

    public override (string?, uint?) Icon => (null, 74);

    public override List<ChocoboItem> GetRows()
    {
        return _iterator.ToList();
    }

    public override ChocoboItem GetRow(uint row)
    {
        return _iterator.FirstOrDefault(r => r.RowId == row);
    }

    public override bool HasRow(uint rowId)
    {
        return _iterator.Any(r => r.RowId == rowId);
    }

    public override string? GetName(ChocoboItem row)
    {
        return row.Item.NameString;
    }

    public override string? GetSubtitle(ChocoboItem row)
    {
        return row.SourceType.ToString();
    }

    public override (string?, uint?) GetIcon(ChocoboItem row)
    {
        return (null, row.Item.Icon);
    }

    public override uint GetRowId(ChocoboItem row)
    {
        return row.RowId;
    }

    public override void BuildColumns(
        CompendiumColumnBuilder<ChocoboItem> builder)
    {
        builder.AddItemColumn(new()
        {
            Key = "icon",
            Name = "##Icon",
            HelpText = "The icon of the chocobo item.",
            Version = "1",
            ValueSelector = row => row.Item.RowId,
        });

        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "Name",
            HelpText = "The name of the chocobo item.",
            Version = "1",
            ValueSelector = r => r.Item.NameString
        });

        builder.AddStringColumn(new()
        {
            Key = "source",
            Name = "Source",
            HelpText = "The source of the chocobo item.",
            Version = "1",
            ValueSelector = r => _itemSourceTypeLocalizer.Format(r.SourceType)
        });

        builder.AddBooleanColumn(new()
        {
            Key = "training_item",
            Name = "Training Item?",
            HelpText = "Is this item used for training?",
            Version = "1",
            ValueSelector = r => r.BuddyItem?.Value.UseTraining
        });

        builder.AddBooleanColumn(new()
        {
            Key = "field_item",
            Name = "Field Item?",
            HelpText = "Is this item used in the field?",
            Version = "1",
            ValueSelector = r => r.BuddyItem?.Value.UseField
        });

        builder.AddBooleanColumn(new()
        {
            Key = "recolor_item",
            Name = "Dye Item?",
            HelpText = "Is this item used to alter a chocobos colour?",
            Version = "1",
            ValueSelector = r => r.BuddyItem?.Value.Unknown0
        });
    }

    public override void BuildViewFields(
        CompendiumViewBuilder viewBuilder,
        ChocoboItem row)
    {
        viewBuilder.Title = row.Item.NameString;
        viewBuilder.Icon = row.Item.Icon;
        viewBuilder.Subtitle = row.SourceType.ToString();

        var useTraining = row.BuddyItem?.Value.UseTraining;
        var useField = row.BuddyItem?.Value.UseField;
        var dyeField = row.BuddyItem?.Value.Unknown0;
        viewBuilder.AddInfoTableSection(new()
        {
            SectionName = "Info",
            Items =
            [
                ("Source", _itemSourceTypeLocalizer.Format(row.SourceType), true),
                ("Training Item?", TriStateFormatted(useTraining), row.BuddyItem != null),
                ("Field Item?", TriStateFormatted(useField), row.BuddyItem != null),
                ("Dye Item?", TriStateFormatted(dyeField), row.BuddyItem != null),
            ]
        });
    }

    public override string GetDefaultGrouping()
    {
        return "source";
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return new()
        {
            new CompendiumGrouping<ChocoboItem>()
            {
                Key = "source",
                Name = "Source",
                GroupFunc = r => r.SourceType,
                GroupMapping = o =>
                {
                    var itemSourceType = (ChocoboItemSourceType)o;
                    return _itemSourceTypeLocalizer.Format(itemSourceType);
                }
            }
        };
    }
}