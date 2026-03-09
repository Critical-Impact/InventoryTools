using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using Newtonsoft.Json;

namespace InventoryTools.Compendium.Services;

public class CompendiumSectionStateService : IDisposable
{
    private readonly IReliableFileStorage _reliableFileStorage;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly Dictionary<ICompendiumType, SectionState> _loadedSections;

    public CompendiumSectionStateService(IReliableFileStorage reliableFileStorage, IDalamudPluginInterface pluginInterface)
    {
        _reliableFileStorage = reliableFileStorage;
        _pluginInterface = pluginInterface;
        _loadedSections = new();
    }

    public string GetStatePath(ICompendiumType compendiumType)
    {
        return Path.Join(this.GetStateDirectory(), compendiumType.Key + ".json");
    }

    public string GetStateDirectory()
    {
        return Path.Join(_pluginInterface.GetPluginConfigDirectory(), "compendium");
    }

    public async Task<SectionState> GetState(ICompendiumType compendiumType)
    {
        if (_loadedSections.TryGetValue(compendiumType, out var state))
        {
            return state;
        }
        var statePath = GetStatePath(compendiumType);
        SectionState? sectionState = null;
        if (_reliableFileStorage.Exists(statePath))
        {
            var contents = await _reliableFileStorage.ReadAllTextAsync(statePath);
            sectionState = JsonConvert.DeserializeObject<SectionState>(contents);
        }

        if (sectionState == null)
        {
            sectionState = new SectionState();
        }
        sectionState.CompendiumType = compendiumType;
        sectionState.PropertyChanged += SectionStateChanged;
        _loadedSections.Add(compendiumType, sectionState);
        return sectionState;
    }

    public async Task SaveState(SectionState sectionState)
    {
        var saveDirectory = new DirectoryInfo(GetStateDirectory());
        if (!saveDirectory.Exists)
        {
            saveDirectory.Create();
        }
        var savePath = GetStatePath(sectionState.CompendiumType);
        var contents = JsonConvert.SerializeObject(sectionState);
        await _reliableFileStorage.WriteAllTextAsync(savePath, contents);
    }

    private void SectionStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is SectionState state)
        {
            _ = SaveState(state);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var loadedSection in _loadedSections)
            {
                SaveState(loadedSection.Value).Wait();
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}