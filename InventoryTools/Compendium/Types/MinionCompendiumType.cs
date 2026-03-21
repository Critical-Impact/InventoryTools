using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class MinionCompendiumType : CompendiumType<Companion>
{
    private readonly IUnlockState _unlockState;
    private readonly ItemInfoCache _itemInfoCache;
    private readonly ExcelSheet<ItemAction> _itemActionSheet;
    private readonly ExcelSheet<Item> _itemSheet;
    private readonly ExcelSheet<Companion> _companionSheet;
    private readonly ExcelSheet<CompanionTransient> _companionTransientSheet;
    private readonly ExcelSheet<MinionRace> _minionRaceSheet;
    private Dictionary<uint, uint>? _companionToItem;
    private Dictionary<uint, uint>? _itemToCompanion;

    public MinionCompendiumType(IUnlockState unlockState,
        ItemInfoCache itemInfoCache,
        ExcelSheet<ItemAction> itemActionSheet,
        ExcelSheet<Item> itemSheet,
        ExcelSheet<Companion> companionSheet,
        ExcelSheet<CompanionTransient> companionTransientSheet,
        ExcelSheet<MinionRace> minionRaceSheet,
        CompendiumTable<Companion>.Factory tableFactory,
        Func<CompendiumColumnBuilder<Companion>> columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory,
        columnBuilder,
        viewBuilderFactory)
    {
        _unlockState = unlockState;
        _itemInfoCache = itemInfoCache;
        _itemActionSheet = itemActionSheet;
        _itemSheet = itemSheet;
        _companionSheet = companionSheet;
        _companionTransientSheet = companionTransientSheet;
        _minionRaceSheet = minionRaceSheet;
    }

    public Item? GetRelatedItem(uint companionId)
    {
        if (_companionToItem == null)
        {
            CalculateMapping();
        }
        return _companionToItem!.TryGetValue(companionId, out var value) ? _itemSheet.GetRow(value) : null;
    }

    private void CalculateMapping()
    {
        _companionToItem = new Dictionary<uint, uint>();
        _itemToCompanion = new Dictionary<uint, uint>();
        foreach (var item in _itemSheet)
        {
            if (item.ItemAction.RowId != 0)
            {
                var itemAction = item.ItemAction.Value;
                if (itemAction.Action.RowId == 853) //Minion
                {
                    var companionId = itemAction.Data[0];
                    if (companionId != 0)
                    {
                        _companionToItem.TryAdd(companionId, item.RowId);
                        _itemToCompanion.TryAdd(item.RowId, companionId);
                    }
                }
            }
        }
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<Companion>()
        {
            Name = "Minions",
            Columns = BuiltColumns(),
            CompendiumType = this,
            Key = "minions",
        });
    }

    public override string? GetName(Companion row)
    {
        return row.Singular.ToImGuiString().FirstCharToUpper();
    }

    public override string? GetSubtitle(Companion row)
    {
        return row.MinionRace.Value.Name.ToImGuiString();
    }

    public override (string?, uint?) GetIcon(Companion row)
    {
        return (null, row.Icon);
    }

    public override Companion GetRow(uint row)
    {
        return _companionSheet.GetRow(row);
    }

    public override List<Companion> GetRows()
    {
        return _companionSheet.Where(c => !c.Singular.IsEmpty).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<Companion> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "##Icon", HelpText = "The icon of the minion", Version = "14.1.2", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.RowId});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the minion", Version = "14.1.2", ValueSelector = this.GetName});
        builder.AddStringColumn(new (){Key = "category", Name = "Category", HelpText = "The category of the minion", Version = "14.1.2", ValueSelector = row => row.MinionRace.Value.Name.ToImGuiString()});
        builder.AddBooleanColumn(new (){Key = "unlocked", Name = "Unlocked", HelpText = "Is the minion unlocked?.", Version = "14.1.2", ValueSelector = row => _unlockState.IsCompanionUnlocked(row)});
        builder.AddItemSourcesColumn(new (){Key = "sources", Name = "Sources", HelpText = "The sources for obtaining the minion", Version = "14.1.2", ValueSelector =
            row =>
            {
                var relatedItem = GetRelatedItem(row.RowId);
                if (relatedItem != null)
                {
                    return _itemInfoCache.GetItemSources(relatedItem.Value.RowId) ?? [];
                }

                return [];
            }});
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, Companion row)
    {
        viewBuilder.SetupDefaults(this, row);
        var transientInformation = _companionTransientSheet.GetRow(row.RowId);
        viewBuilder.Description = transientInformation.DescriptionEnhanced.ToImGuiString();
        viewBuilder.AddTag("Unlocked?", "Is the minion unlocked?", () => _unlockState.IsCompanionUnlocked(row) ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);
        viewBuilder.AddTag("Type: " + transientInformation.MinionSkillType.Value.Name.ToImGuiString(), "The skill type of the minion.");
        viewBuilder.AddTag("Action: " + transientInformation.SpecialActionName.ToImGuiString(), transientInformation.SpecialActionDescription.ToImGuiString());
        viewBuilder.AddInfoTableSection(new InfoTableSectionOptions()
        {
            SectionName = "Stats",
            Items =
            [
                ("Attack", transientInformation.Attack.ToString(), true),
                ("Defense", transientInformation.Defense.ToString(), true),
                ("Attack", transientInformation.Speed.ToString(), true),
            ]
        });
        var relatedItem = GetRelatedItem(row.RowId);
        if (relatedItem != null)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionName = "Related Item",
                RelatedRef = relatedItem.Value.AsUntypedRowRef()
            });
            var itemSources = _itemInfoCache.GetItemSources(relatedItem.Value.RowId);
            viewBuilder.AddItemSourcesSection(new ItemSourcesSectionOptions()
            {
                SectionName = "Sources",
                Sources = itemSources ?? []
            });
        }
    }

    public override bool HasRow(uint rowId)
    {
        if (rowId == 0)
        {
            return false;
        }
        return _companionSheet.HasRow(rowId);
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return new()
        {
            new CompendiumGrouping<Companion>()
            {
                Key = "category",
                Name = "Category",
                GroupFunc = r => r.MinionRace.RowId,
                GroupMapping = o =>
                {
                    var minionRace = (uint)o;
                    return _minionRaceSheet.GetRowOrDefault(minionRace)?.Name.ToImGuiString() ?? "Unknown";
                }
            }
        };
    }

    public override string Singular => "Minion";
    public override string Plural => "Minions";
    public override string Description => "Unlockable minions summonable by the player.";
    public override string Key => "minions";
    public override (string?, uint?) Icon => (null, Icons.MinionIcon);
}