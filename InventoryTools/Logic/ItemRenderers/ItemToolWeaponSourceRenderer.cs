using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using InventoryTools.Localizers;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Logic.ItemRenderers;

public abstract class ItemRelicToolSourceRenderer<T> : ItemInfoRenderer<T> where T : ItemRelicToolSource
{
    private readonly ILocalizer<RelicToolType> _relicToolTypeLocalizer;

    public ItemRelicToolSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicToolType> relicToolTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
        _relicToolTypeLocalizer = relicToolTypeLocalizer;
    }

    public override RendererType RendererType => RendererType.Use;
    public override bool ShouldGroup => false;

    public override IReadOnlyList<ItemInfoRenderCategory>? Categories => [ItemInfoRenderCategory.RelicTool];

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = this.AsSource(source);
        ImGui.TextUnformatted("Class: " + asSource.RelicTool.ClassJob.Value.Name.ToImGuiString().ToTitleCase());
        this.DrawForms("Forms:", asSource.Forms);
    };

    public void DrawForms(string sectionName, IReadOnlyList<RelicTool> items)
    {
        if (items.Count == 0)
        {
            return;
        }
        ImGui.TextUnformatted(sectionName);
        using (ImRaii.PushIndent())
        {
            using (var table = ImRaii.Table("forms", 2))
            {
                if (table)
                {
                    foreach (var relicTool in items)
                    {
                        if (relicTool.ItemId == 0)
                            continue;

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted($"{_relicToolTypeLocalizer.Format(relicTool.Type)}");
                        ImGui.TableNextColumn();
                        var item = ItemSheet.GetRow(relicTool.ItemId);
                        ImGui.Image(
                            TextureProvider
                                .GetFromGameIcon(new GameIconLookup(item.Icon))
                                .GetWrapOrEmpty().Handle,
                            new Vector2(18, 18) * ImGui.GetIO().FontGlobalScale
                        );
                        ImGui.SameLine();
                        ImGui.TextUnformatted($"{item.NameString}");
                    }
                }
            }
        }
    }

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return string.Join(", ", asSource.Items.Select(c => c.NameString));
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.ToolIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return string.Join(", ", asSource.Items.Select(c => c.NameString));
    };

    public override Func<ItemSource, (Type, uint)>? RelatedType => source =>
    {
        var asSource = AsSource(source);
        return (asSource.RelicTool.GetType(), asSource.RelicTool.RowId);
    };
}


public class ItemMastercraftToolSourceRenderer : ItemRelicToolSourceRenderer<ItemMastercraftToolSource>
{
    public ItemMastercraftToolSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicToolType> relicToolTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicToolTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.MastercraftTool;
    public override string SingularName => "Mastercraft Tool";
    public override string HelpText => "Is this a Mastercraft Tool?";
}

public class ItemSkysteelToolSourceRenderer : ItemRelicToolSourceRenderer<ItemSkysteelToolSource>
{
    public ItemSkysteelToolSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicToolType> relicToolTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicToolTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.SkysteelTool;
    public override string SingularName => "Skysteel Tool";
    public override string HelpText => "Is this a Skysteel Tool?";
}

public class ItemResplendentToolSourceRenderer : ItemRelicToolSourceRenderer<ItemResplendentToolSource>
{
    public ItemResplendentToolSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicToolType> relicToolTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicToolTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.ResplendentTool;
    public override string SingularName => "Resplendent Tool";
    public override string HelpText => "Is this a Resplendent Tool?";
}

public class ItemSplendorousToolSourceRenderer : ItemRelicToolSourceRenderer<ItemSplendorousToolSource>
{
    public ItemSplendorousToolSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicToolType> relicToolTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicToolTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.SplendorousTool;
    public override string SingularName => "Splendorous Tool";
    public override string HelpText => "Is this a Splendorous Tool?";
}

public class ItemCosmicToolSourceRenderer : ItemRelicToolSourceRenderer<ItemCosmicToolSource>
{
    public ItemCosmicToolSourceRenderer(ITextureProvider textureProvider, IDalamudPluginInterface dalamudPluginInterface, ItemSheet itemSheet, MapSheet mapSheet, ILocalizer<RelicToolType> relicToolTypeLocalizer) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet, relicToolTypeLocalizer)
    {
    }

    public override ItemInfoType Type => ItemInfoType.CosmicTool;
    public override string SingularName => "Cosmic Tool";
    public override string HelpText => "Is this a Cosmic Tool?";
}