using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Ui
{
    class ENpcWindow : UintWindow
    {
        private readonly IClipboardService _clipboardService;
        private readonly ItemInfoCache _itemInfoCache;
        private readonly ENpcResidentSheet _eNpcResidentSheet;

        public ENpcWindow(ILogger<ENpcWindow> logger,
            MediatorService mediator,
            ImGuiService imGuiService,
            InventoryToolsConfiguration configuration,
            IClipboardService clipboardService,
            ItemInfoCache itemInfoCache,
            ENpcResidentSheet eNpcResidentSheet,
            string name = "NPC Window") : base(logger,
            mediator,
            imGuiService,
            configuration,
            name)
        {
            _clipboardService = clipboardService;
            _itemInfoCache = itemInfoCache;
            _eNpcResidentSheet = eNpcResidentSheet;
        }
        public override void Initialize(uint eNpcId)
        {
            base.Initialize(eNpcId);
            Flags = ImGuiWindowFlags.NoSavedSettings;
            _eNpcId = eNpcId;
            if (ENpcResidentRow != null)
            {
                Key = "enpc_" + eNpcId;
                WindowName = "Allagan Tools - " + ENpcResidentRow.Base.Singular.ExtractText() + "##" + eNpcId;
                Shops = _itemInfoCache.GetNpcShops(eNpcId)?.ToList() ?? [];
            }
            else
            {
                WindowName = "Invalid NPC";
                Key = "enpc_unknown";
            }
        }

        public override bool SaveState => false;
        private uint _eNpcId;
        private ENpcResidentRow? ENpcResidentRow => _eNpcResidentSheet.GetRowOrDefault(_eNpcId);
        public List<IShop>? Shops;
        public override string GenericName => "Npcs";
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (ImGui.GetWindowPos() != CurrentPosition)
            {
                CurrentPosition = ImGui.GetWindowPos();
            }

            if (ENpcResidentRow == null)
            {
                ImGui.TextUnformatted("eNpc with the ID " + _eNpcId + " could not be found.");
            }
            else
            {
                if (ImGui.ImageButton(ImGuiService.GetImageTexture("garlandtools").ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://www.garlandtools.org/db/#eNpc/{_eNpcId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("Open in Garland Tools");
                ImGui.SameLine();
                if (ImGui.ImageButton(ImGuiService.GetImageTexture("teamcraft").ImGuiHandle,
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
                                    var useIcon = ImGuiService.GetIconTexture(item.Item.Icon);
                                    if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale,
                                            new(0, 0), new(1, 1),
                                            0))
                                    {
                                        MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.Item.RowId));
                                    }

                                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                            ImGuiHoveredFlags.AllowWhenOverlapped &
                                                            ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                            ImGuiHoveredFlags
                                                                .AllowWhenBlockedByActiveItem &
                                                            ImGuiHoveredFlags.AnyWindow) &&
                                        ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                    {
                                        ImGui.OpenPopup("RightClickUse" + item.Item.RowId);
                                    }

                                    using (var popup = ImRaii.Popup("RightClickUse"+ item.Item.RowId))
                                    {
                                        if (popup)
                                        {
                                            MediatorService.Publish(
                                                ImGuiService.ImGuiMenuService.DrawRightClickPopup(item.Item));
                                        }
                                    }

                                    float lastButtonX2 = ImGui.GetItemRectMax().X;
                                    float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                                    ImGuiUtil.HoverTooltip(item.Item.NameString);
                                    if (listingCount < shop.ShopListings.Count() && nextButtonX2 < windowVisibleX2)
                                    {
                                        ImGui.SameLine();
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
                        _clipboardService.CopyToClipboard(_eNpcId.ToString());
                    }

                    Utils.PrintOutObject(ENpcResidentRow, 0, new List<string>());
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

        public override string GenericKey => "eNpc";
    }
}