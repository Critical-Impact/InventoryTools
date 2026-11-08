using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel.Sheets;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemBeastmasterFamiliarUseRenderer : ItemInfoRenderer<ItemBeastmasterFamiliarUse>
{
    public ItemBeastmasterFamiliarUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.BeastmasterFamiliar;
    public override string SingularName => "Beastmaster Familiar";
    public override string PluralName => "Beastmaster Familiars";
    public override string HelpText => "Does this item unlock a beastmaster familiar?";
    public override bool ShouldGroup => false;

    public override Func<ItemSource, (Type, uint)>? RelatedType => source =>
    {
        var asSource = AsSource(source);
        return (typeof(AllaganLib.GameSheets.LuminaSheets.XBMPet), asSource.Familiar.RowId);
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        ImGui.Text(GetDescription(source));
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return GetFamiliarName(asSource);
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        var familiarIcon = asSource.Familiar.ValueNullable?.Icon ?? 0;
        return familiarIcon != 0 ? (int)familiarIcon : Icons.MinionIcon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return "Unlocks the " + GetFamiliarName(asSource) + " familiar for beastmasters";
    };

    private string GetFamiliarName(ItemBeastmasterFamiliarUse source)
    {
        var name = source.Pet.ValueNullable?.Name.ExtractText();
        return string.IsNullOrEmpty(name) ? "Unknown" : name.FirstCharToUpper();
    }
}
