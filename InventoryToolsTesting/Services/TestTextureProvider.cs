using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using Lumina.Data.Files;

namespace InventoryToolsTesting.Services
{
    public class TestTextureProvider : ITextureProvider
    {
        public IDalamudTextureWrap CreateEmpty(RawImageSpecification specs, bool cpuRead, bool cpuWrite, string? debugName = null)
        {
            return null!;
        }

        public Task<IDalamudTextureWrap> CreateFromExistingTextureAsync(IDalamudTextureWrap wrap,
            TextureModificationArgs args, bool leaveWrapOpen = false, string? debugName = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return null!;
        }

        public Task<IDalamudTextureWrap> CreateFromImGuiViewportAsync(ImGuiViewportTextureArgs args, string? debugName = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return null!;
        }

        public Task<IDalamudTextureWrap> CreateFromImageAsync(ReadOnlyMemory<byte> bytes, string? debugName = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return null!;
        }

        public Task<IDalamudTextureWrap> CreateFromImageAsync(Stream stream, bool leaveOpen = false, string? debugName = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return null!;
        }

        public IDalamudTextureWrap CreateFromRaw(RawImageSpecification specs, ReadOnlySpan<byte> bytes, string? debugName = null)
        {
            return null!;
        }

        public Task<IDalamudTextureWrap> CreateFromRawAsync(RawImageSpecification specs, ReadOnlyMemory<byte> bytes, string? debugName = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return null!;
        }

        public Task<IDalamudTextureWrap> CreateFromRawAsync(RawImageSpecification specs, Stream stream, bool leaveOpen = false, string? debugName = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return null!;
        }

        public IDalamudTextureWrap CreateFromTexFile(TexFile file)
        {
            return null!;
        }

        public Task<IDalamudTextureWrap> CreateFromTexFileAsync(TexFile file, string? debugName = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return null!;
        }

        public IEnumerable<IBitmapCodecInfo> GetSupportedImageDecoderInfos()
        {
            return null!;
        }

        public ISharedImmediateTexture GetFromGameIcon(in GameIconLookup lookup)
        {
            return null!;
        }

        public bool TryGetFromGameIcon(in GameIconLookup lookup, [NotNullWhen(true)] out ISharedImmediateTexture? texture)
        {
            texture = null;
            return false;
        }

        public ISharedImmediateTexture GetFromGame(string path)
        {
            return null!;
        }

        public ISharedImmediateTexture GetFromFile(string path)
        {
            return null!;
        }

        public ISharedImmediateTexture GetFromFile(FileInfo file)
        {
            return null!;
        }

        public ISharedImmediateTexture GetFromFileAbsolute(string fullPath)
        {
            return null!;
        }

        public ISharedImmediateTexture GetFromManifestResource(Assembly assembly, string name)
        {
            return null!;
        }

        public string GetIconPath(in GameIconLookup lookup)
        {
            return "";
        }

        public bool TryGetIconPath(in GameIconLookup lookup, [NotNullWhen(true)] out string? path)
        {
            path = null;
            return false;
        }

        public bool IsDxgiFormatSupported(int dxgiFormat)
        {
            return false;
        }

        public bool IsDxgiFormatSupportedForCreateFromExistingTextureAsync(int dxgiFormat)
        {
            return false;
        }

        public IntPtr ConvertToKernelTexture(IDalamudTextureWrap wrap, bool leaveWrapOpen = false)
        {
            return IntPtr.Zero;
        }
    }
}