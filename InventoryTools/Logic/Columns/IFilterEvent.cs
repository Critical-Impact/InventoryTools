namespace InventoryTools.Logic.Columns
{
    public interface IFilterEvent
    {
        public void HandleEvent(FilterConfiguration configuration);
    }
}