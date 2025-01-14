using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Models;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemGardeningCrossbreedSourceRenderer : ItemInfoRenderer<ItemGardeningCrossbreedSource>
{
    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.GardeningCrossbreed;
    public override string SingularName => "Gardening Crossbreed";
    public override string HelpText => "Is this item created by crossbreeding 2 seeds?";
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text($"Result: {asSource.SeedResult.NameString}");
        ImGui.Text($"{asSource.Seed1.NameString} + {asSource.Seed2.NameString}");
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return $"{asSource.SeedResult.NameString} - {asSource.Seed1.NameString} + {asSource.Seed2.NameString}";
    };

    public override Action<List<ItemSource>>? DrawTooltipGrouped => sources =>
    {
        var actualSources = AsSource(sources);
        ImGui.Text("Crossbreeds:");
        var chunkedSources = actualSources.OrderBy(c =>c.Seed1.NameString).Chunk(actualSources.Count / MaxColumns);
        using (var table = ImRaii.Table("CrossbreedTable", this.MaxColumns, ImGuiTableFlags.SizingStretchProp))
        {
            if (table)
            {
                ImGui.TableNextRow();
                foreach (var chunkedSource in chunkedSources)
                {
                    ImGui.TableNextColumn();
                    foreach (var source in chunkedSource)
                    {
                        ImGui.Text($"{source.Seed1.NameString} + {source.Seed2.NameString}");
                    }
                }
            }
        }
        //
        // using var style = ImRaii.PushStyle(ImGuiStyleVar.CellPadding, new Vector2(5, 5));
        // using (var table = ImRaii.Table("CrossbreedTable", this.MaxColumns, ImGuiTableFlags.SizingStretchProp))
        // {
        //     if (table)
        //     {
        //         ImGui.TableNextRow();
        //         var count = 0;
        //         foreach (var groupedSource in actualSources.GroupBy(c => c.Seed1).OrderBy(c => c.Key.NameString))
        //         {
        //             ImGui.TableNextColumn();
        //             foreach (var source in groupedSource)
        //             {
        //                 ImGui.Text($"{source.Seed1.NameString} + {source.Seed2.NameString}");
        //             }
        //             count++;
        //             if (count == this.MaxColumns)
        //             {
        //                 count = 0;
        //                 ImGui.TableNextRow();
        //             }
        //         }
        //     }
        // }
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.SeedBagIcon;
}

public class ItemGardeningCrossbreedSourceUseRenderer : ItemGardeningCrossbreedSourceRenderer
{
    public override RendererType RendererType => RendererType.Use;
    public override string SingularName => "Gardening Crossbreed Seed";
    public override string HelpText => "Is this item part of a crossbreed when gardening?";
}