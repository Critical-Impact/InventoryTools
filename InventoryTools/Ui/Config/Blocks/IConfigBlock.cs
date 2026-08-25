using System.Collections.Generic;

namespace InventoryTools.Ui.Config.Blocks;

public interface IConfigBlock
{
    IReadOnlyList<IConfigBlock> Children { get; }

    void Draw(ConfigDrawContext context);
}