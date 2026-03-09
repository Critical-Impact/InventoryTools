using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Sections;

public abstract class CompendiumViewSection : ICompendiumViewSection
{
    private readonly ImGuiService _imGuiService;
    private List<ImGuiService.HeaderButton>? _headerButtons;

    public CompendiumViewSection(ImGuiService imGuiService)
    {
        _imGuiService = imGuiService;
    }

    public void Draw(SectionState sectionState)
    {
        if (!ShouldDraw(sectionState))
        {
            return;
        }

        if (_headerButtons == null)
        {
            _headerButtons = new();
            if (DrawMenu != null)
            {
                _headerButtons.Add(new ImGuiService.HeaderButton()
                {
                    Id = "Menu",
                    Label = "Menu",
                    Image = "menu",
                    Callback = () => { },
                });
            }
            if (DrawOptions != null)
            {
                _headerButtons.Add(new ImGuiService.HeaderButton()
                {
                    Id = "Settings",
                    Image = "wrench-icon",
                    Label = "Settings",
                    Callback = () => { },
                });
            }
        }
        if (_imGuiService.CollapsingHeader(SectionName, out var buttonClicked, _headerButtons, ImGuiTreeNodeFlags.DefaultOpen))
        {
            DrawSection(sectionState);
        }

        if (buttonClicked == "Menu")
        {
            ImGui.OpenPopup("MenuPopup");
        }

        using (var menuPopup = ImRaii.Popup("MenuPopup"))
        {
            if (menuPopup)
            {
                DrawMenu?.Invoke(sectionState);
            }
        }

        if (buttonClicked == "Settings")
        {
            ImGui.OpenPopup("SettingsPopup");
        }

        using (var menuPopup = ImRaii.Popup("SettingsPopup"))
        {
            if (menuPopup)
            {
                DrawOptions?.Invoke(sectionState);
            }
        }
    }

    public virtual bool ShouldDraw(SectionState sectionState)
    {
        return true;
    }

    public abstract string SectionName { get; }

    public virtual Action<SectionState>? DrawOptions => null;
    public virtual Action<SectionState>? DrawMenu => null;

    public abstract void DrawSection(SectionState sectionState);
}