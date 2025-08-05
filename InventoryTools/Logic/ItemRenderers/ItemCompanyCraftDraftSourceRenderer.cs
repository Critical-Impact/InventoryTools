using System;
using System.Collections.Generic;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCompanyCraftDraftSourceRenderer : ItemInfoRenderer<ItemCompanyCraftDraftSource>
{
    private readonly ItemSheet _itemSheet;
    private readonly ITextureProvider _textureProvider;

    public ItemCompanyCraftDraftSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _itemSheet = itemSheet;
        _textureProvider = textureProvider;
    }
    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.CompanyCraftDraft;
    public override string SingularName => "Company Craft Prototype";
    public override string HelpText => "Is this item used in the creation of a company craft prototype?";
    public override bool ShouldGroup => true;
    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text($"Name: {asSource.CompanyCraftDraft.Value.Name.ExtractText()}");

        DrawItems("Possible Reward Items: ", asSource.RewardItems);
        DrawItems("Ingredients: ", asSource.CostItems);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.CompanyCraftDraft.Value.Name.ExtractText();
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        return Icons.DraftBook;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var description = asSource.CompanyCraftDraft.Value.Name.ExtractText();
        var materials = new List<string>();
        for (var index = 0; index < asSource.CompanyCraftDraft.Value.RequiredItem.Count; index++)
        {
            var ingredient = asSource.CompanyCraftDraft.Value.RequiredItem[index];
            var quantity = asSource.CompanyCraftDraft.Value.RequiredItemCount[index];
            if (ingredient.RowId == 0)
            {
                continue;
            }
            var item = _itemSheet.GetRow(ingredient.RowId);

            materials.Add($"{item.NameString} x {quantity}");
        }

        if (materials.Count != 0)
        {
            description += $" ({string.Join(", ", materials)})";
        }

        return description;
    };
}