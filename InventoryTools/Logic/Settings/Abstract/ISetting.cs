using System.Numerics;

namespace InventoryTools.Logic.Settings.Abstract
{
    public interface ISetting
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string HelpText { get; set; }

        public string WizardName { get; }
        public string? Image { get; }
        public Vector2? ImageSize { get; }

        public string Version { get; }

        public bool AppearsInConfigWindow { get; }

        public bool HasValueSet(InventoryToolsConfiguration configuration);

        public void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset,
            bool? disableColouring);

    }
}