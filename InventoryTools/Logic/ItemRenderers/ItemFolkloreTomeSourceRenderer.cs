using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemFolkloreTomeSourceRenderer : ItemInfoRenderer<ItemFolkloreTomeSource>
{
    public ItemFolkloreTomeSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface)
        : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override IReadOnlyList<ItemInfoRenderCategory> Categories =>
        [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.HiddenGathering];

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.FolkloreTome;
    public override string SingularName => "Folklore Tome";
    public override string? PluralName => "Folklore Tomes";
    public override string HelpText => "Does this item unlock additional gathering items when read?";
    public override bool ShouldGroup => true;

    public override Func<ItemSource, (Type, uint)>? RelatedType => source =>
    {
        var asSource = AsSource(source);
        return (typeof(NotebookDivision), asSource.NotebookDivision.RowId);
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var divisionName = asSource.NotebookDivision.ValueNullable?.Name.ToImGuiString();
        if (!string.IsNullOrEmpty(divisionName))
        {
            ImGui.Text("Unlocks: " + divisionName);
        }

        if (asSource.RelatedItems.Count > 0)
        {
            this.DrawItems("Items Unlocked:", asSource.RelatedItems.First().Value);
        }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.NotebookDivision.ValueNullable?.Name.ToImGuiString()
               ?? asSource.Item.NameString;
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        return asSource.Item.Base.Icon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var divisionName = asSource.NotebookDivision.ValueNullable?.Name.ToImGuiString();
        if (!string.IsNullOrEmpty(divisionName))
        {
            return $"Unlocks {divisionName} ({asSource.UnlockedItems.Count} items)";
        }

        return $"Unlocks {asSource.UnlockedItems.Count} items";
    };
}
