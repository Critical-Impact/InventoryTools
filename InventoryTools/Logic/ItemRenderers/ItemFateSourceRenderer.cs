using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemFateSourceRenderer : ItemInfoRenderer<ItemFateSource>
{
    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.Fate;
    public override string HelpText => "Can the item be obtained by completing a fate?";
    public override string SingularName => "Fate";
    public override bool ShouldGroup => true;

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);
        using (ImRaii.PushIndent())
        {
            foreach (var fate in asSources)
            {
                ImGui.Text(fate.Fate.Base.Name.ExtractText());
            }
        }
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text(asSource.Fate.Base.Name.ExtractText());
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.Fate.Base.Name.ExtractText();
    };
    public override Func<ItemSource, int> GetIcon => _ => Icons.Fate;
}