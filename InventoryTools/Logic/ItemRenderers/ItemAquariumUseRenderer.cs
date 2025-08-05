using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemAquariumUseRenderer : ItemInfoRenderer<ItemAquariumSource>
{
    public ItemAquariumUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.Aquarium;
    public override string SingularName => "Aquarium";
    public override string PluralName => "Aquariums";
    public override string HelpText => "Can the item be placed in aquariums?";
    public override bool ShouldGroup => false;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var aquariumSource = AsSource(source);
        ImGui.Text("Size: " + aquariumSource.AquariumFish.Size);
        ImGui.Text("Water Type: " + aquariumSource.AquariumFish.Base.AquariumWater.Value.Name.ExtractText());
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var aquariumSource = AsSource(source);

        return "Aquarium: " + aquariumSource.AquariumFish.Base.AquariumWater.Value.Name.ExtractText() + " (" +
               aquariumSource.AquariumFish.Size + " )";
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.AquariumIcon;

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var aquariumSource = AsSource(source);
        //Open an aquarium window or something
        return null;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return $"Can be placed in {asSource.AquariumFish.Size} aquariums with {asSource.AquariumFish.Base.AquariumWater.Value.Name}";
    };
}