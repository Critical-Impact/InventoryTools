using System.Threading.Tasks;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryToolsTesting.Tests.Abstract;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace InventoryToolsTesting.Tests;

public class TestMediationFlow : BaseTest
{
    [Test]
    public async Task TestMediation()
    {
        var listService = Host.Services.GetRequiredService<IListService>()!;
        var listFilterService = Host.Services.GetRequiredService<ListFilterService>()!;
        var tableService = Host.Services.GetRequiredService<TableService>();
        var filterConfigurationFactory = Host.Services.GetRequiredService<FilterConfiguration.Factory>();


        ListUpdatedMessage? listUpdatedMessage = null;
        TaskCompletionSource<ListUpdatedMessage> tcs = new TaskCompletionSource<ListUpdatedMessage>();

        MediatorService.Subscribe<ListUpdatedMessage>(this, msg =>
        {
            listUpdatedMessage = msg;
            tcs.SetResult(msg);
        });

        var gameItemsList = filterConfigurationFactory.Invoke();
        gameItemsList.Name = "test";
        gameItemsList.FilterType = FilterType.GameItemFilter;
        gameItemsList.AllowRefresh = true;

        listService.AddList(gameItemsList);

        MediatorService.Publish(new RequestListUpdateMessage(gameItemsList));

        await Task.WhenAny(tcs.Task, Task.Delay(10000));
        Assert.IsNotNull(listUpdatedMessage);



    }
}