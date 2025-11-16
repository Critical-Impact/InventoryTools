using System;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemAnimaWeaponSourceRenderer : ItemInfoRenderer<ItemAnimaWeaponSource>
{
    public ItemAnimaWeaponSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.AnimaWeapon;
    public override string SingularName => "Anima Weapon";
    public override string HelpText => "Is this a Anima Weapon?";
    public override bool ShouldGroup => false;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = this.AsSource(source);
        ImGui.TextUnformatted("Class: " + asSource.ClassJob.Base.Name.ToImGuiString().ToTitleCase());
        this.DrawItems("Forms:", asSource.RewardItems);
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return string.Join(", ", asSource.Items.Select(c => c.NameString));
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.SwordIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return string.Join(", ", asSource.Items.Select(c => c.NameString));
    };
}