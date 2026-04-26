using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using InventoryTools.Localizers;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Logic.ItemRenderers;

public abstract class ItemRelicWeaponSourceRenderer<T> : ItemInfoRenderer<T> where T : ItemRelicWeaponSource
{
    private readonly ILocalizer<RelicWeaponType> _relicWeaponTypeLocalizer;

    public ItemRelicWeaponSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicWeaponType> relicWeaponTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _relicWeaponTypeLocalizer = relicWeaponTypeLocalizer;
    }

    public override RendererType RendererType => RendererType.Use;
    public override bool ShouldGroup => false;

    public override IReadOnlyList<ItemInfoRenderCategory>? Categories => [ItemInfoRenderCategory.RelicWeapon];

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = this.AsSource(source);
        ImGui.TextUnformatted("Class: " + asSource.RelicWeapon.ClassJob.Value.Name.ToImGuiString().ToTitleCase());
        this.DrawForms("Forms:", asSource.Forms);
    };

    public void DrawForms(string sectionName, IReadOnlyList<RelicWeapon> items)
    {
        if (items.Count == 0)
        {
            return;
        }
        ImGui.TextUnformatted(sectionName);
        using (ImRaii.PushIndent())
        {
            using (var table = ImRaii.Table("forms", items.Any(c => c.OffhandItemId != 0) ? 3 : 2))
            {
                if (table)
                {
                    foreach (var relicWeapon in items)
                    {
                        if (relicWeapon.ItemId == 0)
                            continue;

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted($"{_relicWeaponTypeLocalizer.Format(relicWeapon.Type)}");
                        ImGui.TableNextColumn();
                        var item = ItemSheet.GetRow(relicWeapon.ItemId);
                        ImGui.Image(
                            TextureProvider
                                .GetFromGameIcon(new GameIconLookup(item.Icon))
                                .GetWrapOrEmpty().Handle,
                            new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale
                        );
                        ImGui.SameLine();
                        ImGui.TextUnformatted($"{item.NameString}");
                        if (relicWeapon.OffhandItemId != 0)
                        {
                            ImGui.TableNextColumn();
                            item = ItemSheet.GetRow(relicWeapon.OffhandItemId);
                            ImGui.Image(
                                TextureProvider
                                    .GetFromGameIcon(new GameIconLookup(item.Icon))
                                    .GetWrapOrEmpty().Handle,
                                new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale
                            );
                            ImGui.SameLine();
                            ImGui.TextUnformatted($"{item.NameString}");
                        }
                    }
                }
            }
        }
    }

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return string.Join(", ", asSource.Items.Select(c => c.NameString));
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.WeaponIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return string.Join(", ", asSource.Items.Select(c => c.NameString));
    };

    public override Func<ItemSource, (Type, uint)>? RelatedType => source =>
    {
        var asSource = AsSource(source);
        return (asSource.RelicWeapon.GetType(), asSource.RelicWeapon.RowId);
    };
}


public class ItemZodiacWeaponSourceRenderer : ItemRelicWeaponSourceRenderer<ItemZodiacWeaponSource>
{
    public ItemZodiacWeaponSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicWeaponType> relicWeaponTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicWeaponTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.ZodiacWeapon;
    public override string SingularName => "Zodiac Weapon";
    public override string HelpText => "Is this a Zodiac Weapon?";
}

public class ItemAnimaWeaponSourceRenderer : ItemRelicWeaponSourceRenderer<ItemAnimaWeaponSource>
{
    public ItemAnimaWeaponSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicWeaponType> relicWeaponTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicWeaponTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.AnimaWeapon;
    public override string SingularName => "Anima Weapon";
    public override string HelpText => "Is this a Anima Weapon?";
}

public class ItemEurekanWeaponSourceRenderer : ItemRelicWeaponSourceRenderer<ItemEurekanWeaponSource>
{
    public ItemEurekanWeaponSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicWeaponType> relicWeaponTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicWeaponTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.EurekanWeapon;
    public override string SingularName => "Eurekan Weapon";
    public override string HelpText => "Is this a Eurekan Weapon?";
}

public class ItemResistanceWeaponSourceRenderer : ItemRelicWeaponSourceRenderer<ItemResistanceWeaponSource>
{
    public ItemResistanceWeaponSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicWeaponType> relicWeaponTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicWeaponTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.ResistanceWeapon;
    public override string SingularName => "Resistance Weapon";
    public override string HelpText => "Is this a Resistance Weapon?";
}

public class ItemMandervilleWeaponSourceRenderer : ItemRelicWeaponSourceRenderer<ItemMandervilleWeaponSource>
{
    public ItemMandervilleWeaponSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicWeaponType> relicWeaponTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicWeaponTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.MandervilleWeapon;
    public override string SingularName => "Manderville Weapon";
    public override string HelpText => "Is this a Manderville Weapon?";
}

public class ItemPhantomWeaponSourceRenderer : ItemRelicWeaponSourceRenderer<ItemPhantomWeaponSource>
{
    public ItemPhantomWeaponSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicWeaponType> relicWeaponTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicWeaponTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.PhantomWeapon;
    public override string SingularName => "Phantom Weapon";
    public override string HelpText => "Is this a Phantom Weapon?";
}
