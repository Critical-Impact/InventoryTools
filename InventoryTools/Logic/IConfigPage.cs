namespace InventoryTools.Logic
{
    public interface IConfigPage
    {
        public string Name { get; }
        public void Draw();
        public bool IsMenuItem { get; }
        
        public bool DrawBorder { get; }
    }
}