using System;
using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Features;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Services;

public class ConfigurationWizardService : IConfigurationWizardService
{
    private readonly InventoryToolsConfiguration _configuration;
    private readonly List<IFeature> _availableFeatures;
    private readonly Dictionary<IFeature, IReadOnlySet<Type>> _versionedSettings = new();
    public ConfigurationWizardService(IEnumerable<IFeature> features, InventoryToolsConfiguration configuration)
    {
        _configuration = configuration;
        _availableFeatures = features.ToList();
    }

    /// <summary>
    /// Returns the settings that are applicable for this feature for this version.
    /// </summary>
    /// <param name="feature">The feature</param>
    /// <returns>A list of applicable settings</returns>
    public IReadOnlySet<Type> GetNewSettingTypes(IFeature feature)
    {
        if (_versionedSettings.TryGetValue(feature, out var value))
        {
            return value;
        }
        value = ConfiguredOnce
            ? feature.RelatedSettings
                .Where(c => !_configuration.WizardVersionsSeen.Contains(c.Version))
                .Select(c => c.GetType())
                .ToHashSet()
            : new HashSet<Type>();
        _versionedSettings[feature] = value;

        return value;
    }

    public List<IFeature> GetFeatures()
    {
        return _availableFeatures.ToList();
    }

    public List<IFeature> GetNewFeatures()
    {
        var versionsSeen = _configuration.WizardVersionsSeen;
        return _availableFeatures.Where(c => !c.RelatedSettings.Select(d => d.Version).Distinct().All(c => versionsSeen.Contains(c))).ToList();
    }

    public bool HasNewFeatures => GetNewFeatures().Count != 0;

    public void MarkFeaturesSeen()
    {
        var seenVersions = _availableFeatures.SelectMany(c => c.RelatedSettings).Select(c => c.Version).Distinct();
        foreach (var version in seenVersions)
        {
            _configuration.MarkWizardVersionSeen(version);
        }
    }

    public bool ShouldShowWizard => HasNewFeatures && _configuration.ShowWizardNewFeatures;
    public bool ConfiguredOnce => _configuration.WizardVersionsSeen.Count != 0;
    public void ClearFeaturesSeen()
    {
        _configuration.WizardVersionsSeen = new();
    }
}

public interface IConfigurationWizardService
{
    public List<IFeature> GetFeatures();
    public List<IFeature> GetNewFeatures();
    public bool HasNewFeatures { get; }
    public void MarkFeaturesSeen();
    public bool ShouldShowWizard { get; }
    public bool ConfiguredOnce { get; }

    public void ClearFeaturesSeen();
}