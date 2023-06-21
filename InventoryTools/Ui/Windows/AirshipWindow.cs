using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
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
    class AirshipWindow : Window
    {
        public override bool SaveState => false;
        public static string AsKey(uint airshipExplorationPointId)
        {
            return "aepid_" + airshipExplorationPointId;
        }
        private uint _airshipExplorationPointId;
        private List<ItemEx> _drops;
        private AirshipExplorationPointEx? AirshipExplorationPointEx => Service.ExcelCache.GetAirshipExplorationPointExSheet().GetRow(_airshipExplorationPointId);

        public AirshipWindow(uint airshipExplorationPointId, string name = "Allagan Tools - Invalid Airship Exploration") : base(name)
        {
            _airshipExplorationPointId = airshipExplorationPointId;
            if (AirshipExplorationPointEx != null)
            {
                WindowName = "Allagan Tools - " + AirshipExplorationPointEx.FormattedNameShort;
                _drops = AirshipExplorationPointEx.Drops.Where(c => c.Value != null).Select(c => c.Value!).ToList();
            }
            else
            {
                _drops = new List<ItemEx>();
            }
        }
        public override string Key => AsKey(_airshipExplorationPointId);
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (AirshipExplorationPointEx == null)
            {
                ImGui.TextUnformatted("Airship Exploration Point with the ID " + _airshipExplorationPointId + " could not be found.");   
            }
            else
            {
                ImGui.TextUnformatted(AirshipExplorationPointEx.NameShort.ToDalamudString().ToString());
                ImGui.TextUnformatted("Unlocked Via: " + AirshipExplorationPointEx.AirshipUnlockEx?.AirshipExplorationPointUnlockEx.Value?.FormattedNameShort ?? "N/A");
                ImGui.TextUnformatted("Rank Required: " + AirshipExplorationPointEx.RankReq);
                ;
                var itemIcon = PluginService.IconStorage[Icons.AirshipIcon];
                ImGui.Image(itemIcon.ImGuiHandle, new Vector2(100, 100) * ImGui.GetIO().FontGlobalScale);
                
                
                if (ImGui.CollapsingHeader("Rewards (" + _drops.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

                    for (var index = 0; index < _drops.Count; index++)
                    {
                        ImGui.PushID("Reward"+index);
                        var drop = _drops[index];
                        
                        var useIcon = PluginService.IconStorage[drop.Icon];
                        if (useIcon != null)
                        {
                            if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                            {
                                PluginService.WindowService.OpenItemWindow(drop.RowId);
                            }
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right)) 
                            {
                                ImGui.OpenPopup("RightClickUse" + drop.RowId);
                            }
                
                            using (var popup = ImRaii.Popup("RightClickUse"+ drop.RowId))
                            {
                                if (popup.Success)
                                {
                                    var itemEx = Service.ExcelCache.GetItemExSheet().GetRow(drop.RowId);
                                    if (itemEx != null)
                                    {
                                        itemEx.DrawRightClickPopup();
                                    }
                                }
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(drop.NameString);
                            if (index + 1 < _drops.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }
                        }

                        ImGui.PopID();
                    }
                }
                
                #if DEBUG
                if (ImGui.CollapsingHeader("Debug"))
                {
                    ImGui.TextUnformatted("Duty ID: " + _airshipExplorationPointId);
                    Utils.PrintOutObject(AirshipExplorationPointEx, 0, new List<string>());
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
    }
}