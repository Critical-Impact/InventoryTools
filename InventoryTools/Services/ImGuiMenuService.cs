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
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Services;

public class ImGuiMenuService
{
    private readonly IListService _listService;
    private readonly IChatUtilities _chatUtilities;
    private readonly TryOn _tryOn;
    private readonly IGameInterface _gameInterface;
    private readonly ICommandManager _commandManager;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IClipboardService _clipboardService;
    private readonly MapSheet _mapSheet;

    public ImGuiMenuService(IListService listService,
        IChatUtilities chatUtilities,
        TryOn tryOn,
        IGameInterface gameInterface,
        ICommandManager commandManager,
        InventoryToolsConfiguration configuration,
        IClipboardService clipboardService,
        MapSheet mapSheet)
    {
        _listService = listService;
        _chatUtilities = chatUtilities;
        _tryOn = tryOn;
        _gameInterface = gameInterface;
        _commandManager = commandManager;
        _configuration = configuration;
        _clipboardService = clipboardService;
        _mapSheet = mapSheet;
    }

    public List<MessageBase> DrawRightClickPopup(SearchResult searchResult, FilterConfiguration? filterConfiguration = null)
    {
        return DrawRightClickPopup(searchResult, new List<MessageBase>(), filterConfiguration);
    }
    public List<MessageBase> DrawRightClickPopup(ItemRow itemRow, FilterConfiguration? filterConfiguration = null)
    {
        return DrawRightClickPopup(new SearchResult(itemRow), new List<MessageBase>(), filterConfiguration);
    }
    public List<MessageBase> DrawRightClickPopup(ItemRow itemRow, List<MessageBase> messages, FilterConfiguration? filterConfiguration = null)
    {
        return DrawRightClickPopup(new SearchResult(itemRow), messages, filterConfiguration);
    }
    public List<MessageBase> DrawRightClickPopup(CriticalCommonLib.Models.InventoryItem inventoryItem, FilterConfiguration? filterConfiguration = null)
    {
        return DrawRightClickPopup(new SearchResult(inventoryItem), new List<MessageBase>(), filterConfiguration);
    }
    public List<MessageBase> DrawRightClickPopup(CriticalCommonLib.Models.InventoryItem inventoryItem, List<MessageBase> messages, FilterConfiguration? filterConfiguration = null)
    {
        return DrawRightClickPopup(new SearchResult(inventoryItem), messages, filterConfiguration);
    }
    public List<MessageBase> DrawRightClickPopup(SearchResult searchResult, List<MessageBase> messages, FilterConfiguration? filterConfiguration = null)
    {
        DrawMenuItems(searchResult, messages);
        bool firstItem = true;

        ImGui.Separator();
        var curatedLists =
            _listService.Lists.Where(c => c.FilterType == FilterType.CuratedList).ToArray();
        if (curatedLists.Length != 0 && ImGui.BeginMenu("Add to Curated List"))
        {
            foreach (var filter in curatedLists)
            {
                if (ImGui.MenuItem(filter.Name))
                {
                    filter.AddCuratedItem(new CuratedItem(searchResult.Item.RowId));
                    messages.Add(new FocusListMessage(typeof(FiltersWindow), filter));
                    filter.NeedsRefresh = true;
                }
            }
            ImGui.EndMenu();
        }

        if (ImGui.Selectable("Add to new Curated List"))
        {
            var filter = _listService.AddNewCuratedList();
            filter.AddCuratedItem(new CuratedItem(searchResult.Item.RowId));
            messages.Add(new FocusListMessage(typeof(FiltersWindow), filter));
            filter.NeedsRefresh = true;
        }

        if (filterConfiguration != null && searchResult.CuratedItem != null && ImGui.Selectable("Remove from Curated List"))
        {
            filterConfiguration.RemoveCuratedItem(searchResult.CuratedItem);
            filterConfiguration.NeedsRefresh = true;
        }

        ImGui.Separator();
        var craftFilters =
            _listService.Lists.Where(c =>
                c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault).ToArray();
        if (craftFilters.Length != 0 && ImGui.BeginMenu("Add to Craft List"))
        {
            foreach (var filter in craftFilters)
            {
                if (ImGui.Selectable(filter.Name))
                {
                    filter.CraftList.AddCraftItem(searchResult.Item.RowId);
                    messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                    messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                    filter.NeedsRefresh = true;
                }
            }
            ImGui.EndMenu();
        }

        if (ImGui.Selectable("Add to new Craft List"))
        {
             var filter = _listService.AddNewCraftList();
             filter.CraftList.AddCraftItem(searchResult.Item.RowId);
             messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
             messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
             filter.NeedsRefresh = true;
        }
        if (ImGui.Selectable("Add to new Craft List (ephemeral)"))
        {
             var filter = _listService.AddNewCraftList(null,true);
             filter.CraftList.AddCraftItem(searchResult.Item.RowId);
             messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
             messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
             filter.NeedsRefresh = true;
        }


        if (filterConfiguration != null && searchResult.CraftItem != null)
        {
            if (searchResult.CraftItem.IsOutputItem)
            {
                if (ImGui.Selectable("Remove from Craft List"))
                {
                    filterConfiguration.CraftList.RemoveCraftItem(searchResult.Item.RowId, searchResult.CraftItem.Flags);
                    filterConfiguration.NeedsRefresh = true;
                }
            }

            if (searchResult.Item.CanBeCrafted && searchResult.CraftItem.IsOutputItem && searchResult.Item.HasSourcesByType(ItemInfoType.FreeCompanyCraftRecipe))
            {
                ImGui.Separator();
                if (searchResult.Item.CompanyCraftSequence != null && searchResult.Item.CompanyCraftSequence.CompanyCraftParts.Length > 1)
                {
                    if (searchResult.CraftItem.Phase != null && ImGui.Selectable("Switch to All Phases"))
                    {
                        filterConfiguration.CraftList.SetCraftPhase(searchResult.Item.RowId, null, searchResult.CraftItem.Phase);
                        filterConfiguration.NeedsRefresh = true;
                    }

                    for (var index = 0u;
                         index < searchResult.Item.CompanyCraftSequence.CompanyCraftParts
                             .Length;
                         index++)
                    {
                        var part =
                            searchResult.Item.CompanyCraftSequence.CompanyCraftParts[index];
                        if (part.RowId == 0) continue;
                        if (searchResult.CraftItem.Phase != index)
                        {
                            if (ImGui.Selectable("Switch to " + ((part.Base.CompanyCraftType.ValueNullable?.Name.ExtractText() ?? "") + " (Phase " + (index + 1) + ")")))
                            {
                                filterConfiguration.CraftList.SetCraftPhase(searchResult.Item.RowId, index,
                                    searchResult.CraftItem.Phase);
                                filterConfiguration.NeedsRefresh = true;
                            }
                        }
                    }
                }
            }

            if (!searchResult.CraftItem.IsOutputItem)
            {
                if (searchResult.Item.CanBeCrafted && !searchResult.Item.HasSourcesByType(ItemInfoType.FreeCompanyCraftRecipe))
                {
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Add " + searchResult.CraftItem.QuantityNeeded + " " +
                                        searchResult.Item.NameString + " to craft list"))
                    {
                        foreach (var filter in craftFilters)
                        {
                            if (ImGui.Selectable(filter.Name))
                            {
                                filter.CraftList.AddCraftItem(searchResult.Item.RowId,
                                    searchResult.CraftItem.QuantityNeeded,
                                    InventoryItem.ItemFlags.None);
                                messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                                messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                                filterConfiguration.NeedsRefresh = true;
                            }
                        }

                        ImGui.EndMenu();
                    }
                    if (ImGui.Selectable("Add " + searchResult.CraftItem.QuantityNeeded + " item to new craft list"))
                    {
                        var filter = _listService.AddNewCraftList();
                        filter.CraftList.AddCraftItem(searchResult.Item.RowId,
                            searchResult.CraftItem.QuantityNeeded,
                            InventoryItem.ItemFlags.None);
                        messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                        messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                        filterConfiguration.NeedsRefresh = true;
                    }

                    if (ImGui.Selectable("Add " + searchResult.CraftItem.QuantityNeeded +
                                         " item to new craft list (ephemeral)"))
                    {
                        var filter = _listService.AddNewCraftList(null, true);
                        filter.CraftList.AddCraftItem(searchResult.Item.RowId,
                            searchResult.CraftItem.QuantityNeeded,
                            InventoryItem.ItemFlags.None);
                        messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                        messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                        filterConfiguration.NeedsRefresh = true;
                    }
                }
            }
        }
        return messages;
    }
    public List<MessageBase> DrawMenuItems(SearchResult searchResult, List<MessageBase> messages, uint? recipeId = null)
    {
        ImGui.Text(searchResult.Item.NameString);
        ImGui.Separator();
        if (ImGui.Selectable("Open in Garland Tools"))
        {
            $"https://www.garlandtools.org/db/#item/{searchResult.Item.GarlandToolsId}".OpenBrowser();
        }
        if (ImGui.Selectable("Open in Teamcraft"))
        {
            $"https://ffxivteamcraft.com/db/en/item/{searchResult.Item.RowId}".OpenBrowser();
        }
        if (ImGui.Selectable("Open in Universalis"))
        {
            $"https://universalis.app/market/{searchResult.Item.RowId}".OpenBrowser();
        }
        if (ImGui.Selectable("Open in Gamer Escape"))
        {
            var name = searchResult.Item.NameString.Replace(' ', '_');
            name = name.Replace('–', '-');

            if (name.StartsWith("_")) // "level sync" icon
                name = name.Substring(2);
            $"https://ffxiv.gamerescape.com/wiki/{HttpUtility.UrlEncode(name)}?useskin=Vector".OpenBrowser();
        }
        if (ImGui.Selectable("Open in Console Games Wiki"))
        {
            var name = searchResult.Item.NameString.Replace("#"," ").Replace("  ", " ").Replace(' ', '_');
            name = name.Replace('–', '-');

            if (name.StartsWith("_")) // "level sync" icon
                name = name.Substring(2);
            $"https://ffxiv.consolegameswiki.com/wiki/{HttpUtility.UrlEncode(name)}".OpenBrowser();
        }
        ImGui.Separator();
        if (ImGui.Selectable("Copy Name"))
        {
            _clipboardService.CopyToClipboard(searchResult.Item.NameString);
        }
        if (ImGui.Selectable("Link"))
        {
            _chatUtilities.LinkItem(searchResult.Item);
        }
        if (searchResult.Item.CanTryOn && ImGui.Selectable("Try On"))
        {
            if (_tryOn.CanUseTryOn)
            {
                _tryOn.TryOnItem(searchResult.Item);
            }
        }
        if (ImGui.Selectable("Search"))
        {
            messages.Add(new ItemSearchRequestedMessage(searchResult.Item.RowId, InventoryItem.ItemFlags.None));
        }

        var actions = this.DrawActionMenu(searchResult);
        if (actions != null)
        {
            messages.AddRange(actions);
        }

        ImGui.Separator();

        if (ImGui.Selectable(_configuration.IsFavouriteItem(searchResult.Item.RowId)
                ? "Unmark Favourite"
                : "Mark Favourite"))
        {
            _configuration.ToggleFavouriteItem(searchResult.Item.RowId);
        }

        if (ImGui.Selectable("More Information"))
        {
            messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), searchResult.Item.RowId));
        }

        return messages;
    }

    public List<MessageBase>? DrawActionMenu(SearchResult searchResult)
    {
        var hasActions = false;
        var messages = new List<MessageBase>();

        if (searchResult.Item.HasSourcesByType(ItemInfoType.CraftRecipe))
        {
            hasActions = true;
            if (searchResult.Item.Recipes.Count == 1 || searchResult.CraftItem != null && searchResult.CraftItem.Recipe != null)
            {
                if (ImGui.MenuItem("Open Crafting Log"))
                {
                    if (searchResult.CraftItem?.Recipe != null)
                    {
                        _gameInterface.OpenCraftingLog(searchResult.Item.RowId, searchResult.CraftItem.Recipe.RowId);
                    }
                    else
                    {
                        _gameInterface.OpenCraftingLog(searchResult.Item.RowId);
                    }
                }
            }

            if (searchResult.Item.Recipes.Count > 1)
            {
                if (ImGui.BeginMenu("Open Crafting Log(Recipes)"))
                {
                    foreach (var recipe in searchResult.Item.Recipes)
                    {
                        if (ImGui.MenuItem(recipe.CraftType?.FormattedName ?? "Unknown"))
                        {
                            _gameInterface.OpenCraftingLog(searchResult.Item.RowId);
                        }
                    }

                    ImGui.EndMenu();
                }
            }
        }

        if (searchResult.Item.HasSourcesByCategory(ItemInfoCategory.Gathering) && ImGui.Selectable("Open Gathering Log"))
        {
            _gameInterface.OpenGatheringLog(searchResult.Item.RowId);
        }

        if (searchResult.Item.ObtainedFishing && ImGui.Selectable("Open Fishing Log"))
        {
            _gameInterface.OpenFishingLog(searchResult.Item.RowId, searchResult.Item.ObtainedSpearFishing);
        }

        if (searchResult.Item.HasSourcesByCategory(ItemInfoCategory.Gathering))
        {
            if (ImGui.MenuItem("Gather (Gatherbuddy)"))
            {
                _commandManager.ProcessCommand("/gather " + searchResult.Item.Base.Name.ExtractText());
            }
            hasActions = true;
            if (ImGui.BeginMenu("Gather (Advanced)"))
            {
                var gatheringSources = searchResult.Item
                    .GetSourcesByCategory<ItemGatheringSource>(ItemInfoCategory.Gathering);

                var groupedGatheringSources = gatheringSources.SelectMany(c => c.GatheringItem.GatheringPoints).DistinctBy(c => c.RowId).GroupBy(c => c.Map.RowId).ToDictionary(c => c.Key, c => c);

                foreach (var groupedGathering in groupedGatheringSources)
                {
                    var map = _mapSheet.GetRow(groupedGathering.Key);
                    if (ImGui.BeginMenu(map.FormattedName))
                    {
                        foreach (var gatheringPoint in groupedGathering.Value.DistinctBy(c => (c.MapX, c.MapY)))
                        {
                            if (ImGui.MenuItem($"Teleport to ({gatheringPoint.GatheringPointNameRow.Base.Singular}) at ({gatheringPoint.MapX.ToString("N2", CultureInfo.InvariantCulture)}, {gatheringPoint.MapY.ToString("N2", CultureInfo.InvariantCulture)})"))
                            {
                                messages.Add(new RequestTeleportToGatheringPointRowMessage(gatheringPoint));
                                _chatUtilities.PrintFullMapLink(gatheringPoint, $"Lv. {gatheringPoint.GatheringPointBase.Base.GatheringLevel} {gatheringPoint.GatheringPointNameRow.Base.Singular.ExtractText().ToTitleCase()}");
                                _chatUtilities.PrintGatheringMapLink(gatheringPoint);
                            }
                        }


                        ImGui.EndMenu();
                    }
                }

                ImGui.EndMenu();
            }
        }

        if (searchResult.Item.HasSourcesByType(ItemInfoType.Fishing))
        {
            hasActions = true;
            if (ImGui.MenuItem("Gather (Gatherbuddy)"))
            {
                _commandManager.ProcessCommand("/gatherfish " + searchResult.Item.Base.Name.ExtractText());
            }
            if (ImGui.BeginMenu("Gather (Advanced)"))
            {

                var gatheringSources = searchResult.Item
                    .GetSourcesByType<ItemFishingSource>(ItemInfoType.Fishing);

                if (gatheringSources.Any())
                {

                    var fishParameter = gatheringSources.First().FishParameter;
                    var groupedGatheringSources = gatheringSources.SelectMany(c => c.FishingSpots)
                        .DistinctBy(c => c.RowId).GroupBy(c => c.Map.RowId).ToDictionary(c => c.Key, c => c);

                    foreach (var groupedGathering in groupedGatheringSources)
                    {
                        var map = _mapSheet.GetRow(groupedGathering.Key);
                        if (ImGui.BeginMenu(map.FormattedName))
                        {
                            foreach (var fishingSpot in groupedGathering.Value.DistinctBy(c => (c.MapX, c.MapY)))
                            {
                                if (ImGui.MenuItem(
                                        $"Teleport to ({fishingSpot.Base.PlaceName.Value.Name.ExtractText()}, {fishParameter.FishRecordType}) at ({fishingSpot.MapX.ToString("N2", CultureInfo.InvariantCulture)}, {fishingSpot.MapY.ToString("N2", CultureInfo.InvariantCulture)})"))
                                {
                                    messages.Add(new RequestTeleportToFishingSpotRowMessage(fishingSpot));
                                    _chatUtilities.PrintFullMapLink(fishingSpot,
                                        $"Lv. {fishingSpot.Base.GatheringLevel} {fishingSpot.Base.FishingSpotCategory}");
                                    _chatUtilities.PrintGatheringMapLink(fishingSpot, fishParameter);
                                }
                            }


                            ImGui.EndMenu();
                        }
                    }
                }
                ImGui.EndMenu();
            }
        }


        if (searchResult.Item.HasSourcesByType(ItemInfoType.Spearfishing))
        {
            hasActions = true;
            if (ImGui.MenuItem("Gather (Gatherbuddy)"))
            {
                _commandManager.ProcessCommand("/gatherfish " + searchResult.Item.Base.Name.ExtractText());
            }
            if (ImGui.BeginMenu("Gather (Advanced)"))
            {

                var gatheringSources = searchResult.Item
                    .GetSourcesByType<ItemSpearfishingSource>(ItemInfoType.Spearfishing);

                if (gatheringSources.Any())
                {
                    var spearfishingItem = gatheringSources.First().SpearfishingItemRow;
                    var groupedGatheringSources = gatheringSources.SelectMany(c => c.SpearfishingItemRow.GatheringPoints)
                        .DistinctBy(c => c.RowId).GroupBy(c => c.SpearfishingNotebook!.TerritoryTypeRow!.Map!.RowId).ToDictionary(c => c.Key, c => c);

                    foreach (var groupedGathering in groupedGatheringSources)
                    {
                        var map = _mapSheet.GetRow(groupedGathering.Key);
                        if (ImGui.BeginMenu(map.FormattedName))
                        {
                            foreach (var fishingSpot in groupedGathering.Value.DistinctBy(c => (c.SpearfishingNotebook!.MapX, c.SpearfishingNotebook!.MapY)))
                            {
                                if (ImGui.MenuItem(
                                        $"Teleport to ({fishingSpot.SpearfishingNotebook!.Base.PlaceName.Value.Name.ExtractText()}, {spearfishingItem.FishRecordType}) at ({fishingSpot.SpearfishingNotebook.MapX.ToString("N2", CultureInfo.InvariantCulture)}, {fishingSpot.SpearfishingNotebook.MapY.ToString("N2", CultureInfo.InvariantCulture)})"))
                                {
                                    messages.Add(new RequestTeleportToSpearFishingSpotRowMessage(fishingSpot.SpearfishingNotebook));
                                    _chatUtilities.PrintFullMapLink(fishingSpot.SpearfishingNotebook!, $"Lv. {fishingSpot.Base.GatheringLevel}");
                                    _chatUtilities.PrintGatheringMapLink(fishingSpot.SpearfishingNotebook!, spearfishingItem);
                                }
                            }


                            ImGui.EndMenu();
                        }
                    }
                }
                ImGui.EndMenu();
            }
        }

        if (searchResult.Item.HasSourcesByCategory(ItemInfoCategory.Shop))
        {
            var hasShopSources = searchResult.Item
                .GetSourcesByCategory<ItemShopSource>(ItemInfoCategory.Shop)
                .Where(c => c.Shop.Name != string.Empty)
                .Any(c => c.Shop.ENpcs.Any(d => d.Locations.Any()));
            if (hasShopSources)
            {
                hasActions = true;
            }

            if (hasShopSources && ImGui.BeginMenu("Buy"))
            {
                var shopSources = searchResult.Item
                    .GetSourcesByCategory<ItemShopSource>(ItemInfoCategory.Shop)
                    .Where(c => c.Shop.Name != string.Empty)
                    .Where(c => c.Shop.ENpcs.Any(d => d.Locations.Any()));
                var groupedShops = new Dictionary<uint, List<ItemShopSource>>();
                foreach (var shopSource in shopSources)
                {
                    foreach (var mapId in shopSource.MapIds ?? [])
                    {
                        groupedShops.TryAdd(mapId, new());
                        groupedShops[mapId].Add(shopSource);
                    }
                }

                foreach (var groupedShop in groupedShops)
                {
                    var map = _mapSheet.GetRow(groupedShop.Key);
                    if (ImGui.BeginMenu(map.FormattedName))
                    {
                        foreach (var shopSource in groupedShop.Value)
                        {
                            if (ImGui.MenuItem(shopSource.Shop.Name + " - Teleport"))
                            {
                                var eNpcBaseRow = shopSource.Shop.ENpcs.FirstOrDefault(c => c.Locations.Any(d => d.Map.RowId == groupedShop.Key));
                                var firstLocation = eNpcBaseRow?.Locations.FirstOrDefault();
                                if (firstLocation != null && eNpcBaseRow != null)
                                {
                                    if (firstLocation.TerritoryType.Value.Aetheryte.RowId !=
                                        0)
                                    {
                                        messages.Add(
                                            new RequestTeleportMessage(
                                                firstLocation.TerritoryType.Value.Aetheryte.RowId));
                                        _chatUtilities.PrintFullMapLink(firstLocation,
                                            eNpcBaseRow.ENpcResidentRow.Base.Singular.ExtractText());
                                    }
                                }
                            }
                        }

                        ImGui.EndMenu();
                    }
                }

                ImGui.EndMenu();
            }
        }

        return hasActions ? messages : null;
    }
}