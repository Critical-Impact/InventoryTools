namespace InventoryTools.Logic.Columns
{
    public class RefreshFilterEvent : IFilterEvent
    {
        public void HandleEvent(FilterConfiguration configuration)
        {
            configuration.NeedsRefresh = true;
            configuration.StartRefresh();
        }
    }
}