using System;
using System.Collections.Generic;
using InventoryTools.Ui.Config.Blocks;

namespace InventoryTools.Ui.Config.Layouts;

public sealed class PageLayout : IConfigBlock
{
    public PageLayout(string key, string name, IReadOnlyList<IConfigBlock> children)
        : this(key, name, children, Array.Empty<PageLayout>())
    {
    }

    public PageLayout(string key, string name, IReadOnlyList<IConfigBlock> children,
        IReadOnlyList<PageLayout> subPages)
    {
        Key = key;
        Name = name;
        Children = children;
        SubPages = subPages;
    }

    public IReadOnlyList<PageLayout> SubPages { get; }

    public string Key { get; }

    public string Name { get; }
    public IReadOnlyList<IConfigBlock> Children { get; }

    public void Draw(ConfigDrawContext context)
    {
        foreach (var child in Children)
        {
            child.Draw(context);
        }
    }
}