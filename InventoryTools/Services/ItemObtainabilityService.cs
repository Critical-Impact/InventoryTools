using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class ItemObtainabilityService : IItemObtainabilityService
{
    private readonly ClassJobService _classJobService;
    private readonly ILogger<ItemObtainabilityService> _logger;

    public ItemObtainabilityService(ClassJobService classJobService, ILogger<ItemObtainabilityService> logger)
    {
        _classJobService = classJobService;
        _logger = logger;
    }

    public IReadOnlyList<ObtainabilityRequirement> GetRequirements(ItemRow item, IngredientPreferenceType source, RecipeRow? recipe = null)
    {
        var requirements = new List<ObtainabilityRequirement>();

        switch (source)
        {
            case IngredientPreferenceType.Crafting:
                AddCraftingRequirements(recipe, requirements);
                break;
            case IngredientPreferenceType.Mining:
            case IngredientPreferenceType.Botany:
                AddGatheringRequirements(item, source, requirements);
                break;
            case IngredientPreferenceType.Fishing:
                AddFishingRequirements(item, requirements);
                break;
            case IngredientPreferenceType.SpearFishing:
                AddSpearfishingRequirements(item, requirements);
                break;
        }

        return requirements;
    }

    private void AddCraftingRequirements(RecipeRow? recipe, List<ObtainabilityRequirement> requirements)
    {
        if (recipe == null) return;

        var levelTable = recipe.RecipeLevelTable;
        if (levelTable != null)
        {
            var requiredLevel = levelTable.Base.ClassJobLevel;
            if (requiredLevel > 0)
            {
                var craftTypeId = recipe.Base.CraftType.RowId;
                var playerLevel = _classJobService.GetPlayerLevelByCraftTypeId(craftTypeId);
                var jobName = recipe.CraftType?.FormattedName ?? "Crafter";
                var craftJob = _classJobService.GetClassJobListFromCraftTypeId(craftTypeId);
                RowRef? classJobRowRef = craftJob.HasValue ? _classJobService.GetClassJobRowRef(craftJob.Value) : null;
                requirements.Add(new ObtainabilityRequirement(
                    ObtainabilityRequirementType.JobLevel,
                    playerLevel >= requiredLevel,
                    $"{jobName} Lv {requiredLevel}",
                    classJobRowRef));
            }
        }

        var bookId = recipe.Base.SecretRecipeBook.RowId;
        if (bookId != 0)
        {
            var bookName = recipe.Base.SecretRecipeBook.ValueNullable?.Name.ExtractText() ?? $"Recipe Book #{bookId}";
            var hasBook = _classJobService.IsSecretRecipeBookUnlocked(recipe.Base.SecretRecipeBook);
            requirements.Add(new ObtainabilityRequirement(
                ObtainabilityRequirementType.SecretRecipeBook,
                hasBook,
                bookName,
                (RowRef)recipe.Base.SecretRecipeBook));
        }

        //Determine later if this is actually a thing
        // if (recipe.Base.IsSpecializationRequired)
        // {
        //     // CraftType RowId 0-7 maps to ClassJob RowId 8-15 (Carpenter-Culinarian)
        //     var classJobId = recipe.Base.CraftType.RowId + 8;
        //     var isSpecialist = _classJobService.IsSpecialist(classJobId);
        //     var jobName = recipe.CraftType?.FormattedName ?? "Crafter";
        //     requirements.Add(new ObtainabilityRequirement(
        //         ObtainabilityRequirementType.Specialization,
        //         isSpecialist,
        //         $"{jobName} Specialist"));
        // }
    }

    private void AddGatheringRequirements(ItemRow item, IngredientPreferenceType preferenceType, List<ObtainabilityRequirement> requirements)
    {
        var infoTypes = preferenceType.ToItemInfoTypes();
        var gatheringSources = item.Sources
            .OfType<ItemGatheringSource>()
            .Where(s => infoTypes.Contains(s.Type))
            .ToList();

        if (gatheringSources.Count == 0) return;

        var minLevel = gatheringSources
            .Select(s => (int)s.GatheringItem.Base.GatheringItemLevel.Value.GatheringItemLevel)
            .Where(l => l > 0)
            .DefaultIfEmpty(0)
            .Min();

        if (minLevel > 0)
        {
            var job = preferenceType == IngredientPreferenceType.Mining
                ? ClassJobService.ClassJobList.Miner
                : ClassJobService.ClassJobList.Botanist;
            var playerLevel = _classJobService.GetPlayerLevel(job);
            var jobName = preferenceType == IngredientPreferenceType.Mining ? "Miner" : "Botanist";
            requirements.Add(new ObtainabilityRequirement(
                ObtainabilityRequirementType.JobLevel,
                playerLevel >= minLevel,
                $"{jobName} Lv {minLevel}",
                _classJobService.GetClassJobRowRef(job)));
        }

        // Folklore tome: GatheringSubCategory.Division is the tomeId for IsFolkloreBookUnlocked
        var folkloreEntry = gatheringSources
            .SelectMany(s => s.GatheringItem.GatheringPoints)
            .Select(p => p.Base.GatheringSubCategory.ValueNullable)
            .FirstOrDefault(sc => sc.HasValue && sc.Value.Division != 0);

        if (folkloreEntry.HasValue)
        {
            var tomeId = (uint)folkloreEntry.Value.Division;
            var noteBookDivision = new RowRef<NotebookDivision>(folkloreEntry.Value.ExcelPage.Module, folkloreEntry.Value.Division);
            var hasBook = _classJobService.IsFolkloreBookUnlocked(noteBookDivision);
            var tomeName = folkloreEntry.Value.Item.ValueNullable?.Name.ExtractText() ?? $"Folklore Tome #{tomeId}";
            RowRef? tomeRowRef = noteBookDivision.Value.AsUntypedRowRef();
            requirements.Add(new ObtainabilityRequirement(
                ObtainabilityRequirementType.FolkloreTome,
                hasBook,
                tomeName,
                tomeRowRef));
        }
    }

    private void AddFishingRequirements(ItemRow item, List<ObtainabilityRequirement> requirements)
    {
        var fishingSources = item.Sources.OfType<ItemFishingSource>().ToList();
        if (fishingSources.Count == 0) return;

        var minLevel = fishingSources
            .Select(s => (int)s.FishParameter.Base.GatheringItemLevel.Value.GatheringItemLevel)
            .Where(l => l > 0)
            .DefaultIfEmpty(0)
            .Min();

        if (minLevel > 0)
        {
            var playerLevel = _classJobService.GetPlayerLevel(ClassJobService.ClassJobList.Fisher);
            requirements.Add(new ObtainabilityRequirement(
                ObtainabilityRequirementType.JobLevel,
                playerLevel >= minLevel,
                $"Fisher Lv {minLevel}",
                _classJobService.GetClassJobRowRef(ClassJobService.ClassJobList.Fisher)));
        }

        // FishParameter.GatheringSubCategory.Division is the tomeId for IsFolkloreBookUnlocked
        var folkloreEntry = fishingSources
            .Select(s => s.FishParameter.Base.GatheringSubCategory.ValueNullable)
            .FirstOrDefault(sc => sc.HasValue && sc.Value.Division != 0);

        if (folkloreEntry.HasValue)
        {
            var tomeId = (uint)folkloreEntry.Value.Division;
            var hasBook = _classJobService.IsFolkloreBookUnlocked(new RowRef<NotebookDivision>(folkloreEntry.Value.ExcelPage.Module, folkloreEntry.Value.Division));
            var tomeName = folkloreEntry.Value.Item.ValueNullable?.Name.ExtractText() ?? $"Folklore Tome #{tomeId}";
            RowRef? tomeRowRef = folkloreEntry.Value.Item.RowId != 0 ? (RowRef)folkloreEntry.Value.Item : null;
            requirements.Add(new ObtainabilityRequirement(
                ObtainabilityRequirementType.FolkloreTome,
                hasBook,
                tomeName,
                tomeRowRef));
        }
    }

    private void AddSpearfishingRequirements(ItemRow item, List<ObtainabilityRequirement> requirements)
    {
        var spearSources = item.Sources.OfType<ItemSpearfishingSource>().ToList();
        if (spearSources.Count == 0) return;

        var minLevel = spearSources
            .Select(s => (int)s.SpearfishingItemRow.Base.GatheringItemLevel.Value.GatheringItemLevel)
            .Where(l => l > 0)
            .DefaultIfEmpty(0)
            .Min();

        if (minLevel > 0)
        {
            var playerLevel = _classJobService.GetPlayerLevel(ClassJobService.ClassJobList.Fisher);
            requirements.Add(new ObtainabilityRequirement(
                ObtainabilityRequirementType.JobLevel,
                playerLevel >= minLevel,
                $"Fisher Lv {minLevel}",
                _classJobService.GetClassJobRowRef(ClassJobService.ClassJobList.Fisher)));
        }
    }
}
