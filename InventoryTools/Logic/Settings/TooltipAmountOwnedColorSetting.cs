using System.Numerics;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipAmountOwnedColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 0;

    public TooltipAmountOwnedColorSetting(ILogger<TooltipAmountOwnedColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipAmountOwnedColor", "Text Colour", "When enabled, what colour should the text be for the 'Add Item Locations' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.AddItemLocations, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
    }
}


public class TooltipMarketPricingColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 0;

    public TooltipMarketPricingColorSetting(ILogger<TooltipMarketPricingColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipMarketPricingColor", "Text Colour", "When enabled, what colour should the text be for the 'Market Pricing' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.MarketPricing, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
    }
}
public class TooltipAmountToRetrieveColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 0;

    public TooltipAmountToRetrieveColorSetting(ILogger<TooltipAmountToRetrieveColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipAmountToRetrieveColor", "Text Colour", "When enabled, what colour should the text be for the 'Amount to Retrieve' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.AmountToRetrieve, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
    }
}
public class TooltipItemUnlockStatusColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 0;

    public TooltipItemUnlockStatusColorSetting(ILogger<TooltipItemUnlockStatusColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipItemUnlockStatusColor", "Text Colour", "When enabled, what colour should the text be for the 'Item Unlock Status' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.ItemUnlockStatus, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
    }
}
public class TooltipSourceInformationColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 0;

    public TooltipSourceInformationColorSetting(ILogger<TooltipSourceInformationColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipSourceInformationColor", "Text Colour", "When enabled, what colour should the text be for the 'Source Information' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.SourceInformation, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
    }
}
public class TooltipUseInformationColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 0;

    public TooltipUseInformationColorSetting(ILogger<TooltipUseInformationColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipUseInformationColor", "Text Colour", "When enabled, what colour should the text be for the 'Use Information' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.UseInformation, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
    }
}