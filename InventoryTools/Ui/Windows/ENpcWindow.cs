using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Ui
{
    class ENpcWindow : UintWindow
    {
        private readonly ExcelCache _excelCache;
        private readonly IClipboardService _clipboardService;

        public ENpcWindow(ILogger<ENpcWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ExcelCache excelCache, IClipboardService clipboardService, string name = "NPC Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _excelCache = excelCache;
            _clipboardService = clipboardService;
        }
        public override void Initialize(uint eNpcId)
        {
            base.Initialize(eNpcId);
            Flags = ImGuiWindowFlags.NoSavedSettings;
            _eNpcId = eNpcId;
            if (eNpc != null)
            {
                Key = "enpc_" + eNpcId;
                WindowName = "Allagan Tools - " + eNpc.Resident!.FormattedSingular + "##" + eNpcId;
                Shops = _excelCache.ENpcCollection?.FindShops(eNpc)?.Select(c => _excelCache.ShopCollection?.Get(c)).Where(c => c != null).Select(c => c!).ToList();
            }
            else
            {
                WindowName = "Invalid NPC";
                Key = "enpc_unknown";
            }
        }

        public override bool SaveState => false;
        private uint _eNpcId;
        private ENpc? eNpc => _excelCache.ENpcCollection?.Get(_eNpcId);
        public List<IShop>? Shops;
        public override string GenericName => "Npcs";
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
                                    if (item.ItemEx.Value != null)
                                    {
                                        var useIcon = ImGuiService.GetIconTexture(item.ItemEx.Value.Icon);
                                        if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                                new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale,
                                                new(0, 0), new(1, 1),
                                                0))
                                        {
                                            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.ItemEx.Row));
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
                                            MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(item.ItemEx.Value));
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

                    Utils.PrintOutObject(eNpc, 0, new List<string>());
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