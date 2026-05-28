using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Compendium.Types.Extra;
using InventoryTools.Ui;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class FolkloreTomeCompendiumType : CompendiumType<FolkloreTome>
{
    private readonly ExcelSheet<GatheringSubCategory> _gatheringSubCategorySheet;
    private readonly ExcelSheet<NotebookDivision> _notebookDivisionSheet;
    private readonly ClassJobService _classJobService;
    private readonly ItemInfoCache _itemInfoCache;
    private readonly Lazy<List<FolkloreTome>> _folkloreTomes;
    private readonly Lazy<Dictionary<uint, FolkloreTome>> _folkloreTomesById;
    private readonly Lazy<(string?, uint?)> _staticIcon;

    public FolkloreTomeCompendiumType(
        ExcelSheet<GatheringSubCategory> gatheringSubCategorySheet,
        ExcelSheet<NotebookDivision> notebookDivisionSheet,
        ClassJobService classJobService,
        ItemInfoCache itemInfoCache,
        CompendiumTable<FolkloreTome>.Factory tableFactory,
        CompendiumColumnBuilder<FolkloreTome>.Factory columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory)
        : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _gatheringSubCategorySheet = gatheringSubCategorySheet;
        _notebookDivisionSheet = notebookDivisionSheet;
        _classJobService = classJobService;
        _itemInfoCache = itemInfoCache;

        _folkloreTomes = new Lazy<List<FolkloreTome>>(BuildFolkloreTomes, LazyThreadSafetyMode.ExecutionAndPublication);
        _folkloreTomesById = new Lazy<Dictionary<uint, FolkloreTome>>(
            () => _folkloreTomes.Value.ToDictionary(t => t.RowId),
            LazyThreadSafetyMode.ExecutionAndPublication);
        _staticIcon = new Lazy<(string?, uint?)>(BuildStaticIcon, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public override string Singular => "Folklore Tome";
    public override string Plural => "Folklore Tomes";
    public override string Description => "Folklore tomes that unlock hidden gathering nodes when read.";
    public override string Key => "folklore_tomes";
    public override (string?, uint?) Icon => _staticIcon.Value;

    public override List<Type>? RelatedTypes => [typeof(NotebookDivision)];

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

    public override string? GetName(FolkloreTome row)
    {
        var itemName = row.TomeItem?.ValueNullable?.Name.ToImGuiString();
        if (!string.IsNullOrEmpty(itemName))
        {
            return itemName;
        }

        return row.NotebookDivision.ValueNullable?.Name.ToImGuiString();
    }

    public override string? GetSubtitle(FolkloreTome row)
    {
        var types = row.GatheringTypes
            .Select(g => g.ValueNullable?.Name.ToImGuiString())
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        return types.Count == 0 ? null : string.Join(", ", types);
    }

    public override (string?, uint?) GetIcon(FolkloreTome row)
    {
        var iconId = row.TomeItem?.ValueNullable?.Icon;
        if (iconId.HasValue && iconId.Value != 0)
        {
            return (null, iconId.Value);
        }

        return (null, null);
    }

    public override uint GetRowId(FolkloreTome row)
    {
        return row.RowId;
    }

    public override FolkloreTome? GetRow(uint row)
    {
        return _folkloreTomesById.Value.GetValueOrDefault(row);
    }

    public override bool HasRow(uint rowId)
    {
        return _folkloreTomesById.Value.ContainsKey(rowId);
    }

    public override List<FolkloreTome> GetRows()
    {
        return _folkloreTomes.Value;
    }

    public override uint? RemapType(Type type, uint rowId)
    {
        if (type == typeof(NotebookDivision) && _folkloreTomesById.Value.ContainsKey(rowId))
        {
            return rowId;
        }

        return null;
    }

    public override void BuildColumns(CompendiumColumnBuilder<FolkloreTome> builder)
    {
        builder.AddCompendiumOpenViewColumn(new()
        {
            Key = "icon",
            Name = "##Icon",
            HelpText = "The icon of the folklore tome",
            Version = "15.0.6",
            ValueSelector = GetIcon,
            CompendiumType = this,
            RowIdSelector = row => row.RowId,
        });
        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "Name",
            HelpText = "The name of the folklore tome",
            Version = "15.0.6",
            ValueSelector = GetName,
        });
        builder.AddStringColumn(new()
        {
            Key = "gathering_types",
            Name = "Gathering Types",
            HelpText = "The gathering types unlocked by this tome",
            Version = "15.0.6",
            ValueSelector = row => FormatGatheringTypes(row),
        });
        builder.AddStringColumn(new()
        {
            Key = "class_job",
            Name = "Class/Job",
            HelpText = "The classes or jobs this tome applies to",
            Version = "15.0.6",
            ValueSelector = row => FormatClassJobs(row),
        });
        builder.AddStringColumn(new()
        {
            Key = "unlock_quest",
            Name = "Unlock Quest",
            HelpText = "The quest required to unlock this tome",
            Version = "15.0.6",
            ValueSelector = row => row.UnlockQuest?.ValueNullable?.Name.ToImGuiString() ?? "",
        });
        builder.AddBooleanColumn(new()
        {
            Key = "unlocked",
            Name = "Unlocked?",
            HelpText = "Has the player unlocked this folklore tome?",
            Version = "15.0.6",
            ValueSelector = row => _classJobService.IsFolkloreBookUnlocked(row.NotebookDivision),
        });
        builder.AddItemColumn(new()
        {
            Key = "tome_item",
            Name = "Tome Item",
            HelpText = "The folklore tome item itself",
            Version = "15.0.6",
            ValueSelector = row => row.TomeItem?.RowId,
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, FolkloreTome row)
    {
        viewBuilder.SetupDefaults(this, row);

        viewBuilder.AddTag(
            () => _classJobService.IsFolkloreBookUnlocked(row.NotebookDivision) ? "Read" : "Not Read",
            () => "Whether the player has read this folklore tome.",
            () => _classJobService.IsFolkloreBookUnlocked(row.NotebookDivision) ? Dalamud.Interface.Colors.ImGuiColors.HealerGreen : Dalamud.Interface.Colors.ImGuiColors.DalamudRed);

        var info = new List<(string Header, string Value, bool IsVisible)>
        {
            ("Gathering Types", FormatGatheringTypes(row), row.GatheringTypes.Count > 0),
            ("Class/Job", FormatClassJobs(row), row.ClassJobs.Count > 0),
        };
        viewBuilder.AddInfoTableSection(new InfoTableSectionOptions()
        {
            SectionKey = "info",
            SectionName = "Info",
            Items = info,
        });

        foreach (var classJob in row.ClassJobs)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionKey = $"class_job_{classJob.RowId}",
                SectionName = "Class/Job",
                RelatedRef = (RowRef)classJob,
            });
        }

        if (row.TomeItem.HasValue && row.TomeItem.Value.RowId != 0)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionKey = "tome_item",
                SectionName = "Tome Item",
                RelatedRef = (RowRef)row.TomeItem.Value,
            });

            var itemSources = _itemInfoCache.GetItemSources(row.TomeItem.Value.RowId);
            viewBuilder.AddItemSourcesSection(new ItemSourcesSectionOptions()
            {
                SectionKey = "tome_item_sources",
                SectionName = "Tome Item Sources",
                Sources = itemSources ?? [],
                SourceType = SourceType.Source,
            });
        }

        var folkloreUse = _itemInfoCache
            .GetItemUsesByType<ItemFolkloreTomeSource>(ItemInfoType.FolkloreTome)
            .FirstOrDefault(s => s.NotebookDivision.RowId == row.NotebookDivision.RowId);
        var unlockedItems = folkloreUse?.UnlockedItems ?? [];
        viewBuilder.AddItemListSection(new ItemListSectionOptions()
        {
            SectionKey = "unlocked_items",
            SectionName = "Items Unlocked",
            Items = unlockedItems.Select(i => new ItemInfo(i)),
        });

        if (row.UnlockQuest.HasValue && row.UnlockQuest.Value.RowId != 0)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionKey = "unlock_quest",
                SectionName = "Unlock Quest",
                RelatedRef = (RowRef)row.UnlockQuest.Value,
            });
        }
    }

    private static string FormatGatheringTypes(FolkloreTome row)
    {
        var names = row.GatheringTypes
            .Select(g => g.ValueNullable?.Name.ToImGuiString())
            .Where(n => !string.IsNullOrEmpty(n));
        return string.Join(", ", names);
    }

    private static string FormatClassJobs(FolkloreTome row)
    {
        var names = row.ClassJobs
            .Select(c => c.ValueNullable?.Name.ToImGuiString())
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!.ToTitleCase());
        return string.Join(", ", names);
    }

    private List<FolkloreTome> BuildFolkloreTomes()
    {
        return _gatheringSubCategorySheet
            .Where(s => s.Division != 0 && s.Item.RowId != 0)
            .GroupBy(s => (uint)s.Division)
            .Select(g => new FolkloreTome(
                new RowRef<NotebookDivision>(_notebookDivisionSheet.Module, g.Key, _notebookDivisionSheet.Language),
                g.ToList()))
            .OrderBy(t => t.RowId)
            .ToList();
    }

    private (string?, uint?) BuildStaticIcon()
    {
        var firstWithItem = _folkloreTomes.Value.FirstOrDefault(t => t.TomeItem?.ValueNullable != null);
        var icon = firstWithItem?.TomeItem?.ValueNullable?.Icon;
        if (icon.HasValue && icon.Value != 0)
        {
            return (null, icon.Value);
        }
        return (null, null);
    }
}
