using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

namespace InventoryToolsMock;

public class MockCharacterMonitor : ICharacterMonitor
{
    private ulong _activeRetainerId;
    private ulong _activeCharacterId;
    private ulong _activeFreeCompanyId;
    private ulong _activeHouseId;
    public void Dispose()
    {
    }

    public MockCharacterMonitor()
    {
        _characters = new Dictionary<ulong, Character>();
    }

    public void UpdateCharacter(Character character)
    {
        Service.Framework.RunOnFrameworkThread(() => { OnCharacterUpdated?.Invoke(character); });
    }

    public void RemoveCharacter(ulong characterId)
    {
        if (_characters.ContainsKey(characterId))
        {
            _characters.Remove(characterId);
            Service.Framework.RunOnFrameworkThread(() => { OnCharacterRemoved?.Invoke(characterId); });
        }
    }

    public void RefreshActiveCharacter()
    {
        if (IsLoggedIn && LocalContentId != 0)
        {
            Service.Log.Verbose("CharacterMonitor: Character has changed to " + LocalContentId);
            Character character;
            if (_characters.ContainsKey(LocalContentId))
            {
                character = _characters[LocalContentId];
            }
            else
            {
                character = new Character();
                character.CharacterId = LocalContentId;
                _characters[character.CharacterId] = character;
            }
            //character.UpdateFromCurrentPlayer(Service.ClientState.LocalPlayer);
            Service.Framework.RunOnFrameworkThread(() => { OnCharacterUpdated?.Invoke(character); });
        }
        else
        {
            Service.Framework.RunOnFrameworkThread(() => { OnCharacterUpdated?.Invoke(null); });
        }
    }
    
    private Dictionary<ulong, Character> _characters;
    private bool _isLoggedIn;
    public Dictionary<ulong, Character> Characters => _characters;


    public event CharacterMonitor.ActiveRetainerChangedDelegate? OnActiveRetainerChanged;
    public event CharacterMonitor.ActiveRetainerChangedDelegate? OnActiveRetainerLoaded;
    public event CharacterMonitor.ActiveFreeCompanyChangedDelegate? OnActiveFreeCompanyChanged;
    public event CharacterMonitor.ActiveHouseChangedDelegate? OnActiveHouseChanged;
    public event CharacterMonitor.CharacterUpdatedDelegate? OnCharacterUpdated;
    public event CharacterMonitor.CharacterRemovedDelegate? OnCharacterRemoved;
    public event CharacterMonitor.CharacterJobChangedDelegate? OnCharacterJobChanged;
    public event ICharacterMonitor.CharacterLoginEventDelegate? OnCharacterLoggedIn;
    public event ICharacterMonitor.CharacterLoginEventDelegate? OnCharacterLoggedOut;

    public KeyValuePair<ulong, Character>[] GetPlayerCharacters()
    {
        return Characters.Where(c => c.Value.OwnerId == 0 && c.Value.CharacterType == CharacterType.Character && c.Key != 0 && c.Value.Name != "").ToArray();
    }

    public KeyValuePair<ulong, Character>[] GetFreeCompanies()
    {
        return Characters.Where(c => c.Value.OwnerId == 0 && c.Value.CharacterType == CharacterType.FreeCompanyChest && c.Key != 0 && c.Value.Name != "").ToArray();
    }

    public KeyValuePair<ulong, Character>[] GetHouses()
    {
        return Characters.Where(c => c.Value.OwnerId == 0 && c.Value.CharacterType == CharacterType.Housing && c.Key != 0 && c.Value.HousingName != "").ToArray();
    }

    public KeyValuePair<ulong, Character>[] AllCharacters()
    {
        return Characters.Where(c => c.Value.Name != "").ToArray();
    }

    public Character? GetCharacterByName(string name, ulong ownerId)
    {
        return Characters.Select(c => c.Value).FirstOrDefault(c => c.Name == name && c.OwnerId == ownerId);
    }

    public bool BelongsToActiveCharacter(ulong characterId)
    {
        if (IsFreeCompany(characterId))
        {
            var activeCharacter = ActiveCharacter;
            if (activeCharacter == null)
            {
                return false;
            }

            return activeCharacter.FreeCompanyId == characterId;
        }
        if (characterId != 0 && Characters.ContainsKey(characterId))
        {
            return Characters[characterId].OwnerId == ActiveCharacterId || Characters[characterId].CharacterId == ActiveCharacterId;
        }
        return false;
    }

    public KeyValuePair<ulong, Character>[] GetRetainerCharacters(ulong retainerId)
    {
        return Characters.Where(c => c.Value.OwnerId == retainerId && c.Value.CharacterType == CharacterType.Retainer && c.Key != 0 && c.Value.Name != "").ToArray();
    }

    public KeyValuePair<ulong, Character>[] GetRetainerCharacters()
    {
        return Characters.Where(c => c.Value.OwnerId != 0 && c.Value.CharacterType == CharacterType.Retainer && c.Key != 0 && c.Value.Name != "").ToArray();
    }

    public KeyValuePair<ulong, Character>[] GetCharacterHouses(ulong characterId)
    {
        return Characters.Where(c => c.Value.Owners.Contains(characterId) && c.Value.CharacterType == CharacterType.Housing && c.Key != 0 && c.Value.Name != "").ToArray();
    }
        
        
    public KeyValuePair<ulong, Character>[] GetCharacterHouses()
    {
        return Characters.Where(c => c.Value.Owners.Count != 0 && c.Value.CharacterType == CharacterType.Housing && c.Key != 0 && c.Value.HousingName != "").ToArray();
    }

    public bool IsCharacter(ulong characterId)
    {
        if (Characters.ContainsKey(characterId))
        {
            return Characters[characterId].CharacterType == CharacterType.Character;
        }
        return false;
    }

    public bool IsRetainer(ulong characterId)
    {
        if (Characters.ContainsKey(characterId))
        {
            return Characters[characterId].CharacterType == CharacterType.Retainer;
        }
        return false;
    }

    public bool IsFreeCompany(ulong characterId)
    {
        if (Characters.ContainsKey(characterId))
        {
            return Characters[characterId].CharacterType == CharacterType.FreeCompanyChest;
        }
        return false;
    }

    public bool IsHousing(ulong characterId)
    {
        if (Characters.ContainsKey(characterId))
        {
            return Characters[characterId].CharacterType == CharacterType.Housing;
        }
        return false;
    }

    public Character? GetCharacterById(ulong characterId)
    {
        if (Characters.ContainsKey(characterId))
        {
            return Characters[characterId];
        }
        return null;
    }
    
    public Character? GetParentCharacterById(ulong characterId)
    {
        var character = GetCharacterById(characterId);

        if (IsFreeCompany(characterId) || IsHousing(characterId)) return character;
        if (character != null)
        {
            return GetCharacterById(character.OwnerId);
        }

        return null;
    }
    
    public string GetCharacterNameById(ulong characterId, bool owner = false)
    {
        if (!owner) return GetCharacterById(characterId)?.FormattedName ?? "Unknown";
        var character = GetParentCharacterById(characterId);
        if (character != null && character.CharacterId == characterId)
            return "";
        return character?.FormattedName ?? "Unknown";
    }

    public void LoadExistingRetainers(Dictionary<ulong, Character> characters)
    {
        foreach (var character in characters)
        {
            _characters[character.Key] = character.Value;
        }
    }

    public ulong ActiveRetainerId
    {
        get
        {
            return _activeRetainerId;
        }
    }

    public ulong ActiveCharacterId
    {
        get
        {
            return _activeCharacterId;
        }
    }

    public ulong ActiveHouseId
    {
        get
        {
            return _activeHouseId;
        }
    }

    public HashSet<uint> GetWorldIds()
    {
        return Characters.Select(c => c.Value.WorldId).ToHashSet();
    }

    public KeyValuePair<ulong, Character>[] GetFreeCompanyCharacters(ulong freeCompanyId)
    {
        return Characters.Where(c => c.Value.Name != "" && (c.Value.CharacterType == CharacterType.Character && c.Value.FreeCompanyId == freeCompanyId)).ToArray();
    }

    public ulong ActiveFreeCompanyId => _activeFreeCompanyId;
    public ulong InternalCharacterId { get; }
    public bool InternalHasHousePermission { get; }
    public short InternalRoomId { get; }
    public byte InternalDivisionId { get; }
    public sbyte InternalPlotId { get; }
    public sbyte InternalWardId { get; }
    public ulong InternalHouseId { get; }

    public Character? ActiveCharacter =>
        _characters.ContainsKey(_activeCharacterId) ? _characters[_activeCharacterId] : null;

    public Character? ActiveFreeCompany =>
        _characters.ContainsKey(_activeFreeCompanyId) ? _characters[_activeFreeCompanyId] : null;
    
    public Character? ActiveRetainer =>
        _characters.ContainsKey(_activeRetainerId) ? _characters[_activeRetainerId] : null;    

    public bool IsLoggedIn
    {
        get
        {
            return _isLoggedIn;
        }
    }

    public ulong LocalContentId
    {
        get
        {
            return 0;
        }
    }

    public void OverrideIsLoggedIn(bool newValue)
    {
        _isLoggedIn = newValue;
    }
    public void OverrideActiveCharacter(ulong activeCharacter)
    {
        _activeCharacterId = activeCharacter;
    }

    public void OverrideActiveRetainer(ulong activeRetainer)
    {
        _activeRetainerId = activeRetainer;
    }

    public void OverrideActiveFreeCompany(ulong activeFreeCompanyId)
    {
        _activeFreeCompanyId = activeFreeCompanyId;
    }
}