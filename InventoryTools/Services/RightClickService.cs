using System.Collections.Generic;
using System.Linq;
using System.Web;
using AllaganLib.GameSheets.Sheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Services;

public class RightClickService
{
    private readonly IListService _listService;
    private readonly IChatUtilities _chatUtilities;
    private readonly TryOn _tryOn;
    private readonly IGameInterface _gameInterface;
    private readonly ICommandManager _commandManager;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IClipboardService _clipboardService;

    public RightClickService(IListService listService, IChatUtilities chatUtilities, TryOn tryOn, IGameInterface gameInterface, ICommandManager commandManager, InventoryToolsConfiguration configuration, IClipboardService clipboardService)
    {
        _listService = listService;
        _chatUtilities = chatUtilities;
        _tryOn = tryOn;
        _gameInterface = gameInterface;
        _commandManager = commandManager;
        _configuration = configuration;
        _clipboardService = clipboardService;
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

        var curatedLists =
            _listService.Lists.Where(c => c.FilterType == FilterType.CuratedList).ToArray();
        foreach (var filter in curatedLists)
        {
            if (firstItem)
            {
                ImGui.Separator();
                firstItem = false;
            }
            if (ImGui.Selectable("Add to curated list - " + filter.Name))
            {
                filter.AddCuratedItem(new CuratedItem(searchResult.Item.RowId));
                messages.Add(new FocusListMessage(typeof(FiltersWindow), filter));
                filter.NeedsRefresh = true;
            }
            //TODO: Check this
            // if (searchResult.Item.CanBeCrafted && searchResult.Item.HasSourcesByType(ItemInfoType.FreeCompanyCraftRecipe))
            // {
            //     if (searchResult.Item.CompanyCraftSequence != null)
            //     {
            //         for (var index = 0u; index < searchResult.Item.CompanyCraftSequence.CompanyCraftParts.Length; index++)
            //         {
            //             var part = searchResult.Item.CompanyCraftSequence.CompanyCraftParts[index];
            //             if (part.RowId == 0) continue;
            //             if (firstItem)
            //             {
            //                 ImGui.Separator();
            //                 firstItem = false;
            //             }
            //
            //             if (ImGui.Selectable("Add " + (part.CompanyCraftProcess..Name ?? "Unknown") + " to curated list - " + filter.Name))
            //             {
            //                 filter.CraftList.AddCraftItem(searchResult.Item.RowId);
            //                 messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
            //                 messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
            //                 filter.NeedsRefresh = true;
            //             }
            //         }
            //     }
            //     ImGui.Separator();
            // }
        }

        if (ImGui.Selectable("Add to new curated list"))
        {
            var filter = _listService.AddNewCuratedList();
            filter.AddCuratedItem(new CuratedItem(searchResult.Item.RowId));
            messages.Add(new FocusListMessage(typeof(FiltersWindow), filter));
            filter.NeedsRefresh = true;
        }

        if (filterConfiguration != null && searchResult.CuratedItem != null && ImGui.Selectable("Remove from curated list"))
        {
            filterConfiguration.RemoveCuratedItem(searchResult.CuratedItem);
            filterConfiguration.NeedsRefresh = true;
        }

        ImGui.Separator();

        var craftFilters =
            _listService.Lists.Where(c =>
                c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault).ToArray();
        foreach (var filter in craftFilters)
        {
            if (!searchResult.Item.HasSourcesByType(ItemInfoType.FreeCompanyCraftRecipe))
            {
                if (firstItem)
                {
                    ImGui.Separator();
                    firstItem = false;
                }
                if (ImGui.Selectable("Add to craft list - " + filter.Name))
                {
                    filter.CraftList.AddCraftItem(searchResult.Item.RowId);
                    messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                    messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                    filter.NeedsRefresh = true;
                }
            }
            //TODO: Check me
            // if (searchResult.Item.HasSourcesByType(ItemInfoType.FreeCompanyCraftRecipe))
            // {
            //     if (searchResult.Item.CompanyCraftSequence != null)
            //     {
            //         for (var index = 0u; index < searchResult.Item.CompanyCraftSequence.CompanyCraftPart.Length; index++)
            //         {
            //             var part = searchResult.Item.CompanyCraftSequence.CompanyCraftPart[index];
            //             if (part.Row == 0) continue;
            //             if (firstItem)
            //             {
            //                 ImGui.Separator();
            //                 firstItem = false;
            //             }
            //
            //             if (ImGui.Selectable("Add " + (part.Value?.CompanyCraftType.Value?.Name ?? "Unknown") + " to craft list - " + filter.Name))
            //             {
            //                 filter.CraftList.AddCraftItem(searchResult.Item.RowId);
            //                 messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
            //                 messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
            //                 filter.NeedsRefresh = true;
            //             }
            //         }
            //     }
            //     ImGui.Separator();
            // }
        }

        if (!searchResult.Item.HasSourcesByType(ItemInfoType.FreeCompanyCraftRecipe))
        {
            if (ImGui.Selectable("Add to new craft list"))
            {
                 var filter = _listService.AddNewCraftList();
                 filter.CraftList.AddCraftItem(searchResult.Item.RowId);
                 messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                 messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                 filter.NeedsRefresh = true;
            }
            if (ImGui.Selectable("Add to new craft list (ephemeral)"))
            {
                 var filter = _listService.AddNewCraftList(null,true);
                 filter.CraftList.AddCraftItem(searchResult.Item.RowId);
                 messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                 messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                 filter.NeedsRefresh = true;
            }
        }

        //TODO: Check this
        // if (_excelCache.IsCompanyCraft(searchResult.Item.RowId))
        // {
        //     if (searchResult.Item.CompanyCraftSequence != null)
        //     {
        //         for (var index = 0u; index < searchResult.Item.CompanyCraftSequence.CompanyCraftPart.Length; index++)
        //         {
        //             var part = searchResult.Item.CompanyCraftSequence.CompanyCraftPart[index];
        //             if (part.Row == 0) continue;
        //             if (ImGui.Selectable("Add " + (part.Value?.CompanyCraftType.Value?.Name ?? "Unknown") + " to new craft list"))
        //             {
        //                 var newPhase = index;
        //                  var filter = _listService.AddNewCraftList();
        //                  filter.CraftList.AddCraftItem(searchResult.Item.RowId,1, InventoryItem.ItemFlags.None, newPhase);
        //                  messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
        //                  messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
        //                  filter.NeedsRefresh = true;
        //             }
        //             if (ImGui.Selectable("Add " + (part.Value?.CompanyCraftType.Value?.Name ?? "Unknown") + " to new craft list (ephemeral)"))
        //             {
        //                 var newPhase = index;
        //                 var filter = _listService.AddNewCraftList(null,true);
        //                 filter.IsEphemeralCraftList = true;
        //                 filter.CraftList.AddCraftItem(searchResult.Item.RowId,1, InventoryItem.ItemFlags.None, newPhase);
        //                 messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
        //                 messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
        //                 filter.NeedsRefresh = true;
        //             }
        //         }
        //     }
        // }

        if (filterConfiguration != null && searchResult.CraftItem != null)
        {
            if (searchResult.CraftItem.IsOutputItem)
            {
                if (firstItem)
                {
                    ImGui.Separator();
                    firstItem = false;
                }

                if (ImGui.Selectable("Remove from craft list"))
                {
                    filterConfiguration.CraftList.RemoveCraftItem(searchResult.Item.RowId, searchResult.CraftItem.Flags);
                    filterConfiguration.NeedsRefresh = true;
                }
            }

            if (searchResult.Item.CanBeCrafted && searchResult.CraftItem.IsOutputItem && searchResult.Item.HasSourcesByType(ItemInfoType.FreeCompanyCraftRecipe))
            {
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
                            if (firstItem)
                            {
                                ImGui.Separator();
                                firstItem = false;
                            }

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
                    foreach (var filter in craftFilters)
                    {
                        if (firstItem)
                        {
                            ImGui.Separator();
                            firstItem = false;
                        }

                        if (ImGui.Selectable("Add " + searchResult.CraftItem.QuantityNeeded + " item to craft list - " +
                                             filter.Name))
                        {
                            filter.CraftList.AddCraftItem(searchResult.Item.RowId,
                                searchResult.CraftItem.QuantityNeeded,
                                InventoryItem.ItemFlags.None);
                            messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                            messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                            filterConfiguration.NeedsRefresh = true;
                        }
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

        if (searchResult.Item.HasSourcesByType(ItemInfoType.CraftRecipe) && ImGui.Selectable("Open Crafting Log"))
        {
            if (recipeId != null)
            {
                var result = _gameInterface.OpenCraftingLog(searchResult.Item.RowId, recipeId.Value);
                if (!result)
                {
                    _chatUtilities.PrintError("Could not open the crafting log, you are currently crafting.");
                }
            }
            else
            {
                var result = _gameInterface.OpenCraftingLog(searchResult.Item.RowId);
                if (!result)
                {
                    _chatUtilities.PrintError("Could not open the crafting log, you are currently crafting.");
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

        if (searchResult.Item.HasSourcesByCategory(ItemInfoCategory.Gathering) && ImGui.Selectable("Gather with Gatherbuddy"))
        {
            _commandManager.ProcessCommand("/gather " + searchResult.Item.NameString);
        }

        if (searchResult.Item.ObtainedFishing && ImGui.Selectable("Gather with Gatherbuddy"))
        {
            _commandManager.ProcessCommand("/gatherfish " + searchResult.Item.NameString);
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
}