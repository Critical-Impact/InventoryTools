using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public abstract class OverlayWindow : GenericWindow, IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IPluginLog pluginLog;
    private readonly Dictionary<string, bool> addonVisibility = [];
    private float currentHeight;
    private bool shouldOpen;
    private bool shouldClose;
    private bool needsBind;

    protected OverlayWindow(
        ILogger logger,
        InventoryToolsConfiguration configuration,
        IAddonLifecycle addonLifecycle,
        IGameGui gameGui,
        IPluginLog pluginLog,
        MediatorService mediator,
        ImGuiService imGuiService,
        string name = "")
        : base(logger, mediator, imGuiService, configuration, name)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.pluginLog = pluginLog;
        this.AttachedAddons = [];
        this.MediatorService.Subscribe<PluginLoadedMessage>(this, this.PluginLoaded);
        this.Flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize;
        this.Size = null;
        this.RespectCloseHotkey = false;
    }

    public enum AttachPosition
    {
        Left,
        Right,
        Top,
        Bottom,
    }

    private HashSet<(string AddonName, AttachPosition AttachPosition)> AttachedAddons { get; }

    public override void PostDraw()
    {
        base.PostDraw();
        this.currentHeight = ImGui.GetWindowSize().Y;
    }

    public unsafe void AttachAddon(string addonName, AttachPosition attachPosition)
    {
        var addonKey = (addonName, attachPosition);
        if (!this.AttachedAddons.Contains(addonKey))
        {
            var addonPtr = this.gameGui.GetAddonByName(addonName);
            this.addonVisibility[addonName] = false;
            if (addonPtr != IntPtr.Zero)
            {
                var atkUnitBase = (AtkUnitBase*)addonPtr;
                if (atkUnitBase != null)
                {
                    this.addonVisibility[addonName] = atkUnitBase->IsVisible;
                }
            }

            this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, addonName, this.OnPostSetup);
            this.addonLifecycle.RegisterListener(AddonEvent.PreFinalize, addonName, this.OnPreFinalize);
            this.addonLifecycle.RegisterListener(AddonEvent.PostRefresh, addonName, this.OnPostRefresh);
            this.addonLifecycle.RegisterListener(AddonEvent.PostDraw, addonName, this.OnPostDraw);
            this.AttachedAddons.Add(addonKey);
        }
    }

    public override bool DrawConditions()
    {
        if (this.needsBind)
        {
            return false;
        }

        return base.DrawConditions();
    }

    public override void PreOpenCheck()
    {
        base.PreOpenCheck();
        if (this.shouldOpen)
        {
            this.IsOpen = true;
            this.shouldOpen = false;
            this.needsBind = true;
        }
        else if (this.shouldClose)
        {
            this.IsOpen = false;
            this.shouldClose = false;
            this.needsBind = true;
        }
    }

    public new void Dispose()
    {
        this.Dispose(true);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var attachedAddon in this.AttachedAddons)
            {
                this.addonLifecycle.UnregisterListener(AddonEvent.PostSetup, attachedAddon.AddonName, this.OnPostSetup);
                this.addonLifecycle.UnregisterListener(AddonEvent.PreFinalize, attachedAddon.AddonName, this.OnPreFinalize);
                this.addonLifecycle.UnregisterListener(AddonEvent.PostRefresh, attachedAddon.AddonName, this.OnPostRefresh);
                this.addonLifecycle.UnregisterListener(AddonEvent.PostDraw, attachedAddon.AddonName, this.OnPostDraw);
            }
        }
    }

    private void PluginLoaded(PluginLoadedMessage obj)
    {
        foreach (var attachedAddon in this.AttachedAddons)
        {
            var addon = this.gameGui.GetAddonByName(attachedAddon.AddonName);
            if (addon != nint.Zero)
            {
                this.BindToWindow(addon);
            }
        }
    }

    private void OnPostDraw(AddonEvent type, AddonArgs args)
    {
        // Window moved potentially?
        this.BindToWindow(args.Addon);
    }

    private unsafe void OnPostRefresh(AddonEvent type, AddonArgs args)
    {
        // Window shown/hidden
        if (args.Addon != IntPtr.Zero)
        {
            var atkUnitBase = (AtkUnitBase*)args.Addon;
            var previousAddonVisibility = this.addonVisibility[args.AddonName];
            if (this.shouldOpen != true && this.shouldClose != true &&
                previousAddonVisibility != atkUnitBase->IsVisible)
            {
                this.addonVisibility[args.AddonName] = atkUnitBase->IsVisible;
                if (atkUnitBase->IsVisible)
                {
                    this.shouldOpen = true;
                }
                else
                {
                    this.shouldClose = true;
                }

                this.BindToWindow(args.Addon);
            }
        }
    }

    private void OnPreFinalize(AddonEvent type, AddonArgs args)
    {
        // Window destroyed
        if (this.IsOpen)
        {
            this.shouldClose = true;
        }
    }

    private void OnPostSetup(AddonEvent type, AddonArgs args)
    {
        // Window visible
        if (!this.IsOpen)
        {
            this.shouldOpen = true;
            this.BindToWindow(args.Addon);
        }
    }

    private unsafe void BindToWindow(nint addon)
    {
        try
        {
            if (addon != IntPtr.Zero)
            {
                var atkUnitBase = (AtkUnitBase*)addon;
                if (this.AttachedAddons.Contains((atkUnitBase->NameString, AttachPosition.Top)))
                {
                    var windowX = (float)atkUnitBase->X + 10;
                    var windowY = (float)atkUnitBase->Y;
                    windowY -= this.currentHeight + 10;
                    this.Position = new Vector2(windowX, windowY);
                    this.ForceMainWindow = true;
                }
                else if (this.AttachedAddons.Contains((atkUnitBase->NameString, AttachPosition.Bottom)))
                {
                    var windowX = (float)atkUnitBase->X + 10;
                    var windowY = ((float)atkUnitBase->Y + atkUnitBase->GetScaledHeight(true)) - 10;
                    this.Position = new Vector2(windowX, windowY);
                    this.ForceMainWindow = true;
                }
                else if (this.AttachedAddons.Contains((atkUnitBase->NameString, AttachPosition.Right)))
                {
                    var windowX = atkUnitBase->X + atkUnitBase->GetScaledWidth(true);
                    var windowY = (float)atkUnitBase->Y + 2;
                    this.Position = new Vector2(windowX, windowY);
                    this.ForceMainWindow = true;
                }
                else if (this.AttachedAddons.Contains((atkUnitBase->NameString, AttachPosition.Left)))
                {
                }

                this.needsBind = false;
            }
        }
        catch (Exception e)
        {
            this.pluginLog.Error(e, "Failed to wrangle imgui window into position");
        }
    }
}
