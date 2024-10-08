using System;
using System.Collections.Generic;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Filters;
using Microsoft.Extensions.Hosting;

namespace InventoryTools.Services.Interfaces
{
    public interface IListService : IDisposable, IHostedService
    {
        List<FilterConfiguration> Lists { get; }

        bool AddList(FilterConfiguration configuration);
        bool AddList(string name, FilterType filterType);
        void AddDefaultColumns(FilterConfiguration configuration);
        FilterConfiguration DuplicateList(FilterConfiguration configuration, string newName);
        FilterConfiguration AddNewCraftList(string? name = null,bool? isEphemeral = false);
        FilterConfiguration AddNewCuratedList(string? name = null);
        bool RemoveList(FilterConfiguration configuration);
        bool RemoveList(string name);
        bool RemoveFilterByKey(string key);
        ColumnConfiguration AddColumn(FilterConfiguration configuration, Type columnType, bool notify = true);

        ColumnConfiguration AddCraftColumn(FilterConfiguration configuration, Type columnType,
            bool notify = true);

        FilterConfiguration? GetActiveUiList(bool ignoreWindowState);
        FilterConfiguration? GetActiveBackgroundList();
        FilterConfiguration? GetActiveCraftList();

        FilterConfiguration? GetActiveList();
        bool HasActiveList();
        bool HasActiveUiList();
        bool HasActiveBackgroundList();
        bool HasActiveCraftList();

        FilterConfiguration? GetList(string name);
        FilterConfiguration? GetListByKey(string key);
        FilterConfiguration? GetListByKeyOrName(string keyOrName);
        bool SetActiveUiList(string name);
        bool SetActiveUiList(FilterConfiguration configuration);
        bool SetActiveUiListByKey(string key);
        bool SetActiveBackgroundList(string name);
        bool SetActiveBackgroundList(FilterConfiguration configuration);
        bool SetActiveBackgroundListByKey(string key);
        bool SetActiveCraftList(FilterConfiguration configuration);
        bool SetActiveCraftListByKey(string key);
        bool ClearActiveUiList();
        bool ClearActiveBackgroundList();
        bool ClearActiveCraftList();
        bool ToggleActiveUiList(string name);
        bool ToggleActiveUiList(FilterConfiguration configuration);
        bool ToggleActiveBackgroundList(string name);
        bool ToggleActiveBackgroundList(FilterConfiguration configuration);
        bool ToggleActiveCraftList(FilterConfiguration configuration);

        bool MoveListUp(FilterConfiguration configuration);

        bool MoveListDown(FilterConfiguration configuration);

        void InvalidateList(FilterConfiguration configuration);

        void InvalidateLists(FilterType? filterType = null);

        void RefreshList(FilterConfiguration configuration);

        void ResetFilter(IEnumerable<IFilter> toReset, FilterConfiguration configuration);
        void ResetFilter(IEnumerable<IFilter> toReset, FilterConfiguration configuration, FilterConfiguration existingConfiguration);

        FilterConfiguration GenerateDefaultCraftList();
        void AddRecommendedColumns(IEnumerable<IColumn> columns, FilterConfiguration configuration);

        delegate void ListAddedDelegate(FilterConfiguration configuration);
        delegate void ListRemovedDelegate(FilterConfiguration configuration);
        delegate void ListConfigurationChangedDelegate(FilterConfiguration configuration);
        delegate void ListTableConfigurationChangedDelegate(FilterConfiguration configuration);
        delegate void ListRefreshedDelegate(FilterConfiguration configuration);
        delegate void ListRepositionedDelegate(FilterConfiguration configuration);
        delegate void ListToggledDelegate(FilterConfiguration configuration, bool newState);

        event ListAddedDelegate ListAdded;
        event ListRemovedDelegate ListRemoved;
        event ListConfigurationChangedDelegate ListConfigurationChanged;
        event ListTableConfigurationChangedDelegate ListTableConfigurationChanged;
        event ListRefreshedDelegate ListRefreshed;
        event ListToggledDelegate UiListToggled;
        event ListToggledDelegate BackgroundListToggled;
        event ListToggledDelegate CraftListToggled;
        event ListRepositionedDelegate ListRepositioned;

        bool HasDefaultCraftList();

        FilterConfiguration GetDefaultCraftList();
    }
}