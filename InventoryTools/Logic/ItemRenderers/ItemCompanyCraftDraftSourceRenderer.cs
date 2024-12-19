using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCompanyCraftDraftSourceRenderer : ItemInfoRenderer<ItemCompanyCraftDraftSource>
{
    private readonly ItemSheet _itemSheet;

    public ItemCompanyCraftDraftSourceRenderer(ItemSheet itemSheet)
    {
        _itemSheet = itemSheet;
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
        ImGui.Text("Ingredients:");
        using (ImRaii.PushIndent())
        {
            for (var index = 0; index < asSource.CompanyCraftDraft.Value.RequiredItem.Count; index++)
            {
                var ingredient = asSource.CompanyCraftDraft.Value.RequiredItem[index];
                var quantity = asSource.CompanyCraftDraft.Value.RequiredItemCount[index];
                if (ingredient.RowId == 0)
                {
                    continue;
                }
                var item = _itemSheet.GetRow(ingredient.RowId);

                ImGui.Text($"{item.NameString} x {quantity}");
            }
        }
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
}