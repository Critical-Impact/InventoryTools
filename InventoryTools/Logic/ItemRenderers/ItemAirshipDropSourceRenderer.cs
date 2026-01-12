using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Ui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemAirshipDropSourceRenderer : ItemInfoRenderer<ItemAirshipDropSource>
{
    public ItemAirshipDropSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.Airship;
    public override string SingularName => "Airship Exploration";
    public override bool ShouldGroup => true;
    public override string HelpText => "Can the item be earned from a airship exploration route?";

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var asSource = AsSource(source);
        return [new OpenUintWindowMessage(typeof(AirshipWindow), asSource.AirshipExplorationPoint.RowId)];
    };

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var airshipDropSources = AsSource(sources).DistinctBy(c => c.AirshipExplorationPoint.RowId);
        foreach (var source in airshipDropSources)
        {
            var asSource = AsSource(source);
            ImGui.Text(this.GetDescription(source));
            if (asSource.AirshipExplorationPoint.Unlock != null)
            {
                using (ImRaii.PushIndent())
                {
                    ImGui.TextUnformatted("Unlocks via: " + asSource.AirshipExplorationPoint.Unlock.Base.Name.ToImGuiString());
                }
            }
        }
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text(this.GetDescription(source));
        if (asSource.AirshipExplorationPoint.Unlock != null)
        {
            using (ImRaii.PushIndent())
            {
                ImGui.TextUnformatted("Unlocks via: " + asSource.AirshipExplorationPoint.Unlock.Base.Name.ToImGuiString());
            }
        }
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var airshipDropSource = AsSource(source);
        return airshipDropSource.AirshipExplorationPoint.Base.Name.ToImGuiString();
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        return Icons.AirshipIcon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return $"{asSource.AirshipExplorationPoint.Base.Name.ExtractText()}";
    };
}