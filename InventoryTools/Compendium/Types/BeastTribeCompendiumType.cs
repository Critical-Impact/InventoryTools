using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using DalaMock.Host.Mediator;
using Dalamud.Utility;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class BeastTribeCompendiumType : CompendiumType<BeastTribe>
{
    private readonly ExcelSheet<BeastTribe> _beastTribeSheet;
    private readonly ExcelSheet<Quest> _questSheet;
    private readonly ExcelSheet<ExVersion> _expansionSheet;

    public BeastTribeCompendiumType(ExcelSheet<BeastTribe> beastTribeSheet, ExcelSheet<Quest> questSheet, ExcelSheet<ExVersion> expansionSheet, CompendiumTable<BeastTribe>.Factory tableFactory, Func<CompendiumColumnBuilder<BeastTribe>> columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _beastTribeSheet = beastTribeSheet;
        _questSheet = questSheet;
        _expansionSheet = expansionSheet;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<BeastTribe>()
        {
            Name = "Beast Tribes",
            Key = "beast_tribes",
            Columns = BuiltColumns(),
            CompendiumType = this
        });
    }

    public override string? GetName(BeastTribe row)
    {
        return row.Name.ToImGuiString().FirstCharToUpper();
    }

    public override string? GetSubtitle(BeastTribe row)
    {
        return row.Expansion.Value.Name.ToImGuiString() + ", Uses " + row.CurrencyItem.Value.Name.ToImGuiString();
    }

    public override (string?, uint?) GetIcon(BeastTribe row)
    {
        return (null, row.Icon);
    }

    public override BeastTribe GetRow(uint row)
    {
        return _beastTribeSheet.GetRow(row);
    }

    public override List<BeastTribe> GetRows()
    {
        return _beastTribeSheet.Where(c => c.RowId != 0).ToList();
    }

    public override unsafe void BuildColumns(CompendiumColumnBuilder<BeastTribe> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "##Icon", HelpText = "The icon of the beast tribe", Version = "14.0.3", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.RowId});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the beast tribe", Version = "14.0.3", ValueSelector = this.GetName});
        builder.AddStringColumn(new (){Key = "expansion", Name = "Expansion", HelpText = "The expansion the beast tribe was introduced", Version = "14.0.3", ValueSelector = row => row.Expansion.Value.Name.ToImGuiString()});
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return new List<ICompendiumGrouping>()
        {
            new CompendiumGrouping<BeastTribe>()
            {
                Key = "expansion",
                Name = "Expansion",
                GroupFunc = row => row.Expansion.RowId,
                GroupMapping = o =>
                {
                    var expansionId = (uint)o;
                    return _expansionSheet.GetRow(expansionId).Name.ToImGuiString();
                }
            }
        };
    }

    public override string? GetDefaultGrouping()
    {
        return "expansion";
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, BeastTribe row)
    {
        viewBuilder.SetupDefaults(this, row);
        var tribeQuests = _questSheet.Where(c => c.BeastTribe.RowId == row.RowId).ToList();
        var repeatableQuests = tribeQuests.Where(c => c.IsRepeatable).Select(c => c.AsUntypedRowRef()).ToList();
        var mainQuests = tribeQuests.Where(c => !c.IsRepeatable).Select(c => c.AsUntypedRowRef()).ToList();
        viewBuilder.AddLevelMapLinkSection(new LevelViewSectionOptions()
        {
            SectionName = "Location",
            Level = row.Level
        });
        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            SectionName = "Currency Item",
            RelatedRef = (RowRef)row.CurrencyItem,
        });
        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            SectionName = "Main Quests",
            RelatedRefs = mainQuests,
            HideIfEmpty = true
        });
        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            SectionName = "Repeatable Quests",
            RelatedRefs = repeatableQuests,
            HideIfEmpty = true
        });
    }

    public override bool HasRow(uint rowId)
    {
        return rowId != 0 && _beastTribeSheet.HasRow(rowId);
    }

    public override string Singular => "Allied Society";
    public override string Plural => "Allied Societies";
    public override string Description => "Allied Societies the player can gain reputation with.";
    public override string Key => "beast_tribes";
    public override (string?, uint?) Icon => (null, Icons.BeastTribeIcon);
}