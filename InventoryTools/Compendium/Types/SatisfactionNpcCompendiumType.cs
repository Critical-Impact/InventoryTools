using System.Collections.Generic;
using System.Linq;
using AllaganLib.Shared.Extensions;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Models;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Misc;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;

namespace InventoryTools.Compendium.Types;

public class SatisfactionNpcCompendiumType : CompendiumType<SatisfactionNpc>
{
    private readonly LevelSheet _levelSheet;
    private readonly ExcelSheet<SatisfactionNpc> _satisfactionSheet;
    private readonly SubrowExcelSheet<SatisfactionSupply> _satisfactionSupplySheet;
    private readonly SubrowExcelSheet<SatisfactionArbitration> _satisfactionArbitrationSheet;
    private readonly ExcelSheet<SatisfactionSupplyReward> _satisfactionSupplyRewardSheet;
    private readonly ExcelSheet<Quest> _questSheet;
    private readonly ExcelSheet<ExVersion> _expansionSheet;
    private readonly ENpcResidentSheet _eNpcResidentSheet;
    private readonly ItemSheet _itemSheet;

    public SatisfactionNpcCompendiumType(
        LevelSheet levelSheet,
        ExcelSheet<SatisfactionNpc> satisfactionSheet,
        SubrowExcelSheet<SatisfactionSupply> satisfactionSupplySheet,
        SubrowExcelSheet<SatisfactionArbitration> satisfactionArbitrationSheet,
        ExcelSheet<SatisfactionSupplyReward> satisfactionSupplyRewardSheet,
        ExcelSheet<Quest> questSheet,
        ExcelSheet<ExVersion> expansionSheet,
        ENpcResidentSheet eNpcResidentSheet,
        ItemSheet itemSheet,
        CompendiumTable<SatisfactionNpc>.Factory tableFactory,
        CompendiumColumnBuilder<SatisfactionNpc>.Factory columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory
    ) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _levelSheet = levelSheet;
        _satisfactionSheet = satisfactionSheet;
        _satisfactionSupplySheet = satisfactionSupplySheet;
        _satisfactionArbitrationSheet = satisfactionArbitrationSheet;
        _satisfactionSupplyRewardSheet = satisfactionSupplyRewardSheet;
        _questSheet = questSheet;
        _expansionSheet = expansionSheet;
        _eNpcResidentSheet = eNpcResidentSheet;
        _itemSheet = itemSheet;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<SatisfactionNpc>()
        {
            CompendiumType = this,
            Key = "custom_deliveries",
            Name = "Custom Deliveries",
            Columns = BuiltColumns
        });
    }

    public override string? GetName(SatisfactionNpc row)
    {
        return row.Npc.ValueNullable?.Singular.ToImGuiString() ?? "Unknown";
    }

    public override string? GetSubtitle(SatisfactionNpc row)
    {
        return $"Level {row.LevelUnlock} Custom Delivery Client";
    }

    public override (string?, uint?) GetIcon(SatisfactionNpc row)
    {
        return (null, (uint)row.RankParams.First(c => c.ImageId != 0).ImageId);
    }

    public override uint GetRowId(SatisfactionNpc row)
    {
        return row.RowId;
    }

    public override SatisfactionNpc GetRow(uint row)
    {
        return _satisfactionSheet.GetRow(row);
    }

    public override bool HasRow(uint rowId)
    {
        return _satisfactionSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<SatisfactionNpc> GetRows()
    {
        return _satisfactionSheet
            .Where(c => c.Npc.RowId != 0)
            .ToList();
    }

    public override bool HasLocation => true;

    public override ILocation? GetLocation(SatisfactionNpc row)
    {
        var npcId = row.Npc.RowId;
        if (npcId == 0)
        {
            return null;
        }

        var npc = _eNpcResidentSheet.GetRow(npcId);
        return npc.ENpcBase.Locations.FirstOrDefault();
    }

    public override void BuildColumns(CompendiumColumnBuilder<SatisfactionNpc> builder)
    {
        builder.AddCompendiumOpenViewColumn(new()
        {
            Key = "icon",
            Name = "##Icon",
            HelpText = "The NPC icon",
            Version = "1.0.0",
            ValueSelector = GetIcon,
            CompendiumType = this,
            RowIdSelector = row => row.RowId
        });

        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "NPC",
            HelpText = "The NPC name",
            Version = "1.0.0",
            ValueSelector = GetName
        });

        builder.AddStringColumn(new()
        {
            Key = "level",
            Name = "Level",
            HelpText = "Unlock level",
            Version = "1.0.0",
            ValueSelector = row => row.LevelUnlock.ToString()
        });

        builder.AddStringColumn(new()
        {
            Key = "deliveries",
            Name = "Deliveries / Week",
            HelpText = "Max weekly deliveries",
            Version = "1.0.0",
            ValueSelector = row => row.DeliveriesPerWeek.ToString()
        });

        builder.AddStringColumn(new()
        {
            Key = "unlock_quest",
            Name = "Unlock Quest",
            HelpText = "Required quest",
            Version = "1.0.0",
            ValueSelector = row => row.QuestRequired.ValueNullable?.Name.ToImGuiString() ?? ""
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, SatisfactionNpc row)
    {
        viewBuilder.SetupDefaults(this, row);
        viewBuilder.AddTag(() => $"{row.DeliveriesPerWeek}/week", () => "The number of times you can deliver to the client per week.");

        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            RelatedRef = (RowRef)row.QuestRequired,
            SectionKey = "unlock_quest",
            SectionName = "Unlock Quest"
        });

        var rankQuests = _satisfactionArbitrationSheet
            .GetRow(row.RowId)
            .Where(a => a.Quest.RowId != 0)
            .OrderBy(a => a.SatisfactionLevel)
            .DistinctBy(c => c.Quest.RowId)
            .Select(a => (RowRef)a.Quest)
            .ToList();

        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            RelatedRefs = rankQuests,
            SectionKey = "rank_quests",
            SectionName = "Rank Quests",
            HideWhenEmpty = false
        });

        var tierParams = row.SatisfactionNpcParams
            .Select((param, index) => (param, index))
            .Where(t => t.param.SupplyIndex != 0)
            .ToList();

        for (var i = 0; i < tierParams.Count; i++)
        {
            var (param, _) = tierParams[i];
            var tierNumber = i + 1;
            var supplyItems = _satisfactionSupplySheet.GetRow((uint)param.SupplyIndex)
                .Where(s => s.Item.RowId != 0)
                .DistinctBy(item => item.Item.RowId)
                .Select(s => new ItemInfo(_itemSheet.GetRow(s.Item.RowId)))
                .ToList();

            viewBuilder.AddItemListSection(new ItemListSectionOptions()
            {
                Items = supplyItems,
                SectionKey = $"requested_items_tier_{tierNumber}",
                SectionName = $"Requested Items (Tier {tierNumber})",
                HideWhenEmpty = true
            });
        }

        var location = GetLocation(row);
        if (location != null)
        {
            viewBuilder.AddMapLinkSectionSection(new MapLinkViewSectionOptions()
            {
                MapLink = new MapLinkEntry(
                    Icons.FlagIcon,
                    GetName(row) ?? "NPC",
                    location.FormattedName,
                    location
                ),
                SectionKey = "location",
                SectionName = "Location"
            });
        }

        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            RelatedRef = (RowRef)row.Npc,
            SectionKey = "related_npc",
            SectionName = "Related NPC"
        });

    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return
        [
            new CompendiumGrouping<SatisfactionNpc>()
            {
                Name = "Expansion",
                Key = "expansion",
                GroupFunc = row => row.QuestRequired.ValueNullable?.Expansion.RowId ?? 0,
                GroupMapping = row =>
                {
                    var id = (uint)row;
                    return _expansionSheet.GetRowOrDefault(id)?.Name.ToImGuiString() ?? "Unknown";
                }
            },
            new CompendiumGrouping<SatisfactionNpc>()
            {
                Name = "Level",
                Key = "level",
                GroupFunc = row => row.LevelUnlock,
                GroupMapping = row => $"Level {(byte)row}"
            }
        ];
    }

    public override string Singular => "Custom Delivery";
    public override string Plural => "Custom Deliveries";
    public override string Description => "NPCs that accept Custom Deliveries (Satisfaction system).";
    public override string Key => "satisfaction_npc";
    public override (string?, uint?) Icon => (null, Icons.CustomDeliveriesIcon);
}