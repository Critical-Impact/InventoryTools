using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Services;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Services;

public class MarketBoardService : IMarketBoardService
{
    private readonly ICharacterMonitor _characterMonitor;
    private readonly InventoryToolsConfiguration _configuration;

    public MarketBoardService(ICharacterMonitor characterMonitor, InventoryToolsConfiguration configuration)
    {
        _characterMonitor = characterMonitor;
        _configuration = configuration;
    }
    
    public List<uint> GetDefaultWorlds()
    {
        var useActiveWorld = _configuration.MarketBoardUseActiveWorld;
        var useHomeWorld = _configuration.MarketBoardUseHomeWorld;
        HashSet<uint> worldIds = _configuration.MarketBoardWorldIds.ToHashSet();

        var activeCharacter = _characterMonitor.ActiveCharacter;
        if (activeCharacter != null)
        {
            if (useActiveWorld && activeCharacter.ActiveWorldId != 0)
            {
                worldIds.Add(activeCharacter.ActiveWorldId);
            }
            if (useHomeWorld && activeCharacter.WorldId != 0)
            {
                worldIds.Add(activeCharacter.WorldId);
            }
        }

        return worldIds.ToList();
    }
}