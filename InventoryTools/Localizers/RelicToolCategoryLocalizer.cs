using System;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Localizers;

public class RelicToolCategoryLocalizer : ILocalizer<RelicToolCategory>
{
    public string Format(RelicToolCategory instance)
    {
        switch (instance)
        {
            case RelicToolCategory.Mastercraft:
                return "Mastercraft Tools";
            case RelicToolCategory.Skysteel:
                return "Skysteel Tools";
            case RelicToolCategory.Resplendent:
                return "Resplendent Tools";
            case RelicToolCategory.Splendorous:
                return "Splendorous Tools";
            case RelicToolCategory.Cosmic:
                return "Cosmic Tools";
        }
        return instance.ToString();
    }
}