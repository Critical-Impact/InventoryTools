using System.Collections.Generic;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;

namespace InventoryTools.Logic.Columns.Buttons;

public class CustomLinkButtonColumn : ButtonColumn
{
    private readonly ICommandManager _commandManager;
    private readonly StringColumnSetting _actionSetting;
    private readonly StringColumnSetting _buttonText;

    public CustomLinkButtonColumn(StringColumnSetting.Factory stringColumnFactory, ICommandManager commandManager)
    {
        _commandManager = commandManager;
        _actionSetting = stringColumnFactory.Invoke("cb_action", "Command",
            "The web link to open. You can add the ***Name*** to output the name of the item or ***ID*** to output the name of the item in the command.",
            "", "https://site.com");
        _buttonText = stringColumnFactory.Invoke("cb_label", "Label", "The label to give the button.","","Button");
        Settings.Add(_buttonText);
        Settings.Add(_actionSetting);
    }
    public override string Name { get; set; } = "Custom Link Button";
    public override float Width { get; set; } = 50;

    public override bool HasFilter { get; set; } = false;

    public override string HelpText { get; set; } =
        "A custom button letting you open a webpage with the item's name or ID optionally embedded into the link.";

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, SearchResult searchResult,
        int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            using var id = ImRaii.PushId(rowIndex + columnIndex.ToString());
            if (ImGui.Button(_buttonText.CurrentValue(columnConfiguration)))
            {
                var webUrl = _actionSetting.CurrentValue(columnConfiguration);
                if (webUrl != null)
                {
                    if (webUrl.Contains("***ID***"))
                    {
                        webUrl = webUrl.Replace("***ID***", searchResult.ItemId.ToString());
                    }

                    if (webUrl.Contains("***Name***"))
                    {
                        webUrl = webUrl.Replace("***Name***", searchResult.Item.NameString);
                    }
                    webUrl.OpenBrowser();
                }
            }
        }

        return null;
    }
}