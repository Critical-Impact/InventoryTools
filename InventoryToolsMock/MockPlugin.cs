using CriticalCommonLib;
using DalaMock.Dalamud;
using DalaMock.Interfaces;
using DalaMock.Mock;
using DalaMock.Windows;
using Dalamud;
using Dalamud.Interface.ImGuiFileDialog;
using InventoryTools;
using InventoryTools.Host;
using InventoryTools.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryToolsMock;

public class MockPlugin : IMockPlugin
{
    private bool _isStarted;
    private PluginLoader? _loader;
    private WindowService? _windowService;
    private FileDialogManager? _fileDialogManager;
    private Service? _service;

    public void Draw()
    {
        if (_isStarted)
        {
            if (_windowService != null)
            {
                for (var index = 0; index < _windowService.WindowSystem.Windows.Count; index++)
                {
                    var window = _windowService.WindowSystem.Windows[index];
                    if (window != null)
                    {
                        window.AllowPinning = false;
                        window.AllowClickthrough = false;
                        window.AllowClickthrough = false;
                    }
                }

                try
                {
                    _windowService.WindowSystem.Draw();
                }
                catch (Exception e)
                {
                }
            }
            _fileDialogManager?.Draw();
        }
    }

    public void Dispose()
    {
        _windowService = null!;
        _fileDialogManager = null!;
        _loader?.Dispose();
        _service?.Dispose();
        _loader = null;
        _service = null;
    }

    public bool IsStarted => _isStarted;

    public void Start(MockProgram program, MockService mockService, MockPluginInterfaceService mockPluginInterfaceService)
    {
        _service = new Service();
        Service.Interface = mockPluginInterfaceService;
        var clientLanguage = ClientLanguage.English;

        _loader = new MockPluginLoader(program, this, mockService, mockPluginInterfaceService, _service, program.SeriLog);
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
        Dispose();
    }
}
