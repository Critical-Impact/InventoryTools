using DalaMock.Core.DI;
using DalaMock.Core.Mocks;

namespace InventoryToolsMock
{
    class Program
    {
        static void Main(string[] args)
        {
            var mockContainer = new MockContainer();
            var mockDalamudUi = mockContainer.GetMockUi();
            var pluginLoader = mockContainer.GetPluginLoader();
            var mockPlugin = pluginLoader.AddPlugin(typeof(InventoryToolsPluginMock));
            mockDalamudUi.Run();
            pluginLoader.StartPlugin(mockPlugin);
        }

    }
}