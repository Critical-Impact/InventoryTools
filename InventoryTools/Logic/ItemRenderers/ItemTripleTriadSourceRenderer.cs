using System;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemTripleTriadSourceRenderer : ItemInfoRenderer<ItemTripleTriadSource>
{
    public ItemTripleTriadSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.TripleTriad;
    public override string SingularName => "Triple Triad Card";
    public override string HelpText => "Is this item acquired from playing triple triad?";
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => (source) =>
    {
        var asSource = this.AsSource(source);

        ImGui.TextUnformatted("Match Cost: " + asSource.TripleTriadRow.Base.Fee + SeIconChar.Gil.ToIconString());
        ImGui.TextUnformatted("Uses Regional Rules: " + (asSource.TripleTriadRow.Base.UsesRegionalRules ? "Yes" : "No"));

        DrawSection("Rules: ", asSource.TripleTriadRow.Base.TripleTriadRule.Where(c => c.RowId != 0).DistinctBy(c => c.RowId).Select(c => c.Value.Name.ToImGuiString()).ToList());

        foreach (var npc in asSource.TripleTriadRow.ENpcBaseRows)
        {
            DrawLocations(npc.Resident.Value.Singular.ToImGuiString(), npc.Locations.ToList());
        }
    };

    public override Func<ItemSource, string> GetName => (source) =>
    {
        return source.Item.NameString;
    };

    public override Func<ItemSource, int> GetIcon => (source) =>
    {
        return Icons.TripleTriadIcon;
    };

    public override Func<ItemSource, string> GetDescription => (source) =>
    {
        return source.Item.NameString;
    };
}