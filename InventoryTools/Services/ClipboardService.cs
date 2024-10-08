using ImGuiNET;

namespace InventoryTools.Services;

public class ClipboardService : IClipboardService
{
    public void CopyToClipboard(string text)
    {
        ImGui.SetClipboardText(text);
    }

    public string PasteFromClipboard()
    {
        return ImGui.GetClipboardText();
    }
}

public interface IClipboardService
{
    public void CopyToClipboard(string text);
    public string PasteFromClipboard();
}