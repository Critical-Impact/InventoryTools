using DalaMock.Core.DI;
using DalaMock.Core.Mocks;

namespace InventoryToolsMock
{
    class Program
    {
        static void Main(string[] args)
        {
            var dalamudConfiguration = new MockDalamudConfiguration();
            var mockContainer = new MockContainer(dalamudConfiguration);
            var mockDalamudUi = mockContainer.GetMockUi();
            var pluginLoader = mockContainer.GetPluginLoader();
            var mockPlugin = pluginLoader.AddPlugin(typeof(InventoryToolsPluginMock));
            mockDalamudUi.Run();
        }

    }
}