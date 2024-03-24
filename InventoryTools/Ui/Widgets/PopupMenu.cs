using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Ui.Widgets;

public class PopupMenu
{
    private List<IPopupMenuItem> _items;
    private PopupMenuButtons _openButtons;
    private string _id;

    public enum PopupMenuButtons
    {
        Left,
        Right,
        Middle,
        LeftRight,
        All
    }
    
    public interface IPopupMenuItem
    {
        public void Draw();
        public string? DrawPopup();
        public Func<bool>? ShouldDraw { get; }
    }
    
    public class PopupMenuItemSelectable : IPopupMenuItem
    {
        private string _name;
        private string _id;
        private string? _tooltip;
        private Action<string>? _callback { get; }
        public Func<bool>? ShouldDraw { get; }

        public PopupMenuItemSelectable(string name, string id, Action<string>? callback, string? tooltip = null, Func<bool>? shouldDraw = null)
        {
            _tooltip = tooltip;
            _name = name;
            _id = id;
            _callback = callback;
            ShouldDraw = shouldDraw;
        }

        public void Draw()
        {

        }

        public string? DrawPopup()
        {
            if (ImGui.Selectable(_name))
            {
                _callback?.Invoke(_id);
                return null;
            }

            if (_tooltip != null)
            {
                OtterGui.ImGuiUtil.HoverTooltip(_tooltip);
            }

            return null;
        }

    }
    
    public class PopupMenuItemSelectableAskName : IPopupMenuItem
    {
        private string _name;
        private string _id;
        private Action<string, string>? _callback { get; }
        private string _newName = "";
        private string _popupName;
        private string _defaultName;
        private string? _tooltip;
        public Func<bool>? ShouldDraw { get; }

        public PopupMenuItemSelectableAskName(string name, string id, string defaultName,
            Action<string, string>? callback, string? tooltip = null, Func<bool>? shouldDraw = null)
        {
            _defaultName = defaultName;
            _newName = defaultName;
            _name = name;
            _id = id;
            _callback = callback;
            _tooltip = tooltip;
            _popupName = _id+"NF";
            ShouldDraw = shouldDraw;
        }

        public void Draw()
        {
            if (ImGuiUtil.OpenNameField(_popupName, ref _newName))
            {
                _callback?.Invoke(_newName, _id);
                _newName = _defaultName;;
            }
        }

        public string? DrawPopup()
        {
            if (ImGui.Selectable(_name))
            {
                _newName = _defaultName;
                return _popupName;
            }

            if (_tooltip != null)
            {
                OtterGui.ImGuiUtil.HoverTooltip(_tooltip);
            }

            return null;
        }
    }
    
    public class PopupMenuItemSelectableConfirm : IPopupMenuItem
    {
        private string _name;
        private string _id;
        private Action<string, bool>? _callback { get; }
        private string _popupName;
        private string _question;
        private string? _tooltip;

        public PopupMenuItemSelectableConfirm(string name, string id, string question,
            Action<string, bool>? callback, string? tooltip = null, Func<bool>? shouldDraw = null)
        {
            _question = question;
            _name = name;
            _id = id;
            _callback = callback;
            _tooltip = tooltip;
            _popupName = _id+"NF";
            ShouldDraw = shouldDraw;
        }

        public void Draw()
        {
            using var popup = ImRaii.Popup(_popupName);
            if (!popup)
                return ;

            ImGui.TextUnformatted(
                _question + "\nThis operation cannot be undone!\n\n");
            ImGui.Separator();

            if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
            {
                _callback?.Invoke(_id, true);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
            {
                _callback?.Invoke(_id, false);
                ImGui.CloseCurrentPopup();
            }
            
        }

        public string? DrawPopup()
        {
            if (ImGui.Selectable(_name))
            {
                return _popupName;
            }

            if (_tooltip != null)
            {
                OtterGui.ImGuiUtil.HoverTooltip(_tooltip);
            }

            return null;
        }

        public Func<bool>? ShouldDraw { get; }
    }

    public class PopupMenuItemSeparator : IPopupMenuItem
    {
        public void Draw()
        {
        }

        public string? DrawPopup()
        {
            ImGui.Separator();
            return null;
        }

        public Func<bool>? ShouldDraw { get; }
    }
    public PopupMenu(string id, PopupMenuButtons openButtons,List<IPopupMenuItem> items)
    {
        _id = id;
        _openButtons = openButtons;
        _items = items;
    }

    public void Open()
    {
        ImGui.OpenPopup("RightClick" + _id);
    }
    
    public void Draw()
    {
        var isMouseReleased = ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _openButtons is PopupMenuButtons.All or PopupMenuButtons.Left or PopupMenuButtons.LeftRight || 
                              ImGui.IsMouseReleased(ImGuiMouseButton.Right) && _openButtons is PopupMenuButtons.All or PopupMenuButtons.Right or PopupMenuButtons.LeftRight || 
                              ImGui.IsMouseReleased(ImGuiMouseButton.Middle) && _openButtons is PopupMenuButtons.All or PopupMenuButtons.Middle;
        
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && isMouseReleased) 
        {
            ImGui.OpenPopup("RightClick" + _id);
        }
        
        string? newPopupName = null;
        using (var popup = ImRaii.Popup("RightClick"+ _id))
        {
            if (popup.Success)
            {
                foreach (var item in _items)
                {
                    if (item.ShouldDraw != null)
                    {
                        if (!item.ShouldDraw())
                        {
                            continue;
                        }
                    }
                    var drawPopup = item.DrawPopup();
                    if (drawPopup != null)
                    {
                        newPopupName = drawPopup;
                    }
                }
            }
        }        
        if (newPopupName != null)
        {
            ImGui.OpenPopup(newPopupName);
        }
        
        foreach (var item in _items)
        {
            item.Draw();
        }
    }
}
