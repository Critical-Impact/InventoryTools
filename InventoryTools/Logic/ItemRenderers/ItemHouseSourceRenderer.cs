using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Time;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Logic.ItemRenderers;

public abstract class ItemHouseSourceRenderer<T> : ItemInfoRenderer<T> where T : ItemHouseSource
{
    private readonly ItemInfoType _type;

    public override IReadOnlyList<ItemInfoRenderCategory>? Categories => [ItemInfoRenderCategory.House];

    public ItemHouseSourceRenderer(ItemInfoType type, ItemSheet itemSheet, MapSheet mapSheet,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _type = type;
    }

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => _type;
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = (ItemHouseSource)source;
        var setName = asSource.HousingPreset.Value.Singular.ExtractText();
        if (setName == string.Empty)
        {
            ImGui.Text("Not default in any house.");
        }
        else
        {
            ImGui.Text("Default in " + setName);
        }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = (ItemHouseSource)source;
        return asSource.Item.NameString;
    };
    public override Func<ItemSource, int> GetIcon => _ =>
    {
        //TODO: come up with an icon for each
        return Icons.RedXIcon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var setName = asSource.HousingPreset.Value.Singular.ExtractText();
        if (setName == string.Empty)
        {
           return "Not default in any house.";
        }
        else
        {
            return "Default in " + setName;
        }
    };
}

public class ItemHouseDoorSourceRenderer : ItemHouseSourceRenderer<ItemHouseDoorSource>
{
    public ItemHouseDoorSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(ItemInfoType.HouseDoor, itemSheet, mapSheet, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "House Fixture (Door)";

    public override string HelpText => "Can the item be placed in the door fixture slot in houses?";
}


public class ItemHouseFlooringSourceRenderer : ItemHouseSourceRenderer<ItemHouseFlooringSource>
{
    public ItemHouseFlooringSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(ItemInfoType.HouseFlooring, itemSheet, mapSheet, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "House Fixture (Flooring)";
    public override string HelpText => "Can the item be placed in the floor fixture slot in houses?";
}

public class ItemHouseLightingSourceRenderer : ItemHouseSourceRenderer<ItemHouseLightingSource>
{
    public ItemHouseLightingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(ItemInfoType.HouseLighting, itemSheet, mapSheet, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "House Fixture (Lighting)";
    public override string HelpText => "Can the item be placed in the lighting fixture slot in houses?";
}

public class ItemHouseRoofSourceRenderer : ItemHouseSourceRenderer<ItemHouseRoofSource>
{
    public ItemHouseRoofSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(ItemInfoType.HouseRoof, itemSheet, mapSheet, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "House Fixture (Roof)";
    public override string HelpText => "Can the item be placed in the roof fixture slot in houses?";
}

public class ItemHouseWallpaperSourceRenderer : ItemHouseSourceRenderer<ItemHouseWallpaperSource>
{
    public ItemHouseWallpaperSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(ItemInfoType.HouseWallpaper, itemSheet, mapSheet, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "House Fixture (Wallpaper)";
    public override string HelpText => "Can the item be placed in the interior wall fixture slot in houses?";
}

public class ItemHouseWallSourceRenderer : ItemHouseSourceRenderer<ItemHouseWallSource>
{
    public ItemHouseWallSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(ItemInfoType.HouseWall, itemSheet, mapSheet, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "House Fixture (Wall)";
    public override string HelpText => "Can the item be placed in the exterior wall fixture slot in houses?";
}

public class ItemHouseWindowSourceRenderer : ItemHouseSourceRenderer<ItemHouseWindowSource>
{
    public ItemHouseWindowSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(ItemInfoType.HouseWindow, itemSheet, mapSheet, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "House Fixture (Window)";
    public override string HelpText => "Can the item be placed in the window fixture slot in houses?";
}
