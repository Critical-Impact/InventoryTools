using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemBuddySourceRenderer : ItemInfoRenderer<ItemBuddySource>
{
    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.BuddyItem;
    public override string SingularName => "Used on Chocobo Companion";
    public override bool ShouldGroup => false;
    public override string HelpText => "Can the item be used on your chocobo companion?";

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var usedField = asSource.BuddyItem.Value.UseField;
        var usedTraining = asSource.BuddyItem.Value.UseTraining;
        var usedDyeing = asSource.BuddyItem.Value.Unknown0;

        if (usedField)
        {
            ImGui.Text("Battle: Increases EXP earned by your chocobo companion.");
        }

        if (usedTraining)
        {
            ImGui.Text("Stable: Training food for a stabled Chocobo companion.");
        }

        if (usedDyeing)
        {
            ImGui.Text("Dying: Used in Chocobo Dyeing.");
        }
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        var usedField = asSource.BuddyItem.Value.UseField;
        var usedTraining = asSource.BuddyItem.Value.UseTraining;
        var usedDyeing = asSource.BuddyItem.Value.Unknown0;
        var name = new List<string>();


        if (usedField)
        {
            name.Add("battle");
        }

        if (usedTraining)
        {
            name.Add("training");
        }

        if (usedDyeing)
        {
            name.Add("dyeing");
        }

        return "chocobo " + string.Join(", ", name);
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.ChocoboIcon;
}