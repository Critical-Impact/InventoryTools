using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Images;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class CheckboxColumn : Column<bool?>
    {
        public override string CsvExport(InventoryItem item)
        {
            return CurrentValue(item) ?? false ? "true" : "false";
        }

        public override string CsvExport(Item item)
        {
            return CurrentValue(item) ?? false ? "true" : "false";
        }

        public override string CsvExport(SortingResult item)
        {
            return CurrentValue(item) ?? false ? "true" : "false";
        }

        public override bool? CurrentValue(CraftItem currentValue)
        {
            if (currentValue.Item == null)
            {
                return null;
            }

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

        public override void Draw(InventoryItem item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }
        public override void Draw(SortingResult item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }
        public override void Draw(Item item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }
        public override void Draw(CraftItem item, int rowIndex, FilterConfiguration configuration)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }

        public override IEnumerable<Item> Filter(IEnumerable<Item> items)
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



        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => CurrentValue(c) ?? false) : items.OrderByDescending(c => CurrentValue(c) ?? false);
        }

        public override IEnumerable<Item> Sort(ImGuiSortDirection direction, IEnumerable<Item> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => CurrentValue(c) ?? false) : items.OrderByDescending(c => CurrentValue(c) ?? false);
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => CurrentValue(c) ?? false) : items.OrderByDescending(c => CurrentValue(c) ?? false);
        }
        

        public override IColumnEvent? DoDraw(bool? currentValue, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (currentValue.HasValue)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) -
                                    GameIcon.CrossIcon.Size.X / 2);
                PluginService.PluginLogic.DrawUldIcon(currentValue.Value ? GameIcon.TickIcon : GameIcon.CrossIcon);
            }
            return null;
        }

        public override void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}