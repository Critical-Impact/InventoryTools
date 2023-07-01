namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class Setting<T> : ISetting
    {
        public abstract T DefaultValue { get; set; }
        public virtual int LabelSize { get; set; } = 300;
        public virtual int InputSize { get; set; } = 250;
        public abstract T CurrentValue(InventoryToolsConfiguration configuration);
        public abstract void Draw(InventoryToolsConfiguration configuration);
        public abstract void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, T newValue);

        public abstract string Key { get; set; }
        public abstract string Name { get; set; }
        public abstract string HelpText { get; set; }
        
        public abstract SettingCategory SettingCategory { get; set; }
        public abstract SettingSubCategory SettingSubCategory { get; }

        public virtual bool HasValueSet(InventoryToolsConfiguration configuration)
        {
            var currentValue = CurrentValue(configuration);
            return !currentValue?.Equals(DefaultValue) ?? true;
        }

        public virtual void Reset(InventoryToolsConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }
    }
}