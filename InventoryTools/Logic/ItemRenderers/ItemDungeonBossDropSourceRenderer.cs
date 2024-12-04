using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Mediator;
using InventoryTools.Ui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemDungeonBossDropSourceRenderer : ItemInfoRenderer<ItemDungeonBossDropSource>
{
    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.DungeonBossDrop;
    public override string SingularName => "Dungeon Boss Drop";
    public override string PluralName => "Dungeon Boss Drops";
    public override string HelpText => "Can the item be drop from a dungeon boss?";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Duty];

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text("Dungeon: " + asSource.ContentFinderCondition.FormattedName);
        using (ImRaii.PushIndent())
        {
            ImGui.Text(asSource.BNpcName.Base.Singular.ExtractText().ToTitleCase() + " (Boss " + (asSource.DungeonBoss.FightNo + 1) + ")");
        }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);

        return asSource.ContentFinderCondition.FormattedName + " - " + asSource.DungeonBoss.BNpcName.Value.Singular.ExtractText();
    };

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);
        var groupedByDungeon = asSources.GroupBy(c => c.DungeonBoss.ContentFinderCondition.RowId);
        foreach (var dungeon in groupedByDungeon)
        {
            ImGui.Text("Dungeon: " + dungeon.First().DungeonBoss.ContentFinderCondition.Value.Name.ExtractText());
            using (ImRaii.PushIndent())
            {
                foreach (var itemSource in dungeon.DistinctBy(c => c.DungeonBoss.BNpcName.RowId))
                {
                    ImGui.Text(itemSource.BNpcName.Base.Singular.ExtractText().ToTitleCase() + " (Boss " + (itemSource.DungeonBoss.FightNo + 1) + ")");
                }
            }
        }

    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.DutyIcon;

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var asSource = AsSource(source);

        return new List<MessageBase>()
            { new OpenUintWindowMessage(typeof(DutyWindow), asSource.ContentFinderCondition.RowId) };
    };
}