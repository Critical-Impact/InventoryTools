using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Services;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemFateShopUseRenderer : ItemFateShopSourceRenderer
{
    public ItemFateShopUseRenderer(ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface, ItemSheet itemSheet, MapSheet mapSheet) : base(textureProvider, pluginInterface, itemSheet, mapSheet)
    {
    }

    public override string HelpText => "Can the item be spent at a bicolor gemstone shop?";

    public override RendererType RendererType => RendererType.Use;

    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        return asSource.Item.Icon;
    };
}

public class ItemFateShopSourceRenderer : ItemInfoRenderer<ItemFateShopSource>
{
    private readonly ITextureProvider _textureProvider;
    private readonly IDalamudPluginInterface _pluginInterface;
    public MapSheet MapSheet { get; }

    public ItemFateShopSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface, ItemSheet itemSheet, MapSheet mapSheet) : base(textureProvider, pluginInterface, itemSheet, mapSheet)
    {
        _textureProvider = textureProvider;
        _pluginInterface = pluginInterface;
        MapSheet = mapSheet;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.FateShop;
    public override string SingularName => "Bicolor Gemstone Shop";
    public override string PluralName => "Bicolor Gemstone Shops";
    public override string HelpText => "Can the item be purchased from a bicolour gem shop?";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Shop];

    public override byte MaxColumns => 4;

    public override Func<List<ItemSource>, List<List<ItemSource>>>? CustomGroup => sources =>
    {
        return sources.GroupBy(c => (c.CostItems.Count, c.CostItems.FirstOrDefault()?.RowId ?? null)).Select(c => c.ToList()).ToList();
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);

        ImGui.Text($"Shop: {asSource.Shop.Name}");
        ImGui.Text("Rewards:");
        using (ImRaii.PushIndent())
        {
            foreach (var reward in asSource.ShopListing.Rewards)
            {
                var itemName = reward.Item.NameString;
                var count = reward.Count;
                var costString = $"{itemName} x {count}";
                ImGui.Image(_textureProvider.GetFromGameIcon(new GameIconLookup(reward.Item.Icon)).GetWrapOrEmpty().ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                ImGui.SameLine();
                ImGui.Text(costString);
                if (reward.IsHq == true)
                {
                    ImGui.SameLine();
                    ImGui.Image(_textureProvider.GetPluginImageTexture(_pluginInterface, "hq").GetWrapOrEmpty().ImGuiHandle,
                        new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                }
            }
        }
        ImGui.Text("Costs:");
        using (ImRaii.PushIndent())
        {
            foreach (var cost in asSource.ShopListing.Costs)
            {
                var itemName = cost.Item.NameString;
                var count = cost.Count;
                var costString = $"{itemName} x {count}";
                ImGui.Image(_textureProvider.GetFromGameIcon(new GameIconLookup(cost.Item.Icon)).GetWrapOrEmpty().ImGuiHandle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                ImGui.SameLine();
                ImGui.Text(costString);
                if (cost.IsHq == true)
                {
                    ImGui.SameLine();
                    ImGui.Image(_textureProvider.GetPluginImageTexture(_pluginInterface, "hq").GetWrapOrEmpty().ImGuiHandle,
                        new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                }
            }
        }

        DrawMaps(source);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        var costs = String.Join(", ",
            asSource.ShopListing.Costs.Select(c => c.Item.NameString + " (" + c.Count + ")"));
        if (asSource.ShopListing.Rewards.Count() > 1)
        {
            var rewards = String.Join(", ",
                asSource.ShopListing.Rewards.Select(c => c.Item.NameString + " (" + c.Count + ")"));
            return $"Costs {costs} - Rewards {rewards}";
        }

        return $"Costs {costs}";
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        return asSource.CostItems.FirstOrDefault()?.Icon ?? asSource.Item.Icon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var description = $"{asSource.Shop.Name}";
        var rewards = string.Join(", ", asSource.ShopListing.Rewards.Select(c => c.Item.NameString + " x " + c.Count + ""));
        var costs = string.Join(", ", asSource.ShopListing.Costs.Select(c => c.Item.NameString + " x " + c.Count + ""));
        return $"{description} ({rewards}) for ({costs})";
    };
}