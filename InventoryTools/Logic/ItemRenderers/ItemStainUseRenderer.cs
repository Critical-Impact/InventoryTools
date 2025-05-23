using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemStainUseRenderer : ItemInfoRenderer<ItemStainSource>
{
    public ItemStainUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.Stain;
    public override string SingularName => "Dye";
    public override string PluralName => "Dyeing";
    public override string HelpText => "Can the item be used to dye an item?";
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var stainSource = AsSource(source);
        ImGui.Text("Colour: " + stainSource.Stain.Value.Name.ExtractText());
        var color = Utils.Convert3ChannelUintToColorVector4(stainSource.Stain.Value.Color);
        if (ImGui.ColorButton("ColorPreview", color, ImGuiColorEditFlags.None, new (64,64)))
        {
        }
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var stainSource = AsSource(source);
        return stainSource.Stain.Value.Name.ExtractText();
    };
    public override Func<ItemSource, int> GetIcon => source =>
    {
        return Icons.DyeIcon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var vec4Color = Utils.Convert3ChannelUintToColorVector4(asSource.Stain.Value.Color);
        return $"{asSource.Stain.Value.Name.ExtractText()} ({Utils.ColorToHex(Utils.ColorFromVector4(vec4Color))})";
    };


}