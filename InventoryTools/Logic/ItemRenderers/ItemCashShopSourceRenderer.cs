using System;
using System.Globalization;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCashShopSourceRenderer : ItemInfoRenderer<ItemCashShopSource>
{
    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.CashShop;
    public override string SingularName => "Bought on SQ Store(real money)";
    public override bool ShouldGroup => true;
    public override string HelpText => "Can the item be purchased through the mogstation?";

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var priceUsd = asSource.PriceUsd.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
        ImGui.TextUnformatted($"Price(USD): {priceUsd}");
        if (asSource.FittingShopItemSetRow?.Items.Count > 1)
        {
            ImGui.TextUnformatted($"Set: {asSource.FittingShopItemSetRow.Base.Unknown6.ExtractText()}");
            ImGui.TextUnformatted($"Contains:");
            using (ImRaii.PushIndent())
            {
                foreach (var item in asSource.FittingShopItemSetRow.Items)
                {
                    ImGui.TextUnformatted(item.NameString);
                }
            }
        }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return (asSource.FittingShopItemSetRow?.Base.Unknown6.ExtractText() ?? "Not in a set");
    };

    public override Func<ItemSource, int> GetIcon => source => Icons.BagStar;
}