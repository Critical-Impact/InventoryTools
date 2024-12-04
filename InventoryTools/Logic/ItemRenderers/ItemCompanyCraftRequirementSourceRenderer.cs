using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Mediator;
using InventoryTools.Ui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemCompanyCraftRequirementSourceRenderer : ItemInfoRenderer<ItemCompanyCraftRequirementSource>
{
    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.FreeCompanyCraftRecipe;
    public override string SingularName => "Company Craft Ingredient";
    public override bool ShouldGroup => true;
    public override string HelpText => "Is the item a material in a company craft recipe?";
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Crafting];

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.TextUnformatted($"Ingredient of Craft Recipe:");
        using (ImRaii.PushIndent())
        {
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
        return asSource.Item.NameString + " (" + (asSource.CompanyCraftSequence.Base.CompanyCraftType.ValueNullable?.Name.ExtractText() ?? "Unknown") + ")";
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.CraftIcon;
}