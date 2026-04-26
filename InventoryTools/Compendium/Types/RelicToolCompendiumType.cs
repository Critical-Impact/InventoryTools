using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using DalaMock.Host.Mediator;
using Dalamud.Utility;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Compendium.Types.Extra;
using InventoryTools.Localizers;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Compendium.Types;

public class RelicToolCompendiumType : CompendiumType<RelicToolGroup>
{
    private readonly ExcelSheet<ClassJob> _classJobSheet;
    private readonly ItemSheet _itemSheet;
    private readonly ILocalizer<RelicToolType> _toolTypeLocalizer;
    private readonly ILocalizer<RelicToolCategory> _toolCategoryLocalizer;
    private readonly Lazy<List<RelicToolGroup>> _groupedTools;

    public RelicToolCompendiumType(List<RelicTool> relicTools, ExcelSheet<ClassJob> classJobSheet, ItemSheet itemSheet, ILocalizer<RelicToolType> toolTypeLocalizer, ILocalizer<RelicToolCategory> toolCategoryLocalizer, CompendiumTable<RelicToolGroup>.Factory tableFactory, CompendiumColumnBuilder<RelicToolGroup>.Factory columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _classJobSheet = classJobSheet;
        _itemSheet = itemSheet;
        _toolTypeLocalizer = toolTypeLocalizer;
        _toolCategoryLocalizer = toolCategoryLocalizer;
        _groupedTools = new Lazy<List<RelicToolGroup>>(() =>
        {
            uint rowId = 0;
            return relicTools.GroupBy(c => (c.Category, c.ClassJob.RowId))
                .Select(c => new RelicToolGroup(c.First().ClassJob, c.Key.Category, rowId++, c.ToList())).ToList();
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<RelicToolGroup>()
        {
            Columns = BuiltColumns,
            CompendiumType = this,
            Key = "relic_tools",
            Name = "Relic Tools",
        });
    }

    public override string? GetName(RelicToolGroup row)
    {
        return row.ClassJob.Value.Name.ToImGuiString().FirstCharToUpper() + " - " + _toolCategoryLocalizer.Format(row.ToolCategory);
    }

    public override string? GetSubtitle(RelicToolGroup row)
    {
        return null; //todo: add formatter
    }

    public override (string?, uint?) GetIcon(RelicToolGroup row)
    {
        return (null, Icons.ToolIcon); //come up with better icons
    }

    public override uint GetRowId(RelicToolGroup row)
    {
        return row.RowId;
    }

    public override RelicToolGroup? GetRow(uint row)
    {
        return _groupedTools.Value[(int)row];
    }

    public override List<RelicToolGroup> GetRows()
    {
        return _groupedTools.Value;
    }

    public override void BuildColumns(CompendiumColumnBuilder<RelicToolGroup> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "##Icon", HelpText = "The icon of the BGM", Version = "14.1.3", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.RowId});
        var grouping = builder.CompendiumGrouping?.Key;
        if (grouping != "category")
        {
            builder.AddStringColumn(new ()
            {
                ValueSelector = row => _toolCategoryLocalizer.Format(row.ToolCategory),
                Name = "Category",
                Key = "Category",
                HelpText = "The category of the item",
                Version = "14.1.3"
            });
        }
        if (grouping != "class_job")
        {
            builder.AddStringColumn(new ()
            {
                ValueSelector = row => row.ClassJob.Value.Name.ToImGuiString().FirstCharToUpper() ?? "Unknown",
                Name = "Class/Job",
                Key = "class_job",
                HelpText = "The class/job of the item",
                Version = "14.1.3"
            });
        }
        if (builder.CompendiumGrouping != null && builder.GroupItem != null)
        {
            if (builder.CompendiumGrouping.Key == "category")
            {
                if (builder.GroupItem is RelicToolCategory toolCategory)
                {
                    RelicToolType relicToolType;
                    switch (toolCategory)
                    {
                        case RelicToolCategory.Mastercraft:
                            relicToolType = RelicToolType.MastercraftBase;
                            break;
                        case RelicToolCategory.Skysteel:
                            relicToolType = RelicToolType.SkysteelBase;
                            break;
                        case RelicToolCategory.Resplendent:
                            relicToolType = RelicToolType.Resplendent;
                            break;
                        case RelicToolCategory.Splendorous:
                            relicToolType = RelicToolType.SplendorousBase;
                            break;
                        case RelicToolCategory.Cosmic:
                            relicToolType = RelicToolType.CosmicPrototype01;
                            break;
                        default:
                            return;
                    }

                    var maxItems = _groupedTools.Value.Where(c => c.ToolCategory == toolCategory).Max(c => c.RelicTools.Count);
                    for (int i = 0; i < maxItems; i++)
                    {
                        var i1 = i;
                        builder.AddItemsColumn(new()
                        {
                            Key = "item_" + i,
                            Name = _toolTypeLocalizer.Format(relicToolType), HelpText = "Form " + (i + 1) + " of this tool.",
                            Version = "14.1.3",
                            ValueSelector = relicTool => toolCategory == relicTool.ToolCategory ? [_itemSheet.GetRow(relicTool.RelicTools[i1].ItemId)] : []
                        });
                        relicToolType++;
                    }
                }


                return;
            }
        }
        for (int i = 0; i < _groupedTools.Value.Max(c => c.RelicTools.Count); i++)
        {
            var i1 = i;
            builder.AddItemColumn(new()
            {
                Key = "item_" + i,
                Name = "Form " + (i + 1), HelpText = "Form " + (i + 1) + " of this tool.",
                Version = "14.1.3",
                ValueSelector = relicTool =>
                    i1 >= 0 && i1 < relicTool.RelicTools.Count ? relicTool.RelicTools[i1].ItemId : null
            });
        }
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, RelicToolGroup row)
    {
        viewBuilder.SetupDefaults(this, row);
        //Maybe need to work on groups or some sort of item display that displays each item in a table the form and an arrow to the next
        List<ItemFlowEntry> itemFlowEntries = [];
        for (var index = 0; index < row.RelicTools.Count; index++)
        {
            var relicTool = row.RelicTools[index];
            itemFlowEntries.Add(new ItemFlowEntry()
            {
                Item = _itemSheet.GetRow(relicTool.ItemId),
                Item2 = null,
                Title = (index + 1) + ". " + _toolTypeLocalizer.Format(relicTool.Type)
            });
        }

        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            SectionName = "Class/Job",
            RelatedRef = (RowRef)row.ClassJob
        });
        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            SectionName = "Related Quests",
            RelatedRefs = row.Quests.Where(c => c.RowId != 0).Select(c => (RowRef)c).ToList(),
        });
        viewBuilder.AddItemFlowSection(new ItemFlowSectionOptions()
        {
            SectionName = "Tools",
            Items = itemFlowEntries,
            ItemsPerColumn = Math.Max(3, (int)Math.Ceiling((double)itemFlowEntries.Count / 3))
        });
    }

    public override bool HasRow(uint rowId)
    {
        return (int)rowId >= 0 && (int)rowId < _groupedTools.Value.Count;
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return new List<ICompendiumGrouping>()
        {
            new CompendiumGrouping<RelicToolGroup>()
            {
                Key = "class_job",
                Name = "Class/Job",
                GroupFunc = row => row.ClassJob.RowId,
                GroupMapping = row =>
                {
                    var classJobId = (uint)row;
                    var name = _classJobSheet.GetRowOrDefault(classJobId)?.Name.ToImGuiString().FirstCharToUpper() ??
                               "None";
                    if (name == string.Empty)
                    {
                        name = "Ungrouped";
                    }

                    return name;
                }
            },
            new CompendiumGrouping<RelicToolGroup>()
            {
                Key = "category",
                Name = "Category",
                GroupFunc = row => row.ToolCategory,
                GroupMapping = row =>
                {
                    var relicToolCategory = (RelicToolCategory)row;
                    return _toolCategoryLocalizer.Format(relicToolCategory);
                }
            }
        };
    }

    public override List<Type>? RelatedTypes => [typeof(RelicTool)];

    public override uint? RemapType(Type type, uint rowId)
    {
        if (type == typeof(RelicTool))
        {
            return _groupedTools.Value.FirstOrDefault(c => c.RelicTools.Any(d => d.RowId == rowId))?.RowId;
        }

        return null;
    }

    public override string? GetDefaultGrouping()
    {
        return "category";
    }

    public override string Singular => "Relic Tool";
    public override string Plural => "Relic Tools";
    public override string Description => "Relic Tools";
    public override string Key => "relic_tools";
    public override (string?, uint?) Icon => (null, Icons.ToolIcon);
}