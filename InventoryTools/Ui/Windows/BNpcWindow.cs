using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using LuminaSupplemental.Excel.Model;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Ui
{
    class BNpcWindow : UintWindow
    {
        private readonly IChatUtilities _chatUtilities;
        private readonly IClipboardService _clipboardService;
        private readonly BNpcNameSheet _bNpcNameSheet;
        private readonly TerritoryTypeSheet _territoryTypeSheet;
        private readonly ItemSheet _itemSheet;

        public BNpcWindow(ILogger<BNpcWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, IChatUtilities chatUtilities, IClipboardService clipboardService, BNpcNameSheet bNpcNameSheet, TerritoryTypeSheet territoryTypeSheet, ItemSheet itemSheet, string name = "Mob Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _chatUtilities = chatUtilities;
            _clipboardService = clipboardService;
            _bNpcNameSheet = bNpcNameSheet;
            _territoryTypeSheet = territoryTypeSheet;
            _itemSheet = itemSheet;
        }
        public override void Initialize(uint bNpcId)
        {
            base.Initialize(bNpcId);
            Flags = ImGuiWindowFlags.NoSavedSettings;
            _bNpcId = bNpcId;
            if (bNpc != null)
            {
                WindowName = bNpc.Base.Singular.ExtractText().ToTitleCase() + "##" + bNpcId;
                Key = "bNpc_" + bNpcId;
                _mobDrops = bNpc.MobDrops;
                _mobSpawns = bNpc.MobSpawnPositions;
            }
            else
            {
                WindowName = "Unknown Mob";
            }
        }

        public override bool SaveState => false;
        private uint _bNpcId;
        private List<MobDrop>? _mobDrops;
        private List<MobSpawnPosition>? _mobSpawns;

        private BNpcNameRow? bNpc => _bNpcNameSheet.GetRowOrDefault(_bNpcId);
        public override string GenericName => "Mob";
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (ImGui.GetWindowPos() != CurrentPosition)
            {
                CurrentPosition = ImGui.GetWindowPos();
            }

            if (bNpc == null)
            {
                ImGui.TextUnformatted("bNpc with the ID " + _bNpcId + " could not be found.");
            }
            else
            {
                ImGui.Text("Type: " + string.Join(", ", bNpc.MobTypes.Select(c => c.ToString())));

                if (bNpc.NotoriousMonster != null)
                {
                    ImGui.Text("Rank: " + bNpc.NotoriousMonster?.RankFormatted());
                }

                var garlandId = bNpc.GarlandToolsId;
                if (garlandId != null)
                {
                    if (ImGui.ImageButton(ImGuiService.GetImageTexture("garlandtools").ImGuiHandle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        $"https://www.garlandtools.org/db/#mob/{garlandId}".OpenBrowser();
                    }

                    ImGuiUtil.HoverTooltip("Open in Garland Tools");
                    ImGui.SameLine();
                }

                if (ImGui.ImageButton(ImGuiService.GetImageTexture("teamcraft").ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://ffxivteamcraft.com/db/en/mob/{_bNpcId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Teamcraft");

                ImGui.Separator();


                if (_mobDrops != null && ImGui.CollapsingHeader("Drops (" + _mobDrops.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    for (var index = 0; index < _mobDrops.Count; index++)
                    {
                        ImGui.PushID("Drop"+index);
                        var drop = _mobDrops[index];
                        var listingCount = 0;

                        if (drop.Item.IsValid)
                        {
                            var useIcon = ImGuiService.GetIconTexture(drop.Item.Value.Icon);
                            if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                    new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale,
                                    new(0, 0), new(1, 1),
                                    0))
                            {
                                MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), drop.Item.RowId));
                            }

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                    ImGuiHoveredFlags.AllowWhenOverlapped &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                    ImGuiHoveredFlags
                                                        .AllowWhenBlockedByActiveItem &
                                                    ImGuiHoveredFlags.AnyWindow) &&
                                ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup("RightClickUse" + drop.Item.RowId);
                            }

                            using (var popup = ImRaii.Popup("RightClickUse"+ drop.Item.RowId))
                            {
                                if (popup)
                                {
                                    MediatorService.Publish(
                                        ImGuiService.ImGuiMenuService.DrawRightClickPopup(
                                            _itemSheet.GetRow(drop.Item.RowId)));
                                }
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(_itemSheet.GetRow(drop.Item.RowId).NameString);
                            if (listingCount < _mobDrops.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }
                        }
                    }
                }

                ImGui.NewLine();

                if (_mobSpawns != null && ImGui.CollapsingHeader("Locations (" + _mobSpawns.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    for (var index = 0; index < _mobSpawns.Count; index++)
                    {
                        ImGui.PushID("Location"+index);
                        var spawn = _mobSpawns[index];
                        var listingCount = 0;

                        var territory = _territoryTypeSheet
                            .GetRowOrDefault(spawn.TerritoryTypeId);
                        if (territory != null)
                        {
                            if (ImGui.ImageButton(ImGuiService.GetIconTexture(60561).ImGuiHandle,
                                    new Vector2(32 * ImGui.GetIO().FontGlobalScale,32 * ImGui.GetIO().FontGlobalScale), new Vector2(0, 0),
                                    new Vector2(1, 1), 0))
                            {
                                _chatUtilities.PrintFullMapLink(
                                    new GenericMapLocation(spawn.Position.X, spawn.Position.Y,
                                        territory.Base.Map,
                                        territory.Base.PlaceName,
                                        territory.RowRef), bNpc.Base.Singular.ExtractText());
                            }

                            if (ImGui.IsItemHovered())
                            {
                                using var tt = ImRaii.Tooltip();
                                ImGui.TextUnformatted((territory.Base.PlaceName.ValueNullable?.Name.ExtractText() ?? "Unknown") + " - " +
                                                      spawn.Position.X +
                                                      " : " + spawn.Position.Y);
                            }
                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(bNpc.Base.Singular.ExtractText());
                            if (listingCount < _mobSpawns.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }
                        }
                    }
                }

                ImGui.NewLine();

                #if DEBUG
                if (ImGui.CollapsingHeader("Debug"))
                {
                    ImGui.TextUnformatted("bNpc ID: " + _bNpcId);
                    if (ImGui.Button("Copy"))
                    {
                        _clipboardService.CopyToClipboard(_bNpcId.ToString());
                    }

                    Utils.PrintOutObject(bNpc, 0, new List<string>());
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

        public override bool SavePosition => true;

        public override string GenericKey => "bNpc";
    }
}