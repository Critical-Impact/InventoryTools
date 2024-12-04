namespace InventoryTools.Logic.Columns.Abstract.ColumnSettings;

public abstract class ColumnSetting<T> : IColumnSetting
{
    public virtual int LabelSize { get; set; } = 220;
    public virtual int InputSize { get; set; } = 250;
    
    public abstract T CurrentValue(ColumnConfiguration configuration);
    public abstract void Draw(ColumnConfiguration configuration, string? helpText);
    public abstract void ResetFilter(ColumnConfiguration configuration);
    public abstract bool HasValueSet(ColumnConfiguration configuration);
    public abstract void UpdateColumnConfiguration(ColumnConfiguration configuration, T newValue);
    
    public abstract string Key { get; set; }
    public abstract string Name { get; set; }
    public abstract string HelpText { get; set; }
    public virtual bool ShowReset { get; set; } = true;
    public abstract T DefaultValue { get; set; }


}