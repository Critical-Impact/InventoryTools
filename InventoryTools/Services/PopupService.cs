using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace InventoryTools.Services;

public class PopupService : IDisposable
{
    private List<IPopup> _popups;
    private Queue<IPopup> _toOpen;
    public PopupService()
    {
        _popups = new();
        _toOpen = new();
    }

    public void Draw(Type windowType)
    {
        if (_toOpen.TryDequeue(out IPopup toOpen))
        {
            ImGui.OpenPopup(toOpen.Id);
        }
        foreach (var popup in _popups)
        {
            if (popup.Window == windowType)
            {
                popup.Draw();
            }
        }
    }

    public void AddPopup(IPopup popup)
    {
        popup.Finalized += PopupOnFinalized;
        _popups.Add(popup);
        _toOpen.Enqueue(popup);
    }

    public void RemovePopup(IPopup popup)
    {
        popup.Finalized -= PopupOnFinalized;
        _popups.Remove(popup);

    }

    private void PopupOnFinalized(IPopup popup)
    {
        RemovePopup(popup);
    }

    public void Dispose()
    {
        foreach (var popup in _popups)
        {
            popup.Finalized -= PopupOnFinalized;
        }
    }
}

public class NamePopup : IPopup
{
    private readonly Type _window;
    private readonly string _id;
    private string _name;
    private bool _drawnOnce = false;
    private readonly Action<(bool, string)> _callback;
    public event IPopup.FinalizedDelegate? Finalized;

    public NamePopup(Type window, string id, string defaultName, Action<(bool, string)> callback)
    {
        _window = window;
        _id = id;
        _name = defaultName;
        _callback = callback;
    }

    public Type Window => _window;
    public string Id => _id;

    public void Draw()
    {
        using var popup = ImRaii.Popup(_id);
        if (!popup)
        {
            if (_drawnOnce)
            {
                _callback.Invoke((false, _name));
                Finalized?.Invoke(this);
            }

            return;
        }

        _drawnOnce = true;
        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            _callback.Invoke((false, _name));
            Finalized?.Invoke(this);
            ImGui.CloseCurrentPopup();
            return;
        }

        ImGui.SetNextItemWidth(300 * ImGuiHelpers.GlobalScale);
        if (ImGui.IsWindowAppearing())
        {
            ImGui.SetKeyboardFocusHere();
        }

        var enterPressed = ImGui.InputTextWithHint("##newName", "Enter New Name...", ref _name, 512, ImGuiInputTextFlags.EnterReturnsTrue);

        if (!enterPressed)
        {
            return;
        }
        _callback.Invoke((true, _name));
        Finalized?.Invoke(this);

        ImGui.CloseCurrentPopup();
    }

    public void Finalize()
    {

    }
}


public class ConfirmPopup : IPopup
{
    private readonly Type _window;
    private readonly string _id;
    private readonly string _question;
    private readonly Action<bool> _callback;
    public event IPopup.FinalizedDelegate? Finalized;

    public ConfirmPopup(Type window, string id, string question, Action<bool> callback)
    {
        _window = window;
        _id = id;
        _question = question;
        _callback = callback;
    }

    public Type Window => _window;
    public string Id => _id;

    public void Draw()
    {
        using var popup = ImRaii.Popup(_id);
        if (!popup)
            return ;

        ImGui.TextUnformatted(
            _question + "\nThis operation cannot be undone!\n\n");
        ImGui.Separator();

        if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
        {
            _callback?.Invoke(true);
            ImGui.CloseCurrentPopup();
            Finalized?.Invoke(this);
        }
        ImGui.SameLine();
        if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
        {
            _callback?.Invoke(false);
            ImGui.CloseCurrentPopup();
            Finalized?.Invoke(this);
        }
    }

    public void Finalize()
    {

    }
}

public interface IPopup
{
    public Type Window { get; }
    string Id { get; }
    void Draw();
    void Finalize();
    delegate void FinalizedDelegate(IPopup popup);
    public event FinalizedDelegate? Finalized;
}