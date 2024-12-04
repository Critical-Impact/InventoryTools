using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemGcExpertDeliverySourceRenderer : ItemInfoRenderer<ItemGCExpertDeliverySource>
{
    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.GCExpertDelivery;
    public override string SingularName => "Grand Company Expert Delivery";
    public override string HelpText => "Can the item be handed in for 'Expert Delivery' at your grand company?";
    public override bool ShouldGroup => false;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var sealsRewarded = asSource.SealsRewarded;
        ImGui.Text("Seals rewarded: " + sealsRewarded);
    };

    public override Func<ItemSource, string> GetName => _ =>
    {
        return "";
    };
    public override Func<ItemSource, int> GetIcon => _ => Icons.FlameSealIcon;
}