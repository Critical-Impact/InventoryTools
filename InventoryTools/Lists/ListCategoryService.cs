using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic;

namespace InventoryTools.Lists;

public class ListCategoryService
{
    private readonly ICharacterMonitor _characterMonitor;
    private readonly InventoryToolsConfiguration _inventoryToolsConfiguration;

    public ListCategoryService(ICharacterMonitor characterMonitor, InventoryToolsConfiguration inventoryToolsConfiguration)
    {
        _characterMonitor = characterMonitor;
        _inventoryToolsConfiguration = inventoryToolsConfiguration;
    }


    public Dictionary<ulong, HashSet<InventoryCategory>> DestinationRetainerCategories(
        FilterConfiguration filterConfiguration)
    {
        var categoryValues = Enum.GetValues<InventoryCategory>();

        Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
        var allRetainers = _characterMonitor.GetRetainerCharacters().Where(c =>
        {
            var destinationIncludeCrossCharacter =
                filterConfiguration.DestinationIncludeCrossCharacter ?? _inventoryToolsConfiguration.DisplayCrossCharacter;
            return _characterMonitor.BelongsToActiveCharacter(c.Key) || destinationIncludeCrossCharacter;
        }).ToDictionary(c => c.Key, c => c.Value);
        if (filterConfiguration.DestinationAllRetainers == true)
        {
            foreach (var retainer in allRetainers)
            {
                foreach (var categoryValue in categoryValues)
                {
                    if (categoryValue.IsRetainerCategory())
                    {
                        if (!categories.ContainsKey(retainer.Key))
                        {
                            categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[retainer.Key].Contains(categoryValue))
                        {
                            categories[retainer.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        if (filterConfiguration.DestinationCategories != null)
        {
            foreach (var categoryValue in filterConfiguration.DestinationCategories)
            {
                foreach (var retainer in allRetainers)
                {
                    if (categoryValue.IsRetainerCategory())
                    {
                        if (!categories.ContainsKey(retainer.Key))
                        {
                            categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[retainer.Key].Contains(categoryValue))
                        {
                            categories[retainer.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        foreach (var category in filterConfiguration.DestinationInventories)
        {
            if (category.Item2.IsRetainerCategory())
            {
                if (!categories.ContainsKey(category.Item1))
                {
                    categories.Add(category.Item1, new HashSet<InventoryCategory>());
                }

                if (!categories[category.Item1].Contains(category.Item2))
                {
                    categories[category.Item1].Add(category.Item2);
                }
            }
        }

        return categories;
    }


    public Dictionary<ulong, HashSet<InventoryCategory>> DestinationFreeCompanyCategories(
        FilterConfiguration filterConfiguration)
    {
        var categoryValues = Enum.GetValues<InventoryCategory>();

        Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
        var allFreeCompanies = _characterMonitor.GetFreeCompanies().Where(c =>
        {
            var destinationIncludeCrossCharacter =
                filterConfiguration.DestinationIncludeCrossCharacter ?? _inventoryToolsConfiguration.DisplayCrossCharacter;
            return _characterMonitor.BelongsToActiveCharacter(c.Key) || destinationIncludeCrossCharacter;
        }).ToDictionary(c => c.Key, c => c.Value);

        if (filterConfiguration.DestinationAllRetainers == true)
        {
            foreach (var retainer in allFreeCompanies)
            {
                foreach (var categoryValue in categoryValues)
                {
                    if (categoryValue.IsFreeCompanyCategory())
                    {
                        if (!categories.ContainsKey(retainer.Key))
                        {
                            categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[retainer.Key].Contains(categoryValue))
                        {
                            categories[retainer.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        if (filterConfiguration.DestinationCategories != null)
        {
            foreach (var categoryValue in filterConfiguration.DestinationCategories)
            {
                foreach (var freeCompany in allFreeCompanies)
                {
                    if (categoryValue.IsFreeCompanyCategory())
                    {
                        if (!categories.ContainsKey(freeCompany.Key))
                        {
                            categories.Add(freeCompany.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[freeCompany.Key].Contains(categoryValue))
                        {
                            categories[freeCompany.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        foreach (var category in filterConfiguration.DestinationInventories)
        {
            if (category.Item2.IsFreeCompanyCategory())
            {
                if (!categories.ContainsKey(category.Item1))
                {
                    categories.Add(category.Item1, new HashSet<InventoryCategory>());
                }

                if (!categories[category.Item1].Contains(category.Item2))
                {
                    categories[category.Item1].Add(category.Item2);
                }
            }
        }

        return categories;
    }

    public Dictionary<ulong, HashSet<InventoryCategory>> DestinationHouseCategories(
        FilterConfiguration filterConfiguration)
    {
        var categoryValues = Enum.GetValues<InventoryCategory>();

        Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
        var allHouses = _characterMonitor.GetHouses().Where(c =>
        {
            var destinationIncludeCrossCharacter =
                filterConfiguration.DestinationIncludeCrossCharacter ?? _inventoryToolsConfiguration.DisplayCrossCharacter;
            return _characterMonitor.BelongsToActiveCharacter(c.Key) || destinationIncludeCrossCharacter;
        }).ToDictionary(c => c.Key, c => c.Value);

        if (filterConfiguration.DestinationAllHouses == true)
        {
            foreach (var house in allHouses)
            {
                foreach (var categoryValue in categoryValues)
                {
                    if (categoryValue.IsHousingCategory())
                    {
                        if (!categories.ContainsKey(house.Key))
                        {
                            categories.Add(house.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[house.Key].Contains(categoryValue))
                        {
                            categories[house.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        if (filterConfiguration.DestinationCategories != null)
        {
            foreach (var categoryValue in filterConfiguration.DestinationCategories)
            {
                foreach (var house in allHouses)
                {
                    if (categoryValue.IsHousingCategory())
                    {
                        if (!categories.ContainsKey(house.Key))
                        {
                            categories.Add(house.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[house.Key].Contains(categoryValue))
                        {
                            categories[house.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        foreach (var category in filterConfiguration.DestinationInventories)
        {
            if (category.Item2.IsHousingCategory())
            {
                if (!categories.ContainsKey(category.Item1))
                {
                    categories.Add(category.Item1, new HashSet<InventoryCategory>());
                }

                if (!categories[category.Item1].Contains(category.Item2))
                {
                    categories[category.Item1].Add(category.Item2);
                }
            }
        }

        return categories;
    }


    public Dictionary<ulong, HashSet<InventoryCategory>> DestinationCharacterCategories(
        FilterConfiguration filterConfiguration)
    {
        var categoryValues = Enum.GetValues<InventoryCategory>();

        Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
        var allCharacters = _characterMonitor.GetPlayerCharacters().Where(c =>
        {
            var destinationIncludeCrossCharacter =
                filterConfiguration.DestinationIncludeCrossCharacter ?? _inventoryToolsConfiguration.DisplayCrossCharacter;
            return _characterMonitor.BelongsToActiveCharacter(c.Key) || destinationIncludeCrossCharacter;
        }).ToDictionary(c => c.Key, c => c.Value);
        if (filterConfiguration.DestinationAllCharacters == true)
        {
            foreach (var character in allCharacters)
            {
                foreach (var categoryValue in categoryValues)
                {
                    if (categoryValue.IsCharacterCategory())
                    {
                        if (!categories.ContainsKey(character.Key))
                        {
                            categories.Add(character.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[character.Key].Contains(categoryValue))
                        {
                            categories[character.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        if (filterConfiguration.DestinationCategories != null)
        {
            foreach (var categoryValue in filterConfiguration.DestinationCategories)
            {
                foreach (var character in allCharacters)
                {
                    if (categoryValue.IsCharacterCategory())
                    {
                        if (!categories.ContainsKey(character.Key))
                        {
                            categories.Add(character.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[character.Key].Contains(categoryValue))
                        {
                            categories[character.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        foreach (var category in filterConfiguration.DestinationInventories)
        {
            if (category.Item2.IsCharacterCategory())
            {
                if (!categories.ContainsKey(category.Item1))
                {
                    categories.Add(category.Item1, new HashSet<InventoryCategory>());
                }

                if (!categories[category.Item1].Contains(category.Item2))
                {
                    categories[category.Item1].Add(category.Item2);
                }
            }
        }

        return categories;
    }


    public Dictionary<ulong, HashSet<InventoryCategory>> SourceRetainerCategories(
        FilterConfiguration filterConfiguration)
    {
        var categoryValues = Enum.GetValues<InventoryCategory>();

        Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
        var allRetainers = _characterMonitor.GetRetainerCharacters().Where(c =>
        {
            var sourceIncludeCrossCharacter =
                filterConfiguration.SourceIncludeCrossCharacter ?? _inventoryToolsConfiguration.DisplayCrossCharacter;
            return _characterMonitor.BelongsToActiveCharacter(c.Key) || sourceIncludeCrossCharacter;
        }).ToDictionary(c => c.Key, c => c.Value);
        if (filterConfiguration.SourceAllRetainers == true)
        {
            foreach (var retainer in allRetainers)
            {
                foreach (var categoryValue in categoryValues)
                {
                    if (categoryValue.IsRetainerCategory())
                    {
                        if (!categories.ContainsKey(retainer.Key))
                        {
                            categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[retainer.Key].Contains(categoryValue))
                        {
                            categories[retainer.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        if (filterConfiguration.SourceCategories != null)
        {
            foreach (var categoryValue in filterConfiguration.SourceCategories)
            {
                foreach (var retainer in allRetainers)
                {
                    if (categoryValue.IsRetainerCategory())
                    {
                        if (!categories.ContainsKey(retainer.Key))
                        {
                            categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[retainer.Key].Contains(categoryValue))
                        {
                            categories[retainer.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        foreach (var category in filterConfiguration.SourceInventories)
        {
            if (category.Item2.IsRetainerCategory())
            {
                if (!categories.ContainsKey(category.Item1))
                {
                    categories.Add(category.Item1, new HashSet<InventoryCategory>());
                }

                if (!categories[category.Item1].Contains(category.Item2))
                {
                    categories[category.Item1].Add(category.Item2);
                }
            }
        }


        if (filterConfiguration.SourceWorlds != null)
        {
            foreach (var retainer in allRetainers)
            {
                if (filterConfiguration.SourceWorlds.Contains(retainer.Value.WorldId))
                {
                    foreach (var categoryValue in categoryValues)
                    {
                        if (categoryValue.IsRetainerCategory())
                        {
                            if (!categories.ContainsKey(retainer.Key))
                            {
                                categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                            }

                            if (!categories[retainer.Key].Contains(categoryValue))
                            {
                                categories[retainer.Key].Add(categoryValue);
                            }
                        }
                    }
                }
            }
        }

        return categories;
    }


    public Dictionary<ulong, HashSet<InventoryCategory>> SourceFreeCompanyCategories(
        FilterConfiguration filterConfiguration)
    {
        var categoryValues = Enum.GetValues<InventoryCategory>();

        Dictionary<ulong, HashSet<InventoryCategory>> categories = new();

        var allFreeCompanies = _characterMonitor.GetFreeCompanies().Where(c =>
        {
            var sourceIncludeCrossCharacter =
                filterConfiguration.SourceIncludeCrossCharacter ?? _inventoryToolsConfiguration.DisplayCrossCharacter;
            return _characterMonitor.BelongsToActiveCharacter(c.Key) || sourceIncludeCrossCharacter;
        }).ToDictionary(c => c.Key, c => c.Value);

        if (filterConfiguration.SourceAllFreeCompanies == true)
        {
            foreach (var retainer in allFreeCompanies)
            {
                foreach (var categoryValue in categoryValues)
                {
                    if (categoryValue.IsFreeCompanyCategory())
                    {
                        if (!categories.ContainsKey(retainer.Key))
                        {
                            categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[retainer.Key].Contains(categoryValue))
                        {
                            categories[retainer.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        if (filterConfiguration.SourceCategories != null)
        {
            foreach (var categoryValue in filterConfiguration.SourceCategories)
            {
                foreach (var freeCompany in allFreeCompanies)
                {
                    if (categoryValue.IsFreeCompanyCategory())
                    {
                        if (!categories.ContainsKey(freeCompany.Key))
                        {
                            categories.Add(freeCompany.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[freeCompany.Key].Contains(categoryValue))
                        {
                            categories[freeCompany.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        foreach (var category in filterConfiguration.SourceInventories)
        {
            if (category.Item2.IsFreeCompanyCategory())
            {
                if (!categories.ContainsKey(category.Item1))
                {
                    categories.Add(category.Item1, new HashSet<InventoryCategory>());
                }

                if (!categories[category.Item1].Contains(category.Item2))
                {
                    categories[category.Item1].Add(category.Item2);
                }
            }
        }


        if (filterConfiguration.SourceWorlds != null)
        {
            foreach (var freeCompany in allFreeCompanies)
            {
                if (filterConfiguration.SourceWorlds.Contains(freeCompany.Value.WorldId))
                {
                    foreach (var categoryValue in categoryValues)
                    {
                        if (categoryValue.IsFreeCompanyCategory())
                        {
                            if (!categories.ContainsKey(freeCompany.Key))
                            {
                                categories.Add(freeCompany.Key, new HashSet<InventoryCategory>());
                            }

                            if (!categories[freeCompany.Key].Contains(categoryValue))
                            {
                                categories[freeCompany.Key].Add(categoryValue);
                            }
                        }
                    }
                }
            }
        }

        return categories;
    }


    public Dictionary<ulong, HashSet<InventoryCategory>> SourceHouseCategories(FilterConfiguration filterConfiguration)
    {
        var categoryValues = Enum.GetValues<InventoryCategory>();

        Dictionary<ulong, HashSet<InventoryCategory>> categories = new();

        var allHouses = _characterMonitor.GetHouses().Where(c =>
        {
            var sourceIncludeCrossCharacter =
                filterConfiguration.SourceIncludeCrossCharacter ?? _inventoryToolsConfiguration.DisplayCrossCharacter;
            return _characterMonitor.BelongsToActiveCharacter(c.Key) || sourceIncludeCrossCharacter;
        }).ToDictionary(c => c.Key, c => c.Value);

        if (filterConfiguration.SourceAllHouses == true)
        {
            foreach (var house in allHouses)
            {
                foreach (var categoryValue in categoryValues)
                {
                    if (categoryValue.IsHousingCategory())
                    {
                        if (!categories.ContainsKey(house.Key))
                        {
                            categories.Add(house.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[house.Key].Contains(categoryValue))
                        {
                            categories[house.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        if (filterConfiguration.SourceCategories != null)
        {
            foreach (var categoryValue in filterConfiguration.SourceCategories)
            {
                foreach (var categoryId in allHouses)
                {
                    if (categoryValue.IsHousingCategory())
                    {
                        if (!categories.ContainsKey(categoryId.Key))
                        {
                            categories.Add(categoryId.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[categoryId.Key].Contains(categoryValue))
                        {
                            categories[categoryId.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        foreach (var category in filterConfiguration.SourceInventories)
        {
            if (category.Item2.IsHousingCategory())
            {
                if (!categories.ContainsKey(category.Item1))
                {
                    categories.Add(category.Item1, new HashSet<InventoryCategory>());
                }

                if (!categories[category.Item1].Contains(category.Item2))
                {
                    categories[category.Item1].Add(category.Item2);
                }
            }
        }


        if (filterConfiguration.SourceWorlds != null)
        {
            foreach (var house in allHouses)
            {
                if (filterConfiguration.SourceWorlds.Contains(house.Value.WorldId))
                {
                    foreach (var categoryValue in categoryValues)
                    {
                        if (categoryValue.IsHousingCategory())
                        {
                            if (!categories.ContainsKey(house.Key))
                            {
                                categories.Add(house.Key, new HashSet<InventoryCategory>());
                            }

                            if (!categories[house.Key].Contains(categoryValue))
                            {
                                categories[house.Key].Add(categoryValue);
                            }
                        }
                    }
                }
            }
        }

        return categories;
    }


    public Dictionary<ulong, HashSet<InventoryCategory>> SourceCharacterCategories(
        FilterConfiguration filterConfiguration)
    {
        var categoryValues = Enum.GetValues<InventoryCategory>();

        Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
        var allCharacters = _characterMonitor.GetPlayerCharacters().Where(c =>
        {
            var sourceIncludeCrossCharacter =
                filterConfiguration.SourceIncludeCrossCharacter ?? _inventoryToolsConfiguration.DisplayCrossCharacter;
            return _characterMonitor.BelongsToActiveCharacter(c.Key) || sourceIncludeCrossCharacter;
        }).ToDictionary(c => c.Key, c => c.Value);
        if (filterConfiguration.SourceAllCharacters == true)
        {
            foreach (var character in allCharacters)
            {
                foreach (var categoryValue in categoryValues)
                {
                    if (categoryValue.IsCharacterCategory())
                    {
                        if (!categories.ContainsKey(character.Key))
                        {
                            categories.Add(character.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[character.Key].Contains(categoryValue))
                        {
                            categories[character.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        if (filterConfiguration.SourceCategories != null)
        {
            foreach (var categoryValue in filterConfiguration.SourceCategories)
            {
                foreach (var character in allCharacters)
                {
                    if (categoryValue.IsCharacterCategory())
                    {
                        if (!categories.ContainsKey(character.Key))
                        {
                            categories.Add(character.Key, new HashSet<InventoryCategory>());
                        }

                        if (!categories[character.Key].Contains(categoryValue))
                        {
                            categories[character.Key].Add(categoryValue);
                        }
                    }
                }
            }
        }

        foreach (var category in filterConfiguration.SourceInventories)
        {
            if (category.Item2.IsCharacterCategory())
            {
                if (!categories.ContainsKey(category.Item1))
                {
                    categories.Add(category.Item1, new HashSet<InventoryCategory>());
                }

                if (!categories[category.Item1].Contains(category.Item2))
                {
                    categories[category.Item1].Add(category.Item2);
                }
            }
        }

        if (filterConfiguration.SourceWorlds != null)
        {
            foreach (var retainer in allCharacters)
            {
                if (filterConfiguration.SourceWorlds.Contains(retainer.Value.WorldId))
                {
                    foreach (var categoryValue in categoryValues)
                    {
                        if (categoryValue.IsRetainerCategory())
                        {
                            if (!categories.ContainsKey(retainer.Key))
                            {
                                categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                            }

                            if (!categories[retainer.Key].Contains(categoryValue))
                            {
                                categories[retainer.Key].Add(categoryValue);
                            }
                        }
                    }
                }
            }
        }

        return categories;
    }
}