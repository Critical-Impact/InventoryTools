using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using InventoryTools.Extensions;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class SortedListFilter<T,TY> : Filter<Dictionary<T, (string, string?)>> where T:notnull
    {
        public abstract bool CanRemove { get; set; }

        public abstract bool CanRemoveItem(FilterConfiguration configuration, T item);

        public abstract TY? GetItem(FilterConfiguration configuration, T item);

        public void RemoveItem(FilterConfiguration configuration, T item)
        {
            var value = CurrentValue(configuration);
            if (value.ContainsKey(item))
            {
                value.Remove(item);
            }
            UpdateFilterConfiguration(configuration, value);
        }

        public void MoveItemUp(FilterConfiguration configuration, T item)
        {
            var value = CurrentValue(configuration);
            var list = value.Select(c => c.Key).ToList();
            list = list.MoveUp( item);
            UpdateFilterConfiguration(configuration, list.ToDictionary(c => c, c => value[c]));
        }

        public void MoveItemDown(FilterConfiguration configuration, T item)
        {
            var value = CurrentValue(configuration);
            var list = value.Select(c => c.Key).ToList();
            list = list.MoveDown( item);
            UpdateFilterConfiguration(configuration, list.ToDictionary(c => c, c => value[c]));
        }

        public void MoveItemTop(FilterConfiguration configuration, T item)
        {
            var value = CurrentValue(configuration);
            var list = value.Select(c => c.Key).ToList();
            list = list.MoveTop( item);
            UpdateFilterConfiguration(configuration, list.ToDictionary(c => c, c => value[c]));
        }

        public void MoveItemBottom(FilterConfiguration configuration, T item)
        {
            var value = CurrentValue(configuration);
            var list = value.Select(c => c.Key).ToList();
            list = list.MoveBottom( item);
            UpdateFilterConfiguration(configuration, list.ToDictionary(c => c, c => value[c]));
        }

        public virtual void DrawItem(FilterConfiguration configuration, KeyValuePair<T,(string, string?)> item, int index)
        {
            string name = item.Value.Item1;
            ImGui.Text(name);
            ImGuiUtil.HoverTooltip(name);
        }

        public virtual void DrawButtons(FilterConfiguration configuration, KeyValuePair<T,(string, string?)> item, int index)
        {
            if (CanRemove && CanRemoveItem(configuration, item.Key))
            {
                if (ImGui.Button("X##Column" + index))
                {
                    RemoveItem(configuration, item.Key);
                }
                ImGui.SameLine();
            }
            if (ImGui.Button("Top##Column" + index))
            {
                MoveItemTop(configuration, item.Key);
            }
            ImGui.SameLine();
            if (ImGui.Button("Up##Column" + index))
            {
                MoveItemUp(configuration, item.Key);
            }

            ImGui.SameLine();
            if (ImGui.Button("Down##Column" + index))
            {
                MoveItemDown(configuration, item.Key);
            }
            ImGui.SameLine();
            if (ImGui.Button("Bottom##Column" + index))
            {
                MoveItemBottom(configuration, item.Key);
            }
        }

        public virtual void DrawTable(FilterConfiguration configuration)
        {
            var value = CurrentValue(configuration);
            using (var table = ImRaii.Table(Key + "ColumnEditTable", 3, ImGuiTableFlags.RowBg))
            {
                if (table.Success)
                {
                    ImGui.TableSetupColumn(Key + "ColumnEditTableName", ImGuiTableColumnFlags.NoSort | ImGuiTableColumnFlags.WidthFixed, LabelSize);
                    ImGui.TableSetupColumn(Key + "ColumnEditTableDelete", ImGuiTableColumnFlags.NoSort | ImGuiTableColumnFlags.WidthFixed, InputSize);
                    var index = 0;
                    foreach (var item in value)
                    {
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 10);
                        string? helpText = item.Value.Item2;
                        ImGui.TableNextColumn();
                        DrawItem(configuration, item, index);
                        ImGui.TableNextColumn();
                        DrawButtons(configuration, item, index);
                        index++;
                        ImGui.TableNextColumn();
                        ImGui.Selectable("", false, ImGuiSelectableFlags.SpanAllColumns,
                            new Vector2(0, 16) * ImGui.GetIO().FontGlobalScale);
                        if (helpText != null)
                        {
                            ImGuiUtil.HoverTooltip(helpText);
                        }
                    }
                }
            }
        }

        public override void Draw(FilterConfiguration configuration)
        {
            DrawTable(configuration);
            ImGui.SameLine();
            ImGuiService.HelpMarker(HelpText);
            if (HasValueSet(configuration) && ShowReset)
            {
                ImGui.SameLine();
                if (ImGui.Button("Reset##" + Key + "Reset"))
                {
                    ResetFilter(configuration);
                }
            }
        }

        protected SortedListFilter(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}