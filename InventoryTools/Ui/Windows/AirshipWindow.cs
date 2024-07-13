using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Logic;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    class AirshipWindow : UintWindow
    {
        private readonly ExcelCache _excelCache;

        public AirshipWindow(ILogger<AirshipWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ExcelCache excelCache, string name = "Airship Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _excelCache = excelCache;
        }
        public override void Initialize(uint airshipExplorationPointId)
        {
            base.Initialize(airshipExplorationPointId);
            _airshipExplorationPointId = airshipExplorationPointId;
            if (AirshipExplorationPointEx != null)
            {
                WindowName = "" + AirshipExplorationPointEx.FormattedNameShort;
                Key = "aepid_" + airshipExplorationPointId;
                _drops = AirshipExplorationPointEx.Drops.Where(c => c.Value != null).Select(c => c.Value!).ToList();
            }
            else
            {
                Key = "aepid_unknown";
                WindowName = "Unknown Airship Point";
                _drops = new List<ItemEx>();
            }
        }
        
        public override bool SaveState => false;

        private uint _airshipExplorationPointId;
        private List<ItemEx> _drops;
        private AirshipExplorationPointEx? AirshipExplorationPointEx => _excelCache.GetAirshipExplorationPointExSheet().GetRow(_airshipExplorationPointId);

        public override string GenericKey => "airship";
        public override string GenericName => "Airship";
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
                var itemIcon = ImGuiService.GetIconTexture(Icons.AirshipIcon);
                ImGui.Image(itemIcon.ImGuiHandle, new Vector2(100, 100) * ImGui.GetIO().FontGlobalScale);
                
                
                if (ImGui.CollapsingHeader("Rewards (" + _drops.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

                    for (var index = 0; index < _drops.Count; index++)
                    {
                        ImGui.PushID("Reward"+index);
                        var drop = _drops[index];
                        
                        var useIcon = ImGuiService.GetIconTexture(drop.Icon);
                        if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                        {
                            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), drop.RowId));
                        }
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClickUse" + drop.RowId);
                        }

                        using (var popup = ImRaii.Popup("RightClickUse"+ drop.RowId))
                        {
                            if (popup.Success)
                            {
                                var itemEx = _excelCache.GetItemExSheet().GetRow(drop.RowId);
                                if (itemEx != null)
                                {
                                    MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(itemEx));
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
        public override Vector2? DefaultSize { get; } = new Vector2(500, 800);
        public override Vector2? MaxSize => new (800, 1500);
        public override Vector2? MinSize => new (100, 100);
    }
}