using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Mediator;
using InventoryTools.Ui;
using LuminaSupplemental.Excel.Model;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemMonsterDropSourceRenderer : ItemInfoRenderer<ItemMonsterDropSource>
{
    private readonly TerritoryTypeSheet _territoryTypeSheet;
    private readonly MapSheet _mapSheet;
    private readonly BNpcNameSheet _bnpcNameSheet;

    public ItemMonsterDropSourceRenderer(TerritoryTypeSheet territoryTypeSheet, MapSheet mapSheet,
        BNpcNameSheet bnpcNameSheet)
    {
        _territoryTypeSheet = territoryTypeSheet;
        _mapSheet = mapSheet;
        _bnpcNameSheet = bnpcNameSheet;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.Monster;
    public override string SingularName => "Monster Drop";
    public override string PluralName => "Monster Drops";
    public override string HelpText => "Is the item dropped from monsters?";
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text("Monster: " + asSource.MobDrop.BNpcName.Value.Singular.ExtractText().ToTitleCase());

        ImGui.Text("Locations: ");
        using (ImRaii.PushIndent())
        {
            foreach (var spawnPosition in asSource.BNpcName.MobSpawnPositions)
            {
                var map = _territoryTypeSheet.GetRowOrDefault(spawnPosition.TerritoryType.RowId)?.Map;
                if (map != null)
                {
                    ImGui.Text($"{map.FormattedName} - {spawnPosition.Position.X} / {spawnPosition.Position.Y}");
                }
            }
        }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);

        return asSource.MobDrop.BNpcName.Value.Singular.ExtractText().ToTitleCase();
    };

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);
        Dictionary<uint, HashSet<MobSpawnPosition>> positionsGroupedByNpcId = new();
        foreach (var asSource in asSources)
        {
            foreach (var position in asSource.BNpcName.MobSpawnPositions)
            {
                if (position.TerritoryType.IsValid && position.TerritoryType.ValueNullable?.Map.ValueNullable != null)
                {
                    positionsGroupedByNpcId.TryAdd(position.TerritoryType.Value.Map.RowId, new());
                    positionsGroupedByNpcId[position.TerritoryType.Value.Map.RowId].Add(position);
                }

            }
        }

        foreach (var npcGroup in positionsGroupedByNpcId)
        {
            ImGui.Text("Map: " + _mapSheet.GetRow(npcGroup.Key).FormattedName);
            ImGui.Text("Locations:");
            using (ImRaii.PushIndent())
            {
                foreach (var mobSpawnPosition in npcGroup.Value)
                {
                    ImGui.Text(
                        $"{mobSpawnPosition.BNpcName.Value.Singular.ExtractText().ToTitleCase()} - {mobSpawnPosition.Position.X} / {mobSpawnPosition.Position.Y}");
                }
            }
        }

    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.MobIcon;

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var asSource = AsSource(source);

        return new List<MessageBase>()
            { new OpenUintWindowMessage(typeof(BNpcWindow), asSource.BNpcName.RowId) };
    };
}