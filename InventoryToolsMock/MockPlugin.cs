using CriticalCommonLib;
using DalaMock.Dalamud;
using DalaMock.Interfaces;
using DalaMock.Mock;
using DalaMock.Windows;
using Dalamud;
using Dalamud.Interface.ImGuiFileDialog;
using InventoryTools;
using InventoryTools.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryToolsMock;

public class MockPlugin : IMockPlugin, IDisposable
{
    private bool _isStarted;
    private PluginLoader _loader;
    private WindowService _windowService;
    private FileDialogManager _fileDialogManager;

    public void Draw()
    {
        if (_isStarted)
        {
            if (_windowService != null)
            {
                foreach (var window in _windowService.WindowSystem.Windows)
                {
                    window.AllowPinning = false;
                    window.AllowClickthrough = false;
                }
                _windowService.WindowSystem.Draw();
            }
            _fileDialogManager.Draw();
        }
    }

    public void Dispose()
    {

    }

    public bool IsStarted => _isStarted;

    public void Start(MockProgram program, MockService mockService, MockPluginInterfaceService mockPluginInterfaceService)
    {
        var service = new Service();
        Service.Interface = mockPluginInterfaceService;
        Service.SeTime = new MockSeTime();
        var clientLanguage = ClientLanguage.English;

        _loader = new MockPluginLoader(program, this, mockService, mockPluginInterfaceService, service, program.SeriLog);
        var host = _loader.Build();
        
        _windowService = host.Services.GetRequiredService<WindowService>();
        _fileDialogManager = host.Services.GetRequiredService<FileDialogManager>();

        var mockGameGuiWindow = new MockGameGuiWindow(mockService.MockGameGui, "Mock Game Gui");
        mockGameGuiWindow.IsOpen = true;
        _windowService.WindowSystem.AddWindow(mockGameGuiWindow);
        _windowService.OpenWindow<MockWindow>();
        _isStarted = true;
    }

    public void Stop(MockProgram program, MockService mockService, MockPluginInterfaceService mockPluginInterfaceService)
    {
        _isStarted = false;
        _windowService = null!;
        _fileDialogManager = null!;
        _loader.Dispose();
        Dispose();
    }
}
