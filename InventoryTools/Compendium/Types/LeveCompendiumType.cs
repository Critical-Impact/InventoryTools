using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Interface.Grid;
using AllaganLib.Shared.Extensions;
using DalaMock.Host.Mediator;
using Humanizer;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Types;

public class LeveCompendiumType : CompendiumType<LeveRow>, ILocations
{
    private readonly LeveSheet _leveSheet;
    private readonly ItemInfoCache _itemInfoCache;

    public LeveCompendiumType(CompendiumTable<LeveRow>.Factory tableFactory, Func<CompendiumColumnBuilder<LeveRow>> columnBuilder, LeveSheet leveSheet, ItemInfoCache itemInfoCache) : base(tableFactory, columnBuilder)
    {
        _leveSheet = leveSheet;
        _itemInfoCache = itemInfoCache;
    }

    public override string Singular => "Leve";
    public override string Plural => "Leves";

    public override IRenderTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = "leves",
            Name = Plural,
            Columns = BuiltColumns(),
            CompendiumType = this,
        });
    }

    public override LeveRow? GetRow(uint row)
    {
        return _leveSheet.GetRowOrDefault(row);
    }

    public override List<LeveRow> GetRows()
    {
        return _leveSheet.Where(c => c.Base.DataId.RowId != 0).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<LeveRow> builder)
    {
        builder.AddIconColumn(new(){Key = "icon", Name = "Icon", HelpText = "The icon of the leve", Version = "14.0.3", ValueSelector = row => row.Base.LeveAssignmentType.Value.Icon});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the leve", Version = "14.0.3", ValueSelector = row => row.Base.Name.ToImGuiString()});
        builder.AddStringColumn(new (){Key = "type", Name = "Type", HelpText = "The type of the leve", Version = "14.0.3", ValueSelector = row => row.LeveType.ToString().Humanize() + "(" + row.Base.LeveAssignmentType.Value.Name.ToImGuiString() + ")"});
        builder.AddIntegerColumn(new (){Key = "level", Name = "Level", HelpText = "The level of the leve", Version = "14.0.3", ValueSelector = row => row.Base.ClassJobLevel.ToString()});
        builder.AddStringColumn(new (){Key = "questgiver", Name = "Quest Giver", HelpText = "The NPC who starts the leve", Version = "14.0.3", ValueSelector = row => row.StartENpc?.Name ?? "N/A"});
        builder.AddIntegerColumn(new (){Key = "exp", Name = "EXP", HelpText = "The exp rewarded on completion of the leve", Version = "14.0.3", ValueSelector = row => row.ExpReward.ToString()});
        builder.AddIntegerColumn(new (){Key = "gil", Name = "Gil", HelpText = "The gil rewarded on completion of the leve", Version = "14.0.3", ValueSelector = row => row.GilReward.ToString()});
        builder.AddStringColumn(new (){Key = "startlocation", Name = "Start Location", HelpText = "The start location of the leve", Version = "14.0.3", ValueSelector = row => row.StartLocation?.FormattedName ?? null});

        //Maybe make a reward display column
        builder.AddItemsColumn(new()
        {
            Key = "rewards", Name = "Rewards", HelpText = "The rewards for the leve", Version = "14.0.3",
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
            Key = "required", Name = "Required Items", HelpText = "The required items for the leve", Version = "14.0.3",
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

    public List<NamedLocation>? GetLocations(uint rowId)
    {
        var locations = new List<NamedLocation>();
        var row = _leveSheet.GetRow(rowId);
        if (row.StartLocation != null)
        {
            locations.Add(new NamedLocation(row.StartLocation, row.StartENpc?.Name ?? "Start"));
        }
        return locations;
    }
}