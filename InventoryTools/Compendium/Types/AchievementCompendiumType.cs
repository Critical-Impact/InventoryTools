using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class AchievementCompendiumType : CompendiumType<AchievementRow>
{
    private readonly AchievementSheet _achievementSheet;
    private readonly ExcelSheet<AchievementCategory> _achievementCategorySheet;
    private readonly ICharacterMonitor _characterMonitor;

    public AchievementCompendiumType(CompendiumTable<AchievementRow>.Factory tableFactory, Func<CompendiumColumnBuilder<AchievementRow>> columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory, AchievementSheet achievementSheet, ExcelSheet<AchievementCategory> achievementCategorySheet, ICharacterMonitor characterMonitor) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _achievementSheet = achievementSheet;
        _achievementCategorySheet = achievementCategorySheet;
        _characterMonitor = characterMonitor;
    }

    public override List<Type>? RelatedTypes => [typeof(Achievement)];


    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = "achievements",
            Name = Plural,
            Columns = BuiltColumns(),
            CompendiumType = this,
        });
    }

    public override string? GetName(AchievementRow row)
    {
        return row.Base.Name.ToImGuiString();
    }

    public override string? GetSubtitle(AchievementRow row)
    {
        return row.Base.AchievementCategory.Value.Name.ToImGuiString();
    }

    public override (string?, uint?) GetIcon(AchievementRow row)
    {
        return (null, row.Base.Icon);
    }

    public override string Singular => "Achievement";
    public override string Plural => "Achievements";
    public override string Description => "Achievements earned by the player.";
    public override string Key => "achievements";
    public override (string?, uint?) Icon => (null, Icons.AchievementCertIcon);


    public override AchievementRow? GetRow(uint row)
    {
        return _achievementSheet.GetRowOrDefault(row);
    }

    public override bool HasRow(uint rowId)
    {
        return _achievementSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<AchievementRow> GetRows()
    {
        return _achievementSheet.Where(c => c.Base.Name.ToImGuiString() != string.Empty && c.Base.AchievementCategory.RowId != 0).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<AchievementRow> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "Icon", HelpText = "The icon of the achievement", Version = "14.0.3", CompendiumType = this, RowIdSelector = row => row.RowId, ValueSelector = this.GetIcon});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the achievement", Version = "14.0.3", ValueSelector = row => row.Base.Name.ToImGuiString()});
        builder.AddStringColumn(new (){Key = "category", Name = "Category", HelpText = "The category of the achievement", Version = "14.0.3", ValueSelector = row => row.Base.AchievementCategory.Value.Name.ToImGuiString()});
        builder.AddStringColumn(new (){Key = "description", Name = "Description", HelpText = "The description of the achievement", Version = "14.0.3", ValueSelector = row => row.Base.Description.ToImGuiString()});
        builder.AddIntegerColumn(new (){Key = "points", Name = "Points", HelpText = "The points earned for the achievement", Version = "14.0.3", ValueSelector = row => row.Base.Points.ToString()});
        builder.AddBooleanColumn(new (){Key = "completed", Name = "Completed", HelpText = "Is the achievement completed?", Version = "14.0.3", ValueSelector = item => _characterMonitor.ActiveCharacter?.IsAchievementCompleted(item.RowId)
        });
        builder.AddIntegerColumn(new (){Key = "title", Name = "Title", HelpText = "The title unlocked for earning this achievement", Version = "14.0.3", ValueSelector =
            item =>
            {
                if (item.Base.Title.RowId == 0 || item.Base.Title.ValueNullable == null)
                {
                    return "";
                }
                return item.Base.Title.ValueNullable.Value.Masculine.ToImGuiString() + "/" + item.Base.Title.ValueNullable.Value.Feminine.ToImGuiString();
            }});
        builder.AddItemColumn(new(){Key = "reward", Name = "Reward Item", HelpText = "The reward item for the achievement", Version = "14.0.3", ValueSelector = row => row.Base.Item.RowId});
    }

    public override List<ICompendiumGrouping>? GetGroupings()
    {
        return
        [
            new CompendiumGrouping<AchievementRow>()
            {
                Key = "category",
                Name = "Category",
                GroupFunc = row => row.Base.AchievementCategory.RowId,
                GroupMapping = row =>
                {
                    var categoryId = (uint)row;
                    return _achievementCategorySheet.GetRow(categoryId).Name.ToImGuiString();
                }
            }
        ];
    }

    public override string? GetDefaultGrouping()
    {
        return "category";
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, AchievementRow row)
    {
        viewBuilder.Title = row.Base.Name.ToImGuiString();
        viewBuilder.Subtitle = row.Base.Description.ToImGuiString();
        viewBuilder.Icon = row.Base.Icon;
        var information = new List<(string Header, string Value, bool IsVisible)>
        {
            ("Category", row.Base.AchievementCategory.Value.Name.ToImGuiString(), true),
            ("Points", row.Base.Points.ToString(), true),
            ("Completed", _characterMonitor.ActiveCharacter?.IsAchievementCompleted(row.RowId) ?? false ? "Yes" : "No", true)
        };
        if (row.Base.Title.RowId != 0 && row.Base.Title.ValueNullable != null)
        {
            information.Add(("Title", row.Base.Title.ValueNullable.Value.Masculine.ToImGuiString() + "/" + row.Base.Title.ValueNullable.Value.Feminine.ToImGuiString(), true));
        }
        viewBuilder.AddInfoTableSection(new CompendiumInfoTableSectionOptions()
        {
            SectionName = "Information",
            Items = information.AsReadOnly()
        });

        List<RowRef> items = [row.Base.Key];
        items.AddRange(row.Base.Data);

        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            RelatedRefs = items,
            Filter = typeof(Achievement),
            SectionName = "Required Achievements"
        });

        var relatedAchievements = _achievementSheet.Where(c => c.Base.Key.RowType == typeof(Achievement) && c.Base.Key.RowId == row.RowId || c.Base.Data.Any(d => d.RowType == typeof(Achievement) && d.RowId == row.RowId)).Select(c => c.Base.AsUntypedRowRef()).ToList();
        if (relatedAchievements.Any())
        {
            viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
            {
                RelatedRefs = relatedAchievements,
                Filter = typeof(Achievement),
                SectionName = "Related Achievements"
            });
        }

        if (row.Base.Item.RowId != 0)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                RelatedRef = row.Base.Item.Value.AsUntypedRowRef()
            });
        }
    }
}