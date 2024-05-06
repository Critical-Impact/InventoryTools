using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Internal;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Lumina.Excel.GeneratedSheets;
using LuminaSupplemental.Excel.Model;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Ui
{
    class DutyWindow : UintWindow
    {
        private readonly IIconService _iconService;
        private readonly ExcelCache _excelCache;

        public DutyWindow(ILogger<DutyWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, IIconService iconService, ExcelCache excelCache, string name = "Duty Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _iconService = iconService;
            _excelCache = excelCache;
        }
        public override void Initialize(uint contentFinderConditionId)
        {
            base.Initialize(contentFinderConditionId);
            _contentFinderConditionId = contentFinderConditionId;
            if (ContentFinderCondition != null)
            {
                WindowName = "Allagan Tools - " + ContentFinderCondition.Name.ToDalamudString();
                Key = "cfcid_" + contentFinderConditionId;
                DungeonChestItems = new HashSet<uint>();
                DungeonRewards = new HashSet<uint>();
                var dungeonChests = _excelCache.DungeonChests ?? new List<DungeonChest>();
                foreach (var dungeonChest in dungeonChests)
                {
                    if (dungeonChest.ContentFinderConditionId == _contentFinderConditionId)
                    {
                        var dungeonChestItems = _excelCache.DungeonChestItems ?? new List<DungeonChestItem>();
                        var items = dungeonChestItems.Where(c => c.ChestId == dungeonChest.RowId);
                        foreach (var item in items)
                        {
                            DungeonChestItems.Add(item.ItemId);
                        }
                    }
                }

                var excelCacheDungeonDrops = _excelCache.DungeonDrops ?? new List<DungeonDrop>();
                foreach (var dungeonDrop in excelCacheDungeonDrops)
                {
                    if (dungeonDrop.ContentFinderConditionId == _contentFinderConditionId)
                    {
                        DungeonRewards.Add(dungeonDrop.ItemId);
                    }
                }

                var dungeonBossDrops = _excelCache.DungeonBossDrops ?? new List<DungeonBossDrop>();
                DungeonBossDrops = dungeonBossDrops.Where(c => c.ContentFinderConditionId == _contentFinderConditionId).GroupBy(c => c.FightNo).ToDictionary(c => c.Key, c => c.ToList());
                var dungeonBosses = _excelCache.DungeonBosses ?? new List<DungeonBoss>();
                DungeonBosses = dungeonBosses.Where(c => c.ContentFinderConditionId == _contentFinderConditionId).ToList();
                var dungeonBossChests = _excelCache.DungeonBossChests ?? new List<DungeonBossChest>();
                DungeonBossChests =  dungeonBossChests.Where(c => c.ContentFinderConditionId == _contentFinderConditionId).GroupBy(c => c.FightNo).ToDictionary(c => c.Key, c => c.ToList());
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
        private ContentFinderCondition? ContentFinderCondition => _excelCache.GetContentFinderConditionExSheet().GetRow(_contentFinderConditionId);

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
                ImGui.TextUnformatted(ContentFinderCondition.Name.ToDalamudString().ToString());
                ImGui.TextUnformatted(ContentFinderCondition.ContentType?.Value?.Name.ToString() ?? "Unknown Content Type");
                ImGui.TextUnformatted("Level Required: " + ContentFinderCondition.ClassJobLevelRequired);
                ImGui.TextUnformatted("Item Level Required: " + ContentFinderCondition.ItemLevelRequired);
                ;
                var itemIcon = _iconService[(int)(ContentFinderCondition.ContentType?.Value?.IconDutyFinder ?? Icons.DutyIcon)];
                ImGui.Image(itemIcon.ImGuiHandle, new Vector2(100, 100) * ImGui.GetIO().FontGlobalScale);
                
                var garlandIcon = _iconService.LoadImage("garlandtools");
                if (ImGui.ImageButton(garlandIcon.ImGuiHandle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://www.garlandtools.org/db/#instance/{ContentFinderCondition.Content}".OpenBrowser();
                }
                foreach (var dungeonBoss in DungeonBosses)
                {
                    if (ImGui.CollapsingHeader(
                            _excelCache.GetBNpcNameExSheet().GetRow(dungeonBoss.BNpcNameId)?.FormattedName + " - Fight " + (dungeonBoss.FightNo + 1) ??
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
                                        (_excelCache.GetItemExSheet().GetRow(c.ItemId), c.Quantity)).ToList();

                                    for (var index = 0; index < items.Count; index++)
                                    {
                                        var item = items[index];
                                        if (item.Item1 == null) continue;
                                        ImGui.PushID("dbc" + dungeonBoss.RowId + "_" + chest.Key + "_" + index);
                                        IDalamudTextureWrap? useIcon = _iconService[item.Item1.Icon];
                                        if (useIcon != null)
                                        {
                                            if (ImGui.ImageButton(useIcon.ImGuiHandle,
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
                                                var itemEx = _excelCache.GetItemExSheet()
                                                    .GetRow(item.Item1.RowId);
                                                if (itemEx != null)
                                                {
                                                    MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(itemEx));
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
                                        }
                                        ImGui.PopID();
                                    }
                                }
                            }
                        }
                        if (DungeonBossDrops.ContainsKey(dungeonBoss.FightNo))
                        {
                            var drops = DungeonBossDrops[dungeonBoss.FightNo].Select(c => _excelCache.GetItemExSheet().GetRow(c.ItemId)).Where(c => c != null).Select(c => c!).ToList();
                            if (ImGui.CollapsingHeader("Drops", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                            {
                                ImGuiStylePtr style = ImGui.GetStyle();
                                float windowVisibleX2 =
                                    ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                                for (var index = 0; index < drops.Count; index++)
                                {
                                    var item = drops[index];
                                    var useIcon = _iconService[item.Icon];
                                    if (useIcon != null)
                                    {
                                        ImGui.PushID("dbd" + dungeonBoss.RowId + "_" + index);
                                        if (ImGui.ImageButton(useIcon.ImGuiHandle,
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
                                            var itemEx = _excelCache.GetItemExSheet()
                                                .GetRow(item.RowId);
                                            if (itemEx != null)
                                            {
                                                MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(itemEx));
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
                }

                if (ImGui.CollapsingHeader("Other Chests (" + DungeonChestItems.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var uses = DungeonChestItems.Select(c => _excelCache.GetItemExSheet().GetRow(c)).Where(c => c != null).Select(c => c!).ToList();
                    for (var index = 0; index < uses.Count; index++)
                    {
                        ImGui.PushID("Use"+index);
                        var use = uses[index];
                        var useIcon = _iconService[use.Icon];
                        if (useIcon != null)
                        {
                            if (ImGui.ImageButton(useIcon.ImGuiHandle,
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
                                var itemEx = _excelCache.GetItemExSheet().GetRow(use.RowId);
                                if (itemEx != null)
                                {
                                    MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(itemEx));
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
                        }

                        ImGui.PopID();
                    }
                }
                
                if (ImGui.CollapsingHeader("Rewards (" + DungeonRewards.Count + ")", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var uses = DungeonRewards.Select(c => _excelCache.GetItemExSheet().GetRow(c)).Where(c => c != null).Select(c => c!).ToList();
                    for (var index = 0; index < uses.Count; index++)
                    {
                        ImGui.PushID("Use"+index);
                        var use = uses[index];
                        
                        var useIcon = _iconService[use.Icon];
                        if (useIcon != null)
                        {
                            if (ImGui.ImageButton(useIcon.ImGuiHandle,
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
                                var itemEx = _excelCache.GetItemExSheet().GetRow(use.RowId);
                                if (itemEx != null)
                                {
                                    MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(itemEx));
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