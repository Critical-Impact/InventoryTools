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

    public string GetStatePath(ICompendiumType compendiumType, CompendiumSectionType compendiumSectionType)
    {
        string prefix = "";
        switch (compendiumSectionType)
        {
            case CompendiumSectionType.List:
                prefix = "list";
                break;
            case CompendiumSectionType.View:
                prefix = "view";
                break;
        }
        return Path.Join(this.GetStateDirectory(), prefix + "_" + compendiumType.Key + ".json");
    }

    public string GetStateDirectory()
    {
        return Path.Join(_pluginInterface.GetPluginConfigDirectory(), "compendium");
    }

    public async Task<SectionState> GetState(ICompendiumType compendiumType, CompendiumSectionType compendiumSectionType)
    {
        if (_loadedSections.TryGetValue(compendiumType, out var state))
        {
            return state;
        }
        var statePath = GetStatePath(compendiumType, compendiumSectionType);
        SectionState? sectionState = null;
        if (_reliableFileStorage.Exists(statePath))
        {
            var contents = await _reliableFileStorage.ReadAllTextAsync(statePath);
            sectionState = JsonConvert.DeserializeObject<SectionState>(contents);
        }

        if (sectionState == null)
        {
            sectionState = new SectionState();
            sectionState.SectionType = compendiumSectionType;
        }
        sectionState.CompendiumType = compendiumType;
        sectionState.PropertyChanged += SectionStateChanged;
        _loadedSections.Add(compendiumType, sectionState);
        return sectionState;
    }

    public async Task SaveState(SectionState sectionState, CompendiumSectionType compendiumSectionType)
    {
        var saveDirectory = new DirectoryInfo(GetStateDirectory());
        if (!saveDirectory.Exists)
        {
            saveDirectory.Create();
        }
        var savePath = GetStatePath(sectionState.CompendiumType, compendiumSectionType);
        var contents = JsonConvert.SerializeObject(sectionState);
        await _reliableFileStorage.WriteAllTextAsync(savePath, contents);
    }

    private void SectionStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is SectionState state)
        {
            _ = SaveState(state, state.SectionType);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var loadedSection in _loadedSections)
            {
                SaveState(loadedSection.Value, loadedSection.Value.SectionType).Wait();
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public enum CompendiumSectionType
{
    List,
    View
}