using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryToolsMock;

public class MockInventoryScanner : IInventoryScanner
{
    public void Dispose()
    {

    }

    public void Enable()
    {
    }

    public event InventoryScanner.BagsChangedDelegate? BagsChanged;
    public event InventoryScanner.ContainerInfoReceivedDelegate? ContainerInfoReceived;
    public void ParseBags()
    {
    }

    public void LogBagChanges(BagChangeContainer bagChanges)
    {
    }

    public InventoryItem[] GetInventoryByType(ulong retainerId, InventoryType type)
    {
        return Array.Empty<InventoryItem>();
    }

    public InventoryItem[] GetInventoryByType(InventoryType type)
    {
        return Array.Empty<InventoryItem>();
    }

    public bool IsBagLoaded(InventoryType type)
    {
        return false;
    }

    public void ClearRetainerCache(ulong retainerId)
    {

    }

    public void ClearCache()
    {
    }

    public HashSet<InventoryType> LoadedInventories { get; set; }

    public HashSet<InventoryType> InMemory => new();
    public Dictionary<ulong, HashSet<InventoryType>> InMemoryRetainers => new();
    public InventoryItem[] CharacterBag1 => Array.Empty<InventoryItem>();
    public InventoryItem[] CharacterBag2 => Array.Empty<InventoryItem>();
    public InventoryItem[] CharacterBag3 => Array.Empty<InventoryItem>();
    public InventoryItem[] CharacterBag4 => Array.Empty<InventoryItem>();
    public InventoryItem[] CharacterEquipped => Array.Empty<InventoryItem>();
    public InventoryItem[] CharacterCrystals => Array.Empty<InventoryItem>();
    public InventoryItem[] CharacterCurrency => Array.Empty<InventoryItem>();
    public InventoryItem[] SaddleBag1 => Array.Empty<InventoryItem>();
    public InventoryItem[] SaddleBag2 => Array.Empty<InventoryItem>();
    public InventoryItem[] PremiumSaddleBag1 => Array.Empty<InventoryItem>();
    public InventoryItem[] PremiumSaddleBag2 => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryMainHand => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryHead => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryBody => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryHands => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryLegs => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryFeet => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryOffHand => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryEars => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryNeck => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryWrists => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmouryRings => Array.Empty<InventoryItem>();
    public InventoryItem[] ArmourySoulCrystals => Array.Empty<InventoryItem>();
    public InventoryItem[] FreeCompanyBag1 => Array.Empty<InventoryItem>();
    public InventoryItem[] FreeCompanyBag2 => Array.Empty<InventoryItem>();
    public InventoryItem[] FreeCompanyBag3 => Array.Empty<InventoryItem>();
    public InventoryItem[] FreeCompanyBag4 => Array.Empty<InventoryItem>();
    public InventoryItem[] FreeCompanyBag5 => Array.Empty<InventoryItem>();
    public InventoryItem[] FreeCompanyGil => Array.Empty<InventoryItem>();
    public InventoryItem[] FreeCompanyCrystals => Array.Empty<InventoryItem>();
    public InventoryItem[] Armoire => Array.Empty<InventoryItem>();
    public InventoryItem[] GlamourChest => Array.Empty<InventoryItem>();
    public Dictionary<ulong, InventoryItem[]> RetainerBag1 => new();
    public Dictionary<ulong, InventoryItem[]> RetainerBag2 => new();
    public Dictionary<ulong, InventoryItem[]> RetainerBag3 => new();
    public Dictionary<ulong, InventoryItem[]> RetainerBag4 => new();
    public Dictionary<ulong, InventoryItem[]> RetainerBag5 => new();
    public Dictionary<ulong, InventoryItem[]> RetainerEquipped => new();
    public Dictionary<ulong, InventoryItem[]> RetainerMarket => new();
    public Dictionary<ulong, InventoryItem[]> RetainerCrystals => new();
    public Dictionary<ulong, InventoryItem[]> RetainerGil => new();
    public Dictionary<ulong, uint[]> RetainerMarketPrices => new();
    public Dictionary<byte, uint[]> GearSets => new();
    public bool[] GearSetsUsed => new bool[0];

    public string[] GearSetNames
    {
        get
        {
            return new string[0];
        }
    }
    public HashSet<(byte, string)> GetGearSets(uint itemId)
    {
        return new HashSet<(byte, string)>();
    }

    public Dictionary<uint, HashSet<(byte, string)>> GetGearSets()
    {
        return new();
    }

    public unsafe void ParseCharacterBags(InventorySortOrder currentSortOrder, BagChangeContainer changeSet)
    {
    }

    public unsafe void ParseSaddleBags(InventorySortOrder currentSortOrder, BagChangeContainer changeSet)
    {
    }

    public unsafe void ParsePremiumSaddleBags(InventorySortOrder currentSortOrder, BagChangeContainer changeSet)
    {
    }

    public unsafe void ParseArmouryChest(InventorySortOrder currentSortOrder, BagChangeContainer changeSet)
    {
    }

    public unsafe void ParseCharacterEquipped(BagChangeContainer changeSet)
    {

    }

    public unsafe void ParseFreeCompanyBags(BagChangeContainer changeSet)
    {
    }

    public unsafe void ParseArmoire(BagChangeContainer changeSet)
    {
    }

    public unsafe void ParseGlamourChest(BagChangeContainer changeSet)
    {
    }

    public unsafe void ParseRetainerBags(InventorySortOrder currentSortOrder, BagChangeContainer changeSet)
    {
    }

    public unsafe bool ParseGearSets(BagChangeContainer changeSet)
    {
        return false;
    }
}