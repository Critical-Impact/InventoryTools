using System;
using System.IO;
using CriticalCommonLib;
using DalaMock.Interfaces;
using Dalamud.Interface.Internal;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using ImGuiScene;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Services;

public class PluginInterfaceService : IPluginInterfaceService, IDisposable
{
    private DalamudPluginInterface _dalamudPluginInterface;
    public PluginInterfaceService(DalamudPluginInterface dalamudPluginInterface)
    {
        _dalamudPluginInterface = dalamudPluginInterface;
        _dalamudPluginInterface.UiBuilder.Draw += UiBuilderOnDraw;
        _dalamudPluginInterface.UiBuilder.OpenConfigUi += UiBuilderOnOpenConfigUi;
        _dalamudPluginInterface.UiBuilder.OpenMainUi += UiBuilderOnOpenMainUi;
    }

    private void UiBuilderOnOpenMainUi()
    {
        OpenMainUi?.Invoke();
    }

    private void UiBuilderOnOpenConfigUi()
    {
        OpenConfigUi?.Invoke();
    }

    private void UiBuilderOnDraw()
    {
        Draw?.Invoke();
    }

    public event Action? Draw;
    public event Action? OpenConfigUi;
    public event Action? OpenMainUi;

    public FileInfo ConfigFile
    {
        get
        {
            return _dalamudPluginInterface.ConfigFile;
        }
    }

    public DirectoryInfo ConfigDirectory
    {
        get
        {
            return _dalamudPluginInterface.ConfigDirectory;
        }
    }

    public FileInfo AssemblyLocation
    {
        get
        {
            return _dalamudPluginInterface.AssemblyLocation;
        }
    }

    public IDalamudTextureWrap LoadImageRaw(byte[] imageData, int width, int height, int numChannels)
    {
        return _dalamudPluginInterface.UiBuilder.LoadImageRaw(imageData, width, height, numChannels);
    }

    public IDalamudTextureWrap LoadImage(string filePath)
    {
        return _dalamudPluginInterface.UiBuilder.LoadImage(filePath);
    }

    public ICallGateProvider<TRet> GetIpcProvider<TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcProvider<TRet>(name);
    }

    public ICallGateProvider<T1, TRet> GetIpcProvider<T1, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcProvider<T1, TRet>(name);
    }

    public ICallGateProvider<T1, T2, TRet> GetIpcProvider<T1, T2, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcProvider<T1, T2, TRet>(name);
    }

    public ICallGateProvider<T1, T2, T3, TRet> GetIpcProvider<T1, T2, T3, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcProvider<T1, T2, T3, TRet>(name);
    }

    public ICallGateProvider<T1, T2, T3, T4, TRet> GetIpcProvider<T1, T2, T3, T4, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcProvider<T1, T2, T3, T4, TRet>(name);
    }

    public ICallGateProvider<T1, T2, T3, T4, T5, TRet> GetIpcProvider<T1, T2, T3, T4, T5, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcProvider<T1, T2, T3, T4, T5, TRet>(name);
    }

    public ICallGateProvider<T1, T2, T3, T4, T5, T6, TRet> GetIpcProvider<T1, T2, T3, T4, T5, T6, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcProvider<T1, T2, T3, T4, T5, T6, TRet>(name);
    }

    public ICallGateProvider<T1, T2, T3, T4, T5, T6, T7, TRet> GetIpcProvider<T1, T2, T3, T4, T5, T6, T7, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcProvider<T1, T2, T3, T4, T5, T6, T7, TRet>(name);
    }

    public ICallGateProvider<T1, T2, T3, T4, T5, T6, T7, T8, TRet> GetIpcProvider<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcProvider<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(name);
    }

    public ICallGateSubscriber<TRet> GetIpcSubscriber<TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcSubscriber<TRet>(name);
    }

    public ICallGateSubscriber<T1, TRet> GetIpcSubscriber<T1, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcSubscriber<T1, TRet>(name);
    }

    public ICallGateSubscriber<T1, T2, TRet> GetIpcSubscriber<T1, T2, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcSubscriber<T1, T2, TRet>(name);
    }

    public ICallGateSubscriber<T1, T2, T3, TRet> GetIpcSubscriber<T1, T2, T3, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcSubscriber<T1, T2, T3, TRet>(name);
    }

    public ICallGateSubscriber<T1, T2, T3, T4, TRet> GetIpcSubscriber<T1, T2, T3, T4, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcSubscriber<T1, T2, T3, T4, TRet>(name);
    }

    public ICallGateSubscriber<T1, T2, T3, T4, T5, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcSubscriber<T1, T2, T3, T4, T5, TRet>(name);
    }

    public ICallGateSubscriber<T1, T2, T3, T4, T5, T6, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, T6, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcSubscriber<T1, T2, T3, T4, T5, T6, TRet>(name);
    }

    public ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, TRet>(name);
    }

    public ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(string name)
    {
        return _dalamudPluginInterface.GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(name);
    }

    
    private bool _disposed;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if(!_disposed && disposing)
        {
            _dalamudPluginInterface.UiBuilder.Draw -= UiBuilderOnDraw;
            _dalamudPluginInterface.UiBuilder.OpenConfigUi -= UiBuilderOnOpenConfigUi;
            _dalamudPluginInterface.UiBuilder.OpenMainUi -= UiBuilderOnOpenMainUi;
        }
        _disposed = true;         
    }
    
        
    ~PluginInterfaceService()
    {
#if DEBUG
        // In debug-builds, make sure that a warning is displayed when the Disposable object hasn't been
        // disposed by the programmer.

        if( _disposed == false )
        {
            Service.Log.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
        }
#endif
        Dispose (true);
    }
}