using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using InventoryTools.Logic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays
{
    public class SelectIconStringOverlay: GameOverlay<AtkSelectIconString>, IAtkOverlayState, IDisposable
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly ShopTrackerService _shopTrackerService;
        private readonly IAddonLifecycle _addonLifecycle;

        public SelectIconStringOverlay(ILogger<SelectIconStringOverlay> logger, AtkSelectIconString overlay, ICharacterMonitor characterMonitor, ShopTrackerService shopTrackerService, IAddonLifecycle addonLifecycle) : base(logger,overlay)
        {
            _characterMonitor = characterMonitor;
            _shopTrackerService = shopTrackerService;
            _addonLifecycle = addonLifecycle;
            _addonLifecycle.RegisterListener(AddonEvent.PostSetup, this.WindowName.ToString(),AddonPostSetup);
        }

        private void AddonPostSetup(AddonEvent type, AddonArgs args)
        {
            NeedsStateRefresh = true;
        }

        public override bool ShouldDraw { get; set; }

        public override bool Draw()
        {
            if (!HasState || !AtkOverlay.HasAddon)
            {
                Logger.LogTrace("no state and no addon");
                return false;
            }
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                Logger.LogTrace("has atk base, setting colors");
                this.AtkOverlay.SetColors(SelectItems);
                return true;
            }

            return false;
        }

        public List<Vector4?> SelectItems = new();

        public override void Setup()
        {

        }

        public override bool HasState { get; set; }
        public override bool NeedsStateRefresh { get; set; }

        public override void UpdateState(FilterState? newState)
        {
            if (_characterMonitor.ActiveCharacterId == 0)
            {
                return;
            }
            if (newState != null && AtkOverlay.HasAddon && newState.ShouldHighlight && newState.HasFilterResult)
            {
                HasState = true;
                var filterResult = newState.FilterResult;
                if (filterResult != null)
                {
                    Logger.LogTrace("Attempting to update state for SelectIconString");
                    var currentShopTypes = _shopTrackerService.GetCurrentShopType();
                    if (currentShopTypes != null)
                    {
                        SelectItems = newState.GetSelectIconStringItems(currentShopTypes.Value.shops);
                    }
                    else
                    {
                        SelectItems = new List<Vector4?>();
                    }

                    Draw();
                    return;
                }
            }

            if (HasState)
            {
                Logger.LogTrace("Clearing select items");
                SelectItems = new List<Vector4?>();
                Clear();
            }

            HasState = false;
        }

        public override void Clear()
        {
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.AtkOverlay.ResetColors();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _addonLifecycle.UnregisterListener(AddonEvent.PostSetup, this.WindowName.ToString(),AddonPostSetup);
            }
        }

        public new void Dispose()
        {
            Dispose(true);
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}