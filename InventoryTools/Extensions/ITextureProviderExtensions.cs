using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;

namespace InventoryTools.Extensions
{
    using Dalamud.Interface.Textures;

    public static class ITextureProviderExtensions
    {
        public static ISharedImmediateTexture? GetUldIcon(this ITextureProvider textureProvider,  string iconName)
        {
            return textureProvider.GetFromGame(string.Format("ui/uld/{0}.tex", iconName));
        }
    }
}