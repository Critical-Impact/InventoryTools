using System.Collections.Generic;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Ui
{
    class SubmarineWindow : UintWindow
    {
        private readonly SubmarineExplorationSheet _submarineExplorationSheet;
        private readonly ItemSheet _itemSheet;

        public SubmarineWindow(ILogger<SubmarineWindow> logger,
            MediatorService mediator,
            ImGuiService imGuiService,
            InventoryToolsConfiguration configuration,
            SubmarineExplorationSheet submarineExplorationSheet,
            ItemSheet itemSheet,
            string name = "Submarine Window") : base(logger,
            mediator,
            imGuiService,
            configuration,
            name)
        {
            _submarineExplorationSheet = submarineExplorationSheet;
            _itemSheet = itemSheet;
        }
        public override void Initialize(uint submarineExplorationPointId)
        {
            base.Initialize(submarineExplorationPointId);
            _submarineExplorationPointId = submarineExplorationPointId;
            if (SubmarineExploration != null)
            {
                Key = "sepid_" + submarineExplorationPointId;
                WindowName = "" + SubmarineExploration.Base.Location.ExtractText();
                _drops = SubmarineExploration.DropItems;
            }
            else
            {
                WindowName = "Invalid Submarine Exploration";
                Key = "sepid_invalid";
                _drops = new List<ItemRow>();
            }
        }
        public override bool SaveState => false;
        private uint _submarineExplorationPointId;
        private List<ItemRow> _drops;
        private SubmarineExplorationRow? SubmarineExploration => _submarineExplorationSheet.GetRowOrDefault(_submarineExplorationPointId);

        public override string GenericKey { get; } = "submarine";
        public override string GenericName { get; } = "Submarines";
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (SubmarineExploration == null)
            {
                ImGui.TextUnformatted("Submarine Exploration Point with the ID " + _submarineExplorationPointId + " could not be found.");
            }
            else
            {
                ImGui.TextUnformatted(SubmarineExploration.Base.Location.ExtractText());
                ImGui.TextUnformatted("Unlocked Via: " + SubmarineExploration.Unlock?.Base.Location.ExtractText() ?? "N/A");
                ImGui.TextUnformatted("Rank Required: " + SubmarineExploration.Base.RankReq);
                ;
                ImGui.Image(ImGuiService.GetIconTexture(Icons.AirshipIcon).ImGuiHandle, new Vector2(100, 100) * ImGui.GetIO().FontGlobalScale);


                if (ImGui.CollapsingHeader("Rewards (" + _drops.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

                    for (var index = 0; index < _drops.Count; index++)
                    {
                        ImGui.PushID("Reward"+index);
                        var drop = _drops[index];

                        if (ImGui.ImageButton(ImGuiService.GetIconTexture(drop.Icon).ImGuiHandle,
                                new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                        {
                            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), drop.RowId));
                        }
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClickUse" + drop.RowId);
                        }

                        if (ImGui.BeginPopup("RightClickUse"+ drop.RowId))
                        {
                            var itemRow = _itemSheet.GetRowOrDefault(drop.RowId);
                            if (itemRow != null)
                            {
                                MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(itemRow));
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

                        ImGui.PopID();
                    }
                }

                #if DEBUG
                if (ImGui.CollapsingHeader("Debug"))
                {
                    ImGui.TextUnformatted("Duty ID: " + _submarineExplorationPointId);
                    Utils.PrintOutObject(SubmarineExploration, 0, new List<string>());
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