using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using InventoryTools.Services;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemDesynthSourceRenderer : ItemSupplementSourceRenderer<ItemDesynthSource>
{
    public ItemDesynthSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Desynthesis, Icons.DesynthesisIcon)
    {
    }

    public override string SingularName => "Desynthesis";
    public override string HelpText => "Can the item be obtained via desynthesis?";
}

public class ItemReductionSourceRenderer : ItemSupplementSourceRenderer<ItemReductionSource>
{
    public ItemReductionSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Reduction, Icons.ReductionIcon)
    {
    }

    public override string SingularName => "Reduction";
    public override string HelpText => "Can the item be obtained via reduction?";
}

public class ItemLootSourceRenderer : ItemSupplementSourceRenderer<ItemLootSource>
{
    public ItemLootSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Loot, Icons.LootIcon)
    {
    }

    public override string SingularName => "Loot";
    public override string HelpText => "Can the item be obtained from another item(normally a chest/material container/coffer)?";
}

public class ItemGardeningSourceRenderer : ItemSupplementSourceRenderer<ItemGardeningSource>
{
    public ItemGardeningSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Gardening, Icons.SproutIcon)
    {
    }

    public override string SingularName => "Gardening";
    public override string HelpText => "Can the item be grown via gardening?";
}

public class ItemDesynthUseRenderer : ItemSupplementUseRenderer<ItemDesynthSource>
{
    public ItemDesynthUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Desynthesis, Icons.DesynthesisIcon)
    {
    }

    public override string SingularName => "Desynthesis";
    public override string HelpText => "Can the item be desynthesized?";
}

public class ItemReductionUseRenderer : ItemSupplementUseRenderer<ItemReductionSource>
{
    public ItemReductionUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Reduction, Icons.ReductionIcon)
    {
    }

    public override string SingularName => "Reduction";
    public override string HelpText => "Can the item be reduced?";
}

public class ItemLootUseRenderer : ItemSupplementUseRenderer<ItemLootSource>
{
    public ItemLootUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Loot, Icons.LootIcon)
    {
    }

    public override string SingularName => "Loot";
    public override string HelpText => "Does this item contain other items?";
}

public class ItemGardeningUseRenderer : ItemSupplementUseRenderer<ItemGardeningSource>
{
    public ItemGardeningUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.Gardening, Icons.SproutIcon)
    {
    }

    public override string SingularName => "Gardening";
    public override string HelpText => "Can the item be used for gardening?";
}

public class ItemCardPackSourceRenderer : ItemSupplementSourceRenderer<ItemCardPackSource>
{
    public ItemCardPackSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.CardPack, Icons.CardPackIcon)
    {
    }

    public override string SingularName => "Card Pack";
    public override string HelpText => "Can the item be obtained from a card pack?";
}

public class ItemCardPackUseRenderer : ItemSupplementUseRenderer<ItemCardPackSource>
{
    public ItemCardPackUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.CardPack, Icons.CardPackIcon)
    {
    }

    public override string SingularName => "Card Pack";
    public override string HelpText => "Does this item contain cards?";
}

public class ItemCofferSourceRenderer : ItemSupplementSourceRenderer<ItemCofferSource>
{
    public ItemCofferSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Coffer, Icons.CofferIcon)
    {
    }

    public override string SingularName => "Coffer";
    public override string HelpText => "Can the item be obtained from a coffer?";
}

public class ItemCofferUseRenderer : ItemSupplementUseRenderer<ItemCofferSource>
{
    public ItemCofferUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.Coffer, Icons.CofferIcon)
    {
    }

    public override string SingularName => "Coffer";
    public override string HelpText => "Is this an item coffer that contains other items?";
}


public class ItemPalaceOfTheDeadSourceRenderer : ItemSupplementSourceRenderer<ItemPalaceOfTheDeadSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.DeepDungeon];
    public ItemPalaceOfTheDeadSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.PalaceOfTheDead, Icons.DeepDungeonIcon)
    {
    }

    public override string SingularName => "Palace of the Dead";
    public override string HelpText => "Can the item be obtained from a loot item in the Palace of the Dead?";
}

public class ItemPalaceOfTheDeadUseRenderer : ItemSupplementUseRenderer<ItemPalaceOfTheDeadSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.DeepDungeon];
    public ItemPalaceOfTheDeadUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.PalaceOfTheDead, Icons.DeepDungeonIcon)
    {
    }

    public override string SingularName => "Palace of the Dead";
    public override string HelpText => "Is this a loot item obtained in the Palace of the Dead?";
}
public class ItemHeavenOnHighSourceRenderer : ItemSupplementSourceRenderer<ItemHeavenOnHighSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.DeepDungeon];
    public ItemHeavenOnHighSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.HeavenOnHigh, Icons.DeepDungeonIcon)
    {
    }

    public override string SingularName => "Heaven on High";
    public override string HelpText => "Can the item be obtained from a loot item in the Heaven on High?";
}

public class ItemHeavenOnHighUseRenderer : ItemSupplementUseRenderer<ItemHeavenOnHighSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.DeepDungeon];
    public ItemHeavenOnHighUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.HeavenOnHigh, Icons.DeepDungeonIcon)
    {
    }

    public override string SingularName => "Heaven on High";
    public override string HelpText => "Is this a loot item obtained in the Heaven on High?";
}
public class ItemEurekaOrthosSourceRenderer : ItemSupplementSourceRenderer<ItemEurekaOrthosSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation];
    public ItemEurekaOrthosSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.EurekaOrthos, Icons.DeepDungeonIcon)
    {
    }

    public override string SingularName => "Eureka Orthos";
    public override string HelpText => "Can the item be obtained from a loot item in the Eureka Orthos?";
}

public class ItemEurekaOrthosUseRenderer : ItemSupplementUseRenderer<ItemEurekaOrthosSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation];
    public ItemEurekaOrthosUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.EurekaOrthos, Icons.DeepDungeonIcon)
    {
    }

    public override string SingularName => "Eureka Orthos";
    public override string HelpText => "Is this a loot item obtained in the Eureka Orthos?";
}

public class ItemAnemosSourceRenderer : ItemSupplementSourceRenderer<ItemAnemosSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation];
    public ItemAnemosSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Anemos, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Eureka Anemos";
    public override string HelpText => "Can the item be obtained from a loot item in Eureka Anemos?";
}

public class ItemAnemosUseRenderer : ItemSupplementUseRenderer<ItemAnemosSource>
{
    public ItemAnemosUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.Anemos, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Eureka Anemos";
    public override string HelpText => "Is this a loot item obtained in the Eureka Anemos?";
}
public class ItemPagosSourceRenderer : ItemSupplementSourceRenderer<ItemPagosSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.Pagos];
    public ItemPagosSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Pagos, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Eureka Pagos";
    public override string HelpText => "Can the item be obtained from a loot item in Eureka Pagos?";
}

public class ItemPagosUseRenderer : ItemSupplementUseRenderer<ItemPagosSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.Pagos];
    public ItemPagosUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.Pagos, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Eureka Pagos";
    public override string HelpText => "Is this a loot item obtained in the Eureka Pagos?";
}
public class ItemPyrosSourceRenderer : ItemSupplementSourceRenderer<ItemPyrosSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.Pyros];
    public ItemPyrosSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Pyros, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Eureka Pyros";
    public override string HelpText => "Can the item be obtained from a loot item in Eureka Pyros?";
}

public class ItemPyrosUseRenderer : ItemSupplementUseRenderer<ItemPyrosSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.Pyros];
    public ItemPyrosUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.Pyros, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Eureka Pyros";
    public override string HelpText => "Is this a loot item obtained in the Eureka Pyros?";
}

public class ItemHydatosSourceRenderer : ItemSupplementSourceRenderer<ItemHydatosSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.Hydatos];
    public ItemHydatosSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Hydatos, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Eureka Hydatos";
    public override string HelpText => "Can the item be obtained from a loot item in Eureka Hydatos?";
}

public class ItemPilgrimsTraverseSourceRenderer : ItemSupplementSourceRenderer<ItemPilgrimsTraverseSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.DeepDungeon];
    public ItemPilgrimsTraverseSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.PilgrimsTraverse, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Pilgrim's Traverse";
    public override string HelpText => "Can the item be obtained from a loot item in Pilgrim's Traverse?";
}

public class ItemOizysSourceRenderer : ItemSupplementSourceRenderer<ItemOizysSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation];
    public ItemOizysSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.Oizys, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Oizys";
    public override string HelpText => "Can the item be obtained from a loot item in Oizys?";
}

public class ItemHydatosUseRenderer : ItemSupplementUseRenderer<ItemHydatosSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.Hydatos];
    public ItemHydatosUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.Hydatos, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Eureka Hydatos";
    public override string HelpText => "Is this a loot item obtained in the Eureka Hydatos?";
}

public class ItemBozjaSourceRenderer : ItemSupplementSourceRenderer<ItemBozjaSource>
{
    public ItemBozjaSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Bozja, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Bozja";
    public override string HelpText => "Can the item be obtained from a loot item in Bozja?";
}

public class ItemBozjaUseRenderer : ItemSupplementUseRenderer<ItemBozjaSource>
{
    public ItemBozjaUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.Bozja, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Bozja";
    public override string HelpText => "Is this a loot item obtained in the Bozja?";
}
public class ItemLogogramSourceRenderer : ItemSupplementSourceRenderer<ItemLogogramSource>
{
    public ItemLogogramSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface,  ItemInfoType.Logogram, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Logogram";
    public override string HelpText => "Can the item be obtained from a logogram?";
}

public class ItemLogogramUseRenderer : ItemSupplementUseRenderer<ItemLogogramSource>
{
    public ItemLogogramUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.Logogram, Icons.FieldOpsIcon)
    {
    }

    public override string SingularName => "Logogram";
    public override string HelpText => "Is this item a logogram?";

    public override Func<ItemSource, int> GetIcon => source =>
    {
        return source.CostItem!.Icon;
    };
}


public abstract class ItemSupplementUseRenderer<T> : ItemSupplementSourceRenderer<T> where T : ItemSupplementSource
{
    public override RendererType RendererType => RendererType.Use;

    protected ItemSupplementUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface, ItemInfoType itemInfoType, ushort icon) : base(itemSheet, mapSheet, textureProvider, pluginInterface, itemInfoType, icon)
    {
    }

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var asSources = AsSource(sources);
        foreach (var source in asSources.OrderBy(c => c.Item.NameString))
        {
            ImGui.Image(TextureProvider.GetFromGameIcon(new GameIconLookup(source.Item.Icon)).GetWrapOrEmpty().Handle, new Vector2(18,18) * ImGui.GetIO().FontGlobalScale);
            ImGui.SameLine();
            ImGui.Text(source.Item.NameString);
            if (source.Supplement.Min != null && source.Supplement.Max != null)
            {
                ImGui.SameLine();
                if (source.Supplement.Min == source.Supplement.Max)
                {
                    ImGui.Text("(Drops 1)");
                }
                else
                {
                    ImGui.Text("(Drops " + source.Supplement.Min.Value + " - " + source.Supplement.Max.Value + ")");
                }
            }

            if (source.Supplement.Probability != null)
            {
                ImGui.SameLine();
                ImGui.TextUnformatted($"{source.Supplement.Probability.Value}%");
            }
        }
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Image(TextureProvider.GetFromGameIcon(new GameIconLookup(source.Item.Icon)).GetWrapOrEmpty().Handle, new Vector2(18,18) * ImGui.GetIO().FontGlobalScale);
        ImGui.SameLine();
        ImGui.Text(source.Item.NameString);
        if (asSource.Supplement.Min != null && asSource.Supplement.Max != null)
        {
            ImGui.SameLine();
            if (asSource.Supplement.Min == asSource.Supplement.Max)
            {
                ImGui.Text("(Drops 1)");
            }
            else
            {
                ImGui.Text("(Drops " + asSource.Supplement.Min.Value + " - " + asSource.Supplement.Max.Value + ")");
            }
        }

        if (asSource.Supplement.Probability != null)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted($"{asSource.Supplement.Probability.Value}%");
        }
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return source.Item.NameString;
    };
}

public abstract class ItemSupplementSourceRenderer<T> : ItemInfoRenderer<T> where T : ItemSupplementSource
{
    public ITextureProvider TextureProvider { get; }
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ItemInfoType _itemInfoType;
    private readonly ushort _icon;

    public ItemSupplementSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface, ItemInfoType itemInfoType, ushort icon) : base(textureProvider, pluginInterface, itemSheet, mapSheet)
    {
        TextureProvider = textureProvider;
        _pluginInterface = pluginInterface;
        _itemInfoType = itemInfoType;
        _icon = icon;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => _itemInfoType;
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);

        this.DrawItems("Reward Items: ", asSource.RewardItems);
        this.DrawItems("Required Items: ", asSource.CostItems);

        if (asSource.Supplement.Probability != null)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted($"{asSource.Supplement.Probability.Value}%");
        }
    };

    public override Func<ItemSource, int> GetIcon => _ => _icon;

    public override Func<ItemSource, string> GetName => _ => "";

    public override Func<ItemSource, string> GetDescription => source =>
    {
        return source.CostItem!.NameString;
    };
}