using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Ui.Widgets;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class CraftSimpleColumn : TextColumn
    {
        private readonly ExcelCache _excelCache;
        private readonly IFont _font;

        public CraftSimpleColumn(ILogger<CraftSimpleColumn> logger, ImGuiService imGuiService,ExcelCache excelCache, IFont font) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
            _font = font;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Crafting;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            return "";
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            var nextStep = configuration.CraftList.GetNextStep(item);
            ImGuiUtil.VerticalAlignTextColored(nextStep.Item2, nextStep.Item1, configuration.TableHeight, true);
            if (item.IsOutputItem && item.IsCompleted)
            {
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + configuration.TableHeight / 2.0f - 9);
                ImGui.Image(ImGuiService.IconService[Icons.RedXIcon].ImGuiHandle, new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale,
                    new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1));
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }

                if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    configuration.CraftList.RemoveCraftItem(item.ItemId);
                    configuration.NeedsRefresh = true;
                }
                OtterGui.ImGuiUtil.HoverTooltip("Delete item");

            }
            if (item.MissingIngredients.Count != 0)
            {
                ImGui.SameLine();
                ImGui.PushFont(_font.IconFont);
                ImGuiUtil.VerticalAlignTextDisabled(FontAwesomeIcon.InfoCircle.ToIconString(), configuration.TableHeight, false);
                ImGui.PopFont();
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
                {
                    using var tt = ImRaii.Tooltip();
                    ImGui.Text("Missing Ingredients: ");
                    foreach (var missingIngredient in item.MissingIngredients)
                    {
                        var itemId = missingIngredient.Key.Item1;
                        var quantity = missingIngredient.Value;
                        var isHq = missingIngredient.Key.Item2;
                        var actualItem = _excelCache.GetItemExSheet().GetRow(itemId);
                        if (actualItem != null)
                        {
                            ImGui.Text(actualItem.NameString + " : " + quantity);
                        }
                    }
                }
            }

            return null;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return "";
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return "";
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return "";
        }

        public override string Name { get; set; } = "Next Step in Craft";
        public override string RenderName => "Next Step";

        public override float Width { get; set; } = 200;
        public override bool? CraftOnly => true;

        public override string HelpText { get; set; } =
            "Shows a simplified version of what you should do next in your craft";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
    }
}