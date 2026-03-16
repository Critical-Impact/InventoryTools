using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Interfaces;

public interface ICompendiumViewSection
{
    public void Draw(SectionState sectionState);
    public bool ShouldDraw(SectionState sectionState);
}