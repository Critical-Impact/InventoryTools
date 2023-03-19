using System;
using System.IO;
using Dalamud.Plugin.Ipc;
using ImGuiScene;

namespace InventoryTools.Services.Interfaces;

public interface IPluginInterfaceService
{
    public event Action? Draw;

    public event Action OpenConfigUi;
    FileInfo ConfigFile { get; }
    
    DirectoryInfo ConfigDirectory { get; }
    
    public FileInfo AssemblyLocation { get; }

    public TextureWrap LoadImageRaw(byte[] imageData, int width, int height, int numChannels);

    public TextureWrap LoadImage(string filePath);
    
    ICallGateProvider<TRet> GetIpcProvider<TRet>(string name);

    /// <inheritdoc cref="ICallGateProvider{TRet}"/>
    ICallGateProvider<T1, TRet> GetIpcProvider<T1, TRet>(string name);

    /// <inheritdoc cref="ICallGateProvider{TRet}"/>
    ICallGateProvider<T1, T2, TRet> GetIpcProvider<T1, T2, TRet>(string name);

    /// <inheritdoc cref="ICallGateProvider{TRet}"/>
    ICallGateProvider<T1, T2, T3, TRet> GetIpcProvider<T1, T2, T3, TRet>(string name);

    /// <inheritdoc cref="ICallGateProvider{TRet}"/>
    ICallGateProvider<T1, T2, T3, T4, TRet> GetIpcProvider<T1, T2, T3, T4, TRet>(string name);

    /// <inheritdoc cref="ICallGateProvider{TRet}"/>
    ICallGateProvider<T1, T2, T3, T4, T5, TRet> GetIpcProvider<T1, T2, T3, T4, T5, TRet>(string name);

    /// <inheritdoc cref="ICallGateProvider{TRet}"/>
    ICallGateProvider<T1, T2, T3, T4, T5, T6, TRet> GetIpcProvider<T1, T2, T3, T4, T5, T6, TRet>(string name);

    /// <inheritdoc cref="ICallGateProvider{TRet}"/>
    ICallGateProvider<T1, T2, T3, T4, T5, T6, T7, TRet> GetIpcProvider<T1, T2, T3, T4, T5, T6, T7, TRet>(string name);

    /// <inheritdoc cref="ICallGateProvider{TRet}"/>
    ICallGateProvider<T1, T2, T3, T4, T5, T6, T7, T8, TRet> GetIpcProvider<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(string name);

    /// <summary>
    /// Gets an IPC subscriber.
    /// </summary>
    /// <typeparam name="TRet">The return type for funcs. Use object if this is unused.</typeparam>
    /// <param name="name">The name of the IPC registration.</param>
    /// <returns>An IPC subscriber.</returns>
    ICallGateSubscriber<TRet> GetIpcSubscriber<TRet>(string name);

    /// <inheritdoc cref="ICallGateSubscriber{TRet}"/>
    ICallGateSubscriber<T1, TRet> GetIpcSubscriber<T1, TRet>(string name);

    /// <inheritdoc cref="ICallGateSubscriber{TRet}"/>
    ICallGateSubscriber<T1, T2, TRet> GetIpcSubscriber<T1, T2, TRet>(string name);

    /// <inheritdoc cref="ICallGateSubscriber{TRet}"/>
    ICallGateSubscriber<T1, T2, T3, TRet> GetIpcSubscriber<T1, T2, T3, TRet>(string name);

    /// <inheritdoc cref="ICallGateSubscriber{TRet}"/>
    ICallGateSubscriber<T1, T2, T3, T4, TRet> GetIpcSubscriber<T1, T2, T3, T4, TRet>(string name);

    /// <inheritdoc cref="ICallGateSubscriber{TRet}"/>
    ICallGateSubscriber<T1, T2, T3, T4, T5, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, TRet>(string name);

    /// <inheritdoc cref="ICallGateSubscriber{TRet}"/>
    ICallGateSubscriber<T1, T2, T3, T4, T5, T6, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, T6, TRet>(string name);

    /// <inheritdoc cref="ICallGateSubscriber{TRet}"/>
    ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, TRet>(string name);

    /// <inheritdoc cref="ICallGateSubscriber{TRet}"/>
    ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TRet> GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(string name);

    void Dispose();
}