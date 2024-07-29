using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class ShortcutColumn : TextColumn
    {
        private readonly TryOn _tryOn;

        public ShortcutColumn(ILogger<ShortcutColumn> logger, ImGuiService imGuiService, TryOn tryOn) : base(logger, imGuiService)
        {
            _tryOn = tryOn;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Tools;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return null;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return null;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return null;
        }

        public override string Name { get; set; } = "Shortcuts";
        public override float Width { get; set; } = 32.0f;

        public override string HelpText { get; set; } =
            "Provides a series of small buttons that allow opening garland tools and trying on items.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryItem item, int rowIndex, int columnIndex)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (ImGui.SmallButton("GT##GT" + rowIndex))
                {
                    $"https://www.garlandtools.org/db/#item/{item.Item.GarlandToolsId}".OpenBrowser();
                }

                if (item.Item.CanTryOn)
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton("Try On##TO" + rowIndex))
                    {
                        if (_tryOn.CanUseTryOn)
                        {
                            _tryOn.TryOnItem(item.Item, 0, item.IsHQ);
                        }
                        else
                        {
                            Logger.LogError("Something went wrong while attempting to try on " + item.Item.NameString);
                        }
                    }
                }
            }

            return null;
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SortingResult item, int rowIndex, int columnIndex)
        {
           return Draw(configuration, columnConfiguration, item.InventoryItem, rowIndex, columnIndex);
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            ItemEx item, int rowIndex, int columnIndex)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (ImGui.SmallButton("G##G" + rowIndex))
                {
                    $"https://www.garlandtools.org/db/#item/{item.GarlandToolsId}".OpenBrowser();
                }

                if (item.CanTryOn)
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton("Try On##TO" + rowIndex))
                    {
                        if (_tryOn.CanUseTryOn)
                        {
                            _tryOn.TryOnItem(item, 0, false);
                        }
                        else
                        {
                            Logger.LogError("Something went wrong while attempting to try on " + item.NameString);
                        }
                    }
                }
            }

            return null;
        }
    }
}