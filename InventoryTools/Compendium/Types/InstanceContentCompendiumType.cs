using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.Model;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using InventoryTools.Compendium.Columns.Options;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Icons = AllaganLib.Shared.Misc.Icons;

namespace InventoryTools.Compendium.Types;

public class InstanceContentCompendiumType : CompendiumType<InstanceContent>
{
    private readonly ExcelSheet<InstanceContent> _instanceContentSheet;
    private readonly ExcelSheet<Quest> _questSheet;
    private readonly IUnlockState _unlockState;
    private readonly IUIStateService _uiStateService;

    public InstanceContentCompendiumType(ExcelSheet<InstanceContent> instanceContentSheet, ExcelSheet<Quest> questSheet, IUnlockState unlockState, IUIStateService uiStateService, CompendiumTable<InstanceContent>.Factory tableFactory, CompendiumColumnBuilder<InstanceContent>.Factory columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _instanceContentSheet = instanceContentSheet;
        _questSheet = questSheet;
        _unlockState = unlockState;
        _uiStateService = uiStateService;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<InstanceContent>()
        {
            CompendiumType = this,
            Key = "instance_content",
            Name = "Instance Content",
            Columns = BuiltColumns
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

    public override uint GetRowId(InstanceContent row)
    {
        return row.RowId;
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

        builder.AddIntegerColumn(new()
        {
            Key = "level",
            Name = "Level",
            HelpText = "Required class/job level",
            Version = "14.0.3",
            ValueSelector = row =>
                row.ContentFinderCondition.Value.ClassJobLevelRequired.ToString()
        });

        builder.AddBooleanColumn(new BooleanColumnOptions<InstanceContent>()
        {
            Key = "unlocked",
            Name = "Unlocked?",
            HelpText = "Is the instance unlocked?",
            Version = "14.1.3",
            ValueSelector = row => _unlockState.IsInstanceContentUnlocked(row)
        });

        builder.AddBooleanColumn(new BooleanColumnOptions<InstanceContent>()
        {
            Key = "completed",
            Name = "Completed?",
            HelpText = "Is the instance completed?",
            Version = "14.1.3",
            ValueSelector = row => _uiStateService.IsInstanceContentCompleted(row)
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

        viewBuilder.AddTag("Unlocked?", "Is the instance unlocked?", () => _unlockState.IsInstanceContentUnlocked(row) ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);
        viewBuilder.AddTag("Completed?", "Is the instance completed?", () => _uiStateService.IsInstanceContentCompleted(row) ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);

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

    public override bool HasLocation => true;

    public override ILocation? GetLocation(InstanceContent row)
    {
        var territoryType = row.ContentFinderCondition.ValueNullable?.TerritoryType;
        if (territoryType == null || territoryType.Value.RowId == 0)
        {
            return null;
        }

        return new TerritoryLocation(territoryType.Value.Value);
    }

    public override string Singular => "Instance";
    public override string Plural => "Instances";
    public override string Description => "Instances including duties, trials, etc";
    public override string Key => "instance";
    public override (string?, uint?) Icon => (null, Icons.DutyIcon);
}