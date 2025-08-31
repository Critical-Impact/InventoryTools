using System;
using System.Collections.Generic;
using CriticalCommonLib.Models;

namespace InventoryTools.Logic.Editors;

public class InventorySearchScope : IEquatable<InventorySearchScope>
{
    public ulong? CharacterId { get; set; }
    public uint? WorldId { get; set; }
    public bool? ActiveCharacter { get; set; }
    public bool? ActiveWorld { get; set; }

    public HashSet<InventoryCategory>? Categories { get; set; }
    public HashSet<CharacterType>? CharacterTypes { get; set; }
    public InventorySearchScopeMode Mode { get; set; }
    public bool Invert { get; set; }

    public bool IncludeOwned { get; set; }

    //Add Enum With Mode, turn into combo box

    public void Reset()
    {
        CharacterId = null;
        WorldId = null;
        ActiveCharacter = null;
        ActiveWorld = null;
    }

    public bool Equals(InventorySearchScope? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        var inventoryCategories = Categories ?? [];
        var otherCategories = other.Categories ?? [];
        var characterTypes = CharacterTypes ?? [];
        var otherCharacterTypes = other.CharacterTypes ?? [];
        return CharacterId == other.CharacterId && WorldId == other.WorldId && ActiveCharacter == other.ActiveCharacter && ActiveWorld == other.ActiveWorld && inventoryCategories.SetEquals(otherCategories) && characterTypes.SetEquals(otherCharacterTypes) && Mode == other.Mode && Invert == other.Invert && IncludeOwned == other.IncludeOwned;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((InventorySearchScope)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(CharacterId);
        hashCode.Add(WorldId);
        hashCode.Add(ActiveCharacter);
        hashCode.Add(ActiveWorld);
        hashCode.Add(Categories);
        hashCode.Add(CharacterTypes);
        hashCode.Add((int)Mode);
        hashCode.Add(Invert);
        hashCode.Add(IncludeOwned);
        return hashCode.ToHashCode();
    }
}