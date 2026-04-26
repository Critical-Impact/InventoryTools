using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Compendium.Types;

public class BGMCompendiumType : CompendiumType<BGM>
{
    private readonly IUnlockState _unlockState;
    private readonly ExcelSheet<BGM> _bgmSheet;
    private readonly ExcelSheet<Item> _itemSheet;
    private readonly Lazy<Dictionary<uint, BGMOrchestrion>> _bgmOrchestrions;
    private readonly Lazy<Dictionary<uint, uint>> _orchestionToItem;

    public BGMCompendiumType(List<BGMOrchestrion> bgmOrchestrions, IUnlockState unlockState, ExcelSheet<BGM> bgmSheet, ExcelSheet<Item> itemSheet, CompendiumTable<BGM>.Factory tableFactory, CompendiumColumnBuilder<BGM>.Factory columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _unlockState = unlockState;
        _bgmSheet = bgmSheet;
        _itemSheet = itemSheet;
        _bgmOrchestrions = new Lazy<Dictionary<uint, BGMOrchestrion>>(() =>
        {
            return bgmOrchestrions.ToDictionary(c => c.BGMId, c => c);
        }, LazyThreadSafetyMode.PublicationOnly);
        _orchestionToItem = new Lazy<Dictionary<uint, uint>>(() =>
        {
            return itemSheet.Where(c => c.FilterGroup == 32).ToDictionary(c => c.AdditionalData.RowId, c => c.RowId);
        }, LazyThreadSafetyMode.PublicationOnly);
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<BGM>()
        {
            Name = "BGMs",
            Columns = BuiltColumns,
            CompendiumType = this,
            Key = "bgms",
        });
    }

    public override string? GetName(BGM row)
    {
        if (_bgmOrchestrions.Value.TryGetValue(row.RowId, out var value))
        {
            if (value.Orchestrion.RowId == 0)
            {
                return value.Name;
            }

            return value.Orchestrion.Value.Name.ToImGuiString();
        }

        return "Unknown";
    }

    public override string? GetSubtitle(BGM row)
    {
        return null;
    }

    public override (string?, uint?) GetIcon(BGM row)
    {
        return (null, Icons.OrchestrionIcon);
    }

    public override uint GetRowId(BGM row)
    {
        return row.RowId;
    }

    public override BGM GetRow(uint row)
    {
        return _bgmSheet.GetRow(row);
    }

    public override List<BGM> GetRows()
    {
        return _bgmSheet.Where(c => _bgmOrchestrions.Value.ContainsKey(c.RowId)).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<BGM> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "##Icon", HelpText = "The icon of the BGM", Version = "14.1.2", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.RowId});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the BGM", Version = "14.1.2", ValueSelector = this.GetName});
        builder.AddBooleanColumn(new (){Key = "orchestrion", Name = "Orchestrion Roll?", HelpText = "Is this available as a orchestrion roll?.", Version = "14.1.2", ValueSelector = row => _bgmOrchestrions.Value.TryGetValue(row.RowId, out var value) && value.Orchestrion.RowId != 0 });
        builder.AddBooleanColumn(new (){Key = "unlocked", Name = "Orchestrion Roll Unlocked?", HelpText = "Is the orchestrion roll unlocked?.", Version = "14.1.2", ValueSelector = row => _bgmOrchestrions.Value.TryGetValue(row.RowId, out var value) && _unlockState.IsOrchestrionUnlocked(value.Orchestrion.Value)});
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, BGM row)
    {
        viewBuilder.SetupDefaults(this, row);
        if (_bgmOrchestrions.Value.TryGetValue(row.RowId, out var bgmOrchestrion))
        {
            if (bgmOrchestrion.Orchestrion.RowId != 0)
            {
                viewBuilder.Description = bgmOrchestrion.Orchestrion.Value.Description.ToImGuiString();
                viewBuilder.AddTag("Unlocked?", "Is the orchestrion roll unlocked?", () => _unlockState.IsOrchestrionUnlocked(bgmOrchestrion.Orchestrion.Value) ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);
                if (_orchestionToItem.Value.TryGetValue(bgmOrchestrion.OrchestrionId, out var orchestrionItemId))
                {
                    viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
                    {
                        SectionName = "Orchestrion Roll",
                        RelatedRef = _itemSheet.GetRow(orchestrionItemId).AsUntypedRowRef()
                    });
                }
            }
        }

    }

    public override bool HasRow(uint rowId)
    {
        if (!_bgmOrchestrions.Value.ContainsKey(rowId))
        {
            return false;
        }
        return _bgmSheet.HasRow(rowId);
    }

    public override string Singular => "BGM";
    public override string Plural => "BGM";
    public override string Description => "Music from the game";
    public override string Key => "bgm";
    public override (string?, uint?) Icon => (null, Icons.OrchestrionIcon);
}