using System.Numerics;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Textures;
using ImGuiNET;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Ui;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryToolsMock;

public class MockGameItemsWindow : GenericWindow
{
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IListService _listService;

    public MockGameItemsWindow(ILogger<MockGameItemsWindow> logger, MediatorService mediator, ImGuiService imGuiService,InventoryToolsConfiguration configuration, IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor, IListService listService, string name = "Item Viewer") : base(logger, mediator, imGuiService, configuration, name)
    {
        _inventoryMonitor = inventoryMonitor;
        _characterMonitor = characterMonitor;
        _listService = listService;
    }
    public override void Initialize()
    {
        WindowName = "Item Viewer";
        Key = "itemviewer";
        _selectedCategory = new Dictionary<ulong, InventoryCategory>();
    }

    public InventoryItem? _cutItem;
    public InventoryItem? _copyItem;
    public Dictionary<ulong, InventoryCategory> _selectedCategory;
    public override void Draw()
    {
        using (var tabBar = ImRaii.TabBar("Bags", ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton))
        {
            if (tabBar.Success)
            {
                foreach (var inventory in _inventoryMonitor.Inventories)
                {
                    var character = _characterMonitor.GetCharacterById(inventory.Key);
                    var characterName = character?.Name ?? "Unknown"; 
                    using (var tabItem = ImRaii.TabItem(characterName + "##" + inventory.Key))
                    {
                        if (tabItem.Success)
                        {
                            using(var sideChild = ImRaii.Child("side", new Vector2(200, 0)))
                            {
                                if (sideChild.Success)
                                {
                                    foreach (var category in inventory.Value.GetAllInventoriesByCategory())
                                    {
                                        if (!_selectedCategory.ContainsKey(inventory.Key))
                                        {
                                            _selectedCategory[inventory.Key] = category.Key;
                                        }
                                        if (ImGui.Selectable(category.Key.FormattedName(), _selectedCategory.ContainsKey(inventory.Key) && _selectedCategory[inventory.Key] == category.Key))
                                        {
                                            _selectedCategory[inventory.Key] = category.Key;
                                        }
                                    }
                                }
                            }
                            ImGui.SameLine();
                            using(var mainChild = ImRaii.Child("main", new Vector2(0, 0)))
                            {
                                if (mainChild.Success)
                                {
                                    foreach (var category in inventory.Value.GetAllInventoriesByCategory())
                                    {
                                        if (_selectedCategory.ContainsKey(inventory.Key) &&
                                            _selectedCategory[inventory.Key] == category.Key)
                                        {
                                            var itemsByType = category.Value.GroupBy(c => c.SortedContainer);
                                            foreach (var type in itemsByType)
                                            {
                                                using (var typeChild = ImRaii.Child(type + "##" + type))
                                                {
                                                    ImGui.Text(type.Key.ToString());
                                                    ImGui.NewLine();
                                                    if (typeChild.Success)
                                                    {
                                                        var chunkedItems = type.OrderBy(c => c.Slot).Chunk(5);
                                                        foreach (var itemChunk in chunkedItems)
                                                        {
                                                            for (var index = 0; index < itemChunk.Length; index++)
                                                            {
                                                                var item = itemChunk[index];
                                                                using (ImRaii.PushId(item.Slot))
                                                                {
                                                                    var texture = item.ItemId == 0
                                                                        ? ImGuiService.TextureProvider.GetFromGameIcon(
                                                                            new GameIconLookup(62574))
                                                                        : ImGuiService.TextureProvider.GetFromGameIcon(
                                                                            new GameIconLookup(item.Icon));
                                                                    if(ImGui.ImageButton(texture.GetWrapOrEmpty().ImGuiHandle,
                                                                        new Vector2(32, 32)))
                                                                    {
                                                                        item.ItemId = 0;
                                                                        _listService.InvalidateLists();
                                                                    }
                                                                    ImGuiUtil.HoverTooltip(item.FormattedName + " - " + item.Quantity + " in slot " + item.Slot);
                                                                    ImGui.SameLine();
                                                                    
                                                                    if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                                                    {
                                                                        ImGui.OpenPopup("RightClick" + item.SortedContainer + "_" + item.SortedSlotIndex);
                                                                    }

                                                                    using (var popup = ImRaii.Popup("RightClick" + item.SortedContainer + "_" + item.SortedSlotIndex))
                                                                    {
                                                                        if (popup.Success)
                                                                        {
                                                                            MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(item));
                                                                            ImGui.Separator();
                                                                            if (ImGui.Selectable("Copy item"))
                                                                            {
                                                                                _copyItem = item;
                                                                                _cutItem = null;
                                                                            }
                                                                            if (ImGui.Selectable("Cut item"))
                                                                            {
                                                                                _cutItem = item;
                                                                                _copyItem = null;
                                                                            }
                                                                            if ((_copyItem != null || _cutItem != null) && ImGui.Selectable("Past item"))
                                                                            {
                                                                                if (_copyItem != null)
                                                                                {
                                                                                    _copyItem = new InventoryItem(
                                                                                        _copyItem);
                                                                                    _copyItem.SortedSlotIndex =
                                                                                        item.SortedSlotIndex;
                                                                                    _copyItem.SortedContainer =
                                                                                        item.SortedContainer;
                                                                                    _copyItem.Container =
                                                                                        item.Container;
                                                                                    _copyItem.Slot =
                                                                                        item.Slot;
                                                                                    _copyItem.RetainerId =
                                                                                        item.RetainerId;
                                                                                    inventory.Value.AddItem(_copyItem);
                                                                                    _copyItem = null;
                                                                                    _inventoryMonitor.SignalRefresh();
                                                                                }
                                                                                else if (_cutItem != null)
                                                                                {
                                                                                    inventory.Value.AddItem(_cutItem);
                                                                                    _cutItem = null;
                                                                                    _inventoryMonitor.SignalRefresh();
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            ImGui.NewLine();
                                                        }
                                                    }

                                                }
                                                ImGui.SameLine();
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }
        }
    }

    public override void Invalidate()
    {
        
    }

    public override FilterConfiguration? SelectedConfiguration { get; } = null;
    public override string GenericKey { get; } = "mockgameitems";
    public override string GenericName { get; } = "Mock Game Items";
    public override bool DestroyOnClose { get; } = true;
    public override bool SaveState { get; } = false;
    public override Vector2? DefaultSize { get; } = new Vector2(1000, 1000);
    public override Vector2? MaxSize { get; } = new Vector2(2000, 2000);
    public override Vector2? MinSize { get; } = new Vector2(300, 300);
    
}