using System;
using System.Collections.Generic;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
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

public abstract class ItemSupplementUseRenderer<T> : ItemSupplementSourceRenderer<T> where T : ItemSupplementSource
{
    public override RendererType RendererType => RendererType.Use;

    protected ItemSupplementUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface, ItemInfoType itemInfoType, ushort icon) : base(itemSheet, mapSheet, textureProvider, pluginInterface, itemInfoType, icon)
    {
    }

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        foreach (var source in sources)
        {
            ImGui.Image(TextureProvider.GetFromGameIcon(new GameIconLookup(source.Item.Icon)).GetWrapOrEmpty().ImGuiHandle, new Vector2(18,18) * ImGui.GetIO().FontGlobalScale);
            ImGui.SameLine();
            ImGui.Text(source.Item.NameString);
        }
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        ImGui.Image(TextureProvider.GetFromGameIcon(new GameIconLookup(source.Item.Icon)).GetWrapOrEmpty().ImGuiHandle, new Vector2(18,18) * ImGui.GetIO().FontGlobalScale);
        ImGui.SameLine();
        ImGui.Text(source.Item.NameString);
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

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        foreach (var source in sources)
        {
            ImGui.Image(TextureProvider.GetFromGameIcon(new GameIconLookup(source.CostItem!.Icon)).GetWrapOrEmpty().ImGuiHandle, new Vector2(18,18) * ImGui.GetIO().FontGlobalScale);
            ImGui.SameLine();
            ImGui.Text(source.CostItem!.NameString);
        }
    };

    public override Action<ItemSource> DrawTooltip => source =>
    {
        ImGui.Image(TextureProvider.GetFromGameIcon(new GameIconLookup(source.CostItem!.Icon)).GetWrapOrEmpty().ImGuiHandle, new Vector2(18,18) * ImGui.GetIO().FontGlobalScale);
        ImGui.SameLine();
        ImGui.Text(source.CostItem!.NameString);
    };

    public override Func<ItemSource, int> GetIcon => _ => _icon;

    public override Func<ItemSource, string> GetName => _ => "";

    public override Func<ItemSource, string> GetDescription => source =>
    {
        return source.CostItem!.NameString;
    };
}