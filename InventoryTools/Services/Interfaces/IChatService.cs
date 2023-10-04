using Dalamud.Plugin.Services;

namespace InventoryTools.Services.Interfaces;

public interface IChatService
{
    public void Print(string message);
}

public class ChatService : IChatService
{
    private IChatGui _chatGui;
    
    public ChatService(IChatGui chatGui)
    {
        _chatGui = chatGui;
    }
    
    public void Print(string message)
    {
        _chatGui.Print(message);
    }
}