using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;

namespace InventoryTools.Services;

using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin;

public struct GameIcon
{
    public string Name;
    public Vector2 Size;
    public Vector2? Uv0;
    public Vector2? Uv1;
}

public class ImGuiService : AllaganLib.Interface.Services.ImGuiService
{
    private readonly IDalamudPluginInterface pluginInterface;
    public ITextureProvider TextureProvider { get; }
    public ImGuiMenuService ImGuiMenuService { get; }

    public ImGuiService(ITextureProvider textureProvider, ImGuiMenuService imGuiMenuService, IDalamudPluginInterface pluginInterface) : base(pluginInterface, textureProvider)
    {
        this.pluginInterface = pluginInterface;
        TextureProvider = textureProvider;
        ImGuiMenuService = imGuiMenuService;
    }


    public readonly GameIcon TickIcon = new GameIcon()
        { Name = "readycheck", Size = new Vector2(32, 32), Uv0 = new Vector2(0f, 0f), Uv1 = new Vector2(0.5f, 1f) };

    public readonly GameIcon CrossIcon = new GameIcon()
        { Name = "readycheck", Size = new Vector2(32, 32), Uv0 = new Vector2(0.5f, 0f), Uv1 = new Vector2(1f, 1f) };

    public readonly GameIcon CheckboxChecked = new GameIcon()
    {
        Name = "CheckBoxA_hr1", Size = new Vector2(16, 16), Uv0 = new Vector2(0.5f, 0f), Uv1 = new Vector2(1f, 1f)
    };

    public readonly GameIcon CheckboxUnChecked = new GameIcon()
    {
        Name = "CheckBoxA_hr1", Size = new Vector2(16, 16), Uv0 = new Vector2(0f, 0f), Uv1 = new Vector2(0.5f, 1f)
    };

    public static bool DrawIconButton(
        IFont font,
        FontAwesomeIcon icon,
        ref float currentCursorX,
        string? tooltip = null,
        bool reverseCursor = false,
        Vector4? textColor = null,
        bool invisible = false,
        float? minWidth = null)
    {
        var success = false;
        var iconString = icon.ToIconString();

        using var pushFont = ImRaii.PushFont(font.IconFont);
        using var pushColor = ImRaii.PushColor(ImGuiCol.Text, textColor ?? new Vector4(1, 1, 1, 1), textColor != null);
        var globalScale = ImGui.GetIO().FontGlobalScale;
        var iconSize = ImGui.CalcTextSize(iconString);
        var framePadding = ImGui.GetStyle().FramePadding * globalScale;

        var buttonSize = iconSize + (framePadding * 2);

        if (reverseCursor)
        {
            currentCursorX -= buttonSize.X + ImGui.GetStyle().ItemSpacing.X;
        }

        if (minWidth != null && minWidth >= buttonSize.X)
        {
            buttonSize.X = minWidth.Value;
        }

        ImGui.SetCursorPosX(currentCursorX);

        var col1 = ImRaii.PushColor(ImGuiCol.Button,        new Vector4(0, 0, 0, 0), invisible);
        var col2 = ImRaii.PushColor(ImGuiCol.ButtonHovered, new Vector4(0, 0, 0, 0), invisible);
        var col3 = ImRaii.PushColor(ImGuiCol.ButtonActive,  new Vector4(0, 0, 0, 0), invisible);
        var col4 = ImRaii.PushColor(ImGuiCol.Border,  new Vector4(0, 0, 0, 0), invisible);
        var style1 = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0.0f, invisible);
        var style2 = ImRaii.PushStyle(ImGuiStyleVar.FramePadding,  new Vector2(0, 0), invisible);
        if (ImGui.Button(iconString, buttonSize))
        {
            success = true;
        }

        style2.Pop();
        style1.Pop();
        col4.Pop();
        col3.Pop();
        col2.Pop();
        col1.Pop();
        pushColor.Pop();
        pushFont.Pop();

        if (ImGui.IsItemHovered() && !string.IsNullOrEmpty(tooltip))
        {
            using var tooltipScope = ImRaii.Tooltip();
            if (tooltipScope)
            {
                ImGui.Text(tooltip);
            }
        }

        return success;
    }

    public IDalamudTextureWrap GetIconTexture(int iconId, bool isHq = false)
    {
        return TextureProvider.TryGetFromGameIcon(new GameIconLookup((uint)iconId, isHq), out var texture) ? texture.GetWrapOrEmpty() : TextureProvider.GetFromGameIcon(new GameIconLookup(60074, isHq)).GetWrapOrEmpty();
    }

    public IDalamudTextureWrap GetIconTexture(uint iconId)
    {
        return TextureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrEmpty();
    }

    public IDalamudTextureWrap GetImageTexture(string filePath, bool pluginImage = true)
    {
        if (pluginImage)
        {
            return LoadImage(filePath).GetWrapOrEmpty();
        }
        return TextureProvider.GetFromFile(filePath).GetWrapOrEmpty();
    }

    public void DrawUldIcon(GameIcon gameIcon, Vector2? size = null)
    {
        DrawUldIcon(gameIcon.Name, size ?? gameIcon.Size, gameIcon.Uv0, gameIcon.Uv1);
    }

    public void DrawUldIcon(string name, Vector2 size, Vector2? uvStart = null, Vector2? uvEnd = null)
    {
        var iconTex = TextureProvider.GetUldIcon(name);
        if (iconTex == null) return;
        var wrap = iconTex.GetWrapOrEmpty();
        if (uvStart.HasValue && uvEnd.HasValue)
        {
            ImGui.Image(wrap.ImGuiHandle, size, uvStart.Value,
                uvEnd.Value);
        }
        else if (uvStart.HasValue)
        {
            ImGui.Image(wrap.ImGuiHandle, size, uvStart.Value);
        }
        else
        {
            ImGui.Image(wrap.ImGuiHandle, size);
        }
    }

    public void DrawIcon(uint icon, Vector2 size, bool hqIcon = false)
    {
        if (icon <= 65103)
        {
            var iconTex = TextureProvider.GetFromGameIcon(new GameIconLookup(icon, hqIcon));
            ImGui.Image(iconTex.GetWrapOrEmpty().ImGuiHandle, size);
        }
        else
        {
            ImGui.Text("Invalid Icon ID");
        }
    }

    public bool DrawUldIconButton(GameIcon gameIcon, Vector2? size = null)
    {
        return DrawUldIconButton(gameIcon.Name, size ?? gameIcon.Size, gameIcon.Uv0, gameIcon.Uv1);
    }

    public bool DrawUldIconButton(string name, Vector2 size, Vector2? uvStart = null, Vector2? uvEnd = null)
    {
        var iconTex = TextureProvider.GetUldIcon(name);
        if (iconTex != null)
        {
            if (uvStart.HasValue && uvEnd.HasValue)
            {
                return ImGui.ImageButton(iconTex.GetWrapOrEmpty().ImGuiHandle, size, uvStart.Value,
                    uvEnd.Value);
            }
            else if (uvStart.HasValue)
            {
                return ImGui.ImageButton(iconTex.GetWrapOrEmpty().ImGuiHandle, size, uvStart.Value);
            }
            else
            {
                return ImGui.ImageButton(iconTex.GetWrapOrEmpty().ImGuiHandle, size);
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void HelpMarker(string helpText, string? imagePath = null, System.Numerics.Vector2? imageSize = null)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            using (var tooltip = ImRaii.Tooltip())
            {
                if (tooltip)
                {
                    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                    ImGui.TextUnformatted(helpText);
                    ImGui.PopTextWrapPos();
                    if (imagePath != null)
                    {
                        var sourceIcon = LoadImage(imagePath);
                        ImGui.Image(sourceIcon.GetWrapOrEmpty().ImGuiHandle, imageSize ??
                                                                             new Vector2(200, 200) *
                                                                             ImGui.GetIO().FontGlobalScale);
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void HelpMarker(List<string> helpText, string? imagePath = null, System.Numerics.Vector2? imageSize = null)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            using (var tooltip = ImRaii.Tooltip())
            {
                if (tooltip)
                {
                    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                    foreach (var line in helpText)
                    {
                        if (line == "")
                        {
                            ImGui.Separator();
                        }
                        else
                        {
                            ImGui.TextUnformatted(line);
                        }
                    }

                    ImGui.PopTextWrapPos();
                    if (imagePath != null)
                    {
                        var sourceIcon = LoadImage(imagePath);
                        ImGui.Image(sourceIcon.GetWrapOrEmpty().ImGuiHandle, imageSize ??
                                                                             new Vector2(200, 200) *
                                                                             ImGui.GetIO().FontGlobalScale);
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void VerticalCenter(string text)
    {
        var offset = (ImGui.GetWindowSize().Y - ImGui.CalcTextSize(text).Y) / 2.0f;
        ImGui.SetCursorPosY(offset);
        ImGui.TextUnformatted(text);
    }

    public void CenterElement(float height)
    {
        ImGui.SetCursorPosY((ImGui.GetWindowSize().Y - height) / 2.0f + (ImGui.GetStyle().FramePadding.Y / 2.0f));
    }

    public ISharedImmediateTexture LoadImage(string imageName)
    {
        var assemblyLocation = pluginInterface.AssemblyLocation.DirectoryName!;
        var imagePath = Path.Combine(assemblyLocation, Path.Combine("Images", $"{imageName}.png"));
        return TextureProvider.GetFromFile(new FileInfo(imagePath));
    }

    public void WrapTableColumnElements<T>(string windowId, IEnumerable<T> items, float elementSize, float rowSize,
        Func<T, bool> drawElement)
    {
        using var pushId = ImRaii.PushId(windowId);
        using (var wrapTableChild = ImRaii.Child("ScrollBox",
                   new Vector2(ImGui.GetContentRegionAvail().X,
                       rowSize + ImGui.GetStyle().CellPadding.Y + ImGui.GetStyle().ItemSpacing.Y), false))
        {
            if (wrapTableChild.Success)
            {
                var columnWidth = ImGui.GetContentRegionAvail().X * ImGui.GetIO().FontGlobalScale;
                var itemWidth = (elementSize + ImGui.GetStyle().ItemSpacing.X) * ImGui.GetIO().FontGlobalScale;
                var maxItems = itemWidth != 0 ? (int)Math.Floor(columnWidth / itemWidth) : 0;
                maxItems = maxItems == 0 ? 1 : maxItems;
                var enumerable = items.ToList();
                var count = 1;
                for (var index = 0; index < enumerable.Count; index++)
                {
                    using (ImRaii.PushId(index))
                    {
                        if (drawElement.Invoke(enumerable[index]))
                        {
                            if (count % maxItems != 0)
                            {
                                ImGui.SameLine();
                            }

                            count++;
                        }
                    }
                }
            }
        }
    }

    public void WrapTableColumnElements<T>(string windowId, IEnumerable<T> items, float rowSize,
        Func<T, bool> drawElement)
    {
        using var pushId = ImRaii.PushId(windowId);
        using (var wrapTableChild = ImRaii.Child("ScrollBox",
                   new Vector2(ImGui.GetContentRegionAvail().X,
                       rowSize + ImGui.GetStyle().CellPadding.Y + ImGui.GetStyle().ItemSpacing.Y), false))
        {
            if (wrapTableChild.Success)
            {
                var columnWidth = ImGui.GetContentRegionAvail().X * ImGui.GetIO().FontGlobalScale;
                var itemWidth = (rowSize + ImGui.GetStyle().ItemSpacing.X) * ImGui.GetIO().FontGlobalScale;
                var maxItems = itemWidth != 0 ? (int)Math.Floor(columnWidth / itemWidth) : 0;
                maxItems = maxItems == 0 ? 1 : maxItems;
                var enumerable = items.ToList();
                var count = 1;
                for (var index = 0; index < enumerable.Count; index++)
                {
                    using (ImRaii.PushId(index))
                    {
                        if (drawElement.Invoke(enumerable[index]))
                        {
                            if (count % maxItems != 0)
                            {
                                ImGui.SameLine();
                            }

                            count++;
                        }
                    }
                }
            }
        }
    }

    public void SpinnerDots(string label, ref float nextdot, float radius, float thickness, uint color = 0xFFFFFFFF,
        float speed = 2.8f, uint dots = 12, float minth = -1f)
    {
        double start = ImGui.GetTime() * speed;
        double bg_angle_offset = Math.PI * 2 / dots;
        dots = Math.Min(dots, 32u);
        uint mdots = dots / 2;

        float def_nextdot = 0;
        ref float ref_nextdot = ref (nextdot >= 0 ? ref nextdot : ref def_nextdot);

        var f = ref_nextdot;
        System.Func<uint, float> thcorrect = i =>
        {
            float nth = minth < 0 ? thickness / 2 : minth;
            return Math.Max(nth, (float)Math.Sin(((i - f) / mdots) * Math.PI) * thickness);
        };

        for (uint i = 0; i <= dots; i++)
        {
            double a = start + i * bg_angle_offset;
            a = a % (Math.PI * 2);
            float th = minth < 0 ? thickness / 2 : minth;

            if (ref_nextdot + mdots < dots)
            {
                if (i > ref_nextdot && i < ref_nextdot + mdots)
                    th = thcorrect(i);
            }
            else
            {
                if ((i > ref_nextdot && i < dots) || (i < (uint)((ref_nextdot + mdots) % dots)))
                    th = thcorrect(i);
            }

            ImGui.GetWindowDrawList().AddCircleFilled(
                new Vector2(
                    ImGui.GetCursorScreenPos().X + ImGui.GetStyle().FramePadding.X + radius * (float)Math.Cos(-a),
                    ImGui.GetCursorScreenPos().Y + ImGui.GetStyle().FramePadding.Y + radius * (float)Math.Sin(-a)), th,
                color, 8);
        }
    }
}