using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCustomDeliverySourceRenderer : ItemInfoRenderer<ItemCustomDeliverySource>
{
    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.CustomDelivery;
    public override string SingularName => "Custom Delivery";
    public override string PluralName => "Custom Deliveries";
    public override string HelpText => "Can the item be delivered in a custom delivery quest?";
    public override bool ShouldGroup => false;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var eNpcResident = asSource.SupplyRow.Npc?.Base.Npc.Value;
        if (eNpcResident != null)
        {
            var collectabilityLow = asSource.SupplyRow.Base.CollectabilityLow;
            var collectabilityMid = asSource.SupplyRow.Base.CollectabilityMid;
            var collectabilityHigh = asSource.SupplyRow.Base.CollectabilityHigh;
            ImGui.Text("NPC: " + eNpcResident.Value.Singular.ExtractText());
            ImGui.Text("Collectability (Low): " + collectabilityLow);
            ImGui.Text("Collectability (Mid): " + collectabilityMid);
            ImGui.Text("Collectability (High): " + collectabilityHigh);
        }
        else
        {
            ImGui.Text("Unknown Npc");
        }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        var eNpcResident = asSource.SupplyRow.Npc?.Base.Npc.Value;
        if (eNpcResident != null)
        {
            return eNpcResident.Value.Singular.ExtractText();
        }

        return "";
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.CustomDeliveryIcon;
}