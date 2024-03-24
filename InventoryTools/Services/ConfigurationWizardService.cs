using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic;
using InventoryTools.Logic.Features;

namespace InventoryTools.Services;

public class ConfigurationWizardService : IConfigurationWizardService
{
    private readonly InventoryToolsConfiguration _configuration;
    private List<IFeature> _availableFeatures = new();
    public ConfigurationWizardService(IEnumerable<IFeature> features, InventoryToolsConfiguration configuration)
    {
        _configuration = configuration;
        _availableFeatures = features.ToList();
        foreach (var feature in _availableFeatures)
        {
            foreach (var setting in feature.RelatedSettings)
            {
                setting.Name = setting.WizardName;
                setting.HideReset = true;
                setting.ColourModified = false;
            }
        }
    }
    
    public List<IFeature> GetFeatures()
    {
        return _availableFeatures.ToList();
    }

    public List<IFeature> GetNewFeatures()
    {
        var versionsSeen = _configuration.WizardVersionsSeen;
        return _availableFeatures.Where(c => !versionsSeen.Overlaps(c.RelatedSettings.Select(d => d.Version))).ToList();
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