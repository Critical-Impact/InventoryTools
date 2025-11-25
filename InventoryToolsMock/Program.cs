using System.Numerics;
using DalaMock.Core.DI;
using DalaMock.Core.Mocks;
using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Lumina.Data;
using Lumina.Excel.Sheets;

namespace InventoryToolsMock
{
    class Program
    {
        static void Main(string[] args)
        {
            var mockContainer = new MockContainer(new MockDalamudConfiguration()
            {
            });
            var mockDalamudUi = mockContainer.GetMockUi();
            var pluginLoader = mockContainer.GetPluginLoader();
            mockContainer.GetGameData().GetExcelSheet<Item>();
            mockContainer.GetGameData().GetExcelSheet<Quest>();
            mockContainer.GetGameData().GetExcelSheet<ENpcBase>();
            mockContainer.GetGameData().GetExcelSheet<ENpcResident>();
            var mockPlugin = pluginLoader.AddPlugin(typeof(InventoryToolsPluginMock));
            mockDalamudUi.Run();
            pluginLoader.StartPlugin(mockPlugin);
        }

    }
}