using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCompanyCraftResultSourceRenderer : ItemInfoRenderer<ItemCompanyCraftResultSource>
{
    private readonly ItemSheet _itemSheet;
    private readonly ITextureProvider _textureProvider;

    public ItemCompanyCraftResultSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _itemSheet = itemSheet;
        _textureProvider = textureProvider;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.FreeCompanyCraftRecipe;
    public override string SingularName => "Company Craft";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Crafting];
    public override string HelpText => "Is the item crafted at the company workshop as a company craft recipe?";
    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text($"Craft Type: {asSource.CompanyCraftSequence.Base.CompanyCraftType.Value.Name}");
        ImGui.Text($"Parts: {asSource.CompanyCraftSequence.CompanyCraftParts.Length}");

        var materialsRequired = asSource.CompanyCraftSequence.MaterialsRequired(null);
        Span<ItemInfo> rewardItems = stackalloc ItemInfo[materialsRequired.Count];

        for (var index = 0; index < materialsRequired.Count; index++)
        {
            rewardItems[index] = new ItemInfo(
                materialsRequired[index].ItemId,
                materialsRequired[index].Quantity,
                false
            );
        }

        DrawItems("Ingredients: ", rewardItems);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.Item.Base.Name.ExtractText() + "(" +
               (asSource.CompanyCraftSequence.Base.CompanyCraftType.ValueNullable?.Name.ExtractText() ?? "Unknown") +
               ")";
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.CraftIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return
            $"{asSource.CompanyCraftSequence.Base.CompanyCraftType.Value.Name} ({asSource.CompanyCraftSequence.CompanyCraftParts.Length}) ({asSource.CompanyCraftSequence.MaterialsRequired(null).Select(c => _itemSheet.GetRow(c.ItemId).NameString + " x " + c.Quantity)})";
    };
}