using CriticalCommonLib.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace InventoryToolsMock;

public class MockTooltipService : ITooltipService
{
    public void AddTooltipTweak(TooltipService.TooltipTweak tooltipTweak)
    {
        
    }

    public void ActionHoveredDetour(ulong a1, int a2, uint a3, int a4, byte a5)
    {
    }

    public unsafe nint ActionTooltipDetour(AtkUnitBase* addon, void* a2, ulong a3)
    {
        return 0;
    }

    public void Disable()
    {
    }

    public void Dispose()
    {
    }
}