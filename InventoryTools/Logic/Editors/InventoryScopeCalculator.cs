using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

namespace InventoryTools.Logic.Editors;

public class InventoryScopeCalculator
{
    private readonly ICharacterMonitor _characterMonitor;
    private ConcurrentDictionary<ulong, uint> _characterWorldIds = new ConcurrentDictionary<ulong, uint>();
    private ConcurrentDictionary<ulong, CharacterType> _characterTypes = new ConcurrentDictionary<ulong, CharacterType>();
    
    public InventoryScopeCalculator(ICharacterMonitor characterMonitor)
    {
        _characterMonitor = characterMonitor;
    }
    
    public bool Filter(IEnumerable<InventorySearchScope> searchScopes, InventoryItem inventoryItem)
    {
        return searchScopes.Any(c => Filter((InventorySearchScope)c, inventoryItem.RetainerId, inventoryItem.SortedCategory));
    }
    
    public bool Filter(IEnumerable<InventorySearchScope> searchScopes, ulong characterId, InventoryCategory category)
    {
        return searchScopes.Any(c => Filter(c, characterId, category));
    }
    
    public bool Filter(IEnumerable<InventorySearchScope> searchScopes, ulong characterId, InventoryType inventoryType)
    {
        return searchScopes.Any(c => Filter(c, characterId, inventoryType.ToInventoryCategory()));
    }
    
    public bool Filter(InventorySearchScope searchScope, ulong characterId, InventoryCategory category)
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
            if (searchScope.Categories != null)
            {
                if (!searchScope.Categories.Contains(category))
                {
                    secondLevelMatch = false;
                }
            }
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
}