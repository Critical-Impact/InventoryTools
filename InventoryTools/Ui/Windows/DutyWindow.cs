using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Ui
{
    class DutyWindow : UintWindow
    {
        private readonly ContentFinderConditionSheet _contentFinderConditionSheet;
        private readonly ItemInfoCache _itemInfoCache;
        private readonly BNpcNameSheet _bNpcNameSheet;
        private readonly ItemSheet _itemSheet;

        public DutyWindow(ILogger<DutyWindow> logger,
            MediatorService mediator,
            ImGuiService imGuiService,
            InventoryToolsConfiguration configuration,
            ContentFinderConditionSheet contentFinderConditionSheet,
            ItemInfoCache itemInfoCache,
            BNpcNameSheet bNpcNameSheet,
            ItemSheet itemSheet,
            string name = "Duty Window") : base(logger,
            mediator,
            imGuiService,
            configuration,
            name)
        {
            _contentFinderConditionSheet = contentFinderConditionSheet;
            _itemInfoCache = itemInfoCache;
            _bNpcNameSheet = bNpcNameSheet;
            _itemSheet = itemSheet;
        }
        public override void Initialize(uint contentFinderConditionId)
        {
            base.Initialize(contentFinderConditionId);
            _contentFinderConditionId = contentFinderConditionId;
            if (ContentFinderCondition != null)
            {
                WindowName = "Allagan Tools - " + ContentFinderCondition.Base.Name.ExtractText();
                Key = "cfcid_" + contentFinderConditionId;
                DungeonChestItems = new HashSet<uint>();
                DungeonRewards = new HashSet<uint>();

                var dungeonChests = _itemInfoCache.GetItemSourcesByType<ItemDungeonChestSource>(ItemInfoType.DungeonChest).Where(c => c.ContentFinderCondition.RowId == _contentFinderConditionId);
                foreach (var dungeonChest in dungeonChests)
                {
                    DungeonChestItems.Add(dungeonChest.DungeonChestItem.ItemId);
                }

                var dungeonDrops = _itemInfoCache.GetItemSourcesByType<ItemDungeonDropSource>(ItemInfoType.DungeonDrop).Where(c => c.ContentFinderCondition.RowId == _contentFinderConditionId);

                foreach (var dungeonDrop in dungeonDrops)
                {
                    DungeonRewards.Add(dungeonDrop.DungeonDrop.RowId);
                }

                var dungeonBossDrops = _itemInfoCache.GetItemSourcesByType<ItemDungeonBossDropSource>(ItemInfoType.DungeonBossDrop).Where(c => c.ContentFinderCondition.RowId == _contentFinderConditionId).ToList();

                DungeonBossDrops = dungeonBossDrops.Select(c => c.DungeonBossDrop).GroupBy(c => c.FightNo).ToDictionary(c => c.Key, c => c.ToList());

                var dungeonBossChests = _itemInfoCache.GetItemSourcesByType<ItemDungeonBossChestSource>(ItemInfoType.DungeonBossChest).Where(c => c.ContentFinderCondition.RowId == _contentFinderConditionId).ToList();

                DungeonBossChests =  dungeonBossChests.Select(c => c.DungeonBossChest).GroupBy(c => c.FightNo).ToDictionary(c => c.Key, c => c.ToList());

                DungeonBosses = dungeonBossDrops.Select(c => c.DungeonBoss).ToList();
            }
            else
            {
                WindowName = "Invalid Duty";
                Key = "cfcid_unknown";
                DungeonChestItems = new HashSet<uint>();
            }
        }
        public override bool SaveState => false;

        private uint _contentFinderConditionId;
        private ContentFinderConditionRow? ContentFinderCondition => _contentFinderConditionSheet.GetRow(_contentFinderConditionId);

        private HashSet<uint> DungeonChestItems { get; set; } = null!;
        private HashSet<uint> DungeonRewards { get; set; } = null!;
        private List<DungeonBoss> DungeonBosses { get; set; } = null!;
        private Dictionary<uint, List<DungeonBossDrop>> DungeonBossDrops { get; set; } = null!;

        private Dictionary<uint, List<DungeonBossChest>> DungeonBossChests { get; set; } = null!;
        public override string GenericKey => "duty";
        public override string GenericName => "Duty";
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (ContentFinderCondition == null)
            {
                ImGui.TextUnformatted("Dungeon with the ID " + _contentFinderConditionId + " could not be found.");
            }
            else
            {
                ImGui.TextUnformatted(ContentFinderCondition.Base.Name.ExtractText());
                ImGui.TextUnformatted(ContentFinderCondition.Base.ContentType.ValueNullable?.Name.ToString() ?? "Unknown Content Type");
                ImGui.TextUnformatted("Level Required: " + ContentFinderCondition.Base.ClassJobLevelRequired);
                ImGui.TextUnformatted("Item Level Required: " + ContentFinderCondition.Base.ItemLevelRequired);
                ;
                var itemIcon = ImGuiService.GetIconTexture((int)(ContentFinderCondition.Base.ContentType.ValueNullable?.IconDutyFinder ?? Icons.DutyIcon));
                ImGui.Image(itemIcon.ImGuiHandle, new Vector2(100, 100) * ImGui.GetIO().FontGlobalScale);

                var garlandIcon = ImGuiService.GetImageTexture("garlandtools");
                if (ImGui.ImageButton(garlandIcon.ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://www.garlandtools.org/db/#instance/{ContentFinderCondition.Base.Content}".OpenBrowser();
                }
                foreach (var dungeonBoss in DungeonBosses)
                {
                    if (ImGui.CollapsingHeader(_bNpcNameSheet.GetRowOrDefault(dungeonBoss.BNpcNameId)?.Base.Singular.ExtractText() + " - Fight " + (dungeonBoss.FightNo + 1) ??
                            "Unknown Boss",
                            ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                    {
                        if (DungeonBossChests.ContainsKey(dungeonBoss.FightNo))
                        {
                            var chests = DungeonBossChests[dungeonBoss.FightNo];
                            foreach (var chest in chests.GroupBy(c => c.CofferNo))
                            {
                                if (ImGui.CollapsingHeader("Coffer " + (chest.Key + 1),
                                        ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                                {
                                    ImGuiStylePtr style = ImGui.GetStyle();
                                    float windowVisibleX2 =
                                        ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                                    var items = chest.Select(c =>
                                        (_itemSheet.GetRowOrDefault(c.ItemId), c.Quantity)).ToList();

                                    for (var index = 0; index < items.Count; index++)
                                    {
                                        var item = items[index];
                                        if (item.Item1 == null) continue;
                                        ImGui.PushID("dbc" + dungeonBoss.RowId + "_" + chest.Key + "_" + index);
                                        if (ImGui.ImageButton(ImGuiService.GetIconTexture(item.Item1.Icon).ImGuiHandle,
                                                new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0),
                                                new(1, 1), 0))
                                        {
                                            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.Item1.RowId));
                                        }

                                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                                ImGuiHoveredFlags.AnyWindow) &&
                                            ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                        {
                                            ImGui.OpenPopup("RightClickUse" + item.Item1.RowId);
                                        }

                                        if (ImGui.BeginPopup("RightClickUse" + item.Item1.RowId))
                                        {
                                            var itemRow = _itemSheet
                                                .GetRowOrDefault(item.Item1.RowId);
                                            if (itemRow != null)
                                            {
                                                MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(itemRow));
                                            }

                                            ImGui.EndPopup();
                                        }

                                        float lastButtonX2 = ImGui.GetItemRectMax().X;
                                        float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                                        ImGuiUtil.HoverTooltip(item.Item1.NameString);
                                        if (index + 1 < items.Count && nextButtonX2 < windowVisibleX2)
                                        {
                                            ImGui.SameLine();
                                        }
                                        ImGui.PopID();
                                    }
                                }
                            }
                        }
                        if (DungeonBossDrops.ContainsKey(dungeonBoss.FightNo))
                        {
                            var drops = DungeonBossDrops[dungeonBoss.FightNo].Select(c => _itemSheet.GetRowOrDefault(c.ItemId)).Where(c => c != null).Select(c => c!).ToList();
                            if (ImGui.CollapsingHeader("Drops", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                            {
                                ImGuiStylePtr style = ImGui.GetStyle();
                                float windowVisibleX2 =
                                    ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                                for (var index = 0; index < drops.Count; index++)
                                {
                                    var item = drops[index];
                                    ImGui.PushID("dbd" + dungeonBoss.RowId + "_" + index);
                                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(item.Icon).ImGuiHandle,
                                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0),
                                            new(1, 1), 0))
                                    {
                                        MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.RowId));
                                    }

                                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                            ImGuiHoveredFlags.AllowWhenOverlapped &
                                                            ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                            ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                            ImGuiHoveredFlags.AnyWindow) &&
                                        ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                    {
                                        ImGui.OpenPopup("RightClickUse" + item.RowId);
                                    }

                                    if (ImGui.BeginPopup("RightClickUse" + item.RowId))
                                    {
                                        var itemRow = _itemSheet
                                            .GetRowOrDefault(item.RowId);
                                        if (itemRow != null)
                                        {
                                            MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(itemRow));
                                        }

                                        ImGui.EndPopup();
                                    }

                                    float lastButtonX2 = ImGui.GetItemRectMax().X;
                                    float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                                    ImGuiUtil.HoverTooltip(item.NameString);
                                    if (index + 1 < drops.Count && nextButtonX2 < windowVisibleX2)
                                    {
                                        ImGui.SameLine();
                                    }
                                    ImGui.PopID();
                                }
                            }
                        }
                    }
                }

                if (ImGui.CollapsingHeader("Other Chests (" + DungeonChestItems.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var uses = DungeonChestItems.Select(c => _itemSheet.GetRowOrDefault(c)).Where(c => c != null).Select(c => c!).ToList();
                    for (var index = 0; index < uses.Count; index++)
                    {
                        ImGui.PushID("Use"+index);
                        var use = uses[index];
                        if (ImGui.ImageButton(ImGuiService.GetIconTexture(use.Icon).ImGuiHandle,
                                new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                        {
                            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), use.RowId));
                        }
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClickUse" + use.RowId);
                        }

                        if (ImGui.BeginPopup("RightClickUse"+ use.RowId))
                        {
                            var itemRow = _itemSheet.GetRowOrDefault(use.RowId);
                            if (itemRow != null)
                            {
                                MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(itemRow));
                            }

                            ImGui.EndPopup();
                        }

                        float lastButtonX2 = ImGui.GetItemRectMax().X;
                        float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                        ImGuiUtil.HoverTooltip(use.NameString);
                        if (index + 1 < uses.Count && nextButtonX2 < windowVisibleX2)
                        {
                            ImGui.SameLine();
                        }

                        ImGui.PopID();
                    }
                }

                if (ImGui.CollapsingHeader("Rewards (" + DungeonRewards.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var uses = DungeonRewards.Select(c => _itemSheet.GetRowOrDefault(c)).Where(c => c != null).Select(c => c!).ToList();
                    for (var index = 0; index < uses.Count; index++)
                    {
                        ImGui.PushID("Use"+index);
                        var use = uses[index];

                        if (ImGui.ImageButton(ImGuiService.GetIconTexture(use.Icon).ImGuiHandle,
                                new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                        {
                            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), use.RowId));
                        }
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClickUse" + use.RowId);
                        }

                        if (ImGui.BeginPopup("RightClickUse"+ use.RowId))
                        {
                            var itemRow = _itemSheet.GetRowOrDefault(use.RowId);
                            if (itemRow != null)
                            {
                                MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(itemRow));
                            }

                            ImGui.EndPopup();
                        }

                        float lastButtonX2 = ImGui.GetItemRectMax().X;
                        float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                        ImGuiUtil.HoverTooltip(use.NameString);
                        if (index + 1 < uses.Count && nextButtonX2 < windowVisibleX2)
                        {
                            ImGui.SameLine();
                        }

                        ImGui.PopID();
                    }
                }

                #if DEBUG
                if (ImGui.CollapsingHeader("Debug"))
                {
                    ImGui.TextUnformatted("Duty ID: " + _contentFinderConditionId);
                    Utils.PrintOutObject(ContentFinderCondition, 0, new List<string>());
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