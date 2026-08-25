using System;
using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config.Blocks;
using InventoryTools.Ui.Config.Layouts;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.Config;

public class SettingCoverageService
{
    private readonly ILogger<SettingCoverageService> _logger;

    public SettingCoverageService(ILogger<SettingCoverageService> logger,
        IEnumerable<ISetting> settings,
        IEnumerable<IConfigLayout> layouts)
    {
        _logger = logger;

        var placements = new Dictionary<Type, int>();
        foreach (var layout in layouts)
        {
            BuildPlacements(layout.Build(), placements);
        }

        var known = settings.ToList();
        var knownTypes = known.Select(c => c.GetType()).ToHashSet();

        AllSettings = known;

        IgnoredSettings = known.Where(c => !c.AppearsInConfigWindow).ToList();

        UnplacedSettings = known.Where(c => c.AppearsInConfigWindow)
            .Where(c => !placements.ContainsKey(c.GetType()))
            .OrderBy(c => c.Name)
            .ToList();
        DuplicateSettings = placements.Where(c => c.Value > 1).Select(c => c.Key).ToList();
        InvalidSettings = placements.Keys.Where(c => !knownTypes.Contains(c)).ToList();
    }

    private IReadOnlyList<ISetting> AllSettings { get; }
    private IReadOnlyList<ISetting> UnplacedSettings { get; }
    private IReadOnlyList<ISetting> IgnoredSettings { get; }
    private IReadOnlyList<Type> DuplicateSettings { get; }
    private IReadOnlyList<Type> InvalidSettings { get; }

    public void Report()
    {
        foreach (var setting in InvalidSettings)
        {
            _logger.LogError("Config layout places {Setting}, which is not registered.", setting.Name);
        }

        foreach (var setting in DuplicateSettings)
        {
            _logger.LogWarning("Setting {Setting} is placed by more than one config layout.", setting.Name);
        }

        if (UnplacedSettings.Count != 0)
        {
            _logger.LogInformation(
                "{Count} of {Total} config settings are not placed by a layout ({Skipped} more are drawn outside the config window): {Settings}",
                UnplacedSettings.Count,
                AllSettings.Count - IgnoredSettings.Count,
                IgnoredSettings.Count,
                string.Join(", ", UnplacedSettings.Select(c => c.GetType().Name)));
        }
        else
        {
            _logger.LogInformation(
                "All {Total} config settings are placed by a layout ({Skipped} more are drawn outside the config window).",
                AllSettings.Count - IgnoredSettings.Count,
                IgnoredSettings.Count);
        }
    }

    private void BuildPlacements(IConfigBlock block, IDictionary<Type, int> placements)
    {
        if (block is SettingBlock settingNode)
        {
            placements.TryGetValue(settingNode.SettingType, out var count);
            placements[settingNode.SettingType] = count + 1;
        }

        foreach (var child in block.Children)
        {
            BuildPlacements(child, placements);
        }

        if (block is not PageLayout pageNode)
        {
            return;
        }

        foreach (var subPage in pageNode.SubPages)
        {
            BuildPlacements(subPage, placements);
        }
    }
}