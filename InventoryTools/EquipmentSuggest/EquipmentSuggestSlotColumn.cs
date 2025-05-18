using AllaganLib.GameSheets.Model;
using AllaganLib.Interface.Grid.ColumnFilters;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;
using Humanizer;
using ImGuiNET;
using InventoryTools.Services;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestSlotColumn : AllaganLib.Interface.Grid.StringColumn<EquipmentSuggestConfig,
    EquipmentSuggestItem, MessageBase>
{
    private readonly EquipmentSuggestModeSetting _modeSetting;
    private readonly InventoryToolsConfiguration _configuration;

    public EquipmentSuggestSlotColumn(ImGuiService imGuiService, StringColumnFilter stringColumnFilter, EquipmentSuggestModeSetting modeSetting, InventoryToolsConfiguration configuration) : base(imGuiService, stringColumnFilter)
    {
        _modeSetting = modeSetting;
        _configuration = configuration;
    }

    public override string DefaultValue { get; set; } = string.Empty;
    public override string Key { get; set; } = "Slot";

    public override string Name
    {
        get { return _modeSetting.CurrentValue(_configuration) == EquipmentSuggestMode.Class ? "Slot" : "Class/Job"; }
        set { }
    }

    public override string HelpText { get; set; } = "The slot to fill";
    public override string Version { get; } = "1.12.0.10";
    public override string? CurrentValue(EquipmentSuggestItem item)
    {
        if (item.EquipmentSlot != null)
        {
            switch (item.EquipmentSlot)
            {
                case EquipSlot.MainHand:
                    return "Main Hand";
                case EquipSlot.OffHand:
                    return "Off Hand";
                case EquipSlot.Head:
                    return "Head";
                case EquipSlot.Body:
                    return "Body";
                case EquipSlot.Gloves:
                    return "Gloves";
                case EquipSlot.Legs:
                    return "Legs";
                case EquipSlot.Feet:
                    return "Feet";
                case EquipSlot.Ears:
                    return "Ears";
                case EquipSlot.Neck:
                    return "Neck";
                case EquipSlot.Wrists:
                    return "Wrists";
                case EquipSlot.FingerR:
                    return "Right Finger";
                case EquipSlot.FingerL:
                    return "Left Finger";
                case EquipSlot.SoulCrystal:
                    return "Soul Crystal";
            }

            return item.EquipmentSlot?.Humanize();
        }

        if (item.ClassJobRow != null)
        {
            return item.ClassJobRow.Base.Name.ToImGuiString().Humanize();
        }

        return "Unknown";
    }

    public override string? RenderName { get; set; } = null;
    public override int Width { get; set; } = 100;
    public override bool HideFilter { get; set; } = true;
    public override ImGuiTableColumnFlags ColumnFlags { get; set; } = ImGuiTableColumnFlags.WidthFixed;
    public override string EmptyText { get; set; } = "";
}