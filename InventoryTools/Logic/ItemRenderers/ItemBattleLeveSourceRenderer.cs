using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemBattleLeveSourceRenderer : ItemInfoRenderer<ItemBattleLeveSource>
{
    private readonly ITextureProvider _textureProvider;
    private readonly ItemSheet _itemSheet;
    private readonly MapSheet _mapSheet;
    private readonly IDalamudPluginInterface _pluginInterface;

    public ItemBattleLeveSourceRenderer(ITextureProvider textureProvider, ItemSheet itemSheet, MapSheet mapSheet, IDalamudPluginInterface pluginInterface) : base(textureProvider, pluginInterface, itemSheet, mapSheet)
    {
        _textureProvider = textureProvider;
        _itemSheet = itemSheet;
        _mapSheet = mapSheet;
        _pluginInterface = pluginInterface;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.BattleLeve;
    public override string SingularName => "Battle Leve";
    public override string PluralName => "Battle Leves";
    public override string HelpText => "Is this item obtained from a battle leve?";
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

        DrawItems("Possible Reward Items: ", asSource.RewardItems);
        DrawMaps(asSource);
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