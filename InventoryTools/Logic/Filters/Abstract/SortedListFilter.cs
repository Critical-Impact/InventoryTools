using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using InventoryTools.Extensions;
using OtterGui;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class SortedListFilter<T> : Filter<Dictionary<T, (string, string?)>> where T:notnull
    {
        public abstract bool CanRemove { get; set; }

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

        public virtual void DrawTable(FilterConfiguration configuration)
        {
            var value = CurrentValue(configuration);

            if (ImGui.BeginTable( Key + "ColumnEditTable", 2, ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn(Key + "ColumnEditTableName", ImGuiTableColumnFlags.NoSort);
                ImGui.TableSetupColumn(Key + "ColumnEditTableDelete", ImGuiTableColumnFlags.NoSort);
                var index = 0;
                foreach (var item in value)
                {
                    ImGui.TableNextRow();
                    string name = item.Value.Item1;
                    string? helpText = item.Value.Item2;
                    ImGui.TableNextColumn();
                    ImGui.Text(name);
                    ImGui.TableNextColumn();
                    ImGui.Selectable("", false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, 32) * ImGui.GetIO().FontGlobalScale);
                    if (helpText != null)
                    {
                        ImGuiUtil.HoverTooltip(helpText);
                    }

                    ImGui.SameLine();
                    if (CanRemove && ImGui.Button( "X##Column" + index))
                    {
                        RemoveItem(configuration, item.Key);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button( "Up##Column" + index))
                    {
                        MoveItemUp(configuration, item.Key);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Down##Column" + index))
                    {
                        MoveItemDown(configuration, item.Key);
                    }
                    index++;
                }
                ImGui.EndTable();
            }
        }
        
        public override void Draw(FilterConfiguration configuration)
        {
            DrawTable(configuration);
            ImGui.SameLine();
            UiHelpers.HelpMarker(HelpText);
        }
    }
}