using Dalamud.Data;
using Lumina.Data.Files;

namespace InventoryTools.Extensions
{
    public static class DataManagerExtensions
    {
        public static TexFile? GetUldIcon(this DataManager dataManager,  string iconName)
        {
            return dataManager.GetFile<TexFile>(string.Format("ui/uld/{0}.tex", iconName));
        }
    }
}