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
    public TooltipAmountOwnedColorSetting(ILogger<TooltipAmountOwnedColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipAmountOwnedColor", "Text Colour", "When enabled, what colour should the text be for the 'Add Item Locations' tooltip text be?", null, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 8;
    }
}


public class TooltipMarketPricingColorSetting : GenericGameColorSetting
{
    public TooltipMarketPricingColorSetting(ILogger<TooltipMarketPricingColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipMarketPricingColor", "Text Colour", "When enabled, what colour should the text be for the 'Market Pricing' tooltip text be?", null, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 559;
    }
}
public class TooltipAmountToRetrieveColorSetting : GenericGameColorSetting
{
    public TooltipAmountToRetrieveColorSetting(ILogger<TooltipAmountToRetrieveColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipAmountToRetrieveColor", "Text Colour", "When enabled, what colour should the text be for the 'Amount to Retrieve' tooltip text be?", null, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 37;
    }
}
public class TooltipItemUnlockStatusColorSetting : GenericGameColorSetting
{
    public TooltipItemUnlockStatusColorSetting(ILogger<TooltipItemUnlockStatusColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipItemUnlockStatusColor", "Text Colour", "When enabled, what colour should the text be for the 'Item Unlock Status' tooltip text be?", null, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 555;
    }
}
public class TooltipSourceInformationColorSetting : GenericGameColorSetting
{
    public TooltipSourceInformationColorSetting(ILogger<TooltipSourceInformationColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipSourceInformationColor", "Text Colour", "When enabled, what colour should the text be for the 'Source Information' tooltip text be?", null, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 502;
    }
}
public class TooltipUseInformationColorSetting : GenericGameColorSetting
{
    public TooltipUseInformationColorSetting(ILogger<TooltipUseInformationColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipUseInformationColor", "Text Colour", "When enabled, what colour should the text be for the 'Use Information' tooltip text be?", null, "1.11.0.11", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 60;
    }
}
public class TooltipIngredientPatchTooltipColorSetting : GenericGameColorSetting
{
    public TooltipIngredientPatchTooltipColorSetting(ILogger<TooltipIngredientPatchTooltipColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipIngredientPatchTooltipColor", "Text Colour", "When enabled, what colour should the text be for the 'Ingredient Patch' tooltip text be?", null, "1.12.0.12", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 540;
    }
}
public class TooltipGlamourReadySetColorSetting : GenericGameColorSetting
{
    public TooltipGlamourReadySetColorSetting(ILogger<TooltipGlamourReadySetColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipGlamourReadySetColor", "Text Colour", "When enabled, what colour should the text be for the 'Outfit Glamour' tooltip text be?", null, "1.12.0.0", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 500;
    }
}

public class TooltipGlamourReadySetAcquiredColorSetting : GenericGameColorSetting
{
    public TooltipGlamourReadySetAcquiredColorSetting(ILogger<TooltipGlamourReadySetAcquiredColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipGlamourReadySetAcquiredColor", "Acquired Item Colour", "In detailed mode, what colour should acquired items be shown in?", null, "1.13.0.0", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 45;
    }
}
public class TooltipGlamourReadySetNotAcquiredColorSetting : GenericGameColorSetting
{
    public TooltipGlamourReadySetNotAcquiredColorSetting(ILogger<TooltipGlamourReadySetNotAcquiredColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipGlamourReadySetNotAcquiredColor", "Not Acquired Item Colour", "In detailed mode, what colour should not-yet-acquired items be shown in?", null, "1.13.0.0", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 17;
    }
}
public class TooltipCofferLootColorSetting : GenericGameColorSetting
{
    public TooltipCofferLootColorSetting(ILogger<TooltipCofferLootColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipCofferLootColor", "Text Colour", "When enabled, what colour should the text be for the 'Coffer Loot' tooltip text be?", null, "1.12.0.0", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 541;
    }
}
public class TooltipCuratedListsColorSetting : GenericGameColorSetting
{
    public TooltipCuratedListsColorSetting(ILogger<TooltipCuratedListsColorSetting> logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base("TooltipCuratedListsColor", "Text Colour", "When enabled, what colour should the text be for the 'Curated List' tooltip text be?", null, "15.0.12", logger, imGuiService, uiColorSheet)
    {
        this.DefaultValue = 535;
    }
}