 using System.Collections.Generic;
 using System.Linq;
 using AllaganLib.GameSheets.Caches;
 using AllaganLib.GameSheets.ItemSources;
 using AllaganLib.GameSheets.Sheets;
 using AllaganLib.GameSheets.Sheets.Rows;
 using CriticalCommonLib.Models;
 using InventoryTools.Logic.Filters.Abstract;
 using InventoryTools.Services;
 using Microsoft.Extensions.Logging;

 namespace InventoryTools.Logic.Filters
 {
     public class PurchasedWithCurrencyFilter : UintMultipleChoiceFilter
     {
         private readonly ItemSheet _itemSheet;
         private readonly ItemInfoCache _itemInfoCache;
         private Dictionary<uint,string>? cachedCurrencies;

         public PurchasedWithCurrencyFilter(ILogger<PurchasedWithCurrencyFilter> logger, ItemSheet itemSheet, ItemInfoCache itemInfoCache, ImGuiService imGuiService) : base(logger, imGuiService)
         {
             _itemSheet = itemSheet;
             _itemInfoCache = itemInfoCache;
         }

         public override string Key { get; set; } = "PurchaseWithCurrency";
         public override string Name { get; set; } = "Purchased with Currency";

         public override string HelpText { get; set; } =
             "Filter items based on the currency they can be purchased with.";

         public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

         public override List<uint> DefaultValue { get; set; } = new();



         public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
         {
             return FilterItem(configuration,item.Item);
         }

         public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
         {
             var currentValue = CurrentValue(configuration);
             if (currentValue.Count == 0)
             {
                 return null;
             }

             return currentValue.Any(u => item.GetSourcesByCategory<ItemShopSource>(ItemInfoCategory.Shop).Any(c => c.CostItems.Any(e => e.RowId == u)));
         }

         public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
         {
             var currencies = _itemInfoCache.GetItemUseIdsByCategory(ItemInfoCategory.Shop);
             currencies.Add(20);
             currencies.Add(21);
             currencies.Add(22);

             cachedCurrencies ??= currencies.ToDictionary(c => c, c => _itemSheet.GetRowOrDefault(c)?.NameString ?? "Unknown").OrderBy(c => c.Value).ToDictionary(c => c.Key, c => c.Value);
             return cachedCurrencies;
         }

         public override bool HideAlreadyPicked { get; set; } = true;
     }
 }