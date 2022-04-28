// See https://aka.ms/new-console-template for more information

using System.Collections.Generic;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools;
using InventoryTools.Logic;

//Not so simple way to test people's filters/inventory files for issues
var lumina = new Lumina.GameData( "H:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack" );
ExcelCache.Initialise(lumina);
var characterMonitor = new CharacterMonitor(true);
var pluginLogic = new PluginLogic(true);
PluginService.InitialiseTesting(characterMonitor, pluginLogic);
ConfigurationManager.Config = new InventoryToolsConfiguration();
var inventories = ConfigurationManager.LoadSavedInventories("inventories.json");
if (inventories != null)
{
    Character? mainCharacter = null;
    foreach (var inventory in inventories)
    {
        var character = new Character();
        if (inventory.Key.ToString().StartsWith("1"))
        {
            character.CharacterId = inventory.Key;
            character.Name = "Character";
            mainCharacter = character;
            characterMonitor.OverrideActiveCharacter(mainCharacter.CharacterId);
        }
        else if(mainCharacter != null)
        {
            character.CharacterId = inventory.Key;
            character.Name = "Retainer";
            character.OwnerId = mainCharacter.CharacterId;
        }
        characterMonitor.Characters.Add(inventory.Key, character);
    }
    var filterManager = new FilterManager(true);
    FilterConfiguration filterOut;
    FilterConfiguration.FromBase64("insert filter here", out filterOut);
    var sampleFilter = new FilterConfiguration("Duplicated SortedItems", FilterType.SortingFilter);
    sampleFilter.DisplayInTabs = true;
    sampleFilter.SourceAllCharacters = true;
    sampleFilter.SourceAllRetainers = true;
    sampleFilter.DestinationAllRetainers = true;
    sampleFilter.FilterItemsInRetainers = true;
    sampleFilter.HighlightWhen = "Always";

    var filteredList = filterManager.GenerateFilteredList(sampleFilter, inventories);
    foreach (var a in filteredList.AllItems)
    {
        
    }
}