using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
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

    public ItemMonsterDropSourceRenderer(TerritoryTypeSheet territoryTypeSheet, ItemSheet itemSheet, MapSheet mapSheet,
        BNpcNameSheet bnpcNameSheet, ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
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
            foreach (var groupedSpawns in asSource.BNpcName.MobSpawnPositions.GroupBy(c => c.TerritoryTypeId))
            {
                var map = _territoryTypeSheet.GetRowOrDefault(groupedSpawns.Key)?.Map;
                if (map != null)
                {
                    var spawns = string.Join(", ", groupedSpawns.Select(spawnPosition => $"{spawnPosition.Position.X}/{spawnPosition.Position.Y}"));
                    ImGui.Text($"{map.FormattedName}");
                    using (ImRaii.PushIndent())
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted(spawns);
                        ImGui.PopTextWrapPos();
                    }
                }
            }
        }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);

        return asSource.MobDrop.BNpcName.Value.Singular.ExtractText().ToTitleCase();
    };


    public override Func<ItemSource, int> GetIcon => _ => Icons.MobIcon;

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var asSource = AsSource(source);

        return new List<MessageBase>()
            { new OpenUintWindowMessage(typeof(BNpcWindow), asSource.BNpcName.RowId) };
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return asSource.MobDrop.BNpcName.Value.Singular.ExtractText().ToTitleCase();
    };
}