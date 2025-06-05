using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Services;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCollectablesShopUseRenderer : ItemCollectablesShopSourceRenderer
{
    private readonly ItemSheet _itemSheet;
    public override string HelpText => "Can the item be spent at a collectables exchange shop?";
    public ItemCollectablesShopUseRenderer(MapSheet mapSheet, ItemSheet itemSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(mapSheet, itemSheet, textureProvider, dalamudPluginInterface)
    {
        _itemSheet = itemSheet;
    }

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);

        var maps = asSources.SelectMany(shopSource => shopSource.MapIds == null || shopSource.MapIds.Count == 0
            ? new List<string>()
            : shopSource.MapIds.Select(c => MapSheet.GetRow(c).FormattedName)).Distinct().ToList();

        ImGui.Text("Items that can be purchased:");

        using (ImRaii.PushIndent())
        {
            foreach (var asSource in asSources.DistinctBy(c => c.Item).Select(c => c.Item.NameString))
            {
                ImGui.TextUnformatted(asSource);
            }
        }

        DrawMaps(sources);
    };

    public override RendererType RendererType => RendererType.Use;
}

public class ItemCollectablesShopSourceRenderer : ItemInfoRenderer<ItemCollectablesShopSource>
{
    public MapSheet MapSheet { get; }
    private readonly ItemSheet _itemSheet;

    public ItemCollectablesShopSourceRenderer(MapSheet mapSheet, ItemSheet itemSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        MapSheet = mapSheet;
        _itemSheet = itemSheet;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.CollectablesShop;
    public override string SingularName => "Collectables Exchange Shop";
    public override string PluralName => "Collectables Exchange Shops";
    public override string HelpText => "Can the item be purchased from a collectables exchange shop?";
    public override bool ShouldGroup => true;

    public override byte MaxColumns => 1;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Shop];

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);

        DrawItems("Costs: ", asSource.CostItems);
        DrawItems("Rewards: ", asSource.RewardItems);

        DrawMaps(source);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        if (asSource.MapIds == null || asSource.MapIds.Count == 0)
        {
            return asSource.CollectablesShop.Name;
        }

        var maps = asSource.MapIds.Distinct().Select(c => MapSheet.GetRow(c).FormattedName);
        return asSource.CollectablesShop.Name + "(" + maps + ")";
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.CollectableShopIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var description = $"{asSource.Shop.Name}";
        var rewards = string.Join(", ", asSource.CollectablesShopListing.Rewards.Select(c => c.Item.NameString + " x " + c.Count + ""));
        var costs = string.Join(", ", asSource.CollectablesShopListing.Costs.Select(c => c.Item.NameString + " x " + c.Count + ""));
        return $"{description} ({rewards}) for ({costs})";
    };
}