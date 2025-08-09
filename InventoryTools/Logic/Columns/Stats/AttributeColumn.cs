using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;
using OtterGui;
using OtterGui.Extensions;

namespace InventoryTools.Logic.Columns.Stats;

public class AttributeColumn : IntegerColumn
{
    private readonly AttributeColumnSetting _attributeColumnSetting;
    private readonly ExcelSheet<BaseParam> _baseParamSheet;

    public AttributeColumn(ILogger<AttributeColumn> logger, AttributeColumnSetting attributeColumnSetting, ExcelSheet<BaseParam> baseParamSheet, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _attributeColumnSetting = attributeColumnSetting;
        _baseParamSheet = baseParamSheet;
    }

    public override ColumnCategory ColumnCategory => ColumnCategory.Stats;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        var currentAttribute = _attributeColumnSetting.CurrentValue(columnConfiguration);
        if (currentAttribute == null)
        {
            return null;
        }

        var hasAttribute = searchResult.Item.Base.BaseParam.IndexOf(a => a.Value.RowId == currentAttribute.Value);

        if (hasAttribute == -1)
        {
            hasAttribute = searchResult.Item.Base.BaseParamSpecial.IndexOf(a => a.Value.RowId == currentAttribute.Value);
            if (hasAttribute == -1)
            {
                return null;
            }

            return searchResult.Item.Base.BaseParamValueSpecial[hasAttribute];
        }

        return searchResult.Item.Base.BaseParamValue[hasAttribute];
    }

    public override List<MessageBase>? DrawEditor(ColumnConfiguration columnConfiguration,
        FilterConfiguration configuration)
    {
        List<MessageBase>? results = new List<MessageBase>();
        ImGui.NewLine();
        ImGui.Separator();
        if (_attributeColumnSetting.Draw(columnConfiguration, "The attribute to show in the column"))
        {
            var newValue = _attributeColumnSetting.CurrentValue(columnConfiguration);
            if (newValue != null)
            {
                var newName = _baseParamSheet.GetRow(newValue.Value).Name.ExtractText();
                results = new List<MessageBase>()
                {
                    new NewColumnSetNameMessage(newName, newName)
                };
            }
        }
        base.DrawEditor(columnConfiguration, configuration);
        return results;
    }


    public override string Name { get; set; } = "Attribute";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "An bonus attributes of the item(Strength, HP, Perception, etc)";
}