using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic.Columns;

namespace InventoryTools.Logic
{
    public abstract class RenderTableBase : IRenderTableBase, IDisposable
    {
        protected ImGuiTableFlags _tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                                ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                                ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                                ImGuiTableFlags.BordersInnerH |
                                                ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                                ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                                ImGuiTableFlags.ScrollY;

        public List<SortingResult> SortedItems { get; set; } = new List<SortingResult>();
        public List<SortingResult> RenderSortedItems { get; set; } = new List<SortingResult>();
        public List<ItemEx> Items { get; set; } = new List<ItemEx>();
        public List<ItemEx> RenderItems { get; set; } = new List<ItemEx>();
        
        public List<InventoryChange> InventoryChanges { get; set; } = new List<InventoryChange>();
        public List<InventoryChange> RenderInventoryChanges { get; set; } = new List<InventoryChange>();

        public RenderTableBase(FilterConfiguration filterConfiguration)
        {
            FilterConfiguration = filterConfiguration;
            filterConfiguration.ConfigurationChanged += FilterConfigurationUpdated;
            filterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
            filterConfiguration.ListUpdated += FilterConfigurationOnListUpdated;
        }

        public string Name
        {
            get
            {
                return FilterConfiguration.Name;
            }
        }

        public virtual string Key
        {
            get
            {
                return FilterConfiguration.TableId;
            }
        }

        public virtual List<IColumn> Columns { get; set; } = new ();
        public int? SortColumn { get; set; }
        public ImGuiSortDirection? SortDirection { get; set; }
        public int? FreezeCols { get; set; }
        public int? FreezeRows { get; set; }
        public abstract void Refresh(InventoryToolsConfiguration configuration);
        public bool ShowFilterRow { get; set; }
        public bool NeedsRefresh { get; set; }
        public bool IsSearching { get; set; }
        public FilterConfiguration FilterConfiguration { get; set; }
        public bool HighlightItems => ConfigurationManager.Config.ActiveUiFilter == FilterConfiguration.Key;

        public bool Disposed => _disposed;

        protected void FilterConfigurationOnTableConfigurationChanged(FilterConfiguration filterconfiguration)
        {
            RefreshColumns();
        }

        protected void FilterConfigurationUpdated(FilterConfiguration filterconfiguration, bool filterInvalidated)
        {
        }
        
        private void FilterConfigurationOnListUpdated(FilterConfiguration filterconfiguration)
        {
            this.NeedsRefresh = true;
        }

        public delegate IEnumerable<SortingResult> PreFilterSortedItemsDelegate(IEnumerable<SortingResult> items);

        public delegate IEnumerable<ItemEx> PreFilterItemsDelegate(IEnumerable<ItemEx> items);

        public delegate void ChangedDelegate(RenderTableBase itemTable);

        public virtual event PreFilterSortedItemsDelegate? PreFilterSortedItems;
        public virtual event PreFilterItemsDelegate? PreFilterItems;
        public virtual event ChangedDelegate? Refreshed;
        
        public abstract void RefreshColumns();

        public abstract bool Draw(Vector2 size, bool shouldDraw = true);
        public abstract void DrawFooterItems();

        private bool _disposed;
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!Disposed && disposing)
            {
                FilterConfiguration.ConfigurationChanged -= FilterConfigurationUpdated;
                FilterConfiguration.ListUpdated -= FilterConfigurationOnListUpdated;
                FilterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
            }
            _disposed = true;         
        }
        
        ~RenderTableBase()
        {
#if DEBUG
            // In debug-builds, make sure that a warning is displayed when the Disposable object hasn't been
            // disposed by the programmer.

            if( Disposed == false )
            {
                PluginLog.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
            }
#endif
            Dispose (true);
        }
    }
}