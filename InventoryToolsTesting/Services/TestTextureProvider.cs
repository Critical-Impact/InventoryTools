using System.IO;
using Dalamud;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;
using Lumina.Data.Files;

namespace InventoryToolsTesting.Services
{
    public class TestTextureProvider : ITextureProvider
    {
        public IDalamudTextureWrap? GetIcon(uint iconId, ITextureProvider.IconFlags flags = ITextureProvider.IconFlags.HiRes, ClientLanguage? language = null,
            bool keepAlive = false)
        {
            return null;
        }

        public string? GetIconPath(uint iconId, ITextureProvider.IconFlags flags = ITextureProvider.IconFlags.HiRes, ClientLanguage? language = null)
        {
            return null;
        }

        public IDalamudTextureWrap? GetTextureFromGame(string path, bool keepAlive = false)
        {
            return null;
        }

        public IDalamudTextureWrap? GetTextureFromFile(FileInfo file, bool keepAlive = false)
        {
            return null;
        }

        public IDalamudTextureWrap GetTexture(TexFile file)
        {
            return null!;
        }
    }
}