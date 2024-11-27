using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using Humanizer;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Mediator;
using InventoryTools.Ui;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemWoodlandExplorationVentureSourceRenderer : ItemVentureSourceRenderer<ItemWoodlandExplorationVentureSource>
{
    public ItemWoodlandExplorationVentureSourceRenderer() : base(ItemInfoType.BotanyExplorationVenture)
    {
    }

    public override string SingularName => "Woodland Exploration Venture (Botany)";
}
public class ItemWatersideExplorationVentureSourceRenderer : ItemVentureSourceRenderer<ItemWatersideExplorationVentureSource>
{
    public ItemWatersideExplorationVentureSourceRenderer() : base(ItemInfoType.FishingExplorationVenture)
    {
    }

    public override string SingularName => "Waterside Exploration Venture (Fishing)";
}
public class ItemHighlandExplorationVentureSourceRenderer : ItemVentureSourceRenderer<ItemHighlandExplorationVentureSource>
{
    public ItemHighlandExplorationVentureSourceRenderer() : base(ItemInfoType.MiningExplorationVenture)
    {
    }

    public override string SingularName => "Highland Exploration Venture (Mining)";
}

public class ItemFieldExplorationVentureSourceRenderer : ItemVentureSourceRenderer<ItemFieldExplorationVentureSource>
{
    public ItemFieldExplorationVentureSourceRenderer() : base(ItemInfoType.CombatExplorationVenture)
    {
    }

    public override string SingularName => "Field Exploration Venture (Combat)";
}

public class ItemBotanistVentureSourceRenderer : ItemVentureSourceRenderer<ItemBotanistVentureSource>
{
    public ItemBotanistVentureSourceRenderer() : base(ItemInfoType.BotanyVenture)
    {
    }

    public override string SingularName => "Venture (Botany)";
}
public class ItemFishingVentureSourceRenderer : ItemVentureSourceRenderer<ItemFishingVentureSource>
{
    public ItemFishingVentureSourceRenderer() : base(ItemInfoType.FishingVenture)
    {
    }

    public override string SingularName => "Venture (Fishing)";
}
public class ItemMiningVentureSourceRenderer : ItemVentureSourceRenderer<ItemMiningVentureSource>
{
    public ItemMiningVentureSourceRenderer() : base(ItemInfoType.MiningVenture)
    {
    }

    public override string SingularName => "Venture (Mining)";
}

public class ItemHuntingVentureSourceRenderer : ItemVentureSourceRenderer<ItemHuntingVentureSource>
{
    public ItemHuntingVentureSourceRenderer() : base(ItemInfoType.CombatVenture)
    {
    }

    public override string SingularName => "Venture (Combat)";
}

public abstract class ItemVentureSourceRenderer<T> : ItemInfoRenderer<T> where T : ItemVentureSource
{
    private readonly ItemInfoType _itemInfoType;

    public ItemVentureSourceRenderer(ItemInfoType itemInfoType)
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
}