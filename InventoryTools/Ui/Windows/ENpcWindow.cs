using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Game.Text;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using LuminaSupplemental.Excel.Model;
using OtterGui;
using OtterGui.Raii;

namespace InventoryTools.Ui
{
    class ENpcWindow : Window
    {
        public override bool SaveState => false;
        public static string AsKey(uint eNpcId)
        {
            return "enpc_" + eNpcId;
        }
        private uint _eNpcId;
        private ENpc? eNpc => Service.ExcelCache.ENpcCollection.Get(_eNpcId);
        public List<IShop>? Shops;

        public ENpcWindow(uint eNpcId, string name = "Allagan Tools - Invalid NPC") : base(name)
        {
            Flags = ImGuiWindowFlags.NoSavedSettings;
            _eNpcId = eNpcId;
            if (eNpc != null)
            {
                WindowName = "Allagan Tools - " + eNpc.Resident!.FormattedSingular + "##" + eNpcId;
                Shops = Service.ExcelCache.ENpcCollection.FindShops(eNpc)?.Select(c => Service.ExcelCache.ShopCollection.Get(c)).Where(c => c != null).Select(c => c!).ToList();
            }
            else
            {
             
            }
        }

        public override string Key => AsKey(_eNpcId);
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (ImGui.GetWindowPos() != CurrentPosition)
            {
                CurrentPosition = ImGui.GetWindowPos();
            }

            if (eNpc == null)
            {
                ImGui.TextUnformatted("eNpc with the ID " + _eNpcId + " could not be found.");   
            }
            else
            {
                var garlandIcon = PluginService.IconStorage[65090];
                if (ImGui.ImageButton(garlandIcon.ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://www.garlandtools.org/db/#eNpc/{_eNpcId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Garland Tools");
                ImGui.SameLine();
                var tcIcon = PluginService.IconStorage[60046];
                if (ImGui.ImageButton(tcIcon.ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://ffxivteamcraft.com/db/en/eNpc/{_eNpcId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Teamcraft");
                
                ImGui.Separator();
                
                if (Shops != null && ImGui.CollapsingHeader("Shops (" + Shops.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var uses = Shops;
                    for (var index = 0; index < uses.Count; index++)
                    {
                        ImGui.PushID("Shop"+index);
                        var shop = uses[index];
                        var listingCount = 0;

                        ImGui.PushID("Listing"+listingCount);
                        if (ImGui.CollapsingHeader(shop.Name,
                                ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                        {
                            foreach (var listing in shop.ShopListings)
                            {
                                foreach (var item in listing.Rewards)
                                {
                                    if (item.ItemEx.Value != null)
                                    {
                                        var useIcon = PluginService.IconStorage[item.ItemEx.Value.Icon];
                                        if (useIcon != null)
                                        {
                                            if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale,
                                                    new(0, 0), new(1, 1),
                                                    0))
                                            {
                                                PluginService.WindowService.OpenItemWindow(item.ItemEx.Row);
                                            }

                                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                                    ImGuiHoveredFlags.AllowWhenOverlapped &
                                                                    ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                                    ImGuiHoveredFlags
                                                                        .AllowWhenBlockedByActiveItem &
                                                                    ImGuiHoveredFlags.AnyWindow) &&
                                                ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                            {
                                                ImGui.OpenPopup("RightClickUse" + item.ItemEx.Row);
                                            }

                                            if (ImGui.BeginPopup("RightClickUse" + item.ItemEx.Row))
                                            {
                                                item.ItemEx.Value.DrawRightClickPopup();

                                                ImGui.EndPopup();
                                            }

                                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                                            ImGuiUtil.HoverTooltip(item.ItemEx.Value.NameString);
                                            if (listingCount < shop.ShopListings.Count() && nextButtonX2 < windowVisibleX2)
                                            {
                                                ImGui.SameLine();
                                            }
                                        }
                                    }
                                }

                                listingCount++;
                            }
                        }
                        ImGui.PopID();
                        ImGui.NewLine();
                    }
                }
                
                
                var hasInformation = false;
                if (!hasInformation)
                {
                    ImGui.TextUnformatted("No information available.");
                }
                
                #if DEBUG
                if (ImGui.CollapsingHeader("Debug"))
                {
                    ImGui.TextUnformatted("eNpc ID: " + _eNpcId);
                    if (ImGui.Button("Copy"))
                    {
                        ImGui.SetClipboardText(_eNpcId.ToString());
                    }

                    Utils.PrintOutObject(eNpc, 0, new List<string>());
                }
                #endif

            }
        }

        public override void Invalidate()
        {
            
        }
        
        public override FilterConfiguration? SelectedConfiguration => null;
        public override Vector2 DefaultSize { get; } = new Vector2(500, 800);
        public override Vector2 MaxSize => new (800, 1500);
        public override Vector2 MinSize => new (100, 100);

        public override bool SavePosition => true;

        public override string GenericKey => "eNpc";
    }
}