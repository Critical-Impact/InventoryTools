using System.Linq;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using InventoryTools.Highlighting;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays;

public class InventoryShopOverlay : GameOverlay<AtkShop>, IAtkOverlayState
{
    private readonly ShopHighlighting _shopHighlighting;

    public InventoryShopOverlay(ILogger<InventoryShopOverlay> logger, ShopHighlighting shopHighlighting, AtkShop overlay) : base(logger, overlay)
    {
        _shopHighlighting = shopHighlighting;
    }


    public override bool ShouldDraw { get; set; }
    public override bool Draw()
    {
        if (!HasState || !AtkOverlay.HasAddon)
        {
            return false;
        }

        return true;
    }

    public override void Setup()
    {
    }

    public override bool HasState { get; set; }
    public override bool NeedsStateRefresh { get; set; }
    public override void UpdateState(FilterState? newState)
    {
        if (newState != null && newState.FilterResult != null)
        {
            HasState = true;
            Clear();
            _shopHighlighting.SetItems(newState.GetItemIds());
            return;
        }

        if (HasState)
        {
            Clear();
        }

        HasState = false;
    }

    public override void Clear()
    {
        this._shopHighlighting.ClearItems();
    }
}