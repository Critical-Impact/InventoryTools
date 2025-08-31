using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Logic.Editors;

public class InventoryScopePicker
{
    private readonly ICharacterMonitor _characterMonitor;
    private readonly ImGuiService _imGuiService;
    private readonly ExcelSheet<World> _worldSheet;
    private readonly HoverImageButton _editButton;

    public InventoryScopePicker(ICharacterMonitor characterMonitor, ImGuiService imGuiService, ExcelSheet<World> worldSheet)
    {
        _characterMonitor = characterMonitor;
        _imGuiService = imGuiService;
        _worldSheet = worldSheet;
        _editButton = new("editButton");
    }

    public string GetScopeName(InventorySearchScope scope)
    {
        var scopeName = "";
        if (scope.Mode == InventorySearchScopeMode.Invert)
        {
            scopeName += "Exclude ";
        }
        if (scope.CharacterId != null && scope.CharacterId != 0)
        {
            var character = _characterMonitor.GetCharacterById(scope.CharacterId.Value);
            scopeName += character?.FormattedName ?? "Unknown Character";
        }
        if (scope.ActiveCharacter != null)
        {
            scopeName += "Active Character";
        }

        if (scope.WorldId != null && scope.WorldId != 0)
        {
            var world = _worldSheet.GetRowOrDefault(scope.WorldId.Value);
            scopeName += world?.Name.ExtractText() ?? "Unknown World";
        }

        if (scopeName == "")
        {
            scopeName = "All";
        }

        if (scope.Invert)
        {
            scopeName += " (Invert)";
        }

        return scopeName;
    }

    private InventorySearchScope? _selectedScope;

    public bool ValidateSearchScopes(List<InventorySearchScope> searchScopes)
    {
        var wasChanged = false;
        foreach (var scope in searchScopes)
        {
            if (scope.CharacterId != null && scope.CharacterId != 0)
            {
                var character = _characterMonitor.GetCharacterById(scope.CharacterId.Value);
                if (character != null && scope.Categories != null)
                {
                    foreach (var category in scope.Categories)
                    {
                        if (!category.IsApplicable(character.CharacterType))
                        {
                            scope.Categories.Remove(category);
                            wasChanged = true;
                            break;
                        }
                    }
                }
            }
        }

        return wasChanged;
    }

    public bool Draw(string label, List<InventorySearchScope> searchScopes)
    {

        var changed = false;
        var fakeRef = searchScopes.Count + " scopes defined.";
        using (var disabled = ImRaii.Disabled())
        {
            if (disabled)
            {
                ImGui.InputText(label, ref fakeRef, 200);
            }
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            using var tt = ImRaii.Tooltip();
            foreach (var searchScope in searchScopes)
            {
                ImGui.TextUnformatted(GetScopeName(searchScope));
                if (searchScope.Categories != null)
                {
                    using (var indent = ImRaii.PushIndent())
                    {
                        foreach (var category in searchScope.Categories)
                        {
                            ImGui.Text(category.FormattedDetailedName());
                        }
                    }
                }

                if (searchScope.CharacterTypes != null)
                {
                    using (var indent = ImRaii.PushIndent())
                    {
                        foreach (var characterType in searchScope.CharacterTypes)
                        {
                            ImGui.Text(characterType.FormattedName() + " (All Inventories)");
                        }
                    }
                }
            }
        }

        ImGui.SameLine();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        if (_editButton.Draw(_imGuiService.LoadImage("edit").GetWrapOrEmpty().Handle, new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale))
        {
            ImGui.OpenPopup("scopePopup");
            ImGui.SetNextWindowPos(cursorScreenPos);
        }

        ImGui.SetNextWindowSize(new Vector2(600, 600));
        using(var combo = ImRaii.Popup("scopePopup", ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoSavedSettings))
        {
            if (combo)
            {
                ImGui.Text("Inventory Scope Editor");
                using(var child = ImRaii.Child("selected", new Vector2(200, 0) * ImGui.GetIO().FontGlobalScale , true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (child)
                    {
                        using (var main = ImRaii.Child("main", new Vector2(0, -24) * ImGui.GetIO().FontGlobalScale))
                        {
                            if (main)
                            {
                                if (searchScopes.Count == 0)
                                {
                                    ImGui.TextWrapped("No scopes defined yet. Press add to start.");
                                }

                                for (var index = 0; index < searchScopes.Count; index++)
                                {
                                    var searchScope = searchScopes[index];
                                    if (ImGui.Selectable(GetScopeName(searchScope) + "##" + index, _selectedScope == searchScope))
                                    {
                                        _selectedScope = searchScope;
                                    }

                                    if (searchScope.Categories != null)
                                    {
                                        using (var indent = ImRaii.PushIndent())
                                        {
                                            foreach (var category in searchScope.Categories)
                                            {
                                                ImGui.Text(category.FormattedDetailedName());
                                            }
                                        }
                                    }

                                    if (searchScope.CharacterTypes != null)
                                    {
                                        using (var indent = ImRaii.PushIndent())
                                        {
                                            foreach (var characterType in searchScope.CharacterTypes)
                                            {
                                                ImGui.Text(characterType.FormattedName());
                                            }
                                        }
                                    }

                                    ImGui.Separator();
                                }
                            }
                        }

                        using (var commandBar = ImRaii.Child("commandBar", new Vector2(0, 24) * ImGui.GetIO().FontGlobalScale))
                        {
                            if (commandBar)
                            {
                                if (ImGui.Button("Add"))
                                {
                                    _selectedScope = new InventorySearchScope();
                                    searchScopes.Add(_selectedScope);
                                    changed = true;
                                }
                                ImGui.SameLine();
                                if (ImGui.Button("Save"))
                                {
                                    ImGui.CloseCurrentPopup();
                                }
                            }
                        }
                    }
                }

                ImGui.SameLine();
                using (var child = ImRaii.Child("editor", new Vector2(0, 0), true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (_selectedScope == null)
                    {
                        ImGui.TextWrapped("The inventory scope editor allows you define which inventories you want to search across.");
                        ImGui.TextWrapped("By default, every inventory Allagan Tools knows about is searched.");
                        ImGui.TextWrapped("By providing a set of scopes, you are narrowing down which inventories are displayed.");
                    }
                    else
                    {
                        if (child)
                        {
                            using (var main = ImRaii.Child("main", new Vector2(0, -24) * ImGui.GetIO().FontGlobalScale))
                            {
                                if (main)
                                {
                                    var isCharacter = _selectedScope.CharacterId != null;
                                    var isWorld = _selectedScope.WorldId != null;
                                    var isActiveCharacter = _selectedScope.ActiveCharacter != null;
                                    ImGui.Text("Search Scope:");
                                    ImGui.Separator();
                                    if (ImGui.RadioButton("All",!isCharacter && !isWorld && !isActiveCharacter))
                                    {
                                        _selectedScope.Reset();
                                    }
                                    ImGui.SameLine();
                                    _imGuiService.HelpMarker("Match against all inventories");
                                    ImGui.NewLine();

                                    if (ImGui.RadioButton("Character",isCharacter))
                                    {
                                        _selectedScope.Reset();
                                        _selectedScope.CharacterId = 0;
                                    }
                                    ImGui.SameLine();
                                    _imGuiService.HelpMarker("Match against a specific character(player character, retainer, free company, etc)");

                                    if (_selectedScope.CharacterId != null)
                                    {
                                        var selectedCharacter = _characterMonitor.GetCharacterById(_selectedScope.CharacterId.Value);
                                        using (var characterSelector = ImRaii.Combo("##character",
                                                   selectedCharacter?.FormattedName ?? "Select Character"))
                                        {
                                            if (characterSelector)
                                            {
                                                var allCharacters = _characterMonitor.AllCharacters();
                                                var byType = allCharacters.GroupBy(c => c.Value.CharacterType);
                                                foreach (var type in byType)
                                                {
                                                    ImGui.Text(type.Key.FormattedName());
                                                    ImGui.Separator();
                                                    foreach (var character in type)
                                                    {
                                                        if (ImGui.Selectable(character.Value.FormattedName))
                                                        {
                                                            _selectedScope.CharacterId = character.Key;
                                                        }
                                                    }
                                                    ImGui.NewLine();
                                                }
                                            }
                                        }
                                    }
                                    ImGui.NewLine();
                                    if (ImGui.RadioButton("Active Character",isActiveCharacter))
                                    {
                                        _selectedScope.Reset();
                                        _selectedScope.ActiveCharacter = true;
                                    }
                                    ImGui.SameLine();
                                    _imGuiService.HelpMarker("Match against the currently logged in character. This includes all retainers/free companies/etc owned by the character. Use categories or character types to filter down further.");
                                    ImGui.NewLine();

                                    if (ImGui.RadioButton("World",isWorld))
                                    {
                                        _selectedScope.Reset();
                                        _selectedScope.WorldId = 0;
                                    }
                                    ImGui.SameLine();
                                    _imGuiService.HelpMarker("Match against a specific world");
                                    if (_selectedScope.WorldId != null)
                                    {
                                        var selectedWorld = _selectedScope.WorldId == 0 ? null : _worldSheet.GetRowOrDefault(_selectedScope.WorldId.Value);
                                        using (var worldSelector = ImRaii.Combo("##world",
                                                   selectedWorld?.Name.ExtractText() ?? "Select World"))
                                        {
                                            if (worldSelector)
                                            {
                                                foreach (var world in _worldSheet.Where(c => c.IsPublic))
                                                {
                                                    if (ImGui.Selectable(world.Name.ExtractText()))
                                                    {
                                                        _selectedScope.WorldId = world.RowId;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    ImGui.NewLine();
                                    ImGui.Separator();

                                    string categoryPreview;
                                    if (_selectedScope.Categories != null)
                                    {
                                        categoryPreview = String.Join(", ", _selectedScope.Categories.Select(c => c.FormattedDetailedName()));
                                    }
                                    else
                                    {
                                        categoryPreview = "Select categories";
                                    }

                                    ImGui.LabelText("##categories", "Inventory Categories: ");
                                    using (var categorySelector = ImRaii.Combo("##categories",categoryPreview))
                                    {
                                        if (categorySelector)
                                        {
                                            var inventoryCategories = Enum.GetValues<InventoryCategory>();
                                            if (_selectedScope.CharacterId != null)
                                            {
                                                var character = _characterMonitor.GetCharacterById(_selectedScope.CharacterId.Value);
                                                if (character != null)
                                                {
                                                    inventoryCategories = inventoryCategories.Where(c =>
                                                        c.IsApplicable(character.CharacterType)).ToArray();
                                                }
                                            }
                                            foreach (var category in inventoryCategories)
                                            {
                                                if (ImGui.Selectable(category.FormattedDetailedName(), _selectedScope.Categories?.Contains(category) ?? false))
                                                {
                                                    if (_selectedScope.Categories == null)
                                                    {
                                                        _selectedScope.Categories = new();
                                                    }

                                                    if (_selectedScope.Categories.Contains(category))
                                                    {
                                                        _selectedScope.Categories.Remove(category);
                                                        if (_selectedScope.Categories.Count == 0)
                                                        {
                                                            _selectedScope.Categories = null;
                                                        }
                                                        changed = true;
                                                    }
                                                    else
                                                    {
                                                        _selectedScope.Categories.Add(category);
                                                        changed = true;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    ImGui.SameLine();
                                    _imGuiService.HelpMarker("When a category is selected, only items from this category will be shown. Select an item again to unselect it.");

                                    if (_selectedScope.CharacterId == null)
                                    {
                                        string characterTypesPreview;
                                        if (_selectedScope.CharacterTypes != null)
                                        {
                                            characterTypesPreview = String.Join(", ",
                                                _selectedScope.CharacterTypes.Select(c => c.FormattedName()));
                                        }
                                        else
                                        {
                                            characterTypesPreview = "Select character types";
                                        }

                                        ImGui.LabelText("##characterTypesLabel", "Character Types: ");
                                        using (var characterTypeSelector =
                                               ImRaii.Combo("##characterTypes", characterTypesPreview))
                                        {
                                            if (characterTypeSelector)
                                            {
                                                foreach (var category in Enum.GetValues<CharacterType>().Where(c =>
                                                             c != CharacterType.Unknown && c != CharacterType.Orphaned))
                                                {
                                                    if (ImGui.Selectable(category.FormattedName(),
                                                            _selectedScope.CharacterTypes?.Contains(category) ?? false))
                                                    {
                                                        if (_selectedScope.CharacterTypes == null)
                                                        {
                                                            _selectedScope.CharacterTypes = new();
                                                        }

                                                        if (_selectedScope.CharacterTypes.Contains(category))
                                                        {
                                                            _selectedScope.CharacterTypes.Remove(category);
                                                            if (_selectedScope.CharacterTypes.Count == 0)
                                                            {
                                                                _selectedScope.CharacterTypes = null;
                                                            }

                                                            changed = true;
                                                        }
                                                        else
                                                        {
                                                            _selectedScope.CharacterTypes.Add(category);
                                                            changed = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        ImGui.SameLine();
                                        _imGuiService.HelpMarker("When 'All' or 'World' is selected, choose the types of characters you want to filter against. Select an item again to unselect it.");
                                    }

                                    ImGui.Separator();
                                    ImGui.NewLine();
                                    var invert = _selectedScope.Invert;
                                    if (ImGui.Checkbox("Invert", ref invert))
                                    {
                                        _selectedScope.Invert = invert;
                                        changed = true;
                                    }

                                    ImGui.SameLine();
                                    _imGuiService.HelpMarker("When checked, match against the opposite of what is selected.");
                                }
                            }

                            using (var commandBar = ImRaii.Child("commandBar", new Vector2(0, 24) * ImGui.GetIO().FontGlobalScale))
                            {
                                if (commandBar)
                                {
                                    if (ImGui.Button("Save"))
                                    {
                                        _selectedScope = null;
                                        changed = true;
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("Delete"))
                                    {
                                        searchScopes.Remove(_selectedScope);
                                        _selectedScope = null;
                                        changed = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (ValidateSearchScopes(searchScopes))
        {
            changed = true;
        }
        return changed;
    }
}