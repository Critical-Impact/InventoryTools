using System;
using System.Collections.Generic;
using System.IO;
using DalaMock.Shared.Interfaces;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace InventoryToolsTesting.Services
{
    public class TestPluginInterfaceService : IPluginInterfaceService
    {
        public event Action? Draw;
        public event Action? OpenConfigUi;
        public event Action? OpenMainUi;
        public event DalamudPluginInterface.LanguageChangedDelegate? LanguageChanged;
        public event DalamudPluginInterface.ActivePluginsChangedDelegate? ActivePluginsChanged;
        public PluginLoadReason Reason { get; }
        public bool IsAutoUpdateComplete { get; }
        public string SourceRepository { get; }
        public string InternalName { get; }
        public bool IsDev { get; }
        public bool IsTesting { get; }
        public DateTime LoadTime { get; }
        public DateTime LoadTimeUTC { get; }
        public TimeSpan LoadTimeDelta { get; }
        public DirectoryInfo DalamudAssetDirectory { get; }
        public FileInfo AssemblyLocation { get; }
        public DirectoryInfo ConfigDirectory { get; }
        public FileInfo ConfigFile { get; }
        public bool IsDevMenuOpen { get; }
        public bool IsDebugging { get; }
        public string UiLanguage { get; }
        public ISanitizer Sanitizer { get; }
        public XivChatType GeneralChatType { get; }
        public IEnumerable<InstalledPluginState> InstalledPlugins { get; }
        public bool OpenPluginInstaller()
        {
            throw new NotImplementedException();
        }

        public T GetOrCreateData<T>(string tag, Func<T> dataGenerator) where T : class
        {
            throw new NotImplementedException();
        }

        public void RelinquishData(string tag)
        {
            throw new NotImplementedException();
        }

        public bool TryGetData<T>(string tag, out T? data) where T : class
        {
            throw new NotImplementedException();
        }

        public T? GetData<T>(string tag) where T : class
        {
            throw new NotImplementedException();
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

        public void SavePluginConfig(IPluginConfiguration? currentConfig)
        {
            throw new NotImplementedException();
        }

        public IPluginConfiguration? GetPluginConfig()
        {
            throw new NotImplementedException();
        }

        public string GetPluginConfigDirectory()
        {
            throw new NotImplementedException();
        }

        public string GetPluginLocDirectory()
        {
            throw new NotImplementedException();
        }

        public DalamudLinkPayload AddChatLinkHandler(uint commandId, Action<uint, SeString> commandAction)
        {
            throw new NotImplementedException();
        }

        public void RemoveChatLinkHandler(uint commandId)
        {
            throw new NotImplementedException();
        }

        public void RemoveChatLinkHandler()
        {
            throw new NotImplementedException();
        }

        public T? Create<T>(params object[] scopedObjects) where T : class
        {
            throw new NotImplementedException();
        }

        public bool Inject(object instance, params object[] scopedObjects)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}