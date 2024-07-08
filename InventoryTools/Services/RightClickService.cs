using System.Collections.Generic;
using System.Linq;
using System.Web;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
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
    private readonly ExcelCache _excelCache;
    private readonly IListService _listService;
    private readonly IChatUtilities _chatUtilities;
    private readonly TryOn _tryOn;
    private readonly IGameInterface _gameInterface;
    private readonly ICommandManager _commandManager;
    private readonly InventoryToolsConfiguration _configuration;

    public RightClickService(ExcelCache excelCache, IListService listService, IChatUtilities chatUtilities, TryOn tryOn, IGameInterface gameInterface, ICommandManager commandManager, InventoryToolsConfiguration configuration)
    {
        _excelCache = excelCache;
        _listService = listService;
        _chatUtilities = chatUtilities;
        _tryOn = tryOn;
        _gameInterface = gameInterface;
        _commandManager = commandManager;
        _configuration = configuration;
    }
    
    public List<MessageBase> DrawRightClickPopup(ItemEx item)
    {
        return DrawRightClickPopup(item, new List<MessageBase>());
    }
    public List<MessageBase> DrawRightClickPopup(ItemEx item, List<MessageBase> messages)
    {
        DrawMenuItems(item, messages);
        bool firstItem = true;
        
        var craftFilters =
            _listService.Lists.Where(c =>
                c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault).ToArray();
        foreach (var filter in craftFilters)
        {
            if (item.CanBeCrafted && !_excelCache.IsCompanyCraft(item.RowId))
            {
                if (firstItem)
                {
                    ImGui.Separator();
                    firstItem = false;
                }
                if (ImGui.Selectable("Add to craft list - " + filter.Name))
                {
                    filter.CraftList.AddCraftItem(item.RowId);
                    messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                    messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                    filter.NeedsRefresh = true;
                }
            }
            if (item.CanBeCrafted && _excelCache.IsCompanyCraft(item.RowId))
            {
                if (item.CompanyCraftSequenceEx != null)
                {
                    for (var index = 0u; index < item.CompanyCraftSequenceEx.CompanyCraftPart.Length; index++)
                    {
                        var part = item.CompanyCraftSequenceEx.CompanyCraftPart[index];
                        if (part.Row == 0) continue;
                        if (firstItem)
                        {
                            ImGui.Separator();
                            firstItem = false;
                        }

                        if (ImGui.Selectable("Add " + (part.Value?.CompanyCraftType.Value?.Name ?? "Unknown") + " to craft list - " + filter.Name))
                        {
                            filter.CraftList.AddCraftItem(item.RowId);
                            messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                            messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                            filter.NeedsRefresh = true;
                        }
                    }
                }
                ImGui.Separator();
            }
        }

        if (item.CanBeCrafted && !_excelCache.IsCompanyCraft(item.RowId))
        {
            if (ImGui.Selectable("Add to new craft list"))
            {
                 var filter = _listService.AddNewCraftList();
                 filter.CraftList.AddCraftItem(item.RowId);
                 messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                 messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                 filter.NeedsRefresh = true;
            }
            if (ImGui.Selectable("Add to new craft list (ephemeral)"))
            {
                 var filter = _listService.AddNewCraftList(null,true);
                 filter.CraftList.AddCraftItem(item.RowId);
                 messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                 messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                 filter.NeedsRefresh = true;
            }
        }

        if (item.CanBeCrafted && _excelCache.IsCompanyCraft(item.RowId))
        {
            if (item.CompanyCraftSequenceEx != null)
            {
                for (var index = 0u; index < item.CompanyCraftSequenceEx.CompanyCraftPart.Length; index++)
                {
                    var part = item.CompanyCraftSequenceEx.CompanyCraftPart[index];
                    if (part.Row == 0) continue;
                    if (ImGui.Selectable("Add " + (part.Value?.CompanyCraftType.Value?.Name ?? "Unknown") + " to new craft list"))
                    {
                        var newPhase = index;
                         var filter = _listService.AddNewCraftList();
                         filter.CraftList.AddCraftItem(item.RowId,1, InventoryItem.ItemFlags.None, newPhase);
                         messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                         messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                         filter.NeedsRefresh = true;
                    }
                    if (ImGui.Selectable("Add " + (part.Value?.CompanyCraftType.Value?.Name ?? "Unknown") + " to new craft list (ephemeral)"))
                    {
                        var newPhase = index;
                        var filter = _listService.AddNewCraftList(null,true);
                        filter.IsEphemeralCraftList = true;
                        filter.CraftList.AddCraftItem(item.RowId,1, InventoryItem.ItemFlags.None, newPhase);
                        messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                        messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                        filter.NeedsRefresh = true;
                    }
                }
            }
        }

        return messages;
    }
    public void DrawRightClickPopup(CraftItem item, FilterConfiguration configuration, List<MessageBase> messages)
    {
        DrawMenuItems(item.Item, messages, item.RecipeId);
        bool firstItem = true;
        if (item.IsOutputItem)
        {
            if (firstItem)
            {
                ImGui.Separator();
                firstItem = false;
            }
            if (ImGui.Selectable("Remove from craft list"))
            {
                configuration.CraftList.RemoveCraftItem(item.ItemId, item.Flags);
                configuration.NeedsRefresh = true;
            }
        }

        if (item.Item.CanBeCrafted && item.IsOutputItem && _excelCache.IsCompanyCraft(item.ItemId))
        {
            if (item.Item.CompanyCraftSequenceEx != null && item.Item.CompanyCraftSequenceEx.ActiveCompanyCraftParts.Length > 1)
            {
                if (item.Phase != null && ImGui.Selectable("Switch to All Phases"))
                {
                    configuration.CraftList.SetCraftPhase(item.ItemId, null, item.Phase);
                    configuration.NeedsRefresh = true;
                }
                for (var index = 0u; index < item.Item.CompanyCraftSequenceEx.ActiveCompanyCraftParts.Length; index++)
                {
                    var part = item.Item.CompanyCraftSequenceEx.ActiveCompanyCraftParts[index];
                    if (part.Row == 0) continue;
                    if (item.Phase != index)
                    {
                        if (firstItem)
                        {
                            ImGui.Separator();
                            firstItem = false;
                        }
                        if (ImGui.Selectable("Switch to " + ((part.Value?.CompanyCraftType.Value?.Name ?? "") + " (Phase " + (index + 1) + ")")))
                        {
                            configuration.CraftList.SetCraftPhase(item.ItemId, index, item.Phase);
                            configuration.NeedsRefresh = true;
                        }
                    }
                }
            }
        }

        if (!item.IsOutputItem)
        {
            var craftFilters =
                _listService.Lists.Where(c =>
                    c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault);
            if (item.Item.CanBeCrafted && !_excelCache.IsCompanyCraft(item.Item.RowId))
            {
                foreach (var filter in craftFilters)
                {
                    if (firstItem)
                    {
                        ImGui.Separator();
                        firstItem = false;
                    }

                    if (ImGui.Selectable("Add " + item.QuantityNeeded + " item to craft list - " + filter.Name))
                    {
                        filter.CraftList.AddCraftItem(item.Item.RowId, item.QuantityNeeded,
                            InventoryItem.ItemFlags.None);
                        messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                        messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                        configuration.NeedsRefresh = true;
                    }
                }
                if (ImGui.Selectable("Add " + item.QuantityNeeded + " item to new craft list"))
                {
                     var filter = _listService.AddNewCraftList();
                     filter.CraftList.AddCraftItem(item.Item.RowId, item.QuantityNeeded,
                         InventoryItem.ItemFlags.None);
                     messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                     messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                     configuration.NeedsRefresh = true;
                }
                if (ImGui.Selectable("Add " + item.QuantityNeeded + " item to new craft list (ephemeral)"))
                {
                    var filter = _listService.AddNewCraftList(null,true);
                    filter.CraftList.AddCraftItem(item.Item.RowId, item.QuantityNeeded,
                        InventoryItem.ItemFlags.None);
                    messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                    messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                    configuration.NeedsRefresh = true;
                }
            }
        }

    }
    
    public List<MessageBase> DrawMenuItems(ItemEx item, List<MessageBase> messages, uint? recipeId = null)
    {
        ImGui.Text(item.NameString);
        ImGui.Separator();
        if (ImGui.Selectable("Open in Garland Tools"))
        {
            $"https://www.garlandtools.org/db/#item/{item.GarlandToolsId}".OpenBrowser();
        }
        if (ImGui.Selectable("Open in Teamcraft"))
        {
            $"https://ffxivteamcraft.com/db/en/item/{item.RowId}".OpenBrowser();
        }
        if (ImGui.Selectable("Open in Universalis"))
        {
            $"https://universalis.app/market/{item.RowId}".OpenBrowser();
        }
        if (ImGui.Selectable("Open in Gamer Escape"))
        {
            var name = item.NameString.Replace(' ', '_');
            name = name.Replace('–', '-');

            if (name.StartsWith("_")) // "level sync" icon
                name = name.Substring(2);
            $"https://ffxiv.gamerescape.com/wiki/{HttpUtility.UrlEncode(name)}?useskin=Vector".OpenBrowser();
        }
        if (ImGui.Selectable("Open in Console Games Wiki"))
        {
            var name = item.NameString.Replace("#"," ").Replace("  ", " ").Replace(' ', '_');
            name = name.Replace('–', '-');

            if (name.StartsWith("_")) // "level sync" icon
                name = name.Substring(2);
            $"https://ffxiv.consolegameswiki.com/wiki/{HttpUtility.UrlEncode(name)}".OpenBrowser();
        }
        ImGui.Separator();
        if (ImGui.Selectable("Copy Name"))
        {
            item.NameString.ToClipboard();
        }
        if (ImGui.Selectable("Link"))
        {
            _chatUtilities.LinkItem(item);
        }
        if (item.CanTryOn && ImGui.Selectable("Try On"))
        {
            if (_tryOn.CanUseTryOn)
            {
                _tryOn.TryOnItem(item);
            }
        }

        if (item.CanOpenCraftLog && ImGui.Selectable("Open Crafting Log"))
        {
            if (recipeId != null)
            {
                _gameInterface.OpenCraftingLog(item.RowId, recipeId.Value);
            }
            else
            {
                _gameInterface.OpenCraftingLog(item.RowId);
            }
        }

        if (item.CanOpenGatheringLog && ImGui.Selectable("Open Gathering Log"))
        {
            _gameInterface.OpenGatheringLog(item.RowId);
        }

        if (item.ObtainedFishing && ImGui.Selectable("Open Fishing Log"))
        {
            _gameInterface.OpenFishingLog(item.RowId, item.IsSpearfishingItem());
        }

        if (item.CanOpenGatheringLog && ImGui.Selectable("Gather with Gatherbuddy"))
        {
            _commandManager.ProcessCommand("/gather " + item.NameString);
        }

        if (item.ObtainedFishing && ImGui.Selectable("Gather with Gatherbuddy"))
        {
            _commandManager.ProcessCommand("/gatherfish " + item.NameString);
        }
        
        ImGui.Separator();

        if (ImGui.Selectable(_configuration.IsFavouriteItem(item.RowId)
                ? "Unmark Favourite"
                : "Mark Favourite"))
        {
            _configuration.ToggleFavouriteItem(item.RowId);
        }

        if (ImGui.Selectable("More Information"))
        {
            messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), item.RowId));
        }

        return messages;
    }
}