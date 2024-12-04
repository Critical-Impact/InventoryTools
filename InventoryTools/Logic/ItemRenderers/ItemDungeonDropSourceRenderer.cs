using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Mediator;
using InventoryTools.Ui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemDungeonDropSourceRenderer : ItemInfoRenderer<ItemDungeonDropSource>
{
    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.DungeonDrop;
    public override string SingularName => "Dungeon Drop";
    public override string PluralName => "Dungeon Drops";
    public override string HelpText => "Can the item be dropped from monsters in dungeons?";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Duty];

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var dungeonDropSource = AsSource(source);
        ImGui.Text("Dungeon: " + dungeonDropSource.ContentFinderCondition.FormattedName);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var dungeonDropSource = AsSource(source);

        return "Dungeon: " + dungeonDropSource.ContentFinderCondition.FormattedName;
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.DutyIcon;

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var dungeonDropSource = AsSource(source);

        return new List<MessageBase>()
            { new OpenUintWindowMessage(typeof(DutyWindow), dungeonDropSource.ContentFinderCondition.RowId) };
    };
}