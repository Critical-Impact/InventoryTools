using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Logic;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    class AirshipWindow : UintWindow
    {
        private readonly AirshipExplorationPointSheet _airshipExplorationPointSheet;
        private readonly ItemSheet _itemSheet;

        public AirshipWindow(ILogger<AirshipWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, AirshipExplorationPointSheet airshipExplorationPointSheet, ItemSheet itemSheet, string name = "Airship Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _airshipExplorationPointSheet = airshipExplorationPointSheet;
            _itemSheet = itemSheet;
        }
        public override void Initialize(uint airshipExplorationPointId)
        {
            base.Initialize(airshipExplorationPointId);
            _airshipExplorationPointId = airshipExplorationPointId;
            if (AirshipExplorationPoint != null)
            {
                WindowName = "" + AirshipExplorationPoint.Base.NameShort.ExtractText();
                Key = "aepid_" + airshipExplorationPointId;
                _drops = _airshipExplorationPointSheet.GetItemsByAirshipExplorationPoint(_airshipExplorationPointId).Select(c => _itemSheet.GetRow(c)).ToList();
            }
            else
            {
                Key = "aepid_unknown";
                WindowName = "Unknown Airship Point";
                _drops = new List<ItemRow>();
            }
        }

        public override bool SaveState => false;

        private uint _airshipExplorationPointId;
        private List<ItemRow> _drops;
        private AirshipExplorationPointRow? AirshipExplorationPoint => _airshipExplorationPointSheet.GetRowOrDefault(_airshipExplorationPointId);

        public override string GenericKey => "airship";
        public override string GenericName => "Airship";
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (AirshipExplorationPoint == null)
            {
                ImGui.TextUnformatted("Airship Exploration Point with the ID " + _airshipExplorationPointId + " could not be found.");
            }
            else
            {
                ImGui.TextUnformatted(AirshipExplorationPoint.Base.NameShort.ExtractText());
                ImGui.TextUnformatted("Unlocked Via: " + AirshipExplorationPoint.Unlock?.Base.NameShort.ExtractText() ?? "N/A");
                ImGui.TextUnformatted("Rank Required: " + AirshipExplorationPoint.Base.RankReq);
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
                                MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(drop));
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
                    Utils.PrintOutObject(AirshipExplorationPoint, 0, new List<string>());
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