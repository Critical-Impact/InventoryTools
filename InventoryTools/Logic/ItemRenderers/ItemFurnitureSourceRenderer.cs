using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemFurnitureSourceRenderer : ItemInfoRenderer<ItemFurnitureSource>
{
    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.FurnitureItem;
    public override string SingularName => "Interior Furniture";
    public override string HelpText => "Can the item be placed inside houses?";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories => [ItemInfoRenderCategory.House];

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text($"Category: {asSource.FurnitureCatalogItem.Value.Category.Value.Category.ExtractText()}");
        ImGui.Text($"Patch Added: {asSource.FurnitureCatalogItem.Value.Patch}");
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        return source.Item.NameString;
    };
    public override Func<ItemSource, int> GetIcon => source => Icons.TableIcon;
}