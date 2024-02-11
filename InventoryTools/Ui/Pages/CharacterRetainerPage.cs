using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Extensions;
using InventoryTools.Ui.Widgets;
using Lumina.Excel.GeneratedSheets;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Sections
{
    public class CharacterRetainerPage : IConfigPage
    {
        private bool _isSeparator = false;
        public string Name { get; } = "Characters/Retainers";

        private ulong _selectedCharacter = 0;
        private uint _currentWorld = 0;
        
        private bool _editMode = false;
        private string _newName = "";
        
        private HoverButton _editIcon { get; } = new(PluginService.IconStorage.LoadImage("edit"),  new Vector2(16, 16));
        
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
                PluginService.CharacterMonitor.RemoveCharacter(characterId);
                PluginService.InventoryMonitor.ClearCharacterInventories(characterId);
            }
        }

        private void ClearInventories(string arg1, bool arg2)
        {
            if (arg2)
            {
                var characterId = ulong.Parse(arg1.Split("ci_", StringSplitOptions.RemoveEmptyEntries)[0]);
                PluginService.InventoryMonitor.ClearCharacterInventories(characterId);
            }
        }

        public void Draw()
        {
            using (var sidebar = ImRaii.Child("charactersBar", new Vector2(160, 0) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (sidebar.Success)
                {
                    var worldIds = PluginService.CharacterMonitor.GetWorldIds();
                    var characters = PluginService.CharacterMonitor.GetPlayerCharacters().Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).ToList();
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
                            tooltip += "\n" + character.Value.ActualClassJob?.FormattedNameEnglish;
                        }

                        tooltip += "\n\nRight Click: Options";
                        ImGuiUtil.HoverTooltip(tooltip);
                        ImGui.SameLine();
                        if (character.Value.ActualClassJob != null)
                        {
                            var icon = PluginService.IconStorage[character.Value.Icon];
                            ImGui.Image(icon.ImGuiHandle, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale);
                        }
                    }
                    ImGui.NewLine();
                    
                    var freeCompanies = PluginService.CharacterMonitor.GetFreeCompanies().Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).ToList();
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
                            var icon = PluginService.IconStorage[freeCompany.Value.Icon];
                            ImGui.Image(icon.ImGuiHandle, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale);
                        }
                    }
                    ImGui.NewLine();
                    
                    var houses = PluginService.CharacterMonitor.GetHouses().Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).ToList();
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
                            var icon = PluginService.IconStorage[house.Value.Icon];
                            ImGui.Image(icon.ImGuiHandle, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale);
                        }
                    }
                    ImGui.NewLine();
                    
                    var retainers = PluginService.CharacterMonitor.GetRetainerCharacters().Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).ToList();
                    ImGui.TextUnformatted("Retainers (" + retainers.Count + ")");
                    ImGui.Separator();

                    for (var index = 0; index < characters.Count; index++)
                    {
                        var character = characters[index];
                        var characterRetainers = PluginService.CharacterMonitor.GetRetainerCharacters(character.Key).Where(c => _currentWorld == 0 || _currentWorld == c.Value.WorldId).ToList();
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
                                tooltip += "\n" + characterRetainer.Value.ActualClassJob?.FormattedNameEnglish;
                            }

                            tooltip += "\n\nRight Click: Options";
                            ImGuiUtil.HoverTooltip(tooltip);
                            if (characterRetainer.Value.ActualClassJob != null)
                            {
                                ImGui.SameLine();
                                var icon = PluginService.IconStorage[characterRetainer.Value.Icon];
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
                                tooltip += "\n" + characterRetainer.Value.ActualClassJob?.FormattedNameEnglish;
                            }

                            tooltip += "\n\nRight Click: Options";
                            ImGuiUtil.HoverTooltip(tooltip);
                            if (characterRetainer.Value.ActualClassJob != null)
                            {
                                ImGui.SameLine();
                                var icon = PluginService.IconStorage[characterRetainer.Value.Icon];
                                ImGui.Image(icon.ImGuiHandle, new Vector2(16, 16) * ImGui.GetIO().FontGlobalScale);
                            }
                        }

                        ImGui.NewLine();
                    }

                    WorldEx? selectedWorld = null;
                    if (_currentWorld != 0)
                    {
                        selectedWorld = Service.ExcelCache.GetWorldSheet().GetRow(_currentWorld);
                    }

                    ImGui.Text("World: ");
                    using var combo = ImRaii.Combo("##activeWorld", selectedWorld?.FormattedName ?? "All");
                    if (combo.Success)
                    {
                        if (ImGui.Selectable("All"))
                        {
                            _currentWorld = 0;
                        }

                        var worlds = Service.ExcelCache.GetWorldSheet().Where(c => worldIds.Contains(c.RowId)).ToList();
                        foreach (var world in worlds)
                        {
                            if (ImGui.Selectable(world.FormattedName))
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
                        var character = PluginService.CharacterMonitor.GetCharacterById(_selectedCharacter);
                        if (character != null)
                        {
                            ImGui.Text(character.FormattedName.ToString());
                            
                            if (character.ActualClassJob != null)
                            {
                                ImGui.SameLine();
                                var icon = PluginService.IconStorage[character.Icon];
                                ImGui.Image(icon.ImGuiHandle, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale);
                            }
                            
                            ImGui.SameLine();
                            if(_editIcon.Draw("editName"))
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
                                        PluginService.CharacterMonitor.UpdateCharacter(character);
                                        _editMode = false;
                                    }
                                    else
                                    {
                                        character.AlternativeName = _newName;
                                        PluginService.CharacterMonitor.UpdateCharacter(character);
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
                                ImGui.Text("World: " + (character.World?.FormattedName ?? "Unknown"));
                                ImGui.Text("Class/Job: " +
                                           (character.ActualClassJob?.NameEnglish.ToString() ?? "Unknown"));
                            }
                            else if (character.CharacterType is CharacterType.Housing)
                            {
                                ImGui.Text("World: " + (character.World?.FormattedName ?? "Unknown"));
                                ImGui.Text("Plot Size: " + character.GetPlotSize());
                                ImGui.Text("Location: " + character.HousingName);
                                ImGui.Text("Owners: ");
                                foreach (var ownerId in character.Owners)
                                {
                                    var owner = PluginService.CharacterMonitor.GetCharacterById(ownerId);
                                    var ownerName = owner?.FormattedName ?? "Missing Character";
                                    ImGui.Text(ownerName);
                                }
                            }
                            else if (character.CharacterType is CharacterType.FreeCompanyChest)
                            {
                                ImGui.Text("World: " + (character.World?.FormattedName ?? "Unknown"));
                                ImGui.Text("Related Characters: ");
                                foreach (var relatedCharacter in PluginService.CharacterMonitor.GetFreeCompanyCharacters(character.CharacterId))
                                {
                                    var relatedCharacterName = relatedCharacter.Value.FormattedName;
                                    ImGui.Text(relatedCharacterName);
                                }
                            }
                            
                            ImGui.NewLine();
                            ImGui.Text("Inventories: ");
                            ImGui.Separator();
                            var inventories =
                                PluginService.InventoryMonitor.Inventories.ContainsKey(character.CharacterId)
                                    ? PluginService.InventoryMonitor.Inventories[character.CharacterId]
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
                                                                                                ? PluginService
                                                                                                    .IconStorage[62574]
                                                                                                    .ImGuiHandle
                                                                                                : PluginService
                                                                                                    .IconStorage[item.Icon]
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
                                                                                        PluginService.WindowService.OpenItemWindow(item.ItemId);   
                                                                                    }

                                                                                    using (var popup = ImRaii.Popup("RightClick" + realSlot))
                                                                                    {
                                                                                        using var _ = ImRaii.PushId("RightClick" + realSlot);
                                                                                        if (popup.Success)
                                                                                        {
                                                                                            item.Item.DrawRightClickPopup();
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
        }

        private static void RenderFreeCompanyTable()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.BeginTable("FreeCompanyTable", 4, ImGuiTableFlags.BordersV |
                                                        ImGuiTableFlags.BordersOuterV |
                                                        ImGuiTableFlags.BordersInnerV |
                                                        ImGuiTableFlags.BordersH |
                                                        ImGuiTableFlags.BordersOuterH |
                                                        ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)0);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)1);
                ImGui.TableSetupColumn("Display Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)3);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)4);
                ImGui.TableHeadersRow();
                var characters = PluginService.CharacterMonitor.GetFreeCompanies();
                if (characters.Length == 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TextUnformatted("No free companies available.");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                }

                for (var index = 0; index < characters.Length; index++)
                {
                    ImGui.TableNextRow();
                    var character = characters[index].Value;
                    ImGui.TableNextColumn();
                    if (character.Name != "")
                    {
                        ImGui.TextUnformatted(character.Name);
                        ImGui.SameLine();
                    }

                    ImGui.TableNextColumn();
                    if (character.WorldId != 0)
                    {
                        ImGui.TextUnformatted(character.World?.Name ?? "Unknown");
                        ImGui.SameLine();
                    }

                    ImGui.TableNextColumn();
                    var value = character.AlternativeName ?? "";
                    if (ImGui.InputText("##" + index + "Input", ref value, 150))
                    {
                        if (value == "")
                        {
                            character.AlternativeName = null;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                        else
                        {
                            character.AlternativeName = value;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                    }

                    ImGui.TableNextColumn();
                    if (character.CharacterId != PluginService.CharacterMonitor.ActiveCharacterId)
                    {
                        if (ImGui.SmallButton("Remove##" + index))
                        {
                            PluginService.CharacterMonitor.RemoveCharacter(character.CharacterId);
                        }

                    }

                    if (ImGui.SmallButton("Clear All Bags##" + index))
                    {
                        ImGui.OpenPopup("Are you sure?##" + index);
                    }

                    if (ImGui.BeginPopupModal("Are you sure?##" + index))
                    {
                        ImGui.TextUnformatted(
                            "Are you sure you want to clear all the bags stored for this free company?.\nThis operation cannot be undone!\n\n");
                        ImGui.Separator();

                        if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            PluginService.InventoryMonitor.ClearCharacterInventories(character.CharacterId);
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.SetItemDefaultFocus();
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.EndPopup();
                    }
                }

                ImGui.EndTable();
            }
            ImGui.PopStyleVar();
        }
        private static void RenderHouseTable()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.BeginTable("HouseTable", 6, ImGuiTableFlags.BordersV |
                                                        ImGuiTableFlags.BordersOuterV |
                                                        ImGuiTableFlags.BordersInnerV |
                                                        ImGuiTableFlags.BordersH |
                                                        ImGuiTableFlags.BordersOuterH |
                                                        ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)0);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)1);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)2);
                ImGui.TableSetupColumn("Owners", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)3);
                ImGui.TableSetupColumn("Display Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)4);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)5);
                ImGui.TableHeadersRow();
                var houses = PluginService.CharacterMonitor.GetCharacterHouses();
                if (houses.Length == 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TextUnformatted("No houses available.");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                }

                for (var index = 0; index < houses.Length; index++)
                {
                    ImGui.TableNextRow();
                    var character = houses[index].Value;
                    ImGui.TableNextColumn();
                    if (character.HousingName != "")
                    {
                        ImGui.TextUnformatted(character.HousingName);
                        ImGui.SameLine();
                    }

                    ImGui.TableNextColumn();
                    if (character.WorldId != 0)
                    {
                        ImGui.TextUnformatted(character.World?.Name ?? "Unknown");
                        ImGui.SameLine();
                    }
                    
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(character.GetPlotSize().ToString());
                    ImGui.SameLine();

                    ImGui.TableNextColumn();
                    if (character.Owners.Count != 0)
                    {
                        ImGui.TextUnformatted(String.Join(",",character.Owners.Select(c => PluginService.CharacterMonitor.GetCharacterById(c)).Where(c => c != null).Select(c => c!.FormattedName).ToList()));
                        ImGui.SameLine();
                    }
                    
                    ImGui.TableNextColumn();
                    var value = character.AlternativeName ?? "";
                    if (ImGui.InputText("##" + index + "Input", ref value, 150))
                    {
                        if (value == "")
                        {
                            character.AlternativeName = null;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                        else
                        {
                            character.AlternativeName = value;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                    }

                    ImGui.TableNextColumn();
                    if (character.CharacterId != PluginService.CharacterMonitor.ActiveCharacterId)
                    {
                        if (ImGui.SmallButton("Remove##" + index))
                        {
                            PluginService.CharacterMonitor.RemoveCharacter(character.CharacterId);
                        }

                        ImGui.SameLine();
                    }

                    if (ImGui.SmallButton("Clear All Bags##" + index))
                    {
                        ImGui.OpenPopup("Are you sure?##" + index);
                    }

                    if (ImGui.BeginPopupModal("Are you sure?##" + index))
                    {
                        ImGui.TextUnformatted(
                            "Are you sure you want to clear all storage for this house?.\nThis operation cannot be undone!\n\n");
                        ImGui.Separator();

                        if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            PluginService.InventoryMonitor.ClearCharacterInventories(character.CharacterId);
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.SetItemDefaultFocus();
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.EndPopup();
                    }
                }

                ImGui.EndTable();
            }
            ImGui.PopStyleVar();
        }

        public void RenderRetainerTable(List<KeyValuePair<ulong, Character>> retainers)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.BeginTable("RetainerTable", 8, ImGuiTableFlags.BordersV |
                                                         ImGuiTableFlags.BordersOuterV |
                                                         ImGuiTableFlags.BordersInnerV |
                                                         ImGuiTableFlags.BordersH |
                                                         ImGuiTableFlags.BordersOuterH |
                                                         ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Hire Order", ImGuiTableColumnFlags.WidthStretch, 30.0f, (uint) 0);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 70.0f, (uint) 1);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthStretch, 70.0f, (uint) 2);
                ImGui.TableSetupColumn("Gil", ImGuiTableColumnFlags.WidthStretch, 30.0f, (uint) 3);
                ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthStretch, 40.0f, (uint) 4);
                ImGui.TableSetupColumn("Owner", ImGuiTableColumnFlags.WidthStretch, 60.0f, (uint) 5);
                ImGui.TableSetupColumn("Display Name", ImGuiTableColumnFlags.WidthStretch, 80.0f, (uint) 6);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 80.0f, (uint) 7);
                ImGui.TableHeadersRow();
                if (retainers.Count == 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TextUnformatted("No retainers available.");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                }

                for (var index = 0; index < retainers.Count; index++)
                {
                    ImGui.TableNextRow();
                    var character = retainers[index].Value;
                    
                    ImGui.TableNextColumn();
                    {
                        ImGui.TextUnformatted((character.HireOrder + 1).ToString());
                        ImGui.SameLine();
                    }
                    
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(character.Name);
                    ImGui.SameLine();
                    
                    ImGui.TableNextColumn();
                    if (character.WorldId != 0)
                    {
                        ImGui.TextUnformatted(character.World?.Name ?? "Unknown");
                        ImGui.SameLine();
                    }
                    
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(character.Gil.ToString());
                    ImGui.SameLine();
                    
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(character.Level.ToString());
                    ImGui.SameLine();
                    
                    ImGui.TableNextColumn();
                    var characterName = "Unknown";
                    if (PluginService.CharacterMonitor.Characters.ContainsKey(character.OwnerId))
                    {
                        var owner = PluginService.CharacterMonitor.Characters[character.OwnerId];
                        characterName = owner.FormattedName;
                    }

                    ImGui.TextUnformatted(characterName);
                    
                    ImGui.TableNextColumn();
                    var value = character.AlternativeName ?? "";
                    if (ImGui.InputText("##"+index+"Input", ref value, 150))
                    {
                        if (value == "")
                        {
                            character.AlternativeName = null;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                        else
                        {
                            character.AlternativeName = value;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                    }
                    ImGui.TableNextColumn();
                    if (character.CharacterId != PluginService.CharacterMonitor.LocalContentId)
                    {
                        if (ImGui.SmallButton("Remove##" + index))
                        {
                            PluginService.CharacterMonitor.RemoveCharacter(character.CharacterId);
                        }

                    }

                    if (ImGui.SmallButton("Clear All Bags##" + index))
                    {
                        ImGui.OpenPopup("Are you sure?##" + index);
                    }
                    if (ImGui.BeginPopupModal("Are you sure?##" + index))
                    {
                        ImGui.TextUnformatted(
                            "Are you sure you want to clear all the bags stored for this retainer?.\nThis operation cannot be undone!\n\n");
                        ImGui.Separator();

                        if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            PluginService.InventoryMonitor.ClearCharacterInventories(character.CharacterId);
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.SetItemDefaultFocus();
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.EndPopup();
                    }
                }

                ImGui.EndTable();
            }

            ImGui.PopStyleVar();            
        }

        public bool IsMenuItem => _isSeparator;
        public bool DrawBorder => false;
    }
}