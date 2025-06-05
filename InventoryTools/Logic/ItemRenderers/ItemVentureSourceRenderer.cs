using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Humanizer;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Mediator;
using InventoryTools.Ui;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemWoodlandExplorationVentureSourceRenderer : ItemVentureSourceRenderer<ItemWoodlandExplorationVentureSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.ExplorationVenture];
    public ItemWoodlandExplorationVentureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, ItemInfoType.BotanyExplorationVenture, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Woodland Exploration Venture (Botany)";
    public override string HelpText => "Can the item be returned by retainers from botany exploration ventures?";
}
public class ItemWatersideExplorationVentureSourceRenderer : ItemVentureSourceRenderer<ItemWatersideExplorationVentureSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.ExplorationVenture];
    public ItemWatersideExplorationVentureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, ItemInfoType.FishingExplorationVenture, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Waterside Exploration Venture (Fishing)";
    public override string HelpText => "Can the item be returned by retainers from fishing exploration ventures?";
}
public class ItemHighlandExplorationVentureSourceRenderer : ItemVentureSourceRenderer<ItemHighlandExplorationVentureSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.ExplorationVenture];
    public ItemHighlandExplorationVentureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, ItemInfoType.MiningExplorationVenture, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Highland Exploration Venture (Mining)";
    public override string HelpText => "Can the item be returned by retainers from mining exploration ventures?";
}

public class ItemFieldExplorationVentureSourceRenderer : ItemVentureSourceRenderer<ItemFieldExplorationVentureSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.ExplorationVenture];
    public ItemFieldExplorationVentureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, ItemInfoType.CombatExplorationVenture, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Field Exploration Venture (Combat)";
    public override string HelpText => "Can the item be returned by retainers from combat exploration ventures?";
}

public class ItemBotanistVentureSourceRenderer : ItemVentureSourceRenderer<ItemBotanistVentureSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Venture];
    public ItemBotanistVentureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, ItemInfoType.BotanyVenture, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Venture (Botany)";
    public override string HelpText => "Can the item be returned by retainers from botany ventures?";
}
public class ItemFishingVentureSourceRenderer : ItemVentureSourceRenderer<ItemFishingVentureSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Venture];
    public ItemFishingVentureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, ItemInfoType.FishingVenture, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Venture (Fishing)";
    public override string HelpText => "Can the item be returned by retainers from fishing ventures?";
}
public class ItemMiningVentureSourceRenderer : ItemVentureSourceRenderer<ItemMiningVentureSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Venture];
    public ItemMiningVentureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, ItemInfoType.MiningVenture, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Venture (Mining)";
    public override string HelpText => "Can the item be returned by retainers from mining ventures?";
}

public class ItemHuntingVentureSourceRenderer : ItemVentureSourceRenderer<ItemHuntingVentureSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Venture];
    public ItemHuntingVentureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, ItemInfoType.CombatVenture, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Venture (Combat)";
    public override string HelpText => "Can the item be returned by retainers from combat ventures?";
}

public abstract class ItemVentureSourceRenderer<T> : ItemInfoRenderer<T> where T : ItemVentureSource
{
    private readonly ItemInfoType _itemInfoType;

    public ItemVentureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ItemInfoType itemInfoType,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _itemInfoType = itemInfoType;
    }
    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => _itemInfoType;
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);

        ImGui.Text($"{asSource.RetainerTaskRow.FormattedName}");
        using (ImRaii.PushIndent())
        {
            ImGui.Text($"Venture Cost: {asSource.RetainerTaskRow.Base.VentureCost}");
            ImGui.Text($"Required Level: {asSource.RetainerTaskRow.Base.RetainerLevel}");
            if (asSource.RetainerTaskRow.Base.RequiredGathering != 0)
            {
                ImGui.Text(
                    $"Required Gathering: {asSource.RetainerTaskRow.Base.RequiredGathering}");
            }

            if (asSource.RetainerTaskRow.Base.RequiredItemLevel != 0)
            {
                ImGui.Text(
                    $"Required Item Level: {asSource.RetainerTaskRow.Base.RequiredItemLevel}");
            }

            ImGui.Text($"Experience: {asSource.RetainerTaskRow.Base.Experience}");
            ImGui.Text(
                $"Time: {asSource.RetainerTaskRow.Base.MaxTimemin.Minutes().ToHumanReadableString()}");
        }
    };

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var asSource = AsSource(source);

        return new List<MessageBase>()
            { new OpenUintWindowMessage(typeof(RetainerTaskWindow), asSource.RetainerTaskRow.RowId) };
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.RetainerTaskRow.FormattedName;
    };
    public override Func<ItemSource, int> GetIcon => _ => Icons.VentureIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return
            $"{asSource.RetainerTaskRow.FormattedName} ({asSource.RetainerTaskRow.Base.VentureCost} ventures, {asSource.RetainerTaskRow.Base.MaxTimemin.Minutes().ToHumanReadableString()})";
    };
}