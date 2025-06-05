using System.Collections.Generic;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;

namespace InventoryTools.Logic.Columns.Buttons;

public class CustomButtonColumn : ButtonColumn
{
    private readonly ICommandManager _commandManager;
    private readonly StringColumnSetting _actionSetting;
    private readonly StringColumnSetting _buttonText;

    public CustomButtonColumn(StringColumnSetting.Factory stringColumnFactory, ICommandManager commandManager)
    {
        _commandManager = commandManager;
        _actionSetting = stringColumnFactory.Invoke("cb_action", "Command",
            "The command to run. A slash will automatically be added for you. You can add the ***Name*** to output the name of the item or ***ID*** to output the name of the item in the command.",
            "", "gather ***Name***");
        _buttonText = stringColumnFactory.Invoke("cb_label", "Label", "The label to give the button.","","Button");
        Settings.Add(_buttonText);
        Settings.Add(_actionSetting);
    }
    public override string Name { get; set; } = "Custom Button";
    public override float Width { get; set; } = 50;

    public override bool HasFilter { get; set; } = false;

    public override string HelpText { get; set; } =
        "A custom button letting you specify a custom command you wish to run with the item's name or ID ";

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, SearchResult searchResult,
        int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            using var id = ImRaii.PushId(rowIndex + columnIndex.ToString());
            if (ImGui.Button(_buttonText.CurrentValue(columnConfiguration)))
            {
                var command = _actionSetting.CurrentValue(columnConfiguration);
                if (command != null)
                {
                    if (command.Contains("***ID***"))
                    {
                        command = command.Replace("***ID***", searchResult.ItemId.ToString());
                    }

                    if (command.Contains("***Name***"))
                    {
                        command = command.Replace("***Name***", searchResult.Item.NameString);
                    }

                    _commandManager.ProcessCommand("/" + command);
                }
            }
        }

        return null;
    }
}