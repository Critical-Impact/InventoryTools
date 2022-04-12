using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CriticalCommonLib;
using Dalamud.Interface;
using ImGuiNET;

namespace InventoryTools
{
    public class PluginFont : IDisposable
    {
        public static ImFontPtr? AppIcons { get; private set; }

        public unsafe PluginFont()
        {
            Service.Interface.UiBuilder.BuildFonts += this.UiBuilder_BuildFonts;
            Service.Interface.UiBuilder.RebuildFonts();
        }


        ~PluginFont()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool _disposed;
        protected void Dispose(bool disposing)
        {
            if (this._disposed) return;
            this._disposed = true;

            if (disposing)
            {
                Service.Interface.UiBuilder.BuildFonts -= this.UiBuilder_BuildFonts;
                Service.Interface.UiBuilder.RebuildFonts();
            }
        }

        private unsafe void UiBuilder_BuildFonts()
        {

            
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (directory != null)
            {
                ImFont* font = ImGui.GetIO().Fonts.AddFontDefault();
                
                ImFontConfigPtr fontCfg = (ImFontConfigPtr) ImGuiNative.ImFontConfig_ImFontConfig();
                fontCfg.PixelSnapH = true;
                string str1 = Path.Combine(Service.Interface.DalamudAssetDirectory.FullName, "UIRes", "NotoSansCJKjp-Medium.otf");
                GCHandle gcHandle1 = GCHandle.Alloc((object) GlyphRangesJapanese.GlyphRanges, GCHandleType.Pinned);
                
                ImGui.GetIO().Fonts.AddFontFromFileTTF(str1, 17f, (ImFontConfigPtr) (ImFontConfig*) null, gcHandle1.AddrOfPinnedObject());
                
                var iconRangeHandle = GCHandle.Alloc(
                    new ushort[]
                    {
                        0xE010,
                        0xf341,
                        0,
                    },
                    GCHandleType.Pinned);
                
                ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig(); 
                config.MergeMode = true; 
                config.PixelSnapH = true;
                config.GlyphOffset.Y += 3.0f;
                string fontPath = Path.Combine(directory, "Fonts", "FFXIVAppIcons.ttf");

                AppIcons = ImGui.GetIO().Fonts
                    .AddFontFromFileTTF(fontPath, 17f, config, iconRangeHandle.AddrOfPinnedObject());




                iconRangeHandle.Free();
                gcHandle1.Free();

            }

        }
        
        private struct ImFontGlyphReal
        {
            public uint ColoredVisibleCodepoint;
            public float AdvanceX;
            public float X0;
            public float Y0;
            public float X1;
            public float Y1;
            public float U0;
            public float V0;
            public float U1;
            public float V1;

            public bool Colored
            {
                get => (this.ColoredVisibleCodepoint & 1U) > 0U;
                set => this.ColoredVisibleCodepoint = (uint) ((int) this.ColoredVisibleCodepoint & -2 | (value ? 1 : 0));
            }

            public bool Visible
            {
                get => (this.ColoredVisibleCodepoint >> 1 & 1U) > 0U;
                set => this.ColoredVisibleCodepoint = (uint) ((int) this.ColoredVisibleCodepoint & -3 | (value ? 2 : 0));
            }

            public int Codepoint
            {
                get => (int) (this.ColoredVisibleCodepoint >> 2);
                set => this.ColoredVisibleCodepoint = (uint) ((int) this.ColoredVisibleCodepoint & 3 | this.Codepoint << 2);
            }
        }
    }
}