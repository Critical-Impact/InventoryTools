namespace InventoryTools.Logic.Columns.Abstract.ColumnSettings;

public interface IColumnSetting
{
    public int LabelSize { get; set; }
    public int InputSize { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public string HelpText { get; set; }
    public bool ShowReset { get; set; }
        
    public bool HasValueSet(ColumnConfiguration configuration);
    public void Draw(ColumnConfiguration configuration, string? helpText);
    public void ResetFilter(ColumnConfiguration configuration);
}