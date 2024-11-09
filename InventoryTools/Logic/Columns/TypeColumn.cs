using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Logic.Columns
{
    public class TypeColumn : TextColumn
    {
        public TypeColumn(ILogger<TypeColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, SearchResult searchResult,
            int rowIndex, int columnIndex)
        {
            if (searchResult.CuratedItem != null && (searchResult.Item.Base.IsCollectable || searchResult.Item.Base.CanBeHq))
            {
                ImGui.TableNextColumn();
                if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
                var value = searchResult.CuratedItem.ItemFlags.FormattedName();
                ImGuiUtil.VerticalAlignButton(configuration.TableHeight);
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                using (var combo = ImRaii.Combo("##"+rowIndex+"Type", value))
                {
                    if (combo)
                    {
                        if (ImGui.Selectable(InventoryItem.ItemFlags.None.FormattedName()))
                        {
                            searchResult.CuratedItem.ItemFlags = InventoryItem.ItemFlags.None;
                        }
                        if (ImGui.Selectable(InventoryItem.ItemFlags.HighQuality.FormattedName()))
                        {
                            searchResult.CuratedItem.ItemFlags = InventoryItem.ItemFlags.HighQuality;
                        }
                        if (ImGui.Selectable(InventoryItem.ItemFlags.Collectable.FormattedName()))
                        {
                            searchResult.CuratedItem.ItemFlags = InventoryItem.ItemFlags.Collectable;
                        }

                        configuration.ConfigurationDirty = true;
                    }
                }

                return null;
            }
            return base.Draw(configuration, columnConfiguration, searchResult, rowIndex, columnIndex);
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem != null)
            {
                return searchResult.InventoryItem.FormattedType;
            }

            return null;
        }
        public override string Name { get; set; } = "Type";
        public override float Width { get; set; } = 80.0f;
        public override string HelpText { get; set; } = "The type of the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public override FilterType DefaultIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.HistoryFilter;
    }
}