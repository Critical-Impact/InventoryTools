using System;
using System.Numerics;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class Setting<T> : ISetting
    {
        public ImGuiService ImGuiService { get; }

        public Setting(ILogger logger, ImGuiService imGuiService)
        {
            ImGuiService = imGuiService;
        }
        public abstract T DefaultValue { get; set; }
        public virtual int InputSize { get; set; } = 300;
        public abstract T CurrentValue(InventoryToolsConfiguration configuration);
        public abstract void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset,
            bool? disableColouring);
        public abstract void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, T newValue);

        public abstract string Key { get; set; }
        public abstract string Name { get; set; }
        public abstract string HelpText { get; set; }
        public abstract SettingCategory SettingCategory { get; set; }
        public abstract SettingSubCategory SettingSubCategory { get; }
        public abstract string Version { get; }

        public virtual string? Image { get; } = null;
        public virtual Vector2? ImageSize { get; } = null;

        public virtual uint? Order { get; } = null;


        public virtual string WizardName
        {
            get
            {
                return Name;
            }
        }

        public virtual bool HasValueSet(InventoryToolsConfiguration configuration)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null && DefaultValue == null)
            {
                return false;
            }
            return !currentValue?.Equals(DefaultValue) ?? true;
        }

        public virtual void Reset(InventoryToolsConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }
    }
}