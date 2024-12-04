using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemExteriorFurnitureSourceRenderer : ItemInfoRenderer<ItemExteriorFurnitureSource>
{
    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.ExteriorFurnitureItem;
    public override string SingularName => "Exterior Furniture";
    public override string HelpText => "Can the item be placed outside houses?";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories => [ItemInfoRenderCategory.House];

    public override Action<ItemSource> DrawTooltip => source =>
    {
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        return source.Item.NameString;
    };
    public override Func<ItemSource, int> GetIcon => source => Icons.TableIcon;
}