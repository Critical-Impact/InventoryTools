using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui.Pages
{
    public class CharacterRetainerPage : Page
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly ExcelSheet<World> _worldSheet;

        public CharacterRetainerPage(ILogger<CharacterRetainerPage> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor, ExcelSheet<World> worldSheet) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
            _inventoryMonitor = inventoryMonitor;
            _worldSheet = worldSheet;
        }
        private bool _isSeparator = false;
        public override void Initialize()
        {
        }

        public override string Name { get; } = "Characters/Retainers";

        private ulong _selectedCharacter = 0;
        private uint _currentWorld = 0;

        private bool _editMode = false;
        private string _newName = "";

        private HoverButton _editIcon = new(new Vector2(16,16));

        private Dictionary<Character, PopupMenu> _popupMenus = new();
        public PopupMenu GetCharacterMenu(Character character)
        {
            if (!_popupMenus.ContainsKey(character))
            {
                _popupMenus[character] = new PopupMenu("cm_" + character.CharacterId, PopupMenu.PopupMenuButtons.Right,
                    new List<PopupMenu.IPopupMenuItem>()
                    {
                        new PopupMenu.PopupMenuItemSelectableConfirm("Clear Inventories", "ci_" + character.CharacterId, "Are you sure you want to clear the inventories of this " + character.CharacterType.FormattedName() + "?", ClearInventories, "Clear the inventories of this " + character.CharacterType.FormattedName() + "?"),
                        new PopupMenu.PopupMenuItemSelectableConfirm("Delete " + character.CharacterType.FormattedName(), "dc_" + character.CharacterId, "Are you sure you want to delete this " + character.CharacterType.FormattedName() + "?", DeleteCharacter, "Delete the " + character.CharacterType.FormattedName() + "?"),
                    }
                );
            }

            return _popupMenus[character];
        }

        private void DeleteCharacter(string arg1, bool arg2)
        {
            if (arg2)
            {
                var characterId = ulong.Parse(arg1.Split("dc_", StringSplitOptions.RemoveEmptyEntries)[0]);
                _characterMonitor.RemoveCharacter(characterId);
                _inventoryMonitor.ClearCharacterInventories(characterId);
            }
        }

        private void ClearInventories(string arg1, bool arg2)
        {
            if (arg2)
            {
                var characterId = ulong.Parse(arg1.Split("ci_", StringSplitOptions.RemoveEmptyEntries)[0]);
                _inventoryMonitor.ClearCharacterInventories(characterId);
            }
        }

        public override List<MessageBase>? Draw()
        {
            var messages = new List<MessageBase>();
            using (var sidebar = ImRaii.Child("charactersBar", new Vector2(160, 0) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (sidebar.Success)
                {
                    var worldIds = _characterMonitor.GetWorldIds();
                    var characters = _characterMonitor.GetPlayerCharacters().Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).OrderBy(c => c.Value.FormattedName).ToList();
                    ImGui.TextUnformatted("Characters (" + characters.Count + ")");
                    ImGui.Separator();
                    for (var index = 0; index < characters.Count; index++)
                    {
                        var character = characters[index];
                        if (ImGui.Selectable(character.Value.FormattedName))
                        {
                            _selectedCharacter = character.Key;
                        }

                        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("cm_" + character.Key);
                        }

                        GetCharacterMenu(character.Value).Draw();

                        var tooltip = character.Value.FormattedName;
                        if (character.Value.ActualClassJob != null)
                        {
                            tooltip += "\n" + character.Value.ActualClassJob?.Base.Name.ExtractText().ToTitleCase();
                        }

                        tooltip += "\n\nRight Click: Options";
                        ImGuiUtil.HoverTooltip(tooltip);
                        ImGui.SameLine();
                        if (character.Value.ActualClassJob != null)
                        {
                            var icon = ImGuiService.GetIconTexture(character.Value.Icon);
                            ImGui.Image(icon.ImGuiHandle, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale);
                        }
                    }
                    ImGui.NewLine();

                    var freeCompanies = _characterMonitor.GetFreeCompanies().Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).OrderBy(c => c.Value.FormattedName).ToList();
                    ImGui.TextUnformatted("Free Companies (" + freeCompanies.Count + ")");
                    ImGui.Separator();
                    for (var index = 0; index < freeCompanies.Count; index++)
                    {
                        var freeCompany = freeCompanies[index];
                        if (ImGui.Selectable(freeCompany.Value.FormattedName))
                        {
                            _selectedCharacter = freeCompany.Key;
                        }

                        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("cm_" + freeCompany.Key);
                        }

                        GetCharacterMenu(freeCompany.Value).Draw();
                        var tooltip = freeCompany.Value.FormattedName;

                        tooltip += "\n\nRight Click: Options";
                        ImGuiUtil.HoverTooltip(tooltip);
                        if (freeCompany.Value.ActualClassJob != null)
                        {
                            ImGui.SameLine();
                            var icon = ImGuiService.GetIconTexture(freeCompany.Value.Icon);
                            ImGui.Image(icon.ImGuiHandle, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale);
                        }
                    }
                    ImGui.NewLine();

                    var houses = _characterMonitor.GetHouses().Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).OrderBy(c => c.Value.FormattedName).ToList();
                    ImGui.TextUnformatted("Residences (" + houses.Count + ")");
                    ImGui.Separator();
                    for (var index = 0; index < houses.Count; index++)
                    {
                        var house = houses[index];
                        if (ImGui.Selectable(house.Value.FormattedName))
                        {
                            _selectedCharacter = house.Key;
                        }
                        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("cm_" + house.Key);
                        }

                        GetCharacterMenu(house.Value).Draw();
                        var tooltip = house.Value.FormattedName;
                        tooltip += "\n" + house.Value.GetPlotSize().ToString();

                        tooltip += "\n\nRight Click: Options";
                        ImGuiUtil.HoverTooltip(tooltip);

                        if (house.Value.ActualClassJob != null)
                        {
                            ImGui.SameLine();
                            var icon = ImGuiService.GetIconTexture(house.Value.Icon);
                            ImGui.Image(icon.ImGuiHandle, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale);
                        }
                    }
                    ImGui.NewLine();

                    var retainers = _characterMonitor.GetRetainerCharacters().Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).OrderBy(c => c.Value.FormattedName).ToList();
                    ImGui.TextUnformatted("Retainers (" + retainers.Count + ")");
                    ImGui.Separator();

                    for (var index = 0; index < characters.Count; index++)
                    {
                        var character = characters[index];
                        var characterRetainers = _characterMonitor.GetRetainerCharacters(character.Key).Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).OrderBy(c => c.Value.FormattedName).ToList();
                        ImGui.TextUnformatted(character.Value.FormattedName + " (" + characterRetainers.Count + ")");
                        ImGui.Separator();
                        for (var index2 = 0; index2 < characterRetainers.Count; index2++)
                        {
                            var characterRetainer = characterRetainers[index2];
                            retainers.Remove(characterRetainer);
                            if (ImGui.Selectable(characterRetainer.Value.FormattedName))
                            {
                                _selectedCharacter = characterRetainer.Key;
                            }
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup("cm_" + characterRetainer.Key);
                            }

                            GetCharacterMenu(characterRetainer.Value).Draw();
                            var tooltip = characterRetainer.Value.FormattedName;
                            if (characterRetainer.Value.ActualClassJob != null)
                            {
                                tooltip += "\n" + characterRetainer.Value.ActualClassJob?.Base.Name.ExtractText().ToTitleCase();
                            }

                            tooltip += "\n\nRight Click: Options";
                            ImGuiUtil.HoverTooltip(tooltip);
                            if (characterRetainer.Value.ActualClassJob != null)
                            {
                                ImGui.SameLine();
                                var icon = ImGuiService.GetIconTexture(characterRetainer.Value.Icon);
                                ImGui.Image(icon.ImGuiHandle, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale);
                            }
                        }
                        ImGui.NewLine();
                    }

                    if (retainers.Count != 0)
                    {
                        ImGui.TextUnformatted("Orphaned Retainers:");
                        ImGui.Separator();
                        for (var index2 = 0; index2 < retainers.Count; index2++)
                        {
                            var characterRetainer = retainers[index2];
                            if (ImGui.Selectable(characterRetainer.Value.FormattedName))
                            {
                                _selectedCharacter = characterRetainer.Key;
                            }

                            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup("cm_" + characterRetainer.Key);
                            }

                            GetCharacterMenu(characterRetainer.Value).Draw();
                            var tooltip = characterRetainer.Value.FormattedName;
                            if (characterRetainer.Value.ActualClassJob != null)
                            {
                                tooltip += "\n" + characterRetainer.Value.ActualClassJob?.Base.Name.ExtractText().ToTitleCase();
                            }

                            tooltip += "\n\nRight Click: Options";
                            ImGuiUtil.HoverTooltip(tooltip);
                            if (characterRetainer.Value.ActualClassJob != null)
                            {
                                ImGui.SameLine();
                                var icon = ImGuiService.GetIconTexture(characterRetainer.Value.Icon);
                                ImGui.Image(icon.ImGuiHandle, new Vector2(16, 16) * ImGui.GetIO().FontGlobalScale);
                            }
                        }

                        ImGui.NewLine();
                    }

                    World? selectedWorld = null;
                    if (_currentWorld != 0)
                    {
                        selectedWorld = _worldSheet.GetRowOrDefault(_currentWorld);
                    }

                    ImGui.Text("World: ");
                    using var combo = ImRaii.Combo("##activeWorld", selectedWorld?.Name.ExtractText() ?? "All");
                    if (combo.Success)
                    {
                        if (ImGui.Selectable("All"))
                        {
                            _currentWorld = 0;
                        }

                        var worlds = _worldSheet.Where(c => worldIds.Contains(c.RowId)).ToList();
                        foreach (var world in worlds)
                        {
                            if (ImGui.Selectable(world.Name.ExtractText()))
                            {
                                _currentWorld = world.RowId;
                                _selectedCharacter = 0;
                            }
                        }
                    }
                }
            }
            ImGui.SameLine();
            using (var main = ImRaii.Child("characterMain", new Vector2(0, 0), true))
            {
                if (main.Success)
                {
                    if (_selectedCharacter != 0)
                    {
                        var character = _characterMonitor.GetCharacterById(_selectedCharacter);
                        if (character != null)
                        {
                            ImGui.Text(character.FormattedName.ToString());

                            if (character.ActualClassJob != null)
                            {
                                ImGui.SameLine();
                                var icon = ImGuiService.GetIconTexture((uint)character.Icon);
                                ImGui.Image(icon.ImGuiHandle, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale);
                            }

                            ImGui.SameLine();
                            if(_editIcon.Draw(ImGuiService.LoadImage("edit"), "editName"))
                            {
                                _editMode = true;
                                _newName = character.AlternativeName ?? "";
                            }

                            ImGuiUtil.HoverTooltip("Edit name, set the name to blank to return it to the original name.");

                            if (_editMode)
                            {
                                var newName = _newName;
                                ImGui.Text("Custom Name: ");
                                ImGui.SameLine();
                                if (ImGui.InputText("##customName", ref newName, 100))
                                {
                                    _newName = newName;
                                }

                                if (character.AlternativeName != null && character.AlternativeName != character.Name)
                                {
                                    ImGui.Text("Original Name: " + character.Name);
                                }

                                if (ImGui.Button("Save"))
                                {
                                    if (_newName == "" || _newName == character.Name)
                                    {
                                        character.AlternativeName = null;
                                        _characterMonitor.UpdateCharacter(character);
                                        _editMode = false;
                                    }
                                    else
                                    {
                                        character.AlternativeName = _newName;
                                        _characterMonitor.UpdateCharacter(character);
                                        _editMode = false;
                                    }
                                }
                            }


                            ImGui.Separator();
                            if (character.CharacterType is CharacterType.Character or CharacterType.Retainer )
                            {
                                ImGui.Text("Level: " + character.Level);
                                ImGui.Text("Gil: " + character.Gil);
                                ImGui.Text("Gender: " + character.Gender);
                                ImGui.Text("Free Company: " + character.FreeCompanyName);
                                ImGui.Text("World: " + (character.World?.Name.ExtractText() ?? "Unknown"));
                                ImGui.Text("Class/Job: " +
                                           (character.ActualClassJob?.Base.Name.ExtractText().ToTitleCase() ?? "Unknown"));
                            }
                            else if (character.CharacterType is CharacterType.Housing)
                            {
                                ImGui.Text("World: " + (character.World?.Name.ExtractText() ?? "Unknown"));
                                ImGui.Text("Plot Size: " + character.GetPlotSize());
                                ImGui.Text("Location: " + character.HousingName);
                                ImGui.Text("Owners: ");
                                foreach (var ownerId in character.Owners)
                                {
                                    var owner = _characterMonitor.GetCharacterById(ownerId);
                                    var ownerName = owner?.FormattedName ?? "Missing Character";
                                    ImGui.Text(ownerName);
                                }
                            }
                            else if (character.CharacterType is CharacterType.FreeCompanyChest)
                            {
                                ImGui.Text("World: " + (character.World?.Name.ExtractText() ?? "Unknown"));
                                ImGui.Text("Related Characters: ");
                                foreach (var relatedCharacter in _characterMonitor.GetFreeCompanyCharacters(character.CharacterId))
                                {
                                    var relatedCharacterName = relatedCharacter.Value.FormattedName;
                                    ImGui.Text(relatedCharacterName);
                                }
                            }

                            ImGui.NewLine();
                            ImGui.Text("Inventories: ");
                            ImGui.Separator();
                            var inventories =
                                _inventoryMonitor.Inventories.ContainsKey(character.CharacterId)
                                    ? _inventoryMonitor.Inventories[character.CharacterId]
                                    : null;
                            if (inventories != null)
                            {
                                var categories = inventories.GetAllInventoriesByCategory();

                                using (var tabBar = ImRaii.TabBar("categories", ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton))
                                {
                                    if (tabBar.Success)
                                    {
                                        foreach (var category in categories)
                                        {
                                            var inventoryWidth = 5;
                                            using (var tabItem = ImRaii.TabItem(category.Key.FormattedName()))
                                            {
                                                if (tabItem.Success)
                                                {
                                                    using (var tabBar2 = ImRaii.TabBar("types", ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton))
                                                    {
                                                        if (tabBar2.Success)
                                                        {
                                                            var itemsByType = category.Value.GroupBy(c => c.SortedContainer);
                                                            foreach (var type in itemsByType)
                                                            {
                                                                using (var typeChild = ImRaii.TabItem(type.Key.FormattedName()))
                                                                {
                                                                    if (typeChild.Success)
                                                                    {
                                                                        var chunkedItems = type.OrderBy(c => c.Slot).Chunk(inventoryWidth);
                                                                        var realSlot = 1;
                                                                        foreach (var itemChunk in chunkedItems)
                                                                        {
                                                                            for (var index = 0;
                                                                                 index < itemChunk.Length;
                                                                                 index++)
                                                                            {
                                                                                var item = itemChunk[index];
                                                                                using (ImRaii.PushId(item.Slot))
                                                                                {
                                                                                    if (ImGui.ImageButton(item.ItemId == 0
                                                                                                ? ImGuiService
                                                                                                    .GetIconTexture(62574)
                                                                                                    .ImGuiHandle
                                                                                                : ImGuiService
                                                                                                    .GetIconTexture(item.Icon)
                                                                                                    .ImGuiHandle,
                                                                                            new Vector2(32, 32)))
                                                                                    {

                                                                                    }

                                                                                    ImGuiUtil.HoverTooltip(item.FormattedName +
                                                                                        " - " + item.Quantity + " in slot " +
                                                                                        realSlot);
                                                                                    ImGui.SameLine();

                                                                                    var hoveredRow = -1;

                                                                                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow)) {
                                                                                        hoveredRow = realSlot;
                                                                                    }

                                                                                    if (hoveredRow == realSlot && item.ItemId != 0 && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                                                                    {
                                                                                        ImGui.OpenPopup("RightClick" + realSlot);
                                                                                    }

                                                                                    if (hoveredRow == realSlot && item.ItemId != 0 && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                                                                                    {
                                                                                        messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), item.ItemId));
                                                                                    }

                                                                                    using (var popup = ImRaii.Popup("RightClick" + realSlot))
                                                                                    {
                                                                                        using var _ = ImRaii.PushId("RightClick" + realSlot);
                                                                                        if (popup.Success)
                                                                                        {
                                                                                            ImGuiService.ImGuiMenuService.DrawRightClickPopup(item, messages);
                                                                                        }
                                                                                    }
                                                                                }

                                                                                realSlot++;
                                                                            }

                                                                            ImGui.NewLine();
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


                            }
                            else
                            {
                                ImGui.Text("No inventories found.");
                            }

                        }
                        else
                        {
                            ImGui.Text("Invalid character selected.");
                        }
                    }
                }
            }
            return messages;
        }

        public override bool IsMenuItem => _isSeparator;
        public override bool DrawBorder => false;
    }
}