using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Interface.Grid;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Types;

public class AchievementCompendiumType : CompendiumType<AchievementRow>
{
    private readonly AchievementSheet _achievementSheet;
    private readonly ICharacterMonitor _characterMonitor;

    public AchievementCompendiumType(CompendiumTable<AchievementRow>.Factory tableFactory, Func<CompendiumColumnBuilder<AchievementRow>> columnBuilder, AchievementSheet achievementSheet, ICharacterMonitor characterMonitor) : base(tableFactory, columnBuilder)
    {
        _achievementSheet = achievementSheet;
        _characterMonitor = characterMonitor;
    }


    public override IRenderTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = "achievements",
            Name = Plural,
            Columns = BuiltColumns(),
            CompendiumType = this,
        });
    }

    public override string Singular => "Achievement";
    public override string Plural => "Achievements";


    public override AchievementRow? GetRow(uint row)
    {
        return _achievementSheet.GetRowOrDefault(row);
    }

    public override List<AchievementRow> GetRows()
    {
        return _achievementSheet.Where(c => c.Base.Name.ToImGuiString() != string.Empty).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<AchievementRow> builder)
    {
        builder.AddIconColumn(new(){Key = "icon", Name = "Icon", HelpText = "The icon of the achievement", Version = "14.0.3", ValueSelector = row => (int?)row.Base.Icon});
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
}