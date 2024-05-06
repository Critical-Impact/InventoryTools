using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays
{
    public class InventoryBuddyOverlay2: InventoryBuddyOverlay
    {
        public InventoryBuddyOverlay2(ILogger<InventoryBuddyOverlay2> logger, AtkInventoryBuddy2 overlay, ICharacterMonitor characterMonitor) : base(logger,overlay, characterMonitor)
        {
        }
        
    }
}