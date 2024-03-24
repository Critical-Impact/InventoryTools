using System.Collections.Generic;

namespace InventoryTools.Services.Interfaces;

public interface IMarketBoardService
{
    List<uint> GetDefaultWorlds();
}