using System.Collections.Generic;
using System.Linq;

namespace InventoryTools.Services.GameCraftSources
{
    public class GameCraftSourceService
    {
        private readonly IEnumerable<IGameCraftSource> _sources;

        public GameCraftSourceService(IEnumerable<IGameCraftSource> sources)
        {
            _sources = sources;
        }

        public IReadOnlyList<GameCraftCategory> GetAvailableCategories()
        {
            return _sources.SelectMany(source => source.GetAvailableCategories()).ToList();
        }
    }
}