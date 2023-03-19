using System;

namespace InventoryTools.Services.Interfaces;

public interface IGuiService
{
    public ulong HoveredItem { get; set; }
    public IntPtr FindAgentInterface(string addonName);

    public unsafe IntPtr GetUIModule();
}