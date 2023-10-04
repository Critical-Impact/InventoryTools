using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;

namespace InventoryTools.Extensions
{
    public static class ITextureProviderExtensions
    {
        public static IDalamudTextureWrap? GetUldIcon(this ITextureProvider textureProvider,  string iconName)
        {
            return textureProvider.GetTextureFromGame(string.Format("ui/uld/{0}.tex", iconName));
        }
    }
}