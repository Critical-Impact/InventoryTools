using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Time;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemMiningSourceRenderer : ItemGatheringSourceRenderer<ItemMiningSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Mining];
    public override string HelpText => "Can the item be gathered from a regular mining node?";
    public ItemMiningSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.Mining, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Mining";
}

public class ItemQuarryingSourceRenderer : ItemGatheringSourceRenderer<ItemQuarryingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Mining];
    public override string HelpText => "Can the item be gathered from a regular quarrying node?";
    public ItemQuarryingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.Quarrying, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Quarrying";
}

public class ItemLoggingSourceRenderer : ItemGatheringSourceRenderer<ItemLoggingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Botany];
    public override string HelpText => "Can the item be gathered from a regular logging node?";
    public ItemLoggingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet,mapSheet, seTime, ItemInfoType.Logging, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Logging";
}

public class ItemHarvestingSourceRenderer : ItemGatheringSourceRenderer<ItemHarvestingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Botany];
    public override string HelpText => "Can the item be gathered from a regular harvesting node?";

    public ItemHarvestingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.Harvesting, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Harvesting";
}

public class ItemHiddenMiningSourceRenderer : ItemGatheringSourceRenderer<ItemHiddenMiningSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Mining, ItemInfoRenderCategory.HiddenGathering];
    public override string HelpText => "Can the item be gathered from a hidden mining node?";

    public ItemHiddenMiningSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.HiddenMining, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Mining (Hidden)";
}

public class ItemHiddenQuarryingSourceRenderer : ItemGatheringSourceRenderer<ItemHiddenQuarryingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Mining, ItemInfoRenderCategory.HiddenGathering];
    public override string HelpText => "Can the item be gathered from a hidden quarrying node?";

    public ItemHiddenQuarryingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.HiddenQuarrying, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Quarrying (Hidden)";
}

public class ItemHiddenLoggingSourceRenderer : ItemGatheringSourceRenderer<ItemHiddenLoggingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Botany, ItemInfoRenderCategory.HiddenGathering];
    public override string HelpText => "Can the item be gathered from a hidden logging node?";

    public ItemHiddenLoggingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.HiddenLogging, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Logging (Hidden)";
}

public class ItemHiddenHarvestingSourceRenderer : ItemGatheringSourceRenderer<ItemHiddenHarvestingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Botany, ItemInfoRenderCategory.HiddenGathering];
    public override string HelpText => "Can the item be gathered from a hidden harvesting node?";

    public ItemHiddenHarvestingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.HiddenHarvesting, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Harvesting (Hidden)";
}

public class ItemTimedMiningSourceRenderer : ItemGatheringSourceRenderer<ItemTimedMiningSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Mining, ItemInfoRenderCategory.TimedGathering];
    public override string HelpText => "Can the item be gathered from a timed mining node?";

    public ItemTimedMiningSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.TimedMining, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Mining (Timed)";
}

public class ItemTimedQuarryingSourceRenderer : ItemGatheringSourceRenderer<ItemTimedQuarryingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Mining, ItemInfoRenderCategory.TimedGathering];
    public override string HelpText => "Can the item be gathered from a timed quarrying node?";

    public ItemTimedQuarryingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.TimedQuarrying, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Quarrying (Timed)";
}

public class ItemTimedLoggingSourceRenderer : ItemGatheringSourceRenderer<ItemTimedLoggingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Botany, ItemInfoRenderCategory.TimedGathering];
    public override string HelpText => "Can the item be gathered from a timed logging node?";

    public ItemTimedLoggingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.TimedLogging, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Logging (Timed)";
}

public class ItemTimedHarvestingSourceRenderer : ItemGatheringSourceRenderer<ItemTimedHarvestingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Botany, ItemInfoRenderCategory.TimedGathering];
    public override string HelpText => "Can the item be gathered from a timed harvesting node?";

    public ItemTimedHarvestingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.TimedHarvesting, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Harvesting (Timed)";
}

public class ItemEphemeralMiningSourceRenderer : ItemGatheringSourceRenderer<ItemEphemeralMiningSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Mining, ItemInfoRenderCategory.EphemeralGathering];
    public override string HelpText => "Can the item be gathered from a ephemeral mining node?";

    public ItemEphemeralMiningSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.EphemeralMining, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Mining (Ephemeral)";
}

public class ItemEphemeralQuarryingSourceRenderer : ItemGatheringSourceRenderer<ItemEphemeralQuarryingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Mining, ItemInfoRenderCategory.EphemeralGathering];
    public override string HelpText => "Can the item be gathered from a ephemeral quarrying node?";
    public ItemEphemeralQuarryingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.EphemeralQuarrying, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Quarrying (Ephemeral)";
}

public class ItemEphemeralLoggingSourceRenderer : ItemGatheringSourceRenderer<ItemEphemeralLoggingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.Botany, ItemInfoRenderCategory.EphemeralGathering];
    public override string HelpText => "Can the item be gathered from a ephemeral logging node?";
    public ItemEphemeralLoggingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.EphemeralLogging, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Logging (Ephemeral)";
}

public class ItemEphemeralHarvestingSourceRenderer : ItemGatheringSourceRenderer<ItemEphemeralHarvestingSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Gathering, ItemInfoRenderCategory.EphemeralGathering];
    public override string HelpText => "Can the item be gathered from a ephemeral harvesting node?";

    public ItemEphemeralHarvestingSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(itemSheet, mapSheet, seTime, ItemInfoType.EphemeralHarvesting, textureProvider, dalamudPluginInterface)
    {
    }

    public override string SingularName => "Harvesting (Ephemeral)";
}

public abstract class ItemGatheringSourceRenderer<T> : ItemInfoRenderer<T> where T : ItemGatheringSource
{
    private readonly MapSheet _mapSheet;
    private readonly ISeTime _seTime;
    private readonly ItemInfoType _type;

    public ItemGatheringSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ISeTime seTime, ItemInfoType type,
        ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _mapSheet = mapSheet;
        _seTime = seTime;
        _type = type;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => _type;
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = (ItemGatheringSource)source;

         var level = asSource.GatheringItem.Base.GatheringItemLevel.Value.GatheringItemLevel;
         ImGui.Text("Level:" + (level == 0 ? "N/A" : level));
         var stars = asSource.GatheringItem.Base.GatheringItemLevel.Value.Stars;
         ImGui.Text("Stars:" + (stars == 0 ? "N/A" : stars));
         var perceptionRequired = asSource.GatheringItem.Base.PerceptionReq;
         ImGui.Text("Perception Required:" + (perceptionRequired == 0 ? "N/A" : stars));

         if (asSource.GatheringItem.AvailableAtTimedNode)
         {
             ImGui.Text("Maps:");
             using (ImRaii.PushIndent())
             {
                 foreach (var gatheringPoint in asSource.GatheringItem.GatheringPoints)
                 {
                     var mapName = _mapSheet.GetRow(gatheringPoint.Base.TerritoryType.Value.Map.RowId).FormattedName;
                     var nextUptime = gatheringPoint.GatheringPointTransient.GetGatheringUptime()
                         ?.NextUptime(_seTime.ServerTime) ?? null;
                     if (nextUptime == null
                         || nextUptime.Equals(TimeInterval.Always)
                         || nextUptime.Equals(TimeInterval.Invalid)
                         || nextUptime.Equals(TimeInterval.Never))
                     {
                         continue;
                     }
                     if (nextUptime.Value.Start > TimeStamp.UtcNow)
                     {
                         using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed))
                         {
                             ImGui.Text(mapName + ": up in " +
                                               TimeInterval.DurationString(nextUptime.Value.Start, TimeStamp.UtcNow,
                                                   true));
                         }
                     }
                     else
                     {
                         using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen))
                         {
                             ImGui.Text(mapName + " up for " +
                                               TimeInterval.DurationString(nextUptime.Value.End, TimeStamp.UtcNow,
                                                   true));
                         }
                     }
                 }
             }
         }
         else
         {
             DrawMaps(source);
         }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = (ItemGatheringSource)source;
        return asSource.Item.NameString;
    };
    public override Func<ItemSource, int> GetIcon => _ =>
    {
        switch (_type)
        {
            case ItemInfoType.Mining:
                return Icons.MiningIcon;
            case ItemInfoType.Quarrying:
                return Icons.QuarryingIcon;
            case ItemInfoType.Harvesting:
                return Icons.HarvestingIcon;
            case ItemInfoType.Logging:
                return Icons.LoggingIcon;
            case ItemInfoType.TimedMining or ItemInfoType.HiddenMining
                or ItemInfoType.EphemeralMining:
                return Icons.TimedMiningIcon;
            case ItemInfoType.TimedQuarrying or ItemInfoType.HiddenQuarrying
                or ItemInfoType.EphemeralQuarrying:
                return Icons.TimedQuarryingIcon;
            case ItemInfoType.TimedHarvesting or ItemInfoType.HiddenHarvesting
                or ItemInfoType.EphemeralHarvesting:
                return Icons.TimedHarvestingIcon;
            case ItemInfoType.TimedLogging or ItemInfoType.HiddenLogging
                or ItemInfoType.EphemeralLogging:
                return Icons.TimedLoggingIcon;
        }

        return Icons.RedXIcon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var level = asSource.GatheringItem.Base.GatheringItemLevel.Value.GatheringItemLevel;
        var perceptionRequired = asSource.GatheringItem.Base.PerceptionReq;
        var stars = asSource.GatheringItem.Base.GatheringItemLevel.Value.Stars;
        var starsString = "";
        for (int i = 0; i < stars; i++)
        {
            starsString += "*";
        }

        return $"Level {(level == 0 ? "N/A" : level)} ({starsString}) ({perceptionRequired} perception required)";
    };
}