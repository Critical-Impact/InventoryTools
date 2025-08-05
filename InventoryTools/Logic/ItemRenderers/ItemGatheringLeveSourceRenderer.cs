using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using InventoryTools.Extensions;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemGatheringLeveSourceRenderer : ItemInfoRenderer<ItemGatheringLeveSource>
{
    private readonly ITextureProvider _textureProvider;
    private readonly ItemSheet _itemSheet;
    private readonly MapSheet _mapSheet;
    private readonly ExcelSheet<EventItem> _eventItemSheet;
    private readonly IDalamudPluginInterface _pluginInterface;

    public ItemGatheringLeveSourceRenderer(ITextureProvider textureProvider, ItemSheet itemSheet, MapSheet mapSheet, ExcelSheet<EventItem> eventItemSheet, IDalamudPluginInterface pluginInterface) : base(textureProvider, pluginInterface, itemSheet, mapSheet)
    {
        _textureProvider = textureProvider;
        _itemSheet = itemSheet;
        _mapSheet = mapSheet;
        _eventItemSheet = eventItemSheet;
        _pluginInterface = pluginInterface;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.GatheringLeve;
    public override string SingularName => "Gathering Leve";
    public override string PluralName => "Gathering Leves";
    public override string HelpText => "Is this item obtained from a gathering leve?";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Leve];
    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var leveRow = asSource.Leve.Value;

        ImGui.TextUnformatted("Leve: " + leveRow.Name.ExtractText());
        ImGui.TextUnformatted("Class: " + leveRow.ClassJobCategory.Value.Name.ExtractText());
        ImGui.TextUnformatted("EXP Reward: " + asSource.ExpReward);
        ImGui.TextUnformatted("Allowance Cost: " + leveRow.AllowanceCost);
        ImGui.TextUnformatted("Loot Chance: " + asSource.LeveRewardItem.Value.ProbabilityPercent[asSource.RewardItemIndex] + "%");

        ImGui.TextUnformatted("Reward Items: ");
        using (ImRaii.PushIndent())
        {
            for (var itemGroupIndex = 0; itemGroupIndex < asSource.Leve.Value.LeveRewardItem.Value.LeveRewardItemGroup.Count; itemGroupIndex++)
            {
                var itemGroup = asSource.Leve.Value.LeveRewardItem.Value.LeveRewardItemGroup[itemGroupIndex];
                if (itemGroup.Value.Item.All(c => c.RowId == 0))
                {
                    continue;
                }
                ImGui.TextUnformatted("Loot Chance: " + asSource.Leve.Value.LeveRewardItem.Value.ProbabilityPercent[itemGroupIndex] + "%");
                for (var index = 0; index < itemGroup.Value.Item.Count; index++)
                {
                    var itemId = itemGroup.Value.Item[index].RowId;
                    var count = itemGroup.Value.Count[index];
                    var isHQ = itemGroup.Value.IsHQ[index];
                    if (itemId == 0)
                    {
                        continue;
                    }

                    var item = _itemSheet.GetRow(itemId);

                    ImGui.Image(
                        _textureProvider.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrEmpty().Handle,
                        new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                    ImGui.SameLine();
                    ImGui.TextUnformatted($"{item.NameString} x {count}");
                    if (isHQ)
                    {
                        ImGui.SameLine();
                        ImGui.Image(
                            _textureProvider.GetPluginImageTexture(_pluginInterface, "hq").GetWrapOrEmpty().Handle,
                            new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale);
                    }
                }
            }
        }

        DrawMaps(source);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        var leveRow = asSource.Leve.Value;
        return leveRow.Name.ExtractText();
    };
    public override Func<ItemSource, int> GetIcon => _ => Icons.LeveIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var leveRow = asSource.Leve.Value;
        return
            $"{leveRow.Name.ExtractText()} ({leveRow.ClassJobCategory.Value.Name.ExtractText()}) ({leveRow.ExpReward} xp) ({leveRow.AllowanceCost} allowances)";
    };
}