using System;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Mediator;
using InventoryTools.Ui.Config.Blocks;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config;

public abstract class LayoutBuilder : ILayout
{
    public abstract PageLayout Build();

    protected PageLayout Page(string key, string name, params IConfigBlock[] children)
    {
        return new PageLayout(key, name, children);
    }

    protected PageLayout PageGroup(string key, string name, params PageLayout[] subPages)
    {
        return new PageLayout(key, name, Array.Empty<IConfigBlock>(), subPages);
    }

    protected SectionBlock Section(string title, params IConfigBlock[] children)
    {
        return new SectionBlock(title, children);
    }

    protected CollapsibleBlock Collapsible(string title, params IConfigBlock[] children)
    {
        return new CollapsibleBlock(title, false, children);
    }

    protected CollapsibleBlock CollapsibleOpen(string title, params IConfigBlock[] children)
    {
        return new CollapsibleBlock(title, true, children);
    }

    protected ParagraphBlock Paragraph(string text)
    {
        return new ParagraphBlock(text);
    }

    protected BulletBlock Bullet(string text)
    {
        return new BulletBlock(text);
    }

    protected ImageBlock Image(string imageName, float width, float height)
    {
        return new ImageBlock(imageName, width, height);
    }

    protected LinkBlock Link(string label, string url)
    {
        return new LinkBlock(label, url);
    }

    protected ButtonBlock OpenWindow<TWindow>(string label) where TWindow : Window
    {
        return new ButtonBlock(label, () => new OpenGenericWindowMessage(typeof(TWindow)));
    }

    protected SettingBlock Setting<TSetting>() where TSetting : ISetting
    {
        return new SettingBlock(typeof(TSetting));
    }

    protected SettingBlock Setting<TSetting>(string nameOverride) where TSetting : ISetting
    {
        return new SettingBlock(typeof(TSetting), nameOverride);
    }

    protected ScrollableBlock Scrollable(string id, float height, params IConfigBlock[] children)
    {
        return new ScrollableBlock(id, height, children);
    }

    protected GatedBlock EnabledBy<TGate>(params IConfigBlock[] children)
        where TGate : Setting<bool>
    {
        return new GatedBlock(typeof(TGate), children);
    }
}