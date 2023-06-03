using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Images;
using OtterGui;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class CheckboxColumn : Column<bool?>
    {
        public override string CsvExport(InventoryItem item)
        {
            return CurrentValue(item) ?? false ? "true" : "false";
        }

        public override string CsvExport(ItemEx item)
        {
            return CurrentValue((ItemEx)item) ?? false ? "true" : "false";
        }

        public override string CsvExport(SortingResult item)
        {
            return CurrentValue(item) ?? false ? "true" : "false";
        }
        
        public override bool? CurrentValue(CraftItem currentValue)
        {
            return CurrentValue(currentValue.Item);
        }

        public override IEnumerable<CraftItem> Filter(IEnumerable<CraftItem> items)
        {
            return items;
        }

        public override IEnumerable<CraftItem> Sort(ImGuiSortDirection direction, IEnumerable<CraftItem> items)
        {
            return items;
        }

        public override bool DrawFilter(string tableKey, int columnIndex)
        {
            var filter = FilterText;
            var hasChanged = false;
            
            ImGui.TableSetColumnIndex(columnIndex);
            ImGui.PushID(Name);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
            bool? isChecked;
            if (filter == "")
            {
                isChecked = null;
            }
            else if (filter == "true")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }
            var checkboxUnChecked = isChecked.HasValue ? (isChecked.Value  ? GameIcon.CheckboxChecked : GameIcon.CheckboxUnChecked) : GameIcon.CheckboxUnChecked;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - checkboxUnChecked.Size.X / 2);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);
            if (isChecked == null)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0.1f));
            }

            if (PluginService.PluginLogic.DrawUldIconButton(checkboxUnChecked))
            {
                if (!isChecked.HasValue)
                {
                    FilterText = "false";
                }
                else if (isChecked.Value == false)
                {
                    FilterText = "true";
                }
                else
                {
                    FilterText = "";
                }
                hasChanged = true;
            }

            if (isChecked == null)
            {
                ImGui.PopStyleColor();
            }

            ImGui.PopStyleVar();
            ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
            ImGui.TableHeader("");
            ImGui.PopID();

            return hasChanged;
        }

        public override void Draw(FilterConfiguration configuration, InventoryItem item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }
        public override void Draw(FilterConfiguration configuration, SortingResult item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }
        public override void Draw(FilterConfiguration configuration, ItemEx item, int rowIndex)
        {
            DoDraw(CurrentValue((ItemEx)item), rowIndex, configuration);
        }
        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }

        public override IEnumerable<ItemEx> Filter(IEnumerable<ItemEx> items)
        {
            bool isChecked;
            if (FilterText == "")
            {
                return items;
            }

            if (FilterText == "true")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue((ItemEx)c);
                if (!currentValue.HasValue)
                {
                    return false;
                }

                if (isChecked)
                {
                    return currentValue.Value;
                }
                return !currentValue.Value;
            });
        }

        public override IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            bool isChecked;
            if (FilterText == "")
            {
                return items;
            }

            if (FilterText == "true")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(c);
                if (!currentValue.HasValue)
                {
                    return false;
                }

                return isChecked && currentValue.Value || !isChecked && !currentValue.Value;
            });
        }

        public override IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            bool isChecked;
            if (FilterText == "")
            {
                return items;
            }

            if (FilterText == "true")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(c);
                if (!currentValue.HasValue)
                {
                    return false;
                }

                return isChecked && currentValue.Value || !isChecked && !currentValue.Value;
            });
        }

        private int GetSortOrder(ItemEx c)
        {
            var currentValue = CurrentValue(c);
            return currentValue switch
            {
                null => 0,
                false => 1,
                _ => 2
            };
        }

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => GetSortOrder(c.Item)) : items.OrderByDescending(c => GetSortOrder(c.Item));
        }

        public override IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(GetSortOrder) : items.OrderByDescending(GetSortOrder);
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => GetSortOrder(c.InventoryItem.Item)) : items.OrderByDescending(c => GetSortOrder(c.InventoryItem.Item));
        }
        

        public override IColumnEvent? DoDraw(bool? currentValue, int rowIndex, FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue.HasValue)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - filterConfiguration.TableHeight / 2.0f);
                PluginService.PluginLogic.DrawUldIcon(currentValue.Value ? GameIcon.TickIcon : GameIcon.CrossIcon, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight));
            }
            return null;
        }

        public override void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}