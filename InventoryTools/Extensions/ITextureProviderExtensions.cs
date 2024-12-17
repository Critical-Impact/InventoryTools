using System.IO;
using Dalamud.Plugin;
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

        public static ISharedImmediateTexture GetPluginImageTexture(this ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface, string imageName)
        {
            var assemblyLocation = pluginInterface.AssemblyLocation.DirectoryName!;
            var imagePath = Path.Combine(assemblyLocation, Path.Combine("Images", $"{imageName}.png"));
            return textureProvider.GetFromFile(new FileInfo(imagePath));
        }
    }
}