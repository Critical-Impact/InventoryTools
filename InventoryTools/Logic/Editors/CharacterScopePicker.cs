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

namespace CharacterTools.Logic.Editors;

public class CharacterSearchScope
{
    public ulong? CharacterId { get; set; }
    public uint? WorldId { get; set; }
    public bool? ActiveCharacter { get; set; }
    public bool? ActiveWorld { get; set; }

    public HashSet<CharacterType>? CharacterTypes { get; set; }
    public CharacterSearchScopeMode Mode { get; set; }
    public bool Invert { get; set; }

    public bool IncludeOwned { get; set; }

    public void Reset()
    {
        CharacterId = null;
        WorldId = null;
        ActiveCharacter = null;
        ActiveWorld = null;
    }
}

public enum CharacterSearchScopeMode
{
    Normal,
    Invert
}


public class CharacterScopeCalculator
{
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IInventoryMonitor _inventoryMonitor;
    private Dictionary<ulong, uint> _characterWorldIds = new Dictionary<ulong, uint>();
    private Dictionary<ulong, CharacterType> _characterTypes = new Dictionary<ulong, CharacterType>();

    public CharacterScopeCalculator(ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor)
    {
        _characterMonitor = characterMonitor;
        _inventoryMonitor = inventoryMonitor;
    }

    public bool Filter(IEnumerable<CharacterSearchScope> searchScopes, InventoryItem characterItem)
    {
        return searchScopes.Any(c => Filter(c, characterItem));
    }

    public bool Filter(CharacterSearchScope searchScope, InventoryItem characterItem)
    {
        bool topLevelMatch = false;
        if (searchScope.CharacterId != null)
        {
            if (characterItem.RetainerId == searchScope.CharacterId)
            {
                topLevelMatch = true;
            }
        }
        else if (searchScope.WorldId != null)
        {
            if (!_characterWorldIds.ContainsKey(characterItem.RetainerId))
            {
                var character = _characterMonitor.GetCharacterById(characterItem.RetainerId);
                if (character == null)
                {
                    return false;
                }
                _characterWorldIds[characterItem.RetainerId] = character.WorldId;
            }

            if (_characterWorldIds[characterItem.RetainerId] == searchScope.WorldId)
            {
                topLevelMatch = true;
            }
        }
        else if (searchScope.ActiveCharacter is true)
        {
            if(_characterMonitor.BelongsToActiveCharacter(characterItem.RetainerId))
            {
                topLevelMatch = true;
            }
        }
        else if (searchScope.ActiveWorld is true)
        {
            if (!_characterWorldIds.ContainsKey(characterItem.RetainerId))
            {
                var character = _characterMonitor.GetCharacterById(characterItem.RetainerId);
                if (character == null)
                {
                    return false;
                }
                _characterWorldIds[characterItem.RetainerId] = character.WorldId;
            }

            if (_characterWorldIds[characterItem.RetainerId] == _characterMonitor.ActiveCharacter?.WorldId)
            {
                topLevelMatch = true;
            }
        }
        else
        {
            topLevelMatch = true;
        }

        var secondLevelMatch = true;
        if (topLevelMatch)
        {
            if (searchScope.CharacterTypes != null)
            {
                if (!_characterTypes.ContainsKey(characterItem.RetainerId))
                {
                    var character = _characterMonitor.GetCharacterById(characterItem.RetainerId);
                    if (character == null)
                    {
                        return false;
                    }
                    _characterTypes[characterItem.RetainerId] = character.CharacterType;
                }

                if (!searchScope.CharacterTypes.Contains(_characterTypes[characterItem.RetainerId]))
                {
                    secondLevelMatch = false;
                }
            }
        }
        else
        {
            secondLevelMatch = false;
        }

        return searchScope.Invert ? !secondLevelMatch : secondLevelMatch;
    }

    public bool Filter(IEnumerable<CharacterSearchScope> searchScopes, Character character)
    {
        return searchScopes.Any(c => Filter(c, character.CharacterId));
    }

    public bool Filter(IEnumerable<CharacterSearchScope> searchScopes, ulong characterId)
    {
        return searchScopes.Any(c => Filter(c, characterId));
    }

    public bool Filter(CharacterSearchScope searchScope, Character character)
    {
        return Filter(searchScope, character.CharacterId);
    }

    public bool Filter(CharacterSearchScope searchScope, ulong characterId)
    {
        bool topLevelMatch = false;
        if (searchScope.CharacterId != null)
        {
            if (characterId == searchScope.CharacterId)
            {
                topLevelMatch = true;
            }
        }
        else if (searchScope.WorldId != null)
        {
            if (!_characterWorldIds.ContainsKey(characterId))
            {
                var character = _characterMonitor.GetCharacterById(characterId);
                if (character == null)
                {
                    return false;
                }
                _characterWorldIds[characterId] = character.WorldId;
            }

            if (_characterWorldIds[characterId] == searchScope.WorldId)
            {
                topLevelMatch = true;
            }
        }
        else if (searchScope.ActiveCharacter is true)
        {
            if(_characterMonitor.BelongsToActiveCharacter(characterId))
            {
                topLevelMatch = true;
            }
        }
        else if (searchScope.ActiveWorld is true)
        {
            if (!_characterWorldIds.ContainsKey(characterId))
            {
                var character = _characterMonitor.GetCharacterById(characterId);
                if (character == null)
                {
                    return false;
                }
                _characterWorldIds[characterId] = character.WorldId;
            }

            if (_characterWorldIds[characterId] == _characterMonitor.ActiveCharacter?.WorldId)
            {
                topLevelMatch = true;
            }
        }
        else
        {
            topLevelMatch = true;
        }

        var secondLevelMatch = true;
        if (topLevelMatch)
        {
            if (searchScope.CharacterTypes != null)
            {
                if (!_characterTypes.ContainsKey(characterId))
                {
                    var character = _characterMonitor.GetCharacterById(characterId);
                    if (character == null)
                    {
                        return false;
                    }
                    _characterTypes[characterId] = character.CharacterType;
                }

                if (!searchScope.CharacterTypes.Contains(_characterTypes[characterId]))
                {
                    secondLevelMatch = false;
                }
            }
        }
        else
        {
            secondLevelMatch = false;
        }

        return searchScope.Invert ? !secondLevelMatch : secondLevelMatch;
    }

    public uint Count(IEnumerable<CharacterSearchScope> searchScopes, uint itemId, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags itemFlags = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)
    {
        return (uint)searchScopes.Sum(c => Count(c, itemId, itemFlags));
    }

    public uint Count(CharacterSearchScope searchScope, uint itemId, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags itemFlags = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)
    {
        var characterIds = new HashSet<Character>();

        foreach (var character in _characterMonitor.Characters.Select(c => c.Value))
        {
            if (searchScope.CharacterId != null)
            {
                if (character.CharacterId == searchScope.CharacterId || searchScope.Invert)
                {
                    characterIds.Add(character);
                }
            }
            else if (searchScope.WorldId != null)
            {
                if (character.WorldId == searchScope.WorldId || searchScope.Invert)
                {
                    if (searchScope.CharacterTypes != null)
                    {
                        if (searchScope.CharacterTypes.Contains(character.CharacterType) || searchScope.Invert)
                        {
                            characterIds.Add(character);
                        }
                    }
                    else
                    {
                        characterIds.Add(character);
                    }
                }
            }
            else if (searchScope.ActiveCharacter is true)
            {
                if(_characterMonitor.BelongsToActiveCharacter(character.CharacterId) || searchScope.Invert)
                {
                    if (searchScope.CharacterTypes != null)
                    {
                        if (searchScope.CharacterTypes.Contains(character.CharacterType) || searchScope.Invert)
                        {
                            characterIds.Add(character);
                        }
                    }
                    else
                    {
                        characterIds.Add(character);
                    }
                }
            }
            else if (searchScope.ActiveWorld is true)
            {
                if (character.WorldId == _characterMonitor.ActiveCharacter?.WorldId || searchScope.Invert)
                {
                    if (searchScope.CharacterTypes != null)
                    {
                        if (searchScope.CharacterTypes.Contains(character.CharacterType) || searchScope.Invert)
                        {
                            characterIds.Add(character);
                        }
                    }
                    else
                    {
                        characterIds.Add(character);
                    }
                }
            }
            else
            {
                if (searchScope.CharacterTypes != null)
                {
                    if (searchScope.CharacterTypes.Contains(character.CharacterType) || searchScope.Invert)
                    {
                        characterIds.Add(character);
                    }
                }
                else
                {
                    characterIds.Add(character);
                }
            }
        }

        return (uint)characterIds.Select(c => _inventoryMonitor.CharacterItemCounts.GetValueOrDefault((itemId, itemFlags, c.CharacterId), 0)).Sum(c => c);
    }
}

public class CharacterScopePicker
{
    private readonly ICharacterMonitor _characterMonitor;
    private readonly ImGuiService _imGuiService;
    private readonly ExcelSheet<World> _worldSheet;
    private readonly HoverImageButton _editButton;

    public CharacterScopePicker(ICharacterMonitor characterMonitor, ImGuiService imGuiService, ExcelSheet<World> worldSheet)
    {
        _characterMonitor = characterMonitor;
        _imGuiService = imGuiService;
        _worldSheet = worldSheet;
        _editButton = new("editButton");
    }

    public string GetScopeName(CharacterSearchScope scope)
    {
        var scopeName = "";
        if (scope.Mode == CharacterSearchScopeMode.Invert)
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

    private CharacterSearchScope? _selectedScope;

    public bool Draw(string label, List<CharacterSearchScope> searchScopes)
    {

        var changed = false;
        var fakeRef = searchScopes.Count + " items selected.";
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
                ImGui.Text("Character Scope Editor");
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
                                    _selectedScope = new CharacterSearchScope();
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
                        ImGui.TextWrapped("The character scope editor allows you define which characters you want to search across.");
                        ImGui.TextWrapped("By default, every character Allagan Tools knows about is searched.");
                        ImGui.TextWrapped("By providing a set of scopes, you are narrowing down from which characters are displayed/counted.");
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
                                    _imGuiService.HelpMarker("Match against all characters");
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
                                    _imGuiService.HelpMarker("Match against the currently logged in character.");
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
                                    if (ImGui.Button("Delete") && _selectedScope != null)
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

        return changed;
    }
}