using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Comparer;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class GameColorSetting : Setting<uint?>
    {
        private readonly ExcelSheet<UIColor> _uiColorSheet;

        void RGBtoHSV(float r, float g, float b, out float h, out float s, out float v)
        {
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));

            v = max;
            float delta = max - min;

            if (max != 0)
                s = delta / max;
            else
            {
                // r = g = b = 0
                s = 0;
                h = 0;
                return;
            }

            if (delta == 0)
            {
                h = 0;
            }
            else if (max == r)
            {
                h = (g - b) / delta;
                if (g < b)
                    h += 6;
            }
            else if (max == g)
            {
                h = (b - r) / delta + 2;
            }
            else // max == b
            {
                h = (r - g) / delta + 4;
            }

            h /= 6;
        }

        public GameColorSetting(ILogger logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base(logger, imGuiService)
        {
            _uiColorSheet = uiColorSheet;
            var list = new List<UIColor>(_uiColorSheet.Distinct(new UIColorComparer()));
            list.Sort((a, b) =>
            {
                var colorA = Utils.ConvertUiColorToColor(a);
                var colorB = Utils.ConvertUiColorToColor(b);

                RGBtoHSV(colorA.X, colorA.Y, colorA.Z, out var aH, out var aS, out var aV);
                RGBtoHSV(colorB.X, colorB.Y, colorB.Z, out var bH, out var bS, out var bV);

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
                if (z.Dark is 0 or 255)
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

            if (DefaultValue == null)
            {
                if (ImGui.Checkbox("Enable##" + Key + "Boolean", ref enabled))
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
                ImGui.SameLine();
            }
            else
            {
            }

            var currentColour = new Vector4(255, 255, 255, 255);
            if (value != null && uiColors.ContainsKey(value.Value))
            {
                currentColour = Utils.ConvertUiColorToColor(uiColors[value.Value]);
            }

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