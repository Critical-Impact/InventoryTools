using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using InventoryTools.Logic.Columns;
using Lumina.Excel.GeneratedSheets;

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
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Item> RenderItems { get; set; } = new List<Item>();

        public RenderTableBase(FilterConfiguration filterConfiguration)
        {
            FilterConfiguration = filterConfiguration;
            filterConfiguration.ConfigurationChanged += FilterConfigurationUpdated;
            filterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
            filterConfiguration.ListUpdated += FilterConfigurationUpdated;
            unsafe
            {
                var clipperNative = Marshal.AllocHGlobal(Marshal.SizeOf<ImGuiListClipper>());
                var clipper = new ImGuiListClipper();
                Marshal.StructureToPtr(clipper, clipperNative, false);
                _clipper = new ImGuiListClipperPtr(clipperNative);
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

        public delegate IEnumerable<Item> PreFilterItemsDelegate(IEnumerable<Item> items);

        public delegate void ChangedDelegate(FilterTable itemTable);

        public virtual event PreFilterSortedItemsDelegate? PreFilterSortedItems;
        public virtual event PreFilterItemsDelegate? PreFilterItems;
        public virtual event ChangedDelegate? Refreshed;
        
        public abstract void RefreshColumns();

        public abstract void Draw(Vector2 size);

        public virtual void Dispose()
        {
            FilterConfiguration.ConfigurationChanged -= FilterConfigurationUpdated;
            FilterConfiguration.ListUpdated -= FilterConfigurationUpdated;
            FilterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
        }
    }
}