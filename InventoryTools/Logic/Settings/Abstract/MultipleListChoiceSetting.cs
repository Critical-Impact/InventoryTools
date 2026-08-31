using System.Collections.Generic;
using System.Linq;
using AllaganLib.Shared.Extensions;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract;

public abstract class MultipleListChoiceSetting : MultipleChoiceSetting<string>
{
    private readonly IListService _listService;

    protected MultipleListChoiceSetting(ILogger logger, ImGuiService imGuiService, IListService listService) : base(logger,
        imGuiService)
    {
        _listService = listService;
    }

    public abstract FilterType? ListFilterType { get; }

    public override List<string> DefaultValue { get; set; } = [];

    public override List<string> CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.Get(Key, (List<string>?)null) ?? [];
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, List<string> newValue)
    {
        configuration.Set(Key, [..newValue]);
    }

    public override Dictionary<string, string> GetChoices(InventoryToolsConfiguration configuration)
    {
        return _listService.Lists
            .Where(c => ListFilterType == null || c.FilterType == ListFilterType.Value)
            .DistinctBy(c => c.Key)
            .ToDictionary(c => c.Key, c => c.NameFormatted);
    }

    public override Dictionary<string, string> GetActiveChoices(InventoryToolsConfiguration configuration)
    {
        var searchString = SearchString.ToParseable();
        var currentChoices = CurrentValue(configuration);
        return GetChoices(configuration)
            .Where(c => FilterSearch(c.Key, c.Value, searchString) &&
                        (!HideAlreadyPicked || !currentChoices.Contains(c.Key)))
            .ToDictionary(c => c.Key, c => c.Value);
    }

    public override string GetPreviewValue(List<string> items)
    {
        return items.Count == 0 ? "All lists" : $"{items.Count} lists selected";
    }

    public override bool HideAlreadyPicked { get; set; } = true;
}