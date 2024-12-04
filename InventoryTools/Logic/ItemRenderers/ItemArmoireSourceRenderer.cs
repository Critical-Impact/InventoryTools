using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemArmoireSourceRenderer : ItemInfoRenderer<ItemArmoireSource>
{
    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.Armoire;
    public override string SingularName => "Stored in Armoire";
    public override bool ShouldGroup => true;
    public override string HelpText => "Can the item be placed in the armoire?";

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text("Category: " +
                   (asSource.Cabinet.CabinetCategory?.Base.Category.Value.Text.ExtractText() ?? "Unknown"));
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);

        return "Category: " + asSource.Cabinet.CabinetCategory?.Base.Category.Value.Text.ExtractText();
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.ArmoireIcon;
}