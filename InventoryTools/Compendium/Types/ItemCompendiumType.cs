using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Ui;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class ItemCompendiumType : CompendiumType<ItemRow>
{
    private readonly ItemSheet _itemSheet;
    private readonly IUnlockTrackerService _unlockTrackerService;
    private readonly IItemObtainabilityService _obtainabilityService;

    public ItemCompendiumType(ItemSheet itemSheet, CompendiumTable<ItemRow>.Factory tableFactory, CompendiumColumnBuilder<ItemRow>.Factory columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory, IUnlockTrackerService unlockTrackerService, IItemObtainabilityService obtainabilityService) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _itemSheet = itemSheet;
        _unlockTrackerService = unlockTrackerService;
        _obtainabilityService = obtainabilityService;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<ItemRow>()
        {
            Columns = BuiltColumns,
            CompendiumType = this,
            Key = "items",
            Name = "Items"
        });
    }

    public override string? GetName(ItemRow row)
    {
        return row.NameString;
    }

    public override string? GetSubtitle(ItemRow row)
    {
        return row.Base.ItemSearchCategory.ValueNullable?.Name.ToImGuiString();
    }

    public override (string?, uint?) GetIcon(ItemRow row)
    {
        return (null, row.Icon);
    }

    public override uint GetRowId(ItemRow row)
    {
        return row.RowId;
    }

    public override ItemRow? GetRow(uint row)
    {
        if (row == 0)
        {
            return null;
        }
        return _itemSheet.GetRow(row);
    }

    public override List<ItemRow> GetRows()
    {
        return _itemSheet.Where(c => c.NameString != string.Empty).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<ItemRow> builder)
    {
        builder.AddCompendiumOpenViewColumn(new() { Key = "icon", Name = "##Icon", HelpText = "The icon of the leve", Version = "14.0.3", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.RowId });
        builder.AddStringColumn(new() { Key = "name", Name = "Name", HelpText = "The name of the leve", Version = "14.0.3", ValueSelector = row => row.NameString });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, ItemRow row)
    {
        viewBuilder.SetupDefaults(this, row);
        viewBuilder.Description = row.Base.Description.ToImGuiString();
        viewBuilder.AddTag(() => "iLvl " + row.Base.LevelItem.RowId, () => "The item level of the item");
        viewBuilder.AddTag(() => "Patch " + row.Patch, () => "The patch the item was introduced");
        if (row.CanBeAcquired)
        {
            viewBuilder.AddTag(
                () =>
                {
                    var isUnlocked = _unlockTrackerService.IsUnlocked(row);
                    if (isUnlocked == null) return "Acquired?";
                    return isUnlocked.Value ? "Acquired" : "Not Acquired";
                },
                () => "Is the item acquired?",
                () =>
                {
                    var isUnlocked = _unlockTrackerService.IsUnlocked(row);
                    if (isUnlocked == null) return ImGuiColors.DalamudYellow;
                    return isUnlocked.Value ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed;
                });
        }

        if (row.CanBeCrafted)
        {
            viewBuilder.AddTag(() => "Craftable", () => "Is the item craftable?");
        }
        if (row.CanBeDesynthed)
        {
            viewBuilder.AddTag(() => "Desynthable", () => "Can the item be desynthed?");
        }

        viewBuilder.AddItemSourcesSection(new ItemSourcesSectionOptions()
        {
            Sources = row.Sources,
            SourceType = SourceType.Source,
            SectionKey = "sources",
            SectionName = "Sources",
        });

        viewBuilder.AddItemSourcesSection(new ItemSourcesSectionOptions()
        {
            Sources = row.Uses,
            SourceType = SourceType.Use,
            SectionKey = "uses",
            SectionName = "Uses"
        });

        viewBuilder.AddMetadataSection(new MetadataSectionOptions()
        {
            SectionKey = "information",
            SectionName = "Information",
            Rows = new List<MetadataSectionOptions.Row>()
            {
                new()
                {
                    Label = "Buy from Vendor Price",
                    Value = () => row.BuyFromVendorPrice + SeIconChar.Gil.ToIconString(),
                    ShouldDraw = () => row.BuyFromVendorPrice != 0 && row.HasSourcesByType(ItemInfoType.GilShop)
                },
                new()
                {
                    Label = "Sell to Vendor Price",
                    Value = () => row.SellToVendorPrice + SeIconChar.Gil.ToIconString(),
                    ShouldDraw = () => row.SellToVendorPrice != 0
                },
            }
        });
        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            SectionKey = "desynthesis_class",
            SectionName = "Desynthesis Class",
            RelatedRef = (RowRef)row.Base.ClassJobRepair,
            HideWhenEmpty = true
        });
        var sharedModels = row.GetSharedModels();
        viewBuilder.AddItemListSection(new ItemListSectionOptions()
        {
            SectionKey = "shared_models",
            SectionName = "Shared Models",
            Items = sharedModels.Select(c => new ItemInfo(c)),
            HideWhenEmpty = true
        });

        var (allRequirements, requirementRows) = BuildObtainabilityRows(row);
        if (allRequirements.Count > 0)
        {
            viewBuilder.AddTag(
                () => allRequirements.All(r => r.IsMet) ? "Unlocked" : "Not Unlocked",
                () => "Are all unlock requirements met for this item?",
                () => allRequirements.All(r => r.IsMet) ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed);
        }

        viewBuilder.AddMetadataSection(new MetadataSectionOptions()
        {
            SectionKey = "unlock_requirements",
            SectionName = "Unlock Requirements",
            Rows = requirementRows,
            HideWhenEmpty = true
        });

        viewBuilder.AddLink($"https://www.garlandtools.org/db/#item/{row.GarlandToolsId}", "Open in Garland Tools", "garlandtools");
        viewBuilder.AddLink($"https://ffxivteamcraft.com/db/en/item/{row.RowId}", "Open in Teamcraft", "teamcraft");
        if (row.CanBePlacedOnMarket)
        {
            viewBuilder.AddLink($"https://universalis.app/market/{row.RowId}", "Open in Universalis", "universalis");
        }

        viewBuilder.AddLink($"https://ffxiv.gamerescape.com/wiki/{HttpUtility.UrlEncode(row.GamerEscapeName)}?useskin=Vector", "Open in Gamerescape", "gamerescape");
        viewBuilder.AddLink($"https://ffxiv.consolegameswiki.com/wiki/{HttpUtility.UrlEncode(row.ConsoleGamesWikiName)}", "Open in Console Games Wiki", "consolegameswiki");
    }

    private (List<ObtainabilityRequirement> AllRequirements, List<MetadataSectionOptions.Row> Rows) BuildObtainabilityRows(ItemRow row)
    {
        var allRequirements = new List<ObtainabilityRequirement>();
        var rows = new List<MetadataSectionOptions.Row>();

        var sourcesToCheck = new (IngredientPreferenceType Type, string Label)[]
        {
            (IngredientPreferenceType.Crafting,    "Crafting"),
            (IngredientPreferenceType.Mining,      "Mining"),
            (IngredientPreferenceType.Botany,      "Botany"),
            (IngredientPreferenceType.Fishing,     "Fishing"),
            (IngredientPreferenceType.SpearFishing,"Spearfishing"),
        };

        foreach (var (preferenceType, label) in sourcesToCheck)
        {
            RecipeRow? recipe = null;
            if (preferenceType == IngredientPreferenceType.Crafting)
            {
                recipe = row.Sources.OfType<ItemCraftResultSource>().FirstOrDefault()?.Recipe;
                if (recipe == null) continue;
            }

            var requirements = _obtainabilityService.GetRequirements(row, preferenceType, recipe);
            foreach (var req in requirements)
            {
                allRequirements.Add(req);
                var captured = req;
                rows.Add(new MetadataSectionOptions.Row
                {
                    Label = $"{label}: {captured.Description}",
                    Value = () => captured.IsMet ? "Met" : "Not Met",
                });
            }
        }

        return (allRequirements, rows);
    }

    public override bool HasRow(uint rowId)
    {
        if (rowId == 0)
        {
            return false;
        }
        return _itemSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<Type>? RelatedTypes => [typeof(Item)];

    public override bool ShowInListing => false;
    public override Type ViewRedirection => typeof(ItemWindow);

    public override string Singular => "Item";
    public override string Plural => "Items";
    public override string Description => "All the items available in the game";
    public override string Key => "items";
    public override (string?, uint?) Icon => (null, Icons.QuestionMarkBag);
}