using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Comparer;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class GameColorSetting : Setting<uint?>
    {
        private readonly ExcelSheet<UIColor> _uiColorSheet;

        public GameColorSetting(ILogger logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base(logger, imGuiService)
        {
            _uiColorSheet = uiColorSheet;
            var list = new List<UIColor>(_uiColorSheet.Distinct(new UIColorComparer()));
            list.Sort((a, b) =>
            {
                var colorA = Utils.ConvertUiColorToColor(a);
                var colorB = Utils.ConvertUiColorToColor(b);
                ImGui.ColorConvertRGBtoHSV(colorA.X, colorA.Y, colorA.Z, out var aH, out var aS, out var aV);
                ImGui.ColorConvertRGBtoHSV(colorB.X, colorB.Y, colorB.Z, out var bH, out var bS, out var bV);

                var hue = aH.CompareTo(bH);
                if (hue != 0)
                {
                    return hue;
                }

                var saturation = aS.CompareTo(bS);
                if (saturation != 0)
                {
                    return saturation;
                }

                var value = aV.CompareTo(bV);
                return value != 0 ? value : 0;
            });
            uiColors = list.ToDictionary(c => c.RowId, c => c);
        }
        private readonly Dictionary<uint, UIColor> uiColors;

        public void DrawColorPopup(InventoryToolsConfiguration configuration, uint? currentColor)
        {
            var index = 0;
            foreach(var uiColor in uiColors)
            {
                var z = uiColor.Value;
                if (z.UIForeground is 0 or 255)
                {
                    continue;
                }

                var color = Utils.ConvertUiColorToColor(z);
                var id = z.RowId.ToString();
                var imGuiColorEditFlags = ImGuiColorEditFlags.NoBorder;
                if (currentColor == z.RowId)
                {
                    imGuiColorEditFlags = ImGuiColorEditFlags.None;
                }

                if (ImGui.ColorButton(id, color, imGuiColorEditFlags))
                {
                    ImGui.CloseCurrentPopup();
                    UpdateFilterConfiguration(configuration, id == "" ? null : UInt32.Parse(id));
                }
                index++;
                if (index % 10 != 0)
                {
                    ImGui.SameLine();
                }
            }
        }

        public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset,
            bool? disableColouring)
        {
            var value = CurrentValue(configuration);
            if (disableColouring != true && HasValueSet(configuration))
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
                ImGui.LabelText("##" + Key + "Label", customName ?? Name);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.LabelText("##" + Key + "Label", customName ?? Name);
            }
            var enabled = value != null;

            if (ImGui.Checkbox("Enable##"+Key+"Boolean", ref enabled))
            {
                if (value == null)
                {
                    value = DefaultValue ?? uiColors.First().Key;
                }
                else
                {
                    value = null;
                }

                UpdateFilterConfiguration(configuration, value);
            }

            var currentColour = new Vector4(255, 255, 255, 255);
            if (value != null && uiColors.ContainsKey(value.Value))
            {
                currentColour = Utils.ConvertUiColorToColor(uiColors[value.Value]);
            }
            ImGui.SameLine();

            using (var disabled = ImRaii.Disabled(value == null))
            {
                if (ImGui.ColorButton("##" + Key + "CurrentVal", currentColour))
                {
                }

                if (ImGui.IsItemHovered())
                {
                    using (var tooltip = ImRaii.Tooltip())
                    {
                        if (tooltip)
                        {
                            ImGui.Text("Click to open colour selector.");
                        }
                    }
                }
            }

            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                ImGui.OpenPopup("##" + Key + "Popup");
            }

            using (var popup = ImRaii.Popup("##" + Key + "Popup"))
            {
                if (popup)
                {
                    DrawColorPopup(configuration, value);
                }
            }
            ImGui.SameLine();

            ImGuiService.HelpMarker(HelpText, Image, ImageSize);
            if (disableReset != true && HasValueSet(configuration))
            {
                ImGui.SameLine();
                if (ImGui.Button("Reset##" + Key + "Reset"))
                {
                    Reset(configuration);
                }
            }

        }
    }
}