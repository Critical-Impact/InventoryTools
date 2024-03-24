using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IconColumn : GameIconColumn
    {
        public IconColumn(ILogger<IconColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override (ushort, bool)? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return (item.Icon, item.IsHQ);
        }

        public override (ushort, bool)? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return (item.Icon, false);
        }

        public override (ushort, bool)? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }
        
        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1;
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1;
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1;
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, CraftItem item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1;
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            InventoryItem item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SortingResult item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            ItemEx item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, (ItemEx)item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryChange item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }

        public override List<MessageBase>? DoDraw(IItem item, (ushort, bool)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            var messages = new List<MessageBase>();
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                var textureWrap = Service.TextureProvider.GetIcon(currentValue.Value.Item1, currentValue.Value.Item2 ? ITextureProvider.IconFlags.ItemHighQuality : ITextureProvider.IconFlags.HiRes);
                if (textureWrap != null)
                {
                    ImGui.PushID("icon" + rowIndex);
                    if (ImGui.ImageButton(textureWrap.ImGuiHandle, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale,new Vector2(0,0), new Vector2(1,1), 2))
                    {
                        ImGui.PopID();
                        messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), item.ItemId));
                    }
                    ImGui.PopID();
                }
                else
                {
                    ImGui.Button("", new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale);
                }
            }
            return messages;
            
        }


        public override string Name { get; set; } = "Icon";
        public override string RenderName => "";
        public override float Width { get; set; } = 60.0f;
        public override string HelpText { get; set; } = "Shows the icon of the item, pressing it will open the more information window for the item.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}