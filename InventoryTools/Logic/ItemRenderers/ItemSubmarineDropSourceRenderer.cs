using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Mediator;
using InventoryTools.Ui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemSubmarineDropSourceRenderer : ItemInfoRenderer<ItemSubmarineDropSource>
{
    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.Submarine;
    public override string SingularName => "Submarine Exploration";
    public override string HelpText => "Can the item be earned from a submarine exploration route?";
    public override bool ShouldGroup => true;

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var asSource = AsSource(source);
        return [new OpenUintWindowMessage(typeof(SubmarineWindow), asSource.SubmarineExploration.RowId)];
    };

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var submarineDropSources = AsSource(sources).DistinctBy(c => c.SubmarineExploration.RowId);
        foreach (var source in submarineDropSources)
        {
            ImGui.Text(
                $"{source.SubmarineExploration.Base.Location.ExtractText()}");
        }
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var submarineDropSource = AsSource(source);
        ImGui.Text(
            $"{submarineDropSource.SubmarineExploration.Base.Location.ExtractText()}");
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var submarineDropSource = AsSource(source);
        return submarineDropSource.SubmarineExploration.Base.Location.ExtractText();
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        return Icons.SubmarineIcon;
    };
}