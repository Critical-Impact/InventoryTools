using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCraftResultSourceRenderer : ItemInfoRenderer<ItemCraftResultSource>
{
    private readonly ItemSheet _itemSheet;
    private readonly IGameInterface _gameInterface;

    public ItemCraftResultSourceRenderer(ItemSheet itemSheet, IGameInterface gameInterface)
    {
        _itemSheet = itemSheet;
        _gameInterface = gameInterface;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.CraftRecipe;
    public override string SingularName => "Craft Recipe";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Crafting];
    public override string HelpText => "Can the item be crafted via a craft recipe?";
    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var asSource = AsSource(source);
        _gameInterface.OpenCraftingLog(asSource.Item.RowId, asSource.Recipe.RowId);
        return null;
    };

    public override Func<List<ItemSource>, List<List<ItemSource>>>? CustomGroup => sources =>
    {
        return sources.GroupBy(c => AsSource(c).Recipe.Base.CraftType.RowId).Select(c => c.ToList()).ToList();
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text($"Craft Type: {asSource.Recipe.Base.CraftType.Value.Name}");
        ImGui.Text($"Yield: {asSource.Recipe.Base.AmountResult}");
        ImGui.Text($"Difficulty: {asSource.Recipe.Base.DifficultyFactor}");
        ImGui.Text($"Required Craftsmanship: {asSource.Recipe.Base.RequiredCraftsmanship}");

        ImGui.Text("Ingredients:");
        using (ImRaii.PushIndent())
        {
            foreach (var ingredient in asSource.Recipe.IngredientCounts)
            {
                var item = _itemSheet.GetRow(ingredient.Key);
                ImGui.Text($"{item.NameString} x {ingredient.Value}");
            }
        }
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.Item.NameString + " (" + (asSource.Recipe.CraftType?.FormattedName ?? "Unknown") + ")";
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        if (asSource.Recipe.CraftType != null)
        {
            return asSource.Recipe.CraftType.Icon;
        }

        return Icons.CraftIcon;
    };
}