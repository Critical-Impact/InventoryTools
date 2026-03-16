using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using DalaMock.Host.Mediator;
using Dalamud.Utility;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Localizers;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace InventoryTools.Compendium.Types;

public class ClassJobCompendiumType : CompendiumType<ClassJobRow>
{
    private readonly ClassJobSheet _classJobSheet;
    private readonly SubrowExcelSheet<ScenarioTreeTipsClassQuest> _treeTipsClassQuestSheet;
    private readonly ILocalizer<RoleType> _roleLocalizer;

    public ClassJobCompendiumType(ClassJobSheet classJobSheet,
        SubrowExcelSheet<ScenarioTreeTipsClassQuest> treeTipsClassQuestSheet,
        ILocalizer<RoleType> roleLocalizer,
        CompendiumTable<ClassJobRow>.Factory tableFactory,
        Func<CompendiumColumnBuilder<ClassJobRow>> columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory,
        columnBuilder,
        viewBuilderFactory)
    {
        _classJobSheet = classJobSheet;
        _treeTipsClassQuestSheet = treeTipsClassQuestSheet;
        _roleLocalizer = roleLocalizer;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<ClassJobRow>()
        {
            CompendiumType = this,
            Key = "classes",
            Name = "Classes",
            Columns = BuiltColumns()
        });
    }

    public override string? GetName(ClassJobRow row)
    {
        return row.Base.Name.ToImGuiString().FirstCharToUpper();
    }

    public override string? GetSubtitle(ClassJobRow row)
    {
        return row.Base.ClassJobCategory.Value.Name.ToImGuiString();
    }

    public override (string?, uint?) GetIcon(ClassJobRow row)
    {
        return (null, (uint)row.Icon);
    }

    public override ClassJobRow? GetRow(uint row)
    {
        if (row == 0)
        {
            return null;
        }
        return _classJobSheet.GetRow(row);
    }

    public override bool HasRow(uint rowId)
    {
        if (rowId == 0)
        {
            return false;
        }
        return _classJobSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<ClassJobRow> GetRows()
    {
        return _classJobSheet.Where(c => c.RowId != 0 && c.Base.StartingLevel != 0).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<ClassJobRow> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "##Icon", HelpText = "The icon of the class", Version = "14.0.3", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.RowId});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the class", Version = "14.0.3", ValueSelector = this.GetName});
        builder.AddStringColumn(new (){Key = "type", Name = "Type", HelpText = "The type of the class", Version = "14.0.3", ValueSelector = row => row.Base.ClassJobCategory.Value.Name.ToImGuiString()});
        builder.AddStringColumn(new (){Key = "role", Name = "Role", HelpText = "The role of the class", Version = "14.0.3", ValueSelector = row => _roleLocalizer.Format(row.Role)});
        builder.AddIntegerColumn(new (){Key = "start_level", Name = "Start Level", HelpText = "The level the class starts at", Version = "14.0.3", ValueSelector = row => row.Base.StartingLevel.ToString()});
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, ClassJobRow row)
    {
        viewBuilder.SetupDefaults(this, row);
        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            SectionName = "Base Class",
            RelatedRef = (RowRef)row.Base.ClassJobParent
        });
        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            SectionName = "Soul Crystal",
            RelatedRef = (RowRef)row.Base.ItemSoulCrystal,
        });
        var firstQuest = _treeTipsClassQuestSheet.GetRow(row.RowId).FirstOrNull();
        if (firstQuest != null)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionName = "Unlock Quest",
                RelatedRef = (RowRef)firstQuest.Value.Quest
            });
        }

        if (firstQuest == null)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionName = "Unlock Quest",
                RelatedRef = (RowRef)row.Base.UnlockQuest
            });
        }

        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            SectionName = "Relic Quest",
            RelatedRef = (RowRef)row.Base.RelicQuest
        });
        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            SectionName = "Prerequisite Quest",
            RelatedRef = (RowRef)row.Base.Prerequisite
        });
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return new List<ICompendiumGrouping>()
        {
            new CompendiumGrouping<ClassJobRow>()
            {
                Key = "role",
                Name = "Role",
                GroupFunc = row => row.Role,
                GroupMapping = o =>
                {
                    var role = (RoleType)o;
                    return _roleLocalizer.Format(role);
                }
            },
        };
    }

    public override List<Type>? RelatedTypes => [typeof(ClassJob)];

    public override string Singular => "Class";
    public override string Plural => "Classes";
    public override string Description => "The classes/jobs your character can learn.";
    public override string Key => "classes";
    public override (string?, uint?) Icon => (null, Icons.ManSwordIcon);
}