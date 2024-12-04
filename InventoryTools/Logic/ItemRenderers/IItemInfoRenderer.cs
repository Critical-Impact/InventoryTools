using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Services.Mediator;


namespace InventoryTools.Logic.ItemRenderers;

public interface IItemInfoRenderer
{
    Type ItemSourceType { get; }
    IReadOnlyList<ItemInfoRenderCategory>? Categories { get; }
    RendererType RendererType { get; }
    ItemInfoType Type { get; }
    string SingularName { get; }
    string? PluralName { get; }
    string HelpText { get; }
    bool ShouldGroup { get; }
    Func<List<ItemSource>, List<List<ItemSource>>>? CustomGroup { get; }
    Action<ItemSource> DrawTooltip { get; }
    Action<List<ItemSource>>? DrawTooltipGrouped { get; }
    Func<ItemSource, List<MessageBase>?>? OnClick { get; }
    Func<ItemSource, List<MessageBase>?>? OnRightClick { get; }
    Func<ItemSource, List<MessageBase>>? DrawMenu { get; }
    Func<ItemSource, string> GetName { get; }
    Func<ItemSource, int> GetIcon { get; }
    byte MaxColumns { get; }
    float TooltipChildWidth { get; }
    float TooltipChildHeight { get; }
}