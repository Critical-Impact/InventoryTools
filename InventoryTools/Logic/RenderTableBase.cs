using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Sheets;
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

        protected ImGuiListClipperPtr _clipper;
        
        public List<SortingResult> SortedItems { get; set; } = new List<SortingResult>();
        public List<SortingResult> RenderSortedItems { get; set; } = new List<SortingResult>();
        public List<ItemEx> Items { get; set; } = new List<ItemEx>();
        public List<ItemEx> RenderItems { get; set; } = new List<ItemEx>();

        public RenderTableBase(FilterConfiguration filterConfiguration)
        {
            FilterConfiguration = filterConfiguration;
            filterConfiguration.ConfigurationChanged += FilterConfigurationUpdated;
            filterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
            filterConfiguration.ListUpdated += FilterConfigurationUpdated;
            unsafe
            {
                _clipper = ImGuiNative.ImGuiListClipper_ImGuiListClipper();
                _clipper.ItemsHeight = 32;
            }

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
        public bool HighlightItems => PluginLogic.PluginConfiguration.ActiveUiFilter == FilterConfiguration.Key;

        protected void FilterConfigurationOnTableConfigurationChanged(FilterConfiguration filterconfiguration)
        {
            RefreshColumns();
        }

        protected void FilterConfigurationUpdated(FilterConfiguration filterconfiguration)
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

        public abstract bool Draw(Vector2 size);
        public abstract void DrawFooterItems();

        private bool _disposed;
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed && disposing)
            {
                FilterConfiguration.ConfigurationChanged -= FilterConfigurationUpdated;
                FilterConfiguration.ListUpdated -= FilterConfigurationUpdated;
                FilterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
            }
            _disposed = true;         
        }
    }
}