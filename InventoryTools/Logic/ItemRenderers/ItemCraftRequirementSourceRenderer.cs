using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCraftRequirementSourceRenderer : ItemInfoRenderer<ItemCraftRequirementSource>
{
    private readonly ITextureProvider _textureProvider;

    public ItemCraftRequirementSourceRenderer(ITextureProvider textureProvider)
    {
        _textureProvider = textureProvider;
    }
    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.CraftRecipe;
    public override string SingularName => "Craft Ingredient";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Crafting];
    public override string HelpText => "Can the item be used as a material in a craft recipe?";

    public override Func<List<ItemSource>, List<List<ItemSource>>>? CustomGroup => sources =>
    {
        return sources.GroupBy(c => AsSource(c).Recipe.Base.CraftType.RowId).Select(c => c.ToList()).ToList();
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.TextUnformatted($"Ingredient of Craft Recipe:");
        using (ImRaii.PushIndent())
        {
            ImGui.Image(_textureProvider.GetFromGameIcon(new GameIconLookup(asSource.Item.Icon)).GetWrapOrEmpty().ImGuiHandle, new Vector2(16,16));
            ImGui.SameLine();
            ImGui.TextUnformatted(GetName(source));
        }
    };

    public override Action<List<ItemSource>>? DrawTooltipGrouped => source =>
    {
        var asSource = AsSource(source);
        asSource = asSource.DistinctBy(c => c.Item.RowId).ToList();
        ImGui.TextUnformatted($"Ingredient of Craft Recipe:");
        using (ImRaii.PushIndent())
        {
            foreach (var row in asSource)
            {
                ImGui.TextUnformatted(GetName(row));
            }
        }
    };
    public override Func<ItemSource, List<MessageBase>?>? OnClick => source => [new OpenUintWindowMessage(typeof(ItemWindow), source.Item.RowId)];
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