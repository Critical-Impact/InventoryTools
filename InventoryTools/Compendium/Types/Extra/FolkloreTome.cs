using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace InventoryTools.Compendium.Types.Extra;

public class FolkloreTome
{
    public FolkloreTome(RowRef<NotebookDivision> notebookDivision, IReadOnlyList<GatheringSubCategory> subCategories)
    {
        NotebookDivision = notebookDivision;
        SubCategories = subCategories;

        var firstWithItem = subCategories.FirstOrNull(s => s.Item.RowId != 0)?.Item;
        if (firstWithItem != null && firstWithItem.Value.RowId != 0)
        {
            TomeItem = firstWithItem.Value;
        }

        var notebookQuest = notebookDivision.ValueNullable?.QuestUnlock;
        if (notebookQuest.HasValue && notebookQuest.Value.RowId != 0)
        {
            UnlockQuest = notebookQuest.Value;
        }
        else
        {
            var subQuest = subCategories.FirstOrNull(s => s.Quest.RowId != 0)?.Quest;
            if (subQuest != null && subQuest.Value.RowId != 0)
            {
                UnlockQuest = subQuest.Value;
            }
        }

        GatheringTypes = subCategories
            .Select(s => s.GatheringType)
            .Where(g => g.RowId != 0)
            .DistinctBy(g => g.RowId)
            .ToList();

        ClassJobs = subCategories
            .Select(s => s.ClassJob)
            .Where(c => c.RowId != 0)
            .DistinctBy(c => c.RowId)
            .ToList();
    }

    public uint RowId => NotebookDivision.RowId;
    public RowRef<NotebookDivision> NotebookDivision { get; }
    public IReadOnlyList<GatheringSubCategory> SubCategories { get; }
    public RowRef<Item>? TomeItem { get; }
    public RowRef<Quest>? UnlockQuest { get; }
    public IReadOnlyList<RowRef<GatheringType>> GatheringTypes { get; }
    public IReadOnlyList<RowRef<ClassJob>> ClassJobs { get; }
}
