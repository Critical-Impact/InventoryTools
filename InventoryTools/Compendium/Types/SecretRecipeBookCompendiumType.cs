using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Ui;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class SecretRecipeBookCompendiumType : CompendiumType<SecretRecipeBook>
{
    private readonly ExcelSheet<SecretRecipeBook> _secretRecipeBookSheet;
    private readonly ItemInfoCache _itemInfoCache;
    private readonly IUnlockState _unlockState;
    private readonly Lazy<List<SecretRecipeBook>> _books;
    private readonly Lazy<Dictionary<uint, SecretRecipeBook>> _booksById;
    private readonly Lazy<Dictionary<uint, ItemSecretRecipeBookUse>> _bookUsesByRowId;

    public SecretRecipeBookCompendiumType(
        ExcelSheet<SecretRecipeBook> secretRecipeBookSheet,
        ItemInfoCache itemInfoCache,
        IUnlockState unlockState,
        CompendiumTable<SecretRecipeBook>.Factory tableFactory,
        CompendiumColumnBuilder<SecretRecipeBook>.Factory columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory)
        : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _secretRecipeBookSheet = secretRecipeBookSheet;
        _itemInfoCache = itemInfoCache;
        _unlockState = unlockState;
        _books = new Lazy<List<SecretRecipeBook>>(
            () => _secretRecipeBookSheet.Where(r => r.Name.ExtractText() != string.Empty && r.Item.RowId != 0 && r.RowId != 16).ToList(),
            LazyThreadSafetyMode.ExecutionAndPublication);
        _booksById = new Lazy<Dictionary<uint, SecretRecipeBook>>(
            () => _books.Value.ToDictionary(r => r.RowId),
            LazyThreadSafetyMode.ExecutionAndPublication);
        _bookUsesByRowId = new Lazy<Dictionary<uint, ItemSecretRecipeBookUse>>(
            () => _itemInfoCache
                .GetItemUsesByType<ItemSecretRecipeBookUse>(ItemInfoType.SecretRecipeBook)
                .ToDictionary(s => s.SecretRecipeBook.RowId),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public override string Singular => "Master Recipe Book";
    public override string Plural => "Master Recipe Books";
    public override string Description => "Master recipe books unlock additional crafting recipes.";
    public override string Key => "secret_recipe_books";
    public override (string?, uint?) Icon => (null, Icons.MasterBookIcon);

    public override List<Type>? RelatedTypes => [typeof(SecretRecipeBook)];

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = Key,
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
        });
    }

    public override string? GetName(SecretRecipeBook row) => row.Name.ToImGuiString();

    public override string? GetSubtitle(SecretRecipeBook row) => null;

    public override (string?, uint?) GetIcon(SecretRecipeBook row)
    {
        var icon = row.Item.ValueNullable?.Icon;
        if (icon.HasValue && icon.Value != 0)
        {
            return (null, icon.Value);
        }

        return (null, Icons.CraftIcon);
    }

    public override uint GetRowId(SecretRecipeBook row) => row.RowId;

    public override SecretRecipeBook GetRow(uint rowId)
    {
        return _booksById.Value.TryGetValue(rowId, out var book) ? book : default;
    }

    public override bool HasRow(uint rowId) => _booksById.Value.ContainsKey(rowId);

    public override List<SecretRecipeBook> GetRows() => _books.Value;

    public override void BuildColumns(CompendiumColumnBuilder<SecretRecipeBook> builder)
    {
        builder.AddCompendiumOpenViewColumn(new()
        {
            Key = "icon",
            Name = "##Icon",
            HelpText = "The icon of the master recipe book",
            Version = "15.0.6",
            ValueSelector = GetIcon,
            CompendiumType = this,
            RowIdSelector = row => row.RowId,
        });
        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "Name",
            HelpText = "The name of the master recipe book",
            Version = "15.0.6",
            ValueSelector = GetName,
        });
        builder.AddBooleanColumn(new()
        {
            Key = "unlocked",
            Name = "Unlocked?",
            HelpText = "Has the player unlocked this master recipe book?",
            Version = "15.0.6",
            ValueSelector = row => _unlockState.IsItemUnlocked(row.Item.Value),
        });
        builder.AddItemsColumn(new()
        {
            Key = "recipes_unlocked",
            Name = "Recipes Unlocked",
            HelpText = "The recipes unlocked by this master recipe book",
            Version = "15.0.6",
            ValueSelector = row => GetBookUse(row)?.Recipes
                .Select(r => r.ItemResult)
                .Where(i => i != null)
                .Select(i => i!)
                .ToList() ?? [],
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, SecretRecipeBook row)
    {
        viewBuilder.SetupDefaults(this, row);

        viewBuilder.AddTag(
            () => _unlockState.IsItemUnlocked(row.Item.Value) ? "Read" : "Not Read",
            () => "Whether the player has read this master recipe book.",
            () => _unlockState.IsItemUnlocked(row.Item.Value) ? Dalamud.Interface.Colors.ImGuiColors.HealerGreen : Dalamud.Interface.Colors.ImGuiColors.DalamudRed);

        var bookUse = GetBookUse(row);
        var recipeCount = bookUse?.Recipes.Count ?? 0;

        var info = new List<(string Header, string Value, bool IsVisible)>
        {
            ("Recipes Unlocked", recipeCount.ToString(), true),
        };
        viewBuilder.AddInfoTableSection(new InfoTableSectionOptions()
        {
            SectionKey = "info",
            SectionName = "Info",
            Items = info,
        });

        var craftTypeId = bookUse?.Recipes.FirstOrDefault()?.Base.CraftType.RowId;
        if (craftTypeId.HasValue)
        {
            var craftSoulCrystalUse = _itemInfoCache
                .GetItemUsesByType<ItemCraftSoulCrystalUse>(ItemInfoType.CraftSoulCrystal)
                .FirstOrDefault(u => u.ClassJob.Base.DohDolJobIndex == (sbyte)craftTypeId.Value);
            if (craftSoulCrystalUse != null)
            {
                viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
                {
                    SectionKey = "class_job",
                    SectionName = "Class",
                    RelatedRef = (RowRef)craftSoulCrystalUse.ClassJob.RowRef,
                });
            }
        }

        if (row.Item.RowId != 0)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionKey = "book_item",
                SectionName = "Book Item",
                RelatedRef = (RowRef)row.Item,
            });

            var itemSources = _itemInfoCache.GetItemSources(row.Item.RowId);
            viewBuilder.AddItemSourcesSection(new ItemSourcesSectionOptions()
            {
                SectionKey = "book_item_sources",
                SectionName = "Book Sources",
                Sources = itemSources ?? [],
                SourceType = SourceType.Source,
            });
        }

        var recipeItems = bookUse?.RelatedItems.Values.FirstOrDefault();
        if (recipeItems != null && recipeItems.Count > 0)
        {
            viewBuilder.AddItemListSection(new ItemListSectionOptions()
            {
                SectionKey = "recipes_unlocked",
                SectionName = "Recipes Unlocked",
                Items = recipeItems,
            });
        }
    }

    private ItemSecretRecipeBookUse? GetBookUse(SecretRecipeBook row)
    {
        return _bookUsesByRowId.Value.GetValueOrDefault(row.RowId);
    }
}