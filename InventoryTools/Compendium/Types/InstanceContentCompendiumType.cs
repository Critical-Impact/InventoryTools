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
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class InstanceContentCompendiumType : CompendiumType<InstanceContent>
{
    private readonly ExcelSheet<InstanceContent> _instanceContentSheet;
    private readonly ExcelSheet<Quest> _questSheet;

    public InstanceContentCompendiumType(ExcelSheet<InstanceContent> instanceContentSheet, ExcelSheet<Quest> questSheet, CompendiumTable<InstanceContent>.Factory tableFactory, Func<CompendiumColumnBuilder<InstanceContent>> columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _instanceContentSheet = instanceContentSheet;
        _questSheet = questSheet;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<InstanceContent>()
        {
            CompendiumType = this,
            Key = "instance_content",
            Name = "Instance Content",
            Columns = BuiltColumns()
        });
    }

    public override string? GetName(InstanceContent row)
    {
        return row.ContentFinderCondition.Value.Name.ToImGuiString().FirstCharToUpper();
    }

    public override string? GetSubtitle(InstanceContent row)
    {
        return row.ContentFinderCondition.Value.ContentType.Value.Name.ToImGuiString();
    }

    public override (string?, uint?) GetIcon(InstanceContent row)
    {
        return (null, row.ContentFinderCondition.Value.ContentType.Value.IconDutyFinder);
    }

    public override InstanceContent GetRow(uint row)
    {
        return _instanceContentSheet.GetRow(row);
    }

    public override bool HasRow(uint rowId)
    {
        return _instanceContentSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<InstanceContent> GetRows()
    {
        return _instanceContentSheet.Where(c => c.ContentFinderCondition.RowId != 0).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<InstanceContent> builder)
    {
        builder.AddCompendiumOpenViewColumn(new()
        {
            Key = "icon",
            Name = "Icon",
            HelpText = "Duty icon",
            Version = "14.0.3",
            CompendiumType = this,
            RowIdSelector = row => row.RowId,
            ValueSelector = GetIcon
        });

        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "Name",
            HelpText = "The name of the duty",
            Version = "14.0.3",
            ValueSelector = GetName
        });
        //
        // builder.AddStringColumn(new()
        // {
        //     Key = "roulettes",
        //     Name = "Roulettes",
        //     HelpText = "Duty roulettes containing this duty",
        //     Version = "14.0.3",
        //     ValueSelector = row =>
        //         row.ContentFinderCondition.Value.Roulettes
        // });

        builder.AddIntegerColumn(new()
        {
            Key = "level",
            Name = "Level",
            HelpText = "Required class/job level",
            Version = "14.0.3",
            ValueSelector = row =>
                row.ContentFinderCondition.Value.ClassJobLevelRequired.ToString()
        });

        builder.AddIntegerColumn(new()
        {
            Key = "sync_level",
            Name = "Sync Level",
            HelpText = "Level sync applied in the duty",
            Version = "14.0.3",
            ValueSelector = row =>
                row.ContentFinderCondition.Value.ClassJobLevelSync.ToString()
        });

        builder.AddIntegerColumn(new()
        {
            Key = "item_level",
            Name = "Item Level",
            HelpText = "Minimum item level required",
            Version = "14.0.3",
            ValueSelector = row =>
                row.ContentFinderCondition.Value.ItemLevelRequired.ToString()
        });

        builder.AddIntegerColumn(new()
        {
            Key = "item_level_sync",
            Name = "Item Level Sync",
            HelpText = "Maximum synced item level",
            Version = "14.0.3",
            ValueSelector = row =>
                row.ContentFinderCondition.Value.ItemLevelSync.ToString()
        });

        builder.AddBooleanColumn(new()
        {
            Key = "allows_undersized",
            Name = "Allows Undersized",
            HelpText = "Whether the duty allows undersized party mode",
            Version = "14.0.3",
            ValueSelector = row =>
                row.ContentFinderCondition.Value.AllowUndersized
        });

        builder.AddBooleanColumn(new()
        {
            Key = "allows_explorer_mode",
            Name = "Allows Explorer Mode",
            HelpText = "Whether the duty supports explorer mode",
            Version = "14.0.3",
            ValueSelector = row =>
                row.ContentFinderCondition.Value.AllowExplorerMode
        });

        builder.AddBooleanColumn(new()
        {
            Key = "pvp",
            Name = "PVP",
            HelpText = "Whether this duty is PvP",
            Version = "14.0.3",
            ValueSelector = row =>
                row.ContentFinderCondition.Value.PvP
        });

        builder.AddStringColumn(new()
        {
            Key = "accepted_classes",
            Name = "Accepted Classes",
            HelpText = "Class/job categories allowed to enter",
            Version = "14.0.3",
            ValueSelector = row =>
                row.ContentFinderCondition.Value.AcceptClassJobCategory.ValueNullable?.Name.ToImGuiString() ?? "Unknown"
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, InstanceContent row)
    {
        viewBuilder.SetupDefaults(this, row);

        var relatedQuests = _questSheet.Where(c => c.InstanceContent.Any(c => c.RowId == row.RowId) || c.QuestParams.Any(c => c.ScriptArg == row.RowId && c.ScriptInstruction.ToString().StartsWith("INSTANCEDUNGEON")))
            .Select(c => c.AsUntypedRowRef()).ToList();
        relatedQuests.Add(row.ContentFinderCondition.Value.UnlockCriteria);
        relatedQuests.Add(row.ContentFinderCondition.Value.UnlockCriteria2);
        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            RelatedRefs = relatedQuests,
            HideIfEmpty = true,
            SectionName = "Related Quests"
        });
        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            RelatedRef = row.ContentFinderCondition.Value.TerritoryType.Value.AsUntypedRowRef(),
            SectionName = "Related Map"
        });

    }

    public override string Singular => "Instance";
    public override string Plural => "Instances";
    public override string Description => "Instances including duties, trials, etc";
    public override string Key => "instance";
    public override (string?, uint?) Icon => (null, Icons.DutyIcon);
}