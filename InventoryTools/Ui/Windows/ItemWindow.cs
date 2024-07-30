using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Web;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models.ItemSources;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Game.Text;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using Lumina.Excel.GeneratedSheets;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Humanizer;
using Humanizer.Localisation;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;
using OtterGui.Log;
using OtterGui.Widgets;
using ImGuiUtil = OtterGui.ImGuiUtil;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Ui
{
    public class WorldPicker : FilterComboBase<WorldEx>
    {
        public HashSet<uint> SelectedWorldIds { get; set; } = new();
        public WorldPicker(IReadOnlyList<WorldEx> items, bool keepStorage, Logger log) : base(items, keepStorage, log)
        {
            
        }

        protected override string ToString(WorldEx obj)
        {
            return obj.FormattedName;
        }
    }
    public class ItemWindow : UintWindow
    {
        private readonly IMarketBoardService _marketBoardService;
        private readonly IFramework _framework;
        private readonly ICommandManager _commandManager;
        private readonly IListService _listService;
        private readonly ExcelCache _excelCache;
        private readonly IGameInterface _gameInterface;
        private readonly IMarketCache _marketCache;
        private readonly IChatUtilities _chatUtilities;
        private readonly Logger _otterLogger;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly ICharacterMonitor _characterMonitor;
        private HashSet<uint> _marketRefreshing = new();
        private HoverButton _refreshPricesButton = new();

        public ItemWindow(ILogger<ItemWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, IMarketBoardService marketBoardService, IFramework framework, ICommandManager commandManager, IListService listService, ExcelCache excelCache, IGameInterface gameInterface, IMarketCache marketCache, IChatUtilities chatUtilities, Logger otterLogger, IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor, string name = "Item Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _marketBoardService = marketBoardService;
            _framework = framework;
            _commandManager = commandManager;
            _listService = listService;
            _excelCache = excelCache;
            _gameInterface = gameInterface;
            _marketCache = marketCache;
            _chatUtilities = chatUtilities;
            _otterLogger = otterLogger;
            _inventoryMonitor = inventoryMonitor;
            _characterMonitor = characterMonitor;
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
            var worlds = _excelCache.GetWorldSheet().Where(c => c.IsPublic).ToList();
            _picker = new WorldPicker(worlds, true, _otterLogger);
            MediatorService.Subscribe<MarketCacheUpdatedMessage>(this, MarketCacheUpdated);
            if (Item != null)
            {
                WindowName = "Allagan Tools - " + Item.NameString;
                Key = "item_" + itemId;
                RetainerTasks = Item.RetainerTasks?.ToArray() ?? Array.Empty<RetainerTaskEx>();
                RecipesResult = Item.RecipesAsResult.ToArray();
                RecipesAsRequirement = Item.RecipesAsRequirement.ToArray();
                Vendors = new List<(IShop shop, ENpc? npc, ILocation? location)>();
                foreach (var vendor in Item.Vendors)
                {
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
                            if ((_excelCache.GetHouseVendor(npc.Key)?.ParentId ?? 0) != 0) continue;
                            if (!npc.Locations.Any())
                            {
                                Vendors.Add(new (vendor, npc, null));
                            }
                            else
                            {
                                foreach (var location in npc.Locations)
                                {
                                    Vendors.Add(new (vendor, npc, location));
                                }
                            }
                        }
                    }
                }
                Vendors = Vendors.OrderByDescending(c => c.npc != null && c.location != null).ToList();
                GatheringSources = Item.GetGatheringSources().ToList();
                SharedModels = Item.GetSharedModels();
                MobDrops = Item.MobDrops.ToArray();
                OwnedItems = _inventoryMonitor.AllItems.Where(c => c.ItemId == itemId).ToList();
                if (Configuration.AutomaticallyDownloadMarketPrices)
                {
                    RequestMarketPrices(false);
                }
                GetMarketPrices();
            }
            else
            {
                RetainerTasks = Array.Empty<RetainerTaskEx>();
                RecipesResult = Array.Empty<RecipeEx>();
                RecipesAsRequirement = Array.Empty<RecipeEx>();
                GatheringSources = new();
                Vendors = new();
                SharedModels = new();
                MobDrops = Array.Empty<MobDropEx>();
                OwnedItems = new List<CriticalCommonLib.Models.InventoryItem>();
                WindowName = "Invalid Item";
                Key = "item_unknown";
            }
        }
        
        public override bool SaveState => false;
        private uint _itemId;
        private ItemEx? Item => _excelCache.GetItemExSheet().GetRow(_itemId);
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
        public List<ItemEx> SharedModels { get;set; }

        private List<GatheringSource> GatheringSources { get;set; }

        private List<(IShop shop, ENpc? npc, ILocation? location)> Vendors { get;set; }

        private RecipeEx[] RecipesAsRequirement { get;set;  }

        private RecipeEx[] RecipesResult { get;set; }

        private RetainerTaskEx[] RetainerTasks { get; set; }
        
        private MobDropEx[] MobDrops { get;set; }
        
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
                ImGui.TextUnformatted("Item Level " + Item.LevelItem.Row.ToString());
                if (Item.DescriptionString != "")
                {
                    ImGui.PushTextWrapPos();
                    ImGui.TextUnformatted(Item.DescriptionString);
                    ImGui.PopTextWrapPos();
                }

                if (Item.CanBeAcquired)
                {
                    ImGui.TextUnformatted("Acquired:" + (_gameInterface.HasAcquired(Item) ? "Yes" : "No"));
                }

                if (Item.SellToVendorPrice != 0)
                {
                    ImGui.TextUnformatted("Sell to Vendor: " + Item.SellToVendorPrice + SeIconChar.Gil.ToIconString());
                }

                if (Item.BuyFromVendorPrice != 0 && Item.ObtainedGil)
                {
                    ImGui.TextUnformatted("Buy from Vendor: " + Item.BuyFromVendorPrice + SeIconChar.Gil.ToIconString());
                }
                ImGui.Image(ImGuiService.GetIconTexture(Item.Icon).ImGuiHandle, new Vector2(100, 100) * ImGui.GetIO().FontGlobalScale);
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
                    this.MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(Item));
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
                
                if (Item.CanOpenCraftLog)
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
                if (Item.CanOpenGatheringLog)
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
                        _gameInterface.OpenFishingLog(_itemId, Item.IsSpearfishingItem());
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
                        ImGui.SetClipboardText(_itemId.ToString());
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
                ImGuiStylePtr style = ImGui.GetStyle();
                float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                var sources = Item.Sources;
                for (var index = 0; index < sources.Count; index++)
                {
                    ImGui.PushID("Source"+index);
                    var source = sources[index];
                    var sourceIcon = ImGuiService.GetIconTexture(source.Icon);
                    if (source.CanOpen)
                    {
                        if (source is ItemSource itemSource && itemSource.ItemId != null )
                        {
                            if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1),
                                    0))
                            {
                                _framework.RunOnFrameworkThread(() =>
                                {
                                    MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), itemSource.ItemId.Value));
                                });
                            }
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                    ImGuiHoveredFlags.AllowWhenOverlapped &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                    ImGuiHoveredFlags.AnyWindow) &&
                                ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup("RightClickSource" + itemSource.ItemId);
                            }
                            if (ImGui.BeginPopup("RightClickSource" + itemSource.ItemId))
                            {
                                ImGui.OpenPopup("RightClickSource" + itemSource.ItemId);
                                var itemEx = _excelCache.GetItemExSheet()
                                    .GetRow(itemSource.ItemId.Value);
                                if (itemEx != null)
                                {
                                    MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(itemEx));
                                }
                                ImGui.EndPopup();
                            }
                        }
                        else if (source is DutySource dutySource)
                        {
                            if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1),
                                    0))
                            {
                                MediatorService.Publish(new OpenUintWindowMessage(typeof(DutyWindow), dutySource.ContentFinderConditionId));
                            }
                        }
                        else if (source is AirshipSource airshipSource)
                        {
                            if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1),
                                    0))
                            {
                                MediatorService.Publish(new OpenUintWindowMessage(typeof(AirshipWindow), airshipSource.AirshipExplorationPointExId));
                            }
                        }
                        else if (source is SubmarineSource submarineSource)
                        {
                            if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1),
                                    0))
                            {
                                MediatorService.Publish(new OpenUintWindowMessage(typeof(SubmarineWindow), submarineSource.SubmarineExplorationExId));
                            }
                        }
                        else if (source is VentureSource ventureSource)
                        {
                            if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1),
                                    0))
                            {
                                MediatorService.Publish(new OpenUintWindowMessage(typeof(RetainerTaskWindow), ventureSource.RetainerTask.RowId));
                            }
                        }
                        else
                        {
                            ImGui.Image(sourceIcon.ImGuiHandle,
                                new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale);
                        }
                    }
                    else
                    {
                        ImGui.Image(sourceIcon.ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale);
                    }

                    float lastButtonX2 = ImGui.GetItemRectMax().X;
                    float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                    ImGuiUtil.HoverTooltip(source.FormattedName);
                    if (index + 1 < sources.Count && nextButtonX2 < windowVisibleX2)
                    {
                        ImGui.SameLine();
                    }

                    ImGui.PopID();
                }
            }
        }

        private bool DrawCraftRecipe()
        {
            bool hasInformation = false;
            
            if (Item is { CanBeCrafted: true })
            {
                var recipes = Item.RecipesAsResult;
                if (_craftTypes == null)
                {
                    var craftTypes = new Dictionary<uint, string>();
                    if (Item.IsCompanyCraft)
                    {
                        craftTypes.Add(0, "All");
                        var companyCraftIndex = 1u;
                        if (Item.CompanyCraftSequenceEx != null)
                        {
                            var craftParts = Item.CompanyCraftSequenceEx.ActiveCompanyCraftParts;
                            foreach (var craftPart in craftParts)
                            {
                                if (craftPart.Value?.CompanyCraftType.Value == null) continue;
                                craftTypes.Add(companyCraftIndex,
                                    craftPart.Value.CompanyCraftType.Value.Name.AsReadOnly().ToString());
                                companyCraftIndex++;
                            }
                        }
                    }
                    else
                    {
                        foreach (var recipe in recipes)
                        {
                            craftTypes[recipe.RowId] = recipe.CraftTypeEx.Value?.FormattedName ?? "Unknown Craft Type";
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
                        var item = _excelCache.GetItemExSheet().GetRow(craftItem.ItemId);
                        if (item != null)
                        {
                            ImGui.PushID(index);
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
                                MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(item));
                                ImGui.EndPopup();
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(item.NameString + " - " + craftItem.QuantityRequired);
                            if (index + 1 < _craftItem.ChildCrafts.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }

                            ImGui.PopID();
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
                        ImGui.PushID(index);
                        var sharedModel = SharedModels[index];
                        if (ImGui.ImageButton(ImGuiService.GetIconTexture(sharedModel.Icon).ImGuiHandle, new(32, 32)))
                        {
                            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), sharedModel.RowId));
                        }
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right)) 
                        {
                            ImGui.OpenPopup("RightClick" + sharedModel.RowId);
                        }
                
                        if (ImGui.BeginPopup("RightClick"+ sharedModel.RowId))
                        {
                            MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(sharedModel));
                            ImGui.EndPopup();
                        }

                        float lastButtonX2 = ImGui.GetItemRectMax().X;
                        float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                        ImGuiUtil.HoverTooltip(sharedModel.NameString);
                        if (index + 1 < SharedModels.Count && nextButtonX2 < windowVisibleX2)
                        {
                            ImGui.SameLine();
                        }

                        ImGui.PopID();
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
                        ImGui.PushID(index);
                        var recipe = RecipesAsRequirement[index];
                        if (recipe.ItemResultEx.Value != null)
                        {
                            var icon = ImGuiService.GetIconTexture(recipe.ItemResultEx.Value.Icon);
                            if (ImGui.ImageButton(icon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                            {
                                MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), recipe.ItemResultEx.Row));
                            }
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right)) 
                            {
                                ImGui.OpenPopup("RightClick" + recipe.RowId);
                            }
                    
                            if (ImGui.BeginPopup("RightClick"+ recipe.RowId))
                            {
                                if (recipe.ItemResultEx.Value != null)
                                {
                                    MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(recipe.ItemResultEx.Value));
                                }

                                ImGui.EndPopup();
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(recipe.ItemResultEx.Value!.NameString + " - " +
                                                   (recipe.CraftType.Value?.Name ?? "Unknown"));
                            if (index + 1 < RecipesAsRequirement.Length && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }
                        }

                        ImGui.PopID();
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
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.Quantity.ToString());
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.IsHQ ? "Yes" : "No");
        }


        void DrawSupplierRow((IShop shop, ENpc? npc, ILocation? location) tuple)
        {
            ImGui.TableNextColumn();
            ImGui.TextWrapped(tuple.shop.Name);
            if (tuple.npc != null)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped(tuple.npc?.Resident?.Singular ?? "");
            }
            if (tuple.npc != null && tuple.location != null)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped(tuple.location + " ( " + Math.Round(tuple.location.MapX, 2) + "/" +
                                  Math.Round(tuple.location.MapY, 2) + ")");
                ImGui.TableNextColumn();
                if (ImGui.Button("Teleport##t" + tuple.shop.RowId + "_" + tuple.npc.Key + "_" +
                                 tuple.location.MapEx.Row))
                {
                    var nearestAetheryte = tuple.location.GetNearestAetheryte();
                    if (nearestAetheryte != null)
                    {
                        MediatorService.Publish(new RequestTeleportMessage(nearestAetheryte.RowId));
                    }
                    _chatUtilities.PrintFullMapLink(tuple.location, Item?.NameString ?? "");
                }
                if (ImGui.Button("Map Link##ml" + tuple.shop.RowId + "_" + tuple.npc.Key + "_" +
                                 tuple.location.MapEx.Row))
                {
                    _chatUtilities.PrintFullMapLink(tuple.location, Item?.NameString ?? "");
                }
            }
            else if (tuple.npc != null && tuple.npc.IsHouseVendor)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped("Housing Vendor");
                ImGuiUtil.LabeledHelpMarker("", "This is a vendor that can be placed inside your house/apartment.");
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
                        var selectedWorld = _excelCache.GetWorldSheet().GetRow((uint)selectedWorldId);
                        if (selectedWorld != null)
                        {
                            var selectedWorldFormattedName = selectedWorld.FormattedName + " X";
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
                ImGui.TextWrapped(obj.World.Value?.FormattedName ?? "Unknown");
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
            if (Item is { IsIshgardCraft: true })
            {
                if (ImGui.CollapsingHeader("Ishgard Restoration", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var crafterSupplyEx = Item.GetHwdCrafterSupply();
                    if (crafterSupplyEx != null)
                    {
                        var supplyItem = crafterSupplyEx.GetSupplyItem(_itemId);
                        if (supplyItem != null)
                        {
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
                                    ImGui.TextWrapped((supplyItem.BaseReward.Value?.ExpReward ?? 0).ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.TextWrapped((supplyItem.BaseReward.Value?.ScriptRewardAmount ?? 0)
                                        .ToString());

                                    ImGui.TableNextRow();
                                    ImGui.TableNextColumn();
                                    ImGui.TextWrapped("Mid");
                                    ImGui.TableNextColumn();
                                    ImGui.TextWrapped(supplyItem.MidCollectableRating.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.TextWrapped((supplyItem.MidReward.Value?.ExpReward ?? 0).ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.TextWrapped((supplyItem.MidReward.Value?.ScriptRewardAmount ?? 0)
                                        .ToString());

                                    ImGui.TableNextRow();
                                    ImGui.TableNextColumn();
                                    ImGui.TextWrapped("High");
                                    ImGui.TableNextColumn();
                                    ImGui.TextWrapped(supplyItem.HighCollectableRating.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.TextWrapped((supplyItem.HighReward.Value?.ExpReward ?? 0).ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.TextWrapped((supplyItem.HighReward.Value?.ScriptRewardAmount ?? 0)
                                        .ToString());
                                }
                            }
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
                        if (mobDrop.BNpcNameEx.Value != null)
                        {
                            var mobSpawns = mobDrops[index].GroupedMobSpawns;
                            if (mobSpawns.Count != 0)
                            {
                                ImGui.PushID("MobDrop" + index);
                                if (ImGui.CollapsingHeader("  " +
                                                           mobDrop.BNpcNameEx.Value.FormattedName + "(" + mobSpawns.Count + ")",ImGuiTreeNodeFlags.CollapsingHeader))
                                {
                                    ImGuiTable.DrawTable("MobSpawns" + index, mobSpawns, DrawMobSpawn,
                                        ImGuiTableFlags.None,
                                        new[] { "Map", "Spawn Locations" });
                                }

                                ImGui.PopID();
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
            
            if (ImGui.CollapsingHeader("Uses/Rewards (" + Item.Uses.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                ImGuiStylePtr style = ImGui.GetStyle();
                float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                var uses = Item.Uses;
                for (var index = 0; index < uses.Count; index++)
                {
                    ImGui.PushID("Use"+index);
                    var use = uses[index];
                    var useIcon = ImGuiService.GetIconTexture(use.Icon);
                    if (use.CanOpen)
                    {
                        if (use is ItemSource itemSource && itemSource.ItemId != null)
                        {
                            if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1),
                                    0))
                            {
                                MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), itemSource.ItemId.Value));
                            }

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                    ImGuiHoveredFlags.AllowWhenOverlapped &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                    ImGuiHoveredFlags.AnyWindow) &&
                                ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup("RightClickUse" + itemSource.ItemId);
                            }

                            if (ImGui.BeginPopup("RightClickUse" + itemSource.ItemId))
                            {
                                var itemEx = _excelCache.GetItemExSheet().GetRow(itemSource.ItemId.Value);
                                if (itemEx != null)
                                {
                                    MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(itemEx));
                                }

                                ImGui.EndPopup();
                            }
                        }
                        else
                        {
                            ImGui.Image(useIcon.ImGuiHandle,
                                new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale);
                        }
                    }
                    else
                    {
                        ImGui.Image(useIcon.ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale);
                    }

                    float lastButtonX2 = ImGui.GetItemRectMax().X;
                    float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                    ImGuiUtil.HoverTooltip(use.FormattedName);
                    if (index + 1 < uses.Count && nextButtonX2 < windowVisibleX2)
                    {
                        ImGui.SameLine();
                    }

                    ImGui.PopID();
                }
            }
        }

        private void DrawMobSpawn(KeyValuePair<TerritoryType, List<MobSpawnPositionEx>> spawnGroup)
        {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(spawnGroup.Key.PlaceName.Value?.Name ?? "Unknown");
            
            ImGui.TableNextColumn();

            using (var locationScrollChild = ImRaii.Child(spawnGroup.Key.RowId + "LocationScroll",
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
                    foreach (var position in spawnGroup.Value)
                    {
                        var territory = position.TerritoryTypeEx;
                        if (territory.Value?.PlaceName.Value != null)
                        {
                            ImGui.PushID("" + position.FormattedId);
                            if (ImGui.ImageButton(ImGuiService.GetIconTexture(60561).ImGuiHandle,
                                    new Vector2(32 * ImGui.GetIO().FontGlobalScale, 32 * ImGui.GetIO().FontGlobalScale),
                                    new Vector2(0, 0), new Vector2(1, 1), 0))
                            {
                                _chatUtilities.PrintFullMapLink(position, position.FormattedName);
                            }

                            if (ImGui.IsItemHovered())
                            {
                                using var tt = ImRaii.Tooltip();
                                ImGui.TextUnformatted(position.FormattedName);
                            }

                            if ((count + 1) % maxItems != 0)
                            {
                                ImGui.SameLine();
                            }

                            ImGui.PopID();
                        }

                        count++;
                    }
                }
            }
        }

        private void DrawGatheringRow(GatheringSource obj)
        {
            ImGui.TableNextColumn();
            ImGui.PushID(obj.GetHashCode());
            var source = obj.Source;
            if (ImGui.ImageButton(ImGuiService.GetIconTexture(source.Icon).ImGuiHandle, new(32, 32)))
            {
                _gameInterface.OpenGatheringLog(_itemId);
            }
            ImGuiUtil.HoverTooltip(source.Name + " - Open in Gathering Log");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(obj.Level.GatheringItemLevel.ToString());     
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.PlaceName.Name + " - " + (obj.TerritoryType.PlaceName.Value?.Name ?? "Unknown"));
            ImGui.PopID();
        }

        private void DrawRecipeResultRow(RecipeEx obj)
        {

        }

        private void DrawRetainerRow(RetainerTaskEx obj)
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