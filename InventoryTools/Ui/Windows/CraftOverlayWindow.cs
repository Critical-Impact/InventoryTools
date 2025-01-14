using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Ui;

public enum CraftOverlayWindowState
{
    Collapsed,
    Single,
    List
}
public class CraftOverlayWindow : OverlayWindow
{
    private readonly IListService _listService;
    private readonly IFont _font;
    private readonly ICommandManager _commandManager;
    private readonly IChatUtilities _chatUtilities;
    private readonly IGameInterface _gameInterface;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly ImGuiMenuService _imGuiMenuService;
    private readonly CraftOverlayMaxExpandedItemsSetting _maxExpandedItemsSetting;
    private readonly CraftOverlayRememberStateSetting _rememberStateSetting;
    private readonly CraftOverlayWindowStateSetting _windowStateSetting;
    private readonly CraftOverlayHideSetting _overlayHideSetting;
    private readonly ShopTrackerService _shopTrackerService;
    private readonly MapSheet _mapSheet;

    public CraftOverlayWindow(ILogger<CraftOverlayWindow> logger,
        InventoryToolsConfiguration configuration,
        IListService listService,
        IAddonLifecycle addonLifecycle,
        MapSheet mapSheet,
        IFont font,
        IGameGui gameGui,
        IPluginLog pluginLog,
        MediatorService mediator,
        ICommandManager commandManager,
        IChatUtilities chatUtilities,
        IGameInterface gameInterface,
        ICharacterMonitor characterMonitor,
        ImGuiMenuService imGuiMenuService,
        ImGuiService imGuiService,
        CraftOverlayMaxExpandedItemsSetting maxExpandedItemsSetting,
        CraftOverlayRememberStateSetting rememberStateSetting,
        CraftOverlayWindowStateSetting windowStateSetting,
        CraftOverlayHideSetting overlayHideSetting,
        ShopTrackerService shopTrackerService) : base(logger,
        configuration,
        addonLifecycle,
        gameGui,
        pluginLog,
        mediator,
        imGuiService,
        "Craft Overlay")
    {
        _listService = listService;
        _mapSheet = mapSheet;
        _font = font;
        _commandManager = commandManager;
        _chatUtilities = chatUtilities;
        _gameInterface = gameInterface;
        _characterMonitor = characterMonitor;
        _imGuiMenuService = imGuiMenuService;
        _maxExpandedItemsSetting = maxExpandedItemsSetting;
        _rememberStateSetting = rememberStateSetting;
        _windowStateSetting = windowStateSetting;
        _overlayHideSetting = overlayHideSetting;
        _shopTrackerService = shopTrackerService;
    }

    public override void Initialize()
    {

    }

    public CraftOverlayWindowState WindowState
    {
        get => _windowStateSetting.CurrentValue(Configuration);
        set => _windowStateSetting.UpdateFilterConfiguration(Configuration, value);
    }

    public override bool DrawConditions()
    {
        if (this._overlayHideSetting.CurrentValue(Configuration) == CraftOverlayHide.AlwaysShow)
        {
            return true;
        }

        return this._overlayHideSetting.ShouldShow();
    }

    public override void Draw()
    {
        if (ImGui.GetWindowPos() != CurrentPosition)
        {
            CurrentPosition = ImGui.GetWindowPos();
        }
        var collapsed = this.WindowState;

        var currentCursorPosX = ImGui.GetCursorPosX();

        this.SizeConstraints = new WindowSizeConstraints()
        {
            MaximumSize = new Vector2(450, 800) * ImGui.GetIO().FontGlobalScale,
        };

        if (WindowState == CraftOverlayWindowState.Collapsed && ImGuiService.DrawIconButton(_font, FontAwesomeIcon.ChevronRight, ref currentCursorPosX))
        {
            this.WindowState = CraftOverlayWindowState.Single;
        }

        if (WindowState == CraftOverlayWindowState.Single && ImGuiService.DrawIconButton(_font, FontAwesomeIcon.ChevronDown, ref currentCursorPosX))
        {
            this.WindowState = CraftOverlayWindowState.List;
        }

        if (WindowState == CraftOverlayWindowState.List && ImGuiService.DrawIconButton(_font, FontAwesomeIcon.ChevronLeft, ref currentCursorPosX))
        {
            this.WindowState = CraftOverlayWindowState.Collapsed;
        }

        if (ImGui.IsItemHovered())
        {
            using (var tooltip = ImRaii.Tooltip())
            {
                if (tooltip)
                {
                    var nextState = "";
                    switch (this.WindowState)
                    {
                        case CraftOverlayWindowState.Collapsed:
                            nextState = "Expand";
                            break;
                        case CraftOverlayWindowState.Single:
                            nextState = "Show More";
                            break;
                        case CraftOverlayWindowState.List:
                            nextState = "Collapse";
                            break;
                    }
                    ImGui.TextUnformatted($"Left Click: {nextState}");
                    ImGui.TextUnformatted("Right Click: Menu");
                }
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("RightClick");
            }
        }

        using (var popup = ImRaii.Popup("RightClick"))
        {
            if (popup)
            {
                if (ImGui.MenuItem("Close"))
                {
                    this.Close();
                }
            }
        }

        if (WindowState == CraftOverlayWindowState.Collapsed)
        {
            return;
        }

        CraftGrouping? currentGroup = null;
        CraftList? craftList = null;
        List<CraftItem>? nextItems = null;
        HashSet<uint>? itemsToRetrieve = null;
        int totalSteps = 0;
        int completedSteps = 0;
        var maxItems = this._maxExpandedItemsSetting.CurrentValue(Configuration);
        if (SelectedConfiguration != null)
        {
            SelectedConfiguration.AllowRefresh = true;
            craftList = SelectedConfiguration.CraftList;
            var outputList = craftList.GetOutputList();
            totalSteps = craftList.GetFlattenedMergedMaterials().Count;
            completedSteps = craftList.GetFlattenedMergedMaterials().Count(c => c.IsCompleted);
            var activeRetainer = _characterMonitor.ActiveRetainer;
            if (activeRetainer != null && SelectedConfiguration.SearchResults != null)
            {
                itemsToRetrieve = SelectedConfiguration.SearchResults
                    .Where(c => c.InventoryItem!.RetainerId == activeRetainer.CharacterId).Select(c => c.ItemId)
                    .Distinct().ToHashSet();
            }

            for (var index = outputList.Count - 1; index >= 0; index--)
            {
                var group = outputList[index];
                if (nextItems != null && nextItems.Count >= maxItems)
                {
                    break;
                }
                for (var i = group.CraftItems.Count - 1; i >= 0; i--)
                {
                    var item = group.CraftItems[i];
                    if (nextItems != null && nextItems.Count >= maxItems)
                    {
                        break;
                    }
                    if (item.IsCompleted)
                    {
                        continue;
                    }

                    if (activeRetainer == null || craftList.GetNextCraftStep(item) != NextCraftStep.Retrieve)
                    {
                        nextItems ??= new List<CraftItem>();
                        nextItems.Add(item);
                        if (currentGroup == null)
                        {
                            currentGroup = group;
                        }
                    }
                    else
                    {
                        if (itemsToRetrieve != null && itemsToRetrieve.Contains(item.ItemId))
                        {
                            nextItems ??= new List<CraftItem>();
                            nextItems.Add(item);
                            if (currentGroup == null)
                            {
                                currentGroup = group;
                            }
                        }
                    }
                }
            }
            SelectedConfiguration.Active = true;
        }



        ImGui.SameLine();
        if (currentGroup != null)
        {
            ImGui.Text(currentGroup.FormattedName() + $" ({completedSteps}/{totalSteps})");
        }
        else
        {
            ImGui.Text("Nothing to do.");
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 150 + 70 + 80);
        }

        ImGui.SameLine();

        currentCursorPosX = ImGui.GetWindowSize().X;

        if (ImGuiService.DrawIconButton(
                _font,
                FontAwesomeIcon.Hammer,
                ref currentCursorPosX,
                "Open the Allagan Tools crafts window.",
                true))
        {
            this.MediatorService.Publish(new ToggleGenericWindowMessage(typeof(CraftsWindow)));
        }

        ImGui.SameLine();

        if (ImGuiService.DrawIconButton(
                _font,
                FontAwesomeIcon.Cog,
                ref currentCursorPosX,
                "Open the Allagan Tools configuration window.",
                true))
        {
            this.MediatorService.Publish(new ToggleGenericWindowMessage(typeof(ConfigurationWindow)));
        }
        ImGui.SameLine();
        var isHighlighting = Configuration.ActiveUiFilter == SelectedConfiguration?.Key;
        if (ImGuiService.DrawIconButton(
                _font,
                FontAwesomeIcon.Lightbulb,
                ref currentCursorPosX,
                "Toggle highlighting.",
                true,
                isHighlighting ? null : ImGuiColors.ParsedGrey))
        {
            if (SelectedConfiguration != null)
            {
                _listService.ToggleActiveUiList(SelectedConfiguration);
            }
        }

        ImGui.SameLine();
        using (var popup = ImRaii.Popup("SelectCraftList"))
        {
            if (popup)
            {
                foreach (var c in _listService.Lists.Where(c =>
                             c is { FilterType: FilterType.CraftFilter, CraftListDefault: false }))
                {
                    if (ImGui.MenuItem(c.Name, "", c == SelectedConfiguration))
                    {
                        _listService.ToggleActiveCraftList(c);
                    }
                }
            }
        }
        if (ImGuiService.DrawIconButton(
                _font,
                FontAwesomeIcon.Bars,
                ref currentCursorPosX,
                "Select active craft list",
                true))
        {
            ImGui.OpenPopup("SelectCraftList");
        }

        ImGui.Separator();

        if (SelectedConfiguration == null)
        {
            ImGui.Text("No craft list active.");
        }
        else if(craftList != null)
        {
            if (nextItems != null)
            {
                using (ImRaii.Table("CraftList", 5, ImGuiTableFlags.SizingStretchProp))
                {
                    ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed,
                        20 * ImGui.GetIO().FontGlobalScale);
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed,
                        150 * ImGui.GetIO().FontGlobalScale);
                    ImGui.TableSetupColumn("Step", ImGuiTableColumnFlags.WidthFixed,
                        80 * ImGui.GetIO().FontGlobalScale);
                    ImGui.TableSetupColumn("Bell", ImGuiTableColumnFlags.WidthFixed,
                        20 * ImGui.GetIO().FontGlobalScale);
                    ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed,
                        70 * ImGui.GetIO().FontGlobalScale);
                    var index = 1;
                    foreach (var currentItem in nextItems.Take(WindowState == CraftOverlayWindowState.Single ? 1 : maxItems))
                    {
                        using (var id = ImRaii.PushId(index))
                        {
                            using (var popup = ImRaii.Popup("MoreInfo"))
                            {
                                if (popup)
                                {
                                    _imGuiMenuService.DrawRightClickPopup(currentItem.Item);
                                }
                            }

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            if (ImGui.ImageButton(ImGuiService.GetIconTexture(currentItem.Item.Icon).ImGuiHandle, new Vector2(16,16)))
                            {
                                this.MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), currentItem.ItemId));
                            }

                            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup("MoreInfo");
                            }

                            ImGui.TableNextColumn();
                            ImGui.PushTextWrapPos();
                            ImGui.TextUnformatted($"{index}. {currentItem.Item.NameString}");
                            ImGui.PopTextWrapPos();
                            ImGui.TableNextColumn();
                            var nextStep = craftList.GetNextStep(currentItem);
                            var nextCraftStep = craftList.GetNextCraftStep(currentItem);
                            using (ImRaii.PushColor(ImGuiCol.Text, nextStep.Item1))
                            {
                                ImGui.PushTextWrapPos();
                                ImGui.Text(nextStep.Item2);
                                ImGui.PopTextWrapPos();
                            }
                            ImGui.TableNextColumn();
                            if (nextCraftStep == NextCraftStep.Retrieve)
                            {
                                ImGuiService.DrawIcon(Icons.RetainerIcon, new (20,20));
                                if (SelectedConfiguration.SearchResults != null &&ImGui.IsItemHovered())
                                {
                                    using (var tooltip = ImRaii.Tooltip())
                                    {
                                        if (tooltip)
                                        {
                                            var sortingResults = SelectedConfiguration.SearchResults
                                                .Where(c => c.SortingResult!.ItemId == currentItem.ItemId)
                                                .Select(c => c.SortingResult!)
                                                .Distinct().ToList();
                                            foreach (var result in sortingResults)
                                            {
                                                ImGui.Text($"{result.Quantity} available to retrieve from {_characterMonitor.GetCharacterById(result.SourceRetainerId)?.FormattedName ?? "Unknown Retainer"} in {result.SourceBag.FormattedName()} at {result.BagLocation.X + 1}/{result.BagLocation.Y + 1}");
                                            }
                                        }
                                    }
                                }
                            }

                            ImGui.TableNextColumn();
                            var buttonPosX = ImGui.GetCursorPosX();
                            if (ImGuiService.DrawIconButton(
                                    _font,
                                    FontAwesomeIcon.BoltLightning,
                                    ref buttonPosX))
                            {
                                ImGui.OpenPopup("ItemAction");
                            }

                            using (var popup = ImRaii.Popup("ItemAction"))
                            {
                                if (popup.Success)
                                {
                                    if (ImGui.MenuItem("More Information"))
                                    {
                                        this.MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), currentItem.ItemId));
                                    }
                                    this.MediatorService.Publish(_imGuiMenuService.DrawActionMenu(new SearchResult(currentItem)));
                                }
                            }

                            index++;
                        }
                    }
                }
            }
        }


    }

    public override void Invalidate()
    {

    }

    public override FilterConfiguration? SelectedConfiguration => _listService.GetActiveCraftList();
    public override string GenericKey { get; } = "CraftOverlay";
    public override string GenericName { get; } = "Craft Overlay";
    public override bool DestroyOnClose { get; } = false;

    public override bool SaveState => this._rememberStateSetting.CurrentValue(Configuration);

    public override bool SavePosition => true;
    public override Vector2? DefaultSize { get; } = null;
    public override Vector2? MaxSize { get; } = null;
    public override Vector2? MinSize { get; } = null;
}