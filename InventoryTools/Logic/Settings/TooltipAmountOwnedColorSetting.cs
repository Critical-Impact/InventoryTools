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
    public override uint? Order { get; } = 1;

    public TooltipAmountOwnedColorSetting(ILogger<TooltipAmountOwnedColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipAmountOwnedColor", "Text Colour", "When enabled, what colour should the text be for the 'Add Item Locations' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.AddItemLocations, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 8;
    }
}


public class TooltipMarketPricingColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 1;

    public TooltipMarketPricingColorSetting(ILogger<TooltipMarketPricingColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipMarketPricingColor", "Text Colour", "When enabled, what colour should the text be for the 'Market Pricing' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.MarketPricing, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 559;
    }
}
public class TooltipAmountToRetrieveColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 1;

    public TooltipAmountToRetrieveColorSetting(ILogger<TooltipAmountToRetrieveColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipAmountToRetrieveColor", "Text Colour", "When enabled, what colour should the text be for the 'Amount to Retrieve' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.AmountToRetrieve, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 37;
    }
}
public class TooltipItemUnlockStatusColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 1;

    public TooltipItemUnlockStatusColorSetting(ILogger<TooltipItemUnlockStatusColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipItemUnlockStatusColor", "Text Colour", "When enabled, what colour should the text be for the 'Item Unlock Status' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.ItemUnlockStatus, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 555;
    }
}
public class TooltipSourceInformationColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 1;

    public TooltipSourceInformationColorSetting(ILogger<TooltipSourceInformationColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipSourceInformationColor", "Text Colour", "When enabled, what colour should the text be for the 'Source Information' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.SourceInformation, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 502;
    }
}
public class TooltipUseInformationColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 1;

    public TooltipUseInformationColorSetting(ILogger<TooltipUseInformationColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipUseInformationColor", "Text Colour", "When enabled, what colour should the text be for the 'Use Information' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.UseInformation, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 60;
    }
}
public class TooltipIngredientPatchTooltipColorSetting : GenericGameColorSetting
{
    public override uint? Order { get; } = 1;

    public TooltipIngredientPatchTooltipColorSetting(ILogger<TooltipIngredientPatchTooltipColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipIngredientPatchTooltipColor", "Text Colour", "When enabled, what colour should the text be for the 'Ingredient Patch' tooltip text be?", null, SettingCategory.ToolTips, SettingSubCategory.IngredientPatch, "1.12.0.12", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 540;
    }
}