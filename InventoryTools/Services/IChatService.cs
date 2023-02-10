using Dalamud.Game.Gui;

namespace InventoryTools.Services;

public interface IChatService
{
    public void Print(string message);
}

public class ChatService : IChatService
{
    private ChatGui _chatGui;
    
    public ChatService(ChatGui chatGui)
    {
        _chatGui = chatGui;
    }
    
    public void Print(string message)
    {
        _chatGui.Print(message);
    }
}