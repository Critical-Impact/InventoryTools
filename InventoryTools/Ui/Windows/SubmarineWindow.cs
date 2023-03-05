using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using LuminaSupplemental.Excel.Model;
using OtterGui;

namespace InventoryTools.Ui
{
    class SubmarineWindow : Window
    {
        public override bool SaveState => false;
        public static string AsKey(uint submarineExplorationPointId)
        {
            return "aepid_" + submarineExplorationPointId;
        }
        private uint _submarineExplorationPointId;
        private List<ItemEx> _drops;
        private SubmarineExplorationEx? SubmarineExplorationEx => Service.ExcelCache.GetSubmarineExplorationExSheet().GetRow(_submarineExplorationPointId);

        public SubmarineWindow(uint submarineExplorationPointId, string name = "Allagan Tools - Invalid Submarine Exploration") : base(name)
        {
            _submarineExplorationPointId = submarineExplorationPointId;
            if (SubmarineExplorationEx != null)
            {
                WindowName = "Allagan Tools - " + SubmarineExplorationEx.FormattedNameShort;
                _drops = SubmarineExplorationEx.Drops.Where(c => c.Value != null).Select(c => c.Value!).ToList();
            }
            else
            {
                _drops = new List<ItemEx>();
            }
        }
        public override string Key => AsKey(_submarineExplorationPointId);
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (SubmarineExplorationEx == null)
            {
                ImGui.Text("Submarine Exploration Point with the ID " + _submarineExplorationPointId + " could not be found.");   
            }
            else
            {
                ImGui.Text(SubmarineExplorationEx.FormattedNameShort);
                ImGui.Text("Unlocked Via: " + SubmarineExplorationEx.SubmarineUnlock?.SubmarineExplorationUnlockEx.Value?.FormattedNameShort ?? "N/A");
                ImGui.Text("Rank Required: " + SubmarineExplorationEx.RankReq);
                ;
                var itemIcon = PluginService.IconStorage[65035];
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
                
                            if (ImGui.BeginPopup("RightClickUse"+ drop.RowId))
                            {
                                var itemEx = Service.ExcelCache.GetItemExSheet().GetRow(drop.RowId);
                                if (itemEx != null)
                                {
                                    itemEx.DrawRightClickPopup();
                                }

                                ImGui.EndPopup();
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
                    ImGui.Text("Duty ID: " + _submarineExplorationPointId);
                    Utils.PrintOutObject(SubmarineExplorationEx, 0, new List<string>());
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