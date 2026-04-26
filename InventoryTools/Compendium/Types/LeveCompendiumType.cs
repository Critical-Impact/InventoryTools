using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using DalaMock.Host.Mediator;
using Humanizer;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Localizers;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class LeveCompendiumType : CompendiumType<LeveRow>
{
    private readonly ILocalizer<ENpcBase> _npcLocalizer;
    private readonly LeveSheet _leveSheet;
    private readonly ItemInfoCache _itemInfoCache;

    public LeveCompendiumType(ILocalizer<ENpcBase> npcLocalizer, CompendiumTable<LeveRow>.Factory tableFactory, CompendiumColumnBuilder<LeveRow>.Factory columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory, LeveSheet leveSheet, ItemInfoCache itemInfoCache) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _npcLocalizer = npcLocalizer;
        _leveSheet = leveSheet;
        _itemInfoCache = itemInfoCache;
    }

    public override string Singular => "Leve";
    public override string Plural => "Leves";
    public override string Description => "Leves the character can undertake.";
    public override string Key => "leves";
    public override (string?, uint?) Icon => (null, Icons.LeveIcon);

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = "leves",
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
        });
    }

    public override string? GetName(LeveRow row)
    {
        return row.Base.Name.ToImGuiString();
    }

    public override string? GetSubtitle(LeveRow row)
    {
        return row.LeveType.ToString().Humanize();
    }

    public override (string?, uint?) GetIcon(LeveRow row)
    {
        return (null, (uint)row.Base.LeveAssignmentType.Value.Icon);
    }

    public override uint GetRowId(LeveRow row)
    {
        return row.RowId;
    }

    public override LeveRow? GetRow(uint row)
    {
        return _leveSheet.GetRowOrDefault(row);
    }

    public override bool HasRow(uint rowId)
    {
        return _leveSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<LeveRow> GetRows()
    {
        return _leveSheet.Where(c => c.Base.DataId.RowId != 0).ToList();
    }

    public override List<Type>? RelatedTypes => [typeof(Leve)];

    public override void BuildColumns(CompendiumColumnBuilder<LeveRow> builder)
    {
        builder.AddCompendiumOpenViewColumn(new() { Key = "icon", Name = "##Icon", HelpText = "The icon of the leve", Version = "14.0.3", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.RowId });
        builder.AddStringColumn(new() { Key = "name", Name = "Name", HelpText = "The name of the leve", Version = "14.0.3", ValueSelector = row => row.Base.Name.ToImGuiString() });
        builder.AddStringColumn(new() { Key = "type", Name = "Type", HelpText = "The type of the leve", Version = "14.0.3", ValueSelector = row => row.LeveType.ToString().Humanize() + "(" + row.Base.LeveAssignmentType.Value.Name.ToImGuiString() + ")" });
        builder.AddIntegerColumn(new() { Key = "level", Name = "Level", HelpText = "The level of the leve", Version = "14.0.3", ValueSelector = row => row.Base.ClassJobLevel.ToString() });
        builder.AddStringColumn(new() { Key = "leveissuer", Name = "Leve Issuer", HelpText = "The NPC who starts the leve", Version = "14.0.3", ValueSelector = row => row.StartENpc == null ? "N/A" : _npcLocalizer.Format(row.StartENpc.ENpcBase.Base) });
        builder.AddIntegerColumn(new() { Key = "exp", Name = "EXP", HelpText = "The exp rewarded on completion of the leve", Version = "14.0.3", ValueSelector = row => row.ExpReward.ToString() });
        builder.AddIntegerColumn(new() { Key = "gil", Name = "Gil", HelpText = "The gil rewarded on completion of the leve", Version = "14.0.3", ValueSelector = row => row.GilReward.ToString() });
        builder.AddStringColumn(new() { Key = "startlocation", Name = "Start Location", HelpText = "The start location of the leve", Version = "14.0.3", ValueSelector = row => row.StartLocation?.FormattedName ?? null });

        //Maybe make a reward display column
        builder.AddItemsColumn(new()
        {
            Key = "rewards",
            Name = "Rewards",
            HelpText = "The rewards for the leve",
            Version = "14.0.3",
            ValueSelector =
                row =>
                {
                    switch (row.LeveType)
                    {
                        case LeveType.Battle:
                            return _itemInfoCache
                                .GetItemSourcesByType<ItemBattleLeveSource>(ItemInfoType.BattleLeve).Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.RewardItems).Select(c => c.ItemRow).DistinctBy(c => c.RowId).ToList();
                        case LeveType.Gathering:
                            return _itemInfoCache
                                .GetItemSourcesByType<ItemGatheringLeveSource>(ItemInfoType.GatheringLeve).Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.RewardItems).Select(c => c.ItemRow).DistinctBy(c => c.RowId).ToList();
                        case LeveType.Craft:
                            return _itemInfoCache
                                .GetItemSourcesByType<ItemCraftLeveSource>(ItemInfoType.CraftLeve).Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.RewardItems).Select(c => c.ItemRow).DistinctBy(c => c.RowId).ToList();
                        case LeveType.Company:
                            return _itemInfoCache
                                .GetItemSourcesByType<ItemCompanyLeveSource>(ItemInfoType.CompanyLeve).Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.RewardItems).Select(c => c.ItemRow).DistinctBy(c => c.RowId).ToList();
                    }

                    return [];
                }
        });
        builder.AddItemsColumn(new()
        {
            Key = "required",
            Name = "Required Items",
            HelpText = "The required items for the leve",
            Version = "14.0.3",
            ValueSelector =
                row =>
                {
                    switch (row.LeveType)
                    {
                        case LeveType.Craft:
                            return _itemInfoCache
                                .GetItemUsesByType<ItemCraftLeveUse>(ItemInfoType.CraftLeve).Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.CostItems).Select(c => c.ItemRow).DistinctBy(c => c.RowId).ToList();
                    }

                    return [];
                }
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, LeveRow row)
    {
        viewBuilder.Title = row.Base.Name.ToImGuiString();
        viewBuilder.Subtitle = row.LeveType.ToString().Humanize() + "(" +
                               row.Base.LeveAssignmentType.Value.Name.ToImGuiString() + ")";
        viewBuilder.Icon = (uint)row.Base.LeveAssignmentType.Value.Icon;
        viewBuilder.Description = row.Base.Description.ToImGuiString();
        viewBuilder.AddInfoTableSection(new()
        {
            SectionName = "Info",
            HideHeader = false,
            Items =
        [
            ("Level", row.Base.ClassJobLevel.ToString(), true),
            ("EXP", row.ExpReward.ToString(), row.ExpReward > 0),
            ("Gil", row.GilReward.ToString(),  row.GilReward > 0),
            ("Allowances", row.Base.AllowanceCost.ToString(), row.Base.AllowanceCost > 0)
        ]
        });
        if (row.StartENpc != null && row.StartENpc.ENpcBase.Locations.Any())
        {
            viewBuilder.AddMapLinkSectionSection(new MapLinkViewSectionOptions()
            {
                SectionName = "Leve Issuer",
                MapLink = new MapLinkEntry(Icons.FlagIcon, _npcLocalizer.Format(row.StartENpc.ENpcBase.Base), row.StartENpc.ENpcBase.Locations.First().FormattedName, row.StartENpc.ENpcBase.Locations.First())
            });
        }
        if (row.StartLocation != null)
        {
            viewBuilder.AddMapLinkSectionSection(new MapLinkViewSectionOptions()
            {
                SectionName = "Leve Start",
                MapLink = new MapLinkEntry(Icons.FlagIcon, row.StartLocation.FormattedName, row.StartLocation.FormattedName, row.StartLocation)
            });
        }

        if (row.LeveType == LeveType.Craft)
        {
            var requiredItems = _itemInfoCache
                .GetItemUsesByType<ItemCraftLeveUse>(ItemInfoType.CraftLeve).Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.CostItems).DistinctBy(c => c.ItemId).OrderBy(c => c.ItemRow.NameString).ToList();

            if (requiredItems.Count > 0)
            {
                viewBuilder.AddItemListSection(new ItemListSectionOptions()
                {
                    SectionName = "Required Items",
                    Items = requiredItems,
                });
            }
        }


        List<ItemInfo> rewards = new();

        switch (row.LeveType)
        {
            case LeveType.Battle:
                rewards = _itemInfoCache
                    .GetItemSourcesByType<ItemBattleLeveSource>(ItemInfoType.BattleLeve)
                    .Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.RewardItems).DistinctBy(c => c.ItemId).OrderBy(c => c.ItemRow.NameString).ToList();
                break;
            case LeveType.Gathering:
                rewards = _itemInfoCache
                    .GetItemSourcesByType<ItemGatheringLeveSource>(ItemInfoType.GatheringLeve).Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.RewardItems).DistinctBy(c => c.ItemId).OrderBy(c => c.ItemRow.NameString).ToList();
                break;
            case LeveType.Craft:
                rewards = _itemInfoCache
                    .GetItemSourcesByType<ItemCraftLeveSource>(ItemInfoType.CraftLeve).Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.RewardItems).DistinctBy(c => c.ItemId).OrderBy(c => c.ItemRow.NameString).ToList();
                break;
            case LeveType.Company:
                rewards = _itemInfoCache
                    .GetItemSourcesByType<ItemCompanyLeveSource>(ItemInfoType.CompanyLeve).Where(c => c.Leve.RowId == row.RowId).SelectMany(c => c.RewardItems).DistinctBy(c => c.ItemId).OrderBy(c => c.ItemRow.NameString).ToList();
                break;
        }

        if (rewards.Count > 0)
        {
            viewBuilder.AddItemListSection(new ItemListSectionOptions()
            {
                SectionName = "Reward Items",
                Items = rewards,
            });
        }
    }

    public override string GetDefaultGrouping()
    {
        return "type";
    }

    public override bool HasLocation => true;

    public override ILocation? GetLocation(LeveRow row)
    {
        return row.StartLocation;
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return new List<ICompendiumGrouping>()
        {
            new CompendiumGrouping<LeveRow>()
            {
                Key = "type",
                Name = "Type",
                GroupFunc = row => row.LeveType,
                GroupMapping = row =>
                {
                    var leveType = (LeveType)row;
                    return leveType switch
                    {
                        LeveType.Battle => "Battle",
                        LeveType.Gathering => "Gathering",
                        LeveType.Craft => "Craft",
                        LeveType.Company => "Company",
                        _ => "Unknown"
                    };
                },
            }
        };
    }
}