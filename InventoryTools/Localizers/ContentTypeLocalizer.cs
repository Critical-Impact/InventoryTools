using AllaganLib.Shared.Extensions;
using Lumina.Excel.Sheets;

namespace InventoryTools.Localizers;

public class ContentTypeLocalizer : ILocalizer<ContentType>
{
    public string Format(ContentType instance)
    {
        return instance.RowId switch
        {
            20 => "Hall of Novice",
            22 => "Seasonal",
            23 => "The Diadem",
            39 => "The Final Verse",
            _ => instance.Name.ToImGuiString()
        };
    }
}
