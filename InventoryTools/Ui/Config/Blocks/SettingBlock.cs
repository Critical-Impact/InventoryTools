using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Ui.Config.Blocks;

public sealed class SettingBlock : IConfigBlock
{
    public SettingBlock(Type settingType, string? nameOverride = null)
    {
        SettingType = settingType;
        NameOverride = nameOverride;
    }

    public Type SettingType { get; }
    public string? NameOverride { get; }
    public IReadOnlyList<IConfigBlock> Children => [];

    public void Draw(ConfigDrawContext context)
    {
        var setting = context.Find(SettingType);
        if (setting == null)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, $"Missing setting: {SettingType.Name}");
            return;
        }

        if (context.Navigation.ShouldScrollTo(SettingType))
        {
            ImGui.SetScrollHereY(0.35f);
        }

        var isHighlighted = context.Navigation.IsHighlighted(SettingType);
        var isNew = context.NewSettings.Contains(SettingType);
        var outlined = isHighlighted || isNew;

        const float outlineMargin = 5f;
        const float outlinePadding = 6f;

        var topLeft = ImGui.GetCursorScreenPos();
        var boxWidth = ImGui.GetContentRegionAvail().X;

        if (outlined)
        {
            ImGui.Dummy(new Vector2(0, outlineMargin));
            topLeft = ImGui.GetCursorScreenPos();
            ImGui.Dummy(new Vector2(0, outlinePadding));
            ImGui.Indent(outlinePadding);
        }

        if (isNew)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen))
            {
                ImGui.TextUnformatted("NEW");
            }
        }

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
        setting.Draw(context.Configuration, NameOverride, null, null);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);

        if (outlined)
        {
            ImGui.Unindent(outlinePadding);
            ImGui.Dummy(new Vector2(0, outlinePadding));

            ImGui.GetWindowDrawList().AddRect(
                new Vector2(topLeft.X + 1f, topLeft.Y),
                new Vector2(topLeft.X + boxWidth - 1f, ImGui.GetCursorScreenPos().Y),
                ImGui.GetColorU32(isHighlighted ? ImGuiColors.DalamudYellow : ImGuiColors.HealerGreen),
                4f);
            ImGui.Dummy(new Vector2(0, outlineMargin));
        }
    }
}