using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCompanyCraftResultSourceRenderer : ItemInfoRenderer<ItemCompanyCraftResultSource>
{
    private readonly ItemSheet _itemSheet;

    public ItemCompanyCraftResultSourceRenderer(ItemSheet itemSheet)
    {
        _itemSheet = itemSheet;
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
        ImGui.Text("Ingredients:");
        using (ImRaii.PushIndent())
        {
            foreach (var ingredient in asSource.CompanyCraftSequence.MaterialsRequired(null))
            {
                var item = _itemSheet.GetRow(ingredient.ItemId);
                ImGui.Text($"{item.NameString} x {ingredient.Quantity}");
            }
        }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.Item.Base.Name.ExtractText() + "(" +
               (asSource.CompanyCraftSequence.Base.CompanyCraftType.ValueNullable?.Name.ExtractText() ?? "Unknown") +
               ")";
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.CraftIcon;
}