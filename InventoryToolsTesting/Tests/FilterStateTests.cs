using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic;
using InventoryToolsTesting.Tests.Abstract;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace InventoryToolsTesting.Tests;

[TestFixture]
public class FilterStateTests : BaseTest
{
    [Test]
    public void TestBagHighlights()
    {
        var filterConfiguration = new FilterConfiguration("Test", FilterType.SearchFilter);
        filterConfiguration.HighlightColor = new Vector4(1,1,1,1);
        filterConfiguration.DestinationHighlightColor = new Vector4(2,2,2,2);
        var filterState = Host.Services.GetRequiredService<FilterState>();
        filterState.Initialize(filterConfiguration);
        var characterMonitor = Host.Services.GetRequiredService<ICharacterMonitor>();
        var inventoryMonitor = Host.Services.GetRequiredService<IInventoryMonitor>();
        var fakeItem = new InventoryItem(InventoryType.Bag0, 0, 6677, 1, 0,00, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None, 0,0,0,0,0,0,0,0,0,0,0,0,0);
        fakeItem.RetainerId = 1;
        var inventory = new List<InventoryItem>() { fakeItem};
        var character = new Character();
        character.CharacterId = 1;
        characterMonitor.LoadExistingRetainers(new Dictionary<ulong, Character>()
        {
            {1,character}
        });
        inventoryMonitor.LoadExistingData(inventory);
        characterMonitor.OverrideActiveCharacter(1);
        var bagLayout = filterState.GenerateBagLayout(InventoryType.Bag0);
        var bagHighlights = filterState.GetBagHighlights(InventoryType.Bag0, new List<SearchResult>(){new (fakeItem)});
        var a = "";
    }

    [Test]
    public void TestBagLayouts()
    {
        var filterState = Host.Services.GetRequiredService<FilterState>();
        var bagLayout = filterState.GenerateBagLayout(InventoryType.Bag0);
        Assert.AreEqual(35, bagLayout.Count);

    }
}