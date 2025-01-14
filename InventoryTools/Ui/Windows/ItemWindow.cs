using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Web;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Game.Text;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;

using OtterGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Humanizer;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui.Widgets;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;
using Microsoft.Extensions.Logging;
using OtterGui.Log;
using OtterGui.Widgets;
using ImGuiUtil = OtterGui.ImGuiUtil;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Ui
{
    public class WorldPicker : FilterComboBase<World>
    {
        public HashSet<uint> SelectedWorldIds { get; set; } = new();
        public WorldPicker(IReadOnlyList<World> items, bool keepStorage, Logger log) : base(items, keepStorage, log)
        {

        }

        protected override string ToString(World obj)
        {
            return obj.Name.ExtractText();
        }
    }
    public class ItemWindow : UintWindow
    {
        private readonly IMarketBoardService _marketBoardService;
        private readonly IFramework _framework;
        private readonly ICommandManager _commandManager;
        private readonly IListService _listService;
        private readonly ItemSheet _itemSheet;
        private readonly ExcelSheet<World> _worldSheet;
        private readonly IGameInterface _gameInterface;
        private readonly IMarketCache _marketCache;
        private readonly IChatUtilities _chatUtilities;
        private readonly Logger _otterLogger;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IClipboardService _clipboardService;
        private readonly ItemInfoRenderService _itemInfoRenderService;
        private readonly BNpcNameSheet _bNpcNameSheet;
        private readonly MapSheet _mapSheet;
        private readonly IUnlockTrackerService _unlockTrackerService;
        private readonly ImGuiTooltipService _tooltipService;
        private readonly ImGuiTooltipModeSetting _tooltipModeSetting;
        private HashSet<uint> _marketRefreshing = new();
        private HoverButton _refreshPricesButton = new();

        public ItemWindow(ILogger<ItemWindow> logger, MediatorService mediator, ImGuiService imGuiService,
            InventoryToolsConfiguration configuration, IMarketBoardService marketBoardService, IFramework framework,
            ICommandManager commandManager, IListService listService, ItemSheet itemSheet, ExcelSheet<World> worldSheet,
            IGameInterface gameInterface, IMarketCache marketCache, IChatUtilities chatUtilities, Logger otterLogger,
            IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor, IClipboardService clipboardService,
            ItemInfoRenderService itemInfoRenderService, BNpcNameSheet bNpcNameSheet, MapSheet mapSheet, IUnlockTrackerService unlockTrackerService,
            ImGuiTooltipService tooltipService, ImGuiTooltipModeSetting tooltipModeSetting,
            string name = "Item Window") : base(
            logger, mediator, imGuiService, configuration, name)
        {
            _marketBoardService = marketBoardService;
            _framework = framework;
            _commandManager = commandManager;
            _listService = listService;
            _itemSheet = itemSheet;
            _worldSheet = worldSheet;
            _gameInterface = gameInterface;
            _marketCache = marketCache;
            _chatUtilities = chatUtilities;
            _otterLogger = otterLogger;
            _inventoryMonitor = inventoryMonitor;
            _characterMonitor = characterMonitor;
            _clipboardService = clipboardService;
            _itemInfoRenderService = itemInfoRenderService;
            _bNpcNameSheet = bNpcNameSheet;
            _mapSheet = mapSheet;
            _unlockTrackerService = unlockTrackerService;
            _tooltipService = tooltipService;
            _tooltipModeSetting = tooltipModeSetting;
        }

        private void MarketCacheUpdated(MarketCacheUpdatedMessage obj)
        {
            if (obj.itemId == WindowId)
            {
                GetMarketPrices();
                _marketRefreshing.Remove(obj.worldId);
            }
        }

        public override void Initialize(uint itemId)
        {
            base.Initialize(itemId);
             Flags = ImGuiWindowFlags.NoSavedSettings;
            _itemId = itemId;
            var worlds = _worldSheet.Where(c => c.IsPublic).ToList();
            _picker = new WorldPicker(worlds, true, _otterLogger);
            MediatorService.Subscribe<MarketCacheUpdatedMessage>(this, MarketCacheUpdated);
            if (Item != null)
            {
                WindowName = "Allagan Tools - " + Item.NameString;
                Key = "item_" + itemId;
                RetainerTasks = Item.GetSourcesByCategory<ItemVentureSource>(ItemInfoCategory.AllVentures).Select(c => c.RetainerTaskRow).ToArray();
                RecipesResult = Item.GetSourcesByType<ItemCraftResultSource>(ItemInfoType.CraftRecipe).Select(c => c.Recipe).ToArray();
                RecipesAsRequirement = Item.RecipesAsRequirement.ToArray();
                Uses = Item.Uses;
                Vendors = new List<(IShop shop, ENpcResidentRow? npc, ILocation? location)>();

                foreach (var shopSource in Item.GetSourcesByCategory<ItemShopSource>(ItemInfoCategory.Shop))
                {
                    var vendor = shopSource.Shop;
                    if (vendor.Name == "")
                    {
                        continue;
                    }
                    if (!vendor.ENpcs.Any())
                    {
                        Vendors.Add(new (vendor, null, null));
                    }
                    else
                    {
                        foreach (var npc in vendor.ENpcs)
                        {
                            if (npc.IsHouseVendorChild) continue;
                            if (!npc.Locations.Any())
                            {
                                Vendors.Add(new (vendor, npc.ENpcResidentRow, null));
                            }
                            else
                            {
                                foreach (var location in npc.Locations)
                                {
                                    Vendors.Add(new (vendor, npc.ENpcResidentRow, location));
                                }
                            }
                        }
                    }
                }
                Vendors = Vendors.OrderByDescending(c => c.npc != null && c.location != null).ToList();
                GatheringSources = Item.GetSourcesByCategory<ItemGatheringSource>(ItemInfoCategory.Gathering).ToList();
                SharedModels = Item.GetSharedModels();
                MobDrops = Item.GetSourcesByType<ItemMonsterDropSource>(ItemInfoType.Monster).Select(c => c.MobDrop).ToArray();
                OwnedItems = _inventoryMonitor.AllItems.Where(c => c.ItemId == itemId).ToList();
                if (Configuration.AutomaticallyDownloadMarketPrices)
                {
                    RequestMarketPrices(false);
                }
                GetMarketPrices();
            }
            else
            {
                RetainerTasks = [];
                RecipesResult = [];
                RecipesAsRequirement = [];
                GatheringSources = new();
                Vendors = new();
                SharedModels = new();
                MobDrops = [];
                Sources = [];
                Uses = [];
                OwnedItems = new List<CriticalCommonLib.Models.InventoryItem>();
                WindowName = "Invalid Item";
                Key = "item_unknown";
            }
        }

        public override bool SaveState => false;
        private uint _itemId;
        private ItemRow? Item => _itemSheet.GetRow(_itemId);
        private CraftItem? _craftItem;
        private List<MarketPricing> _marketPrices = new List<MarketPricing>();
        private WorldPicker _picker;
        private Dictionary<uint, string>? _craftTypes;
        private uint? _craftTypeId;

        private void GetMarketPrices()
        {
            var defaultWorlds = _marketBoardService.GetDefaultWorlds();
            var worldIds = _picker.SelectedWorldIds.ToHashSet();
            foreach (var world in defaultWorlds)
            {
                worldIds.Add(world);
            }

            _marketPrices = _marketCache.GetPricing(_itemId, worldIds.ToList(), true);
        }

        private void RequestMarketPrices(bool forceCheck = true)
        {
            if (Item != null)
            {
                var defaultWorlds = _marketBoardService.GetDefaultWorlds();
                var worldIds = _picker.SelectedWorldIds.ToHashSet();
                foreach (var world in defaultWorlds)
                {
                    worldIds.Add(world);
                }

                foreach (var worldId in worldIds)
                {
                    if (_marketCache.RequestCheck(Item.RowId, worldId, forceCheck))
                    {
                        _marketRefreshing.Add(worldId);
                    }
                }
            }
        }
        public List<ItemRow> SharedModels { get;set; }

        private List<ItemGatheringSource> GatheringSources { get;set; }

        private List<(IShop shop, ENpcResidentRow? npc, ILocation? location)> Vendors { get;set; }

        private RecipeRow[] RecipesAsRequirement { get;set;  }

        private RecipeRow[] RecipesResult { get;set; }

        private RetainerTaskRow[] RetainerTasks { get; set; }

        private MobDrop[] MobDrops { get;set; }

        private List<ItemSource> Sources { get; set; }
        private List<ItemSource> Uses { get; set; }

        private List<CriticalCommonLib.Models.InventoryItem> OwnedItems { get; set; }

        public override string GenericName { get; } = "Item";
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (ImGui.GetWindowPos() != CurrentPosition)
            {
                CurrentPosition = ImGui.GetWindowPos();
            }

            if (Item == null)
            {
                ImGui.TextUnformatted("Item with the ID " + _itemId + " could not be found.");
            }
            else
            {
                ImGui.TextUnformatted("Item Level " + Item.Base.LevelItem.RowId.ToString());
                var description = Item.Base.Description.ExtractText();
                if (description != "")
                {
                    ImGui.PushTextWrapPos();
                    ImGui.TextUnformatted(description);
                    ImGui.PopTextWrapPos();
                }

                if (Item.CanBeAcquired)
                {
                    var hasAcquired = _unlockTrackerService.IsUnlocked(Item);
                    ImGui.TextUnformatted("Acquired:" + (hasAcquired == null ? "Checking" : hasAcquired == true ? "Yes" : "No"));
                }

                if (Item.SellToVendorPrice != 0)
                {
                    ImGui.TextUnformatted("Sell to Vendor: " + Item.SellToVendorPrice + SeIconChar.Gil.ToIconString());
                }

                if (Item.BuyFromVendorPrice != 0 && Item.HasSourcesByType(ItemInfoType.GilShop))
                {
                    ImGui.TextUnformatted("Buy from Vendor: " + Item.BuyFromVendorPrice + SeIconChar.Gil.ToIconString());
                }

                if (Item.BuyFromVendorPrice != 0 && Item.HasSourcesByType(ItemInfoType.CalamitySalvagerShop))
                {
                    ImGui.TextUnformatted("Buy from Calamity Salvager: " + Item.BuyFromVendorPrice + SeIconChar.Gil.ToIconString());
                }
                ImGui.Image(ImGuiService.GetIconTexture(Item.Icon).ImGuiHandle, new Vector2(100, 100) * ImGui.GetIO().FontGlobalScale);
                if (_tooltipModeSetting.CurrentValue(Configuration) != ImGuiTooltipMode.Never)
                {
                    _tooltipService.DrawItemTooltip(new SearchResult(Item));
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                        ImGuiHoveredFlags.AnyWindow))
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && (ImGui.IsMouseReleased(ImGuiMouseButton.Right) || ImGui.IsMouseReleased(ImGuiMouseButton.Left)))
                {
                    ImGui.OpenPopup("RightClick" + _itemId);
                }

                if (ImGui.BeginPopup("RightClick" + _itemId))
                {
                    this.MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(Item));
                    ImGui.EndPopup();
                }

                if (ImGui.ImageButton(ImGuiService.GetImageTexture("garlandtools").ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://www.garlandtools.org/db/#item/{Item.GarlandToolsId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Garland Tools");
                ImGui.SameLine();
                if (ImGui.ImageButton(ImGuiService.GetImageTexture("teamcraft").ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://ffxivteamcraft.com/db/en/item/{_itemId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Teamcraft");

                ImGui.SameLine();
                if (ImGui.ImageButton(ImGuiService.GetImageTexture("gamerescape").ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    var name = Item.NameString.Replace(' ', '_');
                    name = name.Replace('–', '-');

                    if (name.StartsWith("_")) // "level sync" icon
                        name = name.Substring(2);
                    $"https://ffxiv.gamerescape.com/wiki/{HttpUtility.UrlEncode(name)}?useskin=Vector".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Gamer Escape");

                ImGui.SameLine();
                if (ImGui.ImageButton(ImGuiService.GetImageTexture("consolegameswiki").ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    var name = Item.NameString.Replace("#"," ").Replace("  ", " ").Replace(' ', '_');
                    name = name.Replace('–', '-');

                    if (name.StartsWith("_")) // "level sync" icon
                        name = name.Substring(2);
                    $"https://ffxiv.consolegameswiki.com/wiki/{HttpUtility.UrlEncode(name)}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Console Games Wiki");

                if (Item.CanOpenCraftingLog)
                {
                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(66456).ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        var result = _gameInterface.OpenCraftingLog(_itemId);
                        if (!result)
                        {
                            _chatUtilities.PrintError("Could not open the crafting log, you are currently crafting.");
                        }
                    }

                    ImGuiUtil.HoverTooltip("Craftable - Open in Craft Log");
                }
                if (Item.CanBeCrafted)
                {
                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(60858).ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        ImGui.OpenPopup("AddCraftList" + _itemId);
                    }

                    if (ImGui.BeginPopup("AddCraftList" + _itemId))
                    {
                        var craftFilters =
                            _listService.Lists.Where(c =>
                                c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault);
                        foreach (var filter in craftFilters)
                        {
                            using (ImRaii.PushId(filter.Key))
                            {
                                if (ImGui.Selectable("Add item to craft list - " + filter.Name))
                                {
                                    _framework.RunOnFrameworkThread(() =>
                                    {
                                        filter.CraftList.AddCraftItem(_itemId, 1, InventoryItem.ItemFlags.None);
                                        filter.NeedsRefresh = true;
                                        MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                                        MediatorService.Publish(new FocusListMessage(typeof(CraftsWindow), filter));
                                    });
                                }
                            }
                        }
                        ImGui.EndPopup();
                    }

                    ImGuiUtil.HoverTooltip("Craftable - Add to Craft List");
                }
                if (Item.CanBeGathered)
                {
                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(66457).ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        _gameInterface.OpenGatheringLog(_itemId);
                    }

                    ImGuiUtil.HoverTooltip("Gatherable - Open in Gathering Log");

                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(63900).ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        _commandManager.ProcessCommand("/gather " + Item.NameString);
                    }

                    ImGuiUtil.HoverTooltip("Gatherable - Gather with Gatherbuddy");
                }

                if (Item.ObtainedFishing)
                {
                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(66457).ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        _gameInterface.OpenFishingLog(_itemId, Item.ObtainedSpearFishing);
                    }

                    ImGuiUtil.HoverTooltip("Gatherable - Open in Fishing Log");

                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(63900).ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        _commandManager.ProcessCommand("/gatherfish " + Item.NameString);
                    }

                    ImGuiUtil.HoverTooltip("Gatherable - Gather with Gatherbuddy");
                }

                ImGui.Separator();

                DrawSources();

                DrawUses();

                DrawOwned();

                DrawMobDrops();

                DrawVendors();

                DrawIshgardRestoration();

                DrawMarketPricing();

                DrawRetainerTasks();

                DrawGatheringSources();

                DrawRecipes();

                DrawSharedModels();

                DrawCraftRecipe();


#if DEBUG
                if (ImGui.CollapsingHeader("Debug"))
                {
                    ImGui.TextUnformatted("Item ID: " + _itemId);
                    if (ImGui.Button("Copy"))
                    {
                        _clipboardService.CopyToClipboard(_itemId.ToString());
                    }

                    Utils.PrintOutObject(Item, 0, new List<string>());
                }
#endif

            }
        }

        private void DrawSources()
        {
            if (Item == null)
            {
                return;
            }
            if (ImGui.CollapsingHeader("Sources (" + Item.Sources.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var messages = _itemInfoRenderService.DrawItemSourceIcons("Sources", new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, Item.Sources.ToList());
                MediatorService.Publish(messages);
            }
        }

        private bool DrawCraftRecipe()
        {
            bool hasInformation = false;

            if (Item is { CanBeCrafted: true })
            {
                var recipes = Item.Recipes;
                if (_craftTypes == null)
                {
                    var craftTypes = new Dictionary<uint, string>();
                    if (Item.IsCompanyCraft)
                    {
                        craftTypes.Add(0, "All");
                        var companyCraftIndex = 1u;
                        if (Item.CompanyCraftSequence != null)
                        {
                            var craftParts = Item.CompanyCraftSequence.CompanyCraftParts;
                            foreach (var craftPart in craftParts)
                            {
                                if (craftPart.Base.CompanyCraftType.ValueNullable == null) continue;
                                craftTypes.Add(companyCraftIndex,
                                    craftPart.Base.CompanyCraftType.Value.Name.ExtractText());
                                companyCraftIndex++;
                            }
                        }
                    }
                    else
                    {
                        foreach (var recipe in recipes)
                        {
                            craftTypes[recipe.RowId] = recipe.CraftType?.FormattedName ?? "Unknown Craft Type";
                        }
                    }

                    _craftTypes = craftTypes;
                }

                if (_craftTypeId == null && _craftTypes.Count != 0)
                {
                    _craftTypeId = _craftTypes.First().Key;
                }
                else if(_craftTypeId == null)
                {
                    _craftTypeId = 0;
                }

                string headerName = "Recipes - for crafting this item";
                if (ImGui.CollapsingHeader(headerName))
                {
                    if (_craftTypes.Count > 1)
                    {
                        using (var combo = ImRaii.Combo("Craft Types",
                                   _craftTypes.GetValueOrDefault(_craftTypeId.Value, "")))
                        {
                            if (combo)
                            {
                                foreach (var craftType in _craftTypes)
                                {
                                    if (ImGui.Selectable(craftType.Value))
                                    {
                                        _craftTypeId = craftType.Key;
                                        _craftItem = null;
                                    }
                                }
                            }
                        }
                    }

                    if (Item.IsCompanyCraft)
                    {
                        if (_craftItem == null)
                        {
                            var craftList = new CraftList();
                            craftList.AddCraftItem(Item.RowId, 1, InventoryItem.ItemFlags.None,
                                _craftTypeId == 0 ? null : _craftTypeId - 1);
                            craftList.GenerateCraftChildren();
                            _craftItem = craftList.CraftItems.First();
                        }
                    }
                    else
                    {
                        if (_craftItem == null)
                        {
                            var craftList = new CraftList();
                            craftList.AddCraftItem(Item.RowId);
                            if (_craftTypeId != null)
                            {
                                craftList.SetCraftRecipe(Item.RowId, _craftTypeId.Value);
                            }

                            craftList.GenerateCraftChildren();
                            _craftItem = craftList.CraftItems.First();
                        }
                    }

                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var index = 0;
                    foreach (var craftItem in _craftItem.ChildCrafts)
                    {
                        var item = _itemSheet.GetRowOrDefault(craftItem.ItemId);
                        if (item != null)
                        {
                            using (ImRaii.PushId(index))
                            {
                                if (ImGui.ImageButton(ImGuiService.GetIconTexture(item.Icon).ImGuiHandle, new(32, 32)))
                                {
                                    MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.RowId));
                                }

                                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                        ImGuiHoveredFlags.AnyWindow) &&
                                    ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                {
                                    ImGui.OpenPopup("RightClick" + item.RowId);
                                }

                                if (ImGui.BeginPopup("RightClick" + item.RowId))
                                {
                                    MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(item));
                                    ImGui.EndPopup();
                                }

                                float lastButtonX2 = ImGui.GetItemRectMax().X;
                                float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                                ImGuiUtil.HoverTooltip(item.NameString + " - " + craftItem.QuantityRequired);
                                if (index + 1 < _craftItem.ChildCrafts.Count && nextButtonX2 < windowVisibleX2)
                                {
                                    ImGui.SameLine();
                                }
                            }

                            index++;
                        }
                    }
                }


            }

            return hasInformation;
        }


        private bool DrawSharedModels()
        {
            bool hasInformation = false;
            if (SharedModels.Count != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("Shared Models (" + SharedModels.Count + ")"))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    for (var index = 0; index < SharedModels.Count; index++)
                    {
                        using (ImRaii.PushId(index))
                        {
                            var sharedModel = SharedModels[index];
                            if (ImGui.ImageButton(ImGuiService.GetIconTexture(sharedModel.Icon).ImGuiHandle,
                                    new(32, 32)))
                            {
                                MediatorService.Publish(
                                    new OpenUintWindowMessage(typeof(ItemWindow), sharedModel.RowId));
                            }

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                    ImGuiHoveredFlags.AllowWhenOverlapped &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                    ImGuiHoveredFlags.AnyWindow) &&
                                ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup("RightClick" + sharedModel.RowId);
                            }

                            if (ImGui.BeginPopup("RightClick" + sharedModel.RowId))
                            {
                                MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(sharedModel));
                                ImGui.EndPopup();
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(sharedModel.NameString);
                            if (index + 1 < SharedModels.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }
                        }
                    }
                }
            }

            return hasInformation;
        }

        private bool DrawRecipes()
        {
            bool hasInformation = false;
            if (RecipesAsRequirement.Length != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("Recipes - Item is a requirement (" + RecipesAsRequirement.Length + ")"))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    for (var index = 0; index < RecipesAsRequirement.Length; index++)
                    {
                        using (ImRaii.PushId(index))
                        {
                            var recipe = RecipesAsRequirement[index];
                            if (recipe.ItemResult != null)
                            {
                                var icon = ImGuiService.GetIconTexture(recipe.ItemResult.Icon);
                                if (ImGui.ImageButton(icon.ImGuiHandle,
                                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                                {
                                    MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow),
                                        recipe.ItemResult.RowId));
                                }

                                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                        ImGuiHoveredFlags.AnyWindow) &&
                                    ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                {
                                    ImGui.OpenPopup("RightClick" + recipe.RowId);
                                }

                                if (ImGui.BeginPopup("RightClick" + recipe.RowId))
                                {
                                    if (recipe.ItemResult != null)
                                    {
                                        MediatorService.Publish(
                                            ImGuiService.ImGuiMenuService.DrawRightClickPopup(recipe.ItemResult));
                                    }

                                    ImGui.EndPopup();
                                }

                                float lastButtonX2 = ImGui.GetItemRectMax().X;
                                float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                                ImGuiUtil.HoverTooltip(recipe.ItemResult!.NameString + " - " +
                                                       (recipe.CraftType?.FormattedName ?? "Unknown"));
                                if (index + 1 < RecipesAsRequirement.Length && nextButtonX2 < windowVisibleX2)
                                {
                                    ImGui.SameLine();
                                }
                            }
                        }
                    }
                }
            }

            return hasInformation;
        }

        private bool DrawGatheringSources()
        {
            var hasInformation = false;
            if (GatheringSources.Count != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("Gathering (" + GatheringSources.Count + ")"))
                {
                    ImGuiTable.DrawTable("Gathering", GatheringSources, DrawGatheringRow,
                        ImGuiTableFlags.None, new[] { "", "Level", "Location", "" });
                }
            }

            return hasInformation;
        }

        private bool DrawRetainerTasks()
        {
            bool hasInformation = false;
            if (RetainerTasks.Length != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("Ventures (" + RetainerTasks.Count() + ")"))
                {
                    ImGuiTable.DrawTable("Ventures", RetainerTasks, DrawRetainerRow, ImGuiTableFlags.SizingStretchProp,
                        new[] { "Name", "Time", "Quantities" });
                }
            }

            return hasInformation;
        }

        private bool DrawVendors()
        {
            bool hasInformation = false;
            if (Vendors.Count != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("Shops (" + Vendors.Count + ")"))
                {
                    ImGui.TextUnformatted("Shops: ");
                    ImGuiTable.DrawTable("VendorsText", Vendors, DrawSupplierRow, ImGuiTableFlags.None,
                        new[] { "Shop Name","NPC", "Location", "" });
                }
            }

            return hasInformation;
        }

        private void DrawOwned()
        {
            if (ImGui.CollapsingHeader("Owned (" + OwnedItems.Count + ")"))
            {
                ImGuiTable.DrawTable("OwnedItems", OwnedItems, DrawOwnedItem, ImGuiTableFlags.None,
                    new[] { "Character","Location", "Qty", "Is HQ?" });
            }
        }

        private void DrawOwnedItem(CriticalCommonLib.Models.InventoryItem obj)
        {
            ImGui.TableNextColumn();
            ImGui.TextWrapped(_characterMonitor.GetCharacterNameById(obj.RetainerId));
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.FormattedBagLocation);
            if (obj.SortedCategory == InventoryCategory.GlamourChest && obj.GlamourId != 0)
            {
                ImGui.SameLine();
                ImGui.Image(this.ImGuiService.GetIconTexture(Icons.MannequinIcon).ImGuiHandle, new Vector2(16,16));
                if (ImGui.IsItemHovered())
                {
                    using (var tooltip = ImRaii.Tooltip())
                    {
                        if (tooltip)
                        {
                            ImGui.TextUnformatted("This item has been combined into a single glamour ready item.");
                        }
                    }
                }
            }
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.Quantity.ToString());
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.IsHQ ? "Yes" : "No");
        }


        void DrawSupplierRow((IShop shop, ENpcResidentRow? npc, ILocation? location) tuple)
        {
            ImGui.TableNextColumn();
            ImGui.TextWrapped(tuple.shop.Name);
            if (tuple.npc != null)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped(tuple.npc.Base.Singular.ExtractText());
            }
            if (tuple.npc != null && tuple.location != null)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped(tuple.location + " ( " + Math.Round(tuple.location.MapX, 2) + "/" +
                                  Math.Round(tuple.location.MapY, 2) + ")");
                ImGui.TableNextColumn();
                if (ImGui.Button("Teleport##t" + tuple.shop.RowId + "_" + tuple.npc.RowId + "_" +
                                 tuple.location.Map.RowId))
                {
                    var nearestAetheryte = tuple.location.GetNearestAetheryte();
                    if (nearestAetheryte != null)
                    {
                        MediatorService.Publish(new RequestTeleportMessage(nearestAetheryte.Value.RowId));
                    }
                    _chatUtilities.PrintFullMapLink(tuple.location, Item?.NameString ?? "");
                }
                if (ImGui.Button("Map Link##ml" + tuple.shop.RowId + "_" + tuple.npc.RowId + "_" +
                                 tuple.location.Map.RowId))
                {
                    _chatUtilities.PrintFullMapLink(tuple.location, Item?.NameString ?? "");
                }
            }
            else if (tuple.npc is { ENpcBase.IsHouseVendor: true })
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped("Housing Vendor");
                ImGuiService.HelpMarker("This is a vendor that can be placed inside your house/apartment.");
                ImGui.TableNextColumn();
            }
            else
            {
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
            }

        }

        private void DrawMarketPricing()
        {
            if (Item is { CanBePlacedOnMarket: true })
            {
                var prePosition = ImGui.GetCursorPos();
                if (ImGui.CollapsingHeader("Market Pricing",
                        ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if(_marketRefreshing.Count != 0)
                    {
                        var postPosition = ImGui.GetCursorPos();
                        prePosition.X = ImGui.GetWindowWidth() - 20;
                        prePosition.Y = prePosition.Y + 6;
                        ImGui.SetCursorPos(prePosition);
                        float nextDot = 3.0f;
                        if (_marketRefreshing.Count != 0)
                        {
                            ImGuiService.SpinnerDots("hai", ref nextDot, 7, 1);
                        }

                        ImGui.SetCursorPos(postPosition);
                    }


                    var selected = 0;
                    if (_picker.Draw("Worlds", "", "", ref selected, 100, 20, ImGuiComboFlags.None))
                    {
                        var world = _picker.Items[selected];
                        _picker.SelectedWorldIds.Add(world.RowId);
                        RequestMarketPrices();
                    }

                    if (_picker.SelectedWorldIds.Count != 0)
                    {
                        ImGui.SameLine();
                    }

                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX = ImGui.GetWindowContentRegionMax().X - style.ScrollbarSize;
                    float X = ImGui.GetCursorPosX();

                    var count = 0;

                    foreach (var selectedWorldId in _picker.SelectedWorldIds)
                    {
                        var selectedWorld = _worldSheet.GetRowOrDefault((uint)selectedWorldId);
                        if (selectedWorld != null)
                        {
                            var selectedWorldFormattedName = selectedWorld.Value.Name.ExtractText() + " X";
                            var itemWidth = ImGui.CalcTextSize(selectedWorldFormattedName).X  + (2 * ImGui.GetStyle().FramePadding.X) + 5;
                            if (windowVisibleX > X + itemWidth)
                            {
                                if (count != 0)
                                {
                                    ImGui.SameLine();
                                }

                                X += itemWidth;
                            }
                            else
                            {
                                X = itemWidth;
                                ImGui.NewLine();
                            }

                            count++;

                            if (ImGui.Button(selectedWorldFormattedName))
                            {
                                _picker.SelectedWorldIds.Remove(selectedWorldId);
                            }
                        }
                    }

                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 22 - ImGui.GetStyle().FramePadding.X);
                    if (_refreshPricesButton.Draw(ImGuiService.GetImageTexture("refresh-web").ImGuiHandle, "refreshPrices"))
                    {
                        RequestMarketPrices();
                    }
                    ImGuiUtil.HoverTooltip("Refresh the current prices.");
                    ImGuiTable.DrawTable("MarketPrices", _marketPrices, DrawMarketRow, ImGuiTableFlags.None,
                        new[] { "Server","Updated At", "Available", "Min. Price" });
                }
            }

            void DrawMarketRow(MarketPricing obj)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped(obj.World.Value.Name.ExtractText() ?? "Unknown");
                ImGui.TableNextColumn();
                ImGui.TextWrapped((obj.LastUpdate - DateTime.Now).Humanize(minUnit: TimeUnit.Minute, maxUnit: TimeUnit.Hour, precision: 1) + " ago");
                ImGui.TableNextColumn();
                ImGui.TextWrapped(obj.Available.ToString());
                ImGui.TableNextColumn();
                ImGui.TextWrapped(obj.MinPriceNq.ToString("N0", CultureInfo.InvariantCulture) + SeIconChar.Gil.ToIconString() + "/" + obj.MinPriceHq.ToString("N0", CultureInfo.InvariantCulture) + SeIconChar.Gil.ToIconString());
            }
        }

        private void DrawIshgardRestoration()
        {
            if (Item?.HasUsesByType(ItemInfoType.SkybuilderHandIn) ?? false)
            {
                var skybuilderHandIn = Item.GetUsesByType<ItemSkybuilderHandInSource>(ItemInfoType.SkybuilderHandIn).First();
                if (ImGui.CollapsingHeader("Ishgard Restoration", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var supplyItem = skybuilderHandIn.HWDCrafterSupplyParams;
                    using (var table = ImRaii.Table("SupplyItems", 4 ,ImGuiTableFlags.None))
                    {
                        if (table.Success)
                        {
                            ImGui.TableNextColumn();
                            ImGui.TableHeader("Level");
                            ImGui.TableNextColumn();
                            ImGui.TableHeader("Collectable Rating");
                            ImGui.TableNextColumn();
                            ImGui.TableHeader("XP");
                            ImGui.TableNextColumn();
                            ImGui.TableHeader("Scrip");

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped("Base");
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped(supplyItem.BaseCollectableRating.ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.BaseCollectableReward.ValueNullable?.ExpReward ?? 0).ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.BaseCollectableReward.ValueNullable?.ScriptRewardAmount ?? 0)
                                .ToString());

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped("Mid");
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped(supplyItem.MidCollectableRating.ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.MidCollectableReward.ValueNullable?.ExpReward ?? 0).ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.MidCollectableReward.ValueNullable?.ScriptRewardAmount ?? 0)
                                .ToString());

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped("High");
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped(supplyItem.HighCollectableRating.ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.HighCollectableReward.ValueNullable?.ExpReward ?? 0).ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.HighCollectableReward.ValueNullable?.ScriptRewardAmount ?? 0)
                                .ToString());
                        }
                    }
                }
            }
        }

        private void DrawMobDrops()
        {
            if (MobDrops.Length != 0)
            {
                if (ImGui.CollapsingHeader("Mob Drops (" + MobDrops.Length + ")", ImGuiTreeNodeFlags.CollapsingHeader))
                {
                     var mobDrops = MobDrops;
                     for (var index = 0; index < mobDrops.Length; index++)
                     {
                         var mobDrop = mobDrops[index];
                         var bnpcName = _bNpcNameSheet.GetRowOrDefault(mobDrop.BNpcNameId);
                         if (bnpcName != null)
                         {
                             var mobSpawns = bnpcName.MobSpawnPositions.GroupBy(c => c.TerritoryType.RowId).ToList();
                             if (mobSpawns.Count != 0)
                             {
                                 using (ImRaii.PushId("MobDrop" + index))
                                 {
                                     if (ImGui.CollapsingHeader("  " +
                                                                bnpcName.Base.Singular.ExtractText() + "(" +
                                                                mobSpawns.Count + ")",
                                             ImGuiTreeNodeFlags.CollapsingHeader))
                                     {
                                         ImGuiTable.DrawTable("MobSpawns" + index, mobSpawns, DrawMobSpawn,
                                             ImGuiTableFlags.None,
                                             new[] { "Map", "Spawn Locations" });
                                     }
                                 }
                             }
                         }
                     }
                }
            }
        }

        private void DrawUses()
        {
            if (Item == null)
            {
                return;
            }

            if (ImGui.CollapsingHeader("Uses (" + Item.Uses.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var messages = _itemInfoRenderService.DrawItemUseIcons("Uses", new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, Item.Uses.ToList());
                MediatorService.Publish(messages);
            }
        }

        private void DrawMobSpawn(IGrouping<uint, MobSpawnPosition> mobSpawnPositions)
        {
            ImGui.TableNextColumn();
            var territoryType = mobSpawnPositions.First().TerritoryType.Value;
            ImGui.TextUnformatted(territoryType.PlaceName.Value.Name.ExtractText());
            ImGui.TableNextColumn();

            using (var locationScrollChild = ImRaii.Child(territoryType.RowId + "LocationScroll",
                       new Vector2(ImGui.GetColumnWidth() * ImGui.GetIO().FontGlobalScale,
                           32 + ImGui.GetStyle().CellPadding.Y) * ImGui.GetIO().FontGlobalScale, false))
            {
                if (locationScrollChild.Success)
                {
                    var columnWidth = ImGui.GetColumnWidth() - ImGui.GetStyle().ItemSpacing.X;
                    var itemWidth = (32 + ImGui.GetStyle().ItemSpacing.X) * ImGui.GetIO().FontGlobalScale;
                    var maxItems = itemWidth != 0 ? (int)Math.Floor(columnWidth / itemWidth) : 0;
                    maxItems = maxItems == 0 ? 1 : maxItems;
                    maxItems--;
                    var count = 0;
                    for (var index = 0; index < mobSpawnPositions.ToList().Count; index++)
                    {
                        var position = mobSpawnPositions.ToList()[index];
                        var territory = position.TerritoryType;
                        if (territory.ValueNullable?.PlaceName.ValueNullable != null)
                        {
                            using (ImRaii.PushId(index))
                            {
                                if (ImGui.ImageButton(ImGuiService.GetIconTexture(60561).ImGuiHandle,
                                        new Vector2(32 * ImGui.GetIO().FontGlobalScale,
                                            32 * ImGui.GetIO().FontGlobalScale),
                                        new Vector2(0, 0), new Vector2(1, 1), 0))
                                {
                                    _chatUtilities.PrintFullMapLink(position,
                                        position.TerritoryType.Value.PlaceName.Value.Name.ExtractText());
                                }

                                if (ImGui.IsItemHovered())
                                {
                                    using var tt = ImRaii.Tooltip();
                                    ImGui.TextUnformatted(
                                        position.TerritoryType.Value.PlaceName.Value.Name.ExtractText());
                                }

                                if ((count + 1) % maxItems != 0)
                                {
                                    ImGui.SameLine();
                                }
                            }
                        }

                        count++;
                    }
                }
            }
        }

        private void DrawGatheringRow(ItemGatheringSource obj)
        {
            ImGui.TableNextColumn();
            using (ImRaii.PushId(obj.GetHashCode()))
            {
                var source = obj.Item;
                if (ImGui.ImageButton(ImGuiService.GetIconTexture(source.Icon).ImGuiHandle, new(32, 32)))
                {
                    _gameInterface.OpenGatheringLog(_itemId);
                }

                ImGuiUtil.HoverTooltip(source.NameString + " - Open in Gathering Log");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(obj.GatheringItem.Base.GatheringItemLevel.RowId.ToString());
                ImGui.TableNextColumn();
                if (obj.MapIds != null)
                {
                    foreach (var location in obj.MapIds)
                    {
                        var map = _mapSheet.GetRowOrDefault(location);
                        if (map != null)
                        {
                            ImGui.TextWrapped(map.FormattedName);
                        }
                    }
                }
            }
        }

        private void DrawRetainerRow(RetainerTaskRow obj)
        {
            ImGui.TableNextColumn();
            ImGui.TextWrapped( obj.FormattedName);
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(obj.DurationString);
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.Quantities);
        }

        public override void Invalidate()
        {

        }

        public override FilterConfiguration? SelectedConfiguration => null;
        public override Vector2? DefaultSize { get; } = new Vector2(500, 800);
        public override Vector2? MaxSize => new (800, 1500);
        public override Vector2? MinSize => new (100, 100);

        public override bool SavePosition => true;

        public override string GenericKey => "item";

    }
}