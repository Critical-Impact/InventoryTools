using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib.Services.Mediator;


namespace InventoryTools.Logic.ItemRenderers;

public abstract class ItemInfoRenderer<T> : IItemInfoRenderer where T : ItemSource
{
    public Type ItemSourceType => typeof(T);
    public virtual IReadOnlyList<ItemInfoRenderCategory>? Categories => null;
    public abstract RendererType RendererType { get; }
    public abstract ItemInfoType Type { get; }
    public abstract string SingularName { get; }
    public virtual string? PluralName => null;
    public abstract string HelpText { get; }
    public abstract bool ShouldGroup { get; }
    public virtual Func<List<ItemSource>, List<List<ItemSource>>>? CustomGroup => null;
    public abstract Action<ItemSource> DrawTooltip { get; }
    public virtual Action<List<ItemSource>>? DrawTooltipGrouped => null;

    public virtual Func<ItemSource, List<MessageBase>?>? OnClick => null;
    public virtual Func<ItemSource, List<MessageBase>?>? OnRightClick => null;

    public virtual Func<ItemSource, List<MessageBase>>? DrawMenu => null;
    public abstract Func<ItemSource, string> GetName { get; }
    public abstract Func<ItemSource, int> GetIcon { get; }
    public virtual byte MaxColumns { get; set; } = 3;
    public virtual float TooltipChildWidth { get; set; } = 250;
    public virtual float TooltipChildHeight { get; set; } = 150;

    public T AsSource(ItemSource source)
    {
        return (T)source;
    }

    public List<T> AsSource(List<ItemSource> source)
    {
        return source.Cast<T>().ToList();
    }
}