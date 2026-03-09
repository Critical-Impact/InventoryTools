using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.LuminaSheets;
using AllaganLib.Shared.Extensions;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Misc;
using InventoryTools.Compendium.Interfaces;

namespace InventoryTools.Compendium.Types;

public class QuestCompendiumType : CompendiumType<Quest>
{
    private readonly LevelSheet _levelSheet;
    private readonly ExcelSheet<ENpcBase> _eNpcBaseSheet;
    private readonly ExcelSheet<Quest> _questSheet;
    private readonly Func<string, ExcelSheet<QuestDialogue>> _questDialogueFactory;

    public QuestCompendiumType(LevelSheet levelSheet, ExcelSheet<ENpcBase> eNpcBaseSheet, ExcelSheet<Quest> questSheet, Func<string, ExcelSheet<QuestDialogue>> questDialogueFactory, CompendiumTable<Quest>.Factory tableFactory, Func<CompendiumColumnBuilder<Quest>> columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _levelSheet = levelSheet;
        _eNpcBaseSheet = eNpcBaseSheet;
        _questSheet = questSheet;
        _questDialogueFactory = questDialogueFactory;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<Quest>()
        {
            CompendiumType = this,
            Key = "quests",
            Name = "Quests",
            Columns = BuiltColumns()
        });
    }

    public override string? GetName(Quest row)
    {
        return row.Name.ToImGuiString();
    }

    public override string? GetSubtitle(Quest row)
    {
        return row.JournalGenre.Value.Name.ToImGuiString();
    }

    public override (string?, uint?) GetIcon(Quest row)
    {
        if (row.EventIconType.RowId != 0 && row.EventIconType.IsValid)
        {
            return (null, row.EventIconType.Value.NpcIconAvailable + 1);
        }
        if (row.JournalGenre.RowId != 0)
        {
            return (null, (uint)row.JournalGenre.Value.Icon);
        }
        else if (row.ClassJobUnlock.RowId != 0 && row.ClassJobUnlock.ValueNullable != null)
        {
            return (null, 62000 + row.ClassJobUnlock.RowId);
        }
        return  (null, null);
    }

    public override Quest GetRow(uint row)
    {
        return _questSheet.GetRow(row);
    }

    public override bool HasRow(uint rowId)
    {
        return _questSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<Quest> GetRows()
    {
        return _questSheet.Where(c => !c.Name.IsEmpty).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<Quest> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "##Icon", HelpText = "The icon of the quest", Version = "14.0.3", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.RowId});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the quest", Version = "14.0.3", ValueSelector = this.GetName});
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, Quest row)
    {
        viewBuilder.SetupDefaults(this, row);
        var dialogueSheet = _questDialogueFactory.Invoke(row.Id.ToImGuiString());
        var dialogue = dialogueSheet.GetRowOrDefault(0);
        if (dialogue != null)
        {
            viewBuilder.Description = dialogue.Value.Value.ToImGuiString();
        }
        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            RelatedRefs = row.PreviousQuest.Select(c => (RowRef)c).ToList(),
            SectionName = "Previous Quests",
            HideIfEmpty = false
        });
        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            RelatedRefs = _questSheet.Where(c => c.PreviousQuest.Any(c => c.RowId == row.RowId)).Select(c => (RowRef)new RowRef<Quest>(c.ExcelPage.Module, c.RowId)).ToList(),
            SectionName = "Next Quests",
            HideIfEmpty = false
        });
        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            RelatedRef = (RowRef)row.ClassJobRequired,
            SectionName = "Required Class"
        });
        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            RelatedRef = (RowRef)row.ClassJobUnlock,
            SectionName = "Class Unlocked"
        });
        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            RelatedRef = (RowRef)row.BeastTribe,
            SectionName = "Allied Society (Beast Tribe)"
        });
        //TODO: Add in some sort of automatic level mapping shit
        if (row.IssuerLocation.RowId != 0)
        {
            var issuerLocation = _levelSheet.GetRow(row.IssuerLocation.RowId);
            string issuerName;
            if (row.IssuerStart.Is<ENpcResident>())
            {
                issuerName = row.IssuerStart.GetValueOrDefault<ENpcResident>()!.Value.Singular.ToImGuiString();
            }
            else if (row.IssuerStart.Is<EObjName>())
            {
                issuerName = row.IssuerStart.GetValueOrDefault<EObjName>()!.Value.Singular.ToImGuiString();
            }
            else
            {
                issuerName = "Issuer";
            }
            viewBuilder.AddMapLinkSectionSection(new MapLinkViewSectionOptions()
            {
                MapLink = new MapLinkEntry(60453, issuerName,  issuerLocation.FormattedName, issuerLocation),
                SectionName = "Issuer Location"
            });
        }

        var relatedNpcs = _eNpcBaseSheet.Where(c => c.ENpcData.Any(c => c.RowId == row.RowId)).Select(c => c.AsUntypedRowRef()).ToList();

        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            RelatedRefs = relatedNpcs,
            SectionName = "Related NPCs",
            Filter = typeof(ENpcBase)
        });
    }

    public override string Singular => "Quest";
    public override string Plural => "Quests";
    public override string Description => "Quests the character can complete.";
    public override string Key => "quest";
    public override (string?, uint?) Icon => (null, Icons.QuestIcon);
}