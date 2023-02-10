using CriticalCommonLib;
using CriticalCommonLib.Models;
using Dalamud.Logging;
using InventoryTools;

namespace InventoryToolsMock;

public class MockCharacterMonitor : ICharacterMonitor
{
    private ulong _activeRetainer;
    private ulong _activeCharacterId;
    public void Dispose()
    {
    }

    public MockCharacterMonitor()
    {
        _characters = new Dictionary<ulong, Character>();
    }

    public void UpdateCharacter(Character character)
    {
        Service.FrameworkService.RunOnFrameworkThread(() => { OnCharacterUpdated?.Invoke(character); });
    }

    public void RemoveCharacter(ulong characterId)
    {
        if (_characters.ContainsKey(characterId))
        {
            _characters.Remove(characterId);
            Service.FrameworkService.RunOnFrameworkThread(() => { OnCharacterRemoved?.Invoke(characterId); });
        }
    }

    public void RefreshActiveCharacter()
    {
        if (IsLoggedIn && LocalContentId != 0)
        {
            PluginLog.Verbose("CharacterMonitor: Character has changed to " + LocalContentId);
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
            Service.FrameworkService.RunOnFrameworkThread(() => { OnCharacterUpdated?.Invoke(character); });
        }
        else
        {
            Service.FrameworkService.RunOnFrameworkThread(() => { OnCharacterUpdated?.Invoke(null); });
        }
    }
    
    private Dictionary<ulong, Character> _characters;
    public Dictionary<ulong, Character> Characters => _characters;


    public event CharacterMonitor.ActiveRetainerChangedDelegate? OnActiveRetainerChanged;
    public event CharacterMonitor.ActiveRetainerChangedDelegate? OnActiveRetainerLoaded;
    public event CharacterMonitor.CharacterUpdatedDelegate? OnCharacterUpdated;
    public event CharacterMonitor.CharacterRemovedDelegate? OnCharacterRemoved;
    public event CharacterMonitor.CharacterJobChangedDelegate? OnCharacterJobChanged;
    public event CharacterMonitor.GilUpdatedDelegate? OnGilUpdated;
    
    public KeyValuePair<ulong, Character>[] GetPlayerCharacters()
    {
        return Characters.Where(c => c.Value.OwnerId == 0 && c.Key != 0 && c.Value.Name != "").ToArray();
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
        if (characterId != 0 && Characters.ContainsKey(characterId))
        {
            return Characters[characterId].OwnerId == ActiveCharacterId || Characters[characterId].CharacterId == ActiveCharacterId;
        }
        return false;
    }

    public KeyValuePair<ulong, Character>[] GetRetainerCharacters(ulong retainerId)
    {
        return Characters.Where(c => c.Value.OwnerId == retainerId && c.Key != 0 && c.Value.Name != "").ToArray();
    }

    public KeyValuePair<ulong, Character>[] GetRetainerCharacters()
    {
        return Characters.Where(c => c.Value.OwnerId != 0 && c.Key != 0 && c.Value.Name != "").ToArray();
    }

    public bool IsRetainer(ulong characterId)
    {
        if (Characters.ContainsKey(characterId))
        {
            return Characters[characterId].OwnerId != 0;
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

    public void LoadExistingRetainers(Dictionary<ulong, Character> characters)
    {
        foreach (var character in characters)
        {
            _characters[character.Key] = character.Value;
        }
    }

    public ulong ActiveRetainer
    {
        get
        {
            return 0;
        }
    }

    public ulong ActiveCharacterId
    {
        get
        {
            return 0;
        }
    }

    public Character? ActiveCharacter
    {
        get
        {
            return null;
        }
    }

    public bool IsLoggedIn
    {
        get
        {
            return false;
        }
    }

    public ulong LocalContentId
    {
        get
        {
            return 0;
        }
    }
    public void OverrideActiveCharacter(ulong activeCharacter)
    {
        _activeCharacterId = activeCharacter;
    }

    public void OverrideActiveRetainer(ulong activeRetainer)
    {
        _activeRetainer = activeRetainer;
    }
}