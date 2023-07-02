using Dalamud.Plugin.Ipc;
using ImGuiScene;
using InventoryTools.Services.Interfaces;
using StbiSharp;
using Veldrid;

namespace InventoryToolsMock;

public class MockPluginInterfaceService : IPluginInterfaceService
{
    public MockPluginInterfaceService(FileInfo configFile, DirectoryInfo configDirectory)
    {
        ConfigFile = configFile;
        ConfigDirectory = configDirectory;
    }
    public event Action? Draw;
    public event Action? OpenConfigUi;
    public FileInfo ConfigFile { get; }
    public DirectoryInfo ConfigDirectory { get; }

    public FileInfo AssemblyLocation
    {
        get
        {
            return new FileInfo(System.Environment.ProcessPath);
        }
    }
    public TextureWrap LoadImageRaw(byte[] imageData, int width, int height, int numChannels)
    {
        var texture = Program._gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)width, (uint)height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
        var CPUframeBufferTextureId = Program._controller.GetOrCreateImGuiBinding(Program._gd.ResourceFactory, texture);
        Program._gd.UpdateTexture(texture, imageData, 0,0,0, (uint)width, (uint)height, 1, 0,0);
        var veldridTextureWrap =
            new VeldridTextureMap(CPUframeBufferTextureId, width, height);
        return veldridTextureWrap;        
    }

    public TextureWrap LoadImage(string filePath)
    {
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var ms = new MemoryStream())
        {
            fs.CopyTo(ms);
            var image = Stbi.LoadFromMemory(ms, 4);
            var texture = Program._gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)image.Width,
                (uint)image.Height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            var CPUframeBufferTextureId =
                Program._controller.GetOrCreateImGuiBinding(Program._gd.ResourceFactory, texture);
            Program._gd.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)image.Width, (uint)image.Height, 1, 0, 0);
            var veldridTextureWrap =
                new VeldridTextureMap(CPUframeBufferTextureId, image.Width, image.Height);
            return veldridTextureWrap;        
        }

    }

    public ICallGateProvider<TRet> GetIpcProvider<TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateProvider<T1, TRet> GetIpcProvider<T1, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateProvider<T1, T2, TRet> GetIpcProvider<T1, T2, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateProvider<T1, T2, T3, TRet> GetIpcProvider<T1, T2, T3, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateProvider<T1, T2, T3, T4, TRet> GetIpcProvider<T1, T2, T3, T4, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateProvider<T1, T2, T3, T4, T5, TRet> GetIpcProvider<T1, T2, T3, T4, T5, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateProvider<T1, T2, T3, T4, T5, T6, TRet> GetIpcProvider<T1, T2, T3, T4, T5, T6, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateProvider<T1, T2, T3, T4, T5, T6, T7, TRet> GetIpcProvider<T1, T2, T3, T4, T5, T6, T7, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateProvider<T1, T2, T3, T4, T5, T6, T7, T8, TRet> GetIpcProvider<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateSubscriber<TRet> GetIpcSubscriber<TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateSubscriber<T1, TRet> GetIpcSubscriber<T1, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateSubscriber<T1, T2, TRet> GetIpcSubscriber<T1, T2, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateSubscriber<T1, T2, T3, TRet> GetIpcSubscriber<T1, T2, T3, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateSubscriber<T1, T2, T3, T4, TRet> GetIpcSubscriber<T1, T2, T3, T4, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateSubscriber<T1, T2, T3, T4, T5, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateSubscriber<T1, T2, T3, T4, T5, T6, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, T6, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(string name)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}