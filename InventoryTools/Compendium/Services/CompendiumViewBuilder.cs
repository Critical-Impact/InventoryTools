using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.Shared.Extensions;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Services;
using OtterGui;

namespace InventoryTools.Compendium.Services;

public class CompendiumViewBuilder
{
    private readonly ITextureProvider _textureProvider;
    private readonly ImGuiService _imGuiService;
    private readonly InfoTableSection.Factory _infoTableFactory;
    private readonly ItemListSection.Factory _itemListFactory;
    private readonly MapLinkViewSection.Factory _mapLinkViewFactory;
    private readonly MapLinksViewSection.Factory _mapLinksViewFactory;
    private readonly SingleRowRefSection.Factory _singleRowRefFactory;
    private readonly CollectionRowRefSection.Factory _collectionRowRefFactory;
    private readonly LevelViewSection.Factory _levelViewFactory;
    private readonly MetadataSection.Factory _metadataSectionFactory;
    private readonly ItemSourcesSection.Factory _itemSourcesSectionFactory;
    private readonly ItemFlowSection.Factory _itemFlowSectionFactory;
    private readonly LinksSection.Factory _linksSectionFactory;
    private readonly NestedSection.Factory _nestedSectionFactory;
    private string _title;
    private string? _subtitle;
    private string? _description;
    private uint _icon;
    private LinksSection? _linksSection;
    private LinksSectionOptions? _linksSectionOptions;
    private List<(Func<string> Tag, Func<string> HelpText, Func<Vector4>? color)>? _tags;
    private List<(string Title, string HelpText, Action action)>? _buttons;
    private List<ICompendiumViewSection>? _sections;
    private string? _draggedSectionKey;

    public delegate CompendiumViewBuilder Factory(ICompendiumType compendiumType);

    public CompendiumViewBuilder(ICompendiumType compendiumType,
        ITextureProvider textureProvider,
        ImGuiService imGuiService,
        InfoTableSection.Factory infoTableFactory,
        ItemListSection.Factory itemListFactory,
        MapLinkViewSection.Factory mapLinkViewFactory,
        MapLinksViewSection.Factory mapLinksViewFactory,
        SingleRowRefSection.Factory singleRowRefFactory,
        CollectionRowRefSection.Factory collectionRowRefFactory,
        LevelViewSection.Factory levelViewFactory,
        MetadataSection.Factory metadataSectionFactory,
        ItemSourcesSection.Factory itemSourcesSectionFactory,
        ItemFlowSection.Factory itemFlowSectionFactory,
        LinksSection.Factory linksSectionFactory,
        NestedSection.Factory nestedSectionFactory)
    {
        _textureProvider = textureProvider;
        _imGuiService = imGuiService;
        _infoTableFactory = infoTableFactory;
        _itemListFactory = itemListFactory;
        _mapLinkViewFactory = mapLinkViewFactory;
        _mapLinksViewFactory = mapLinksViewFactory;
        _singleRowRefFactory = singleRowRefFactory;
        _collectionRowRefFactory = collectionRowRefFactory;
        _levelViewFactory = levelViewFactory;
        _metadataSectionFactory = metadataSectionFactory;
        _itemSourcesSectionFactory = itemSourcesSectionFactory;
        _itemFlowSectionFactory = itemFlowSectionFactory;
        _linksSectionFactory = linksSectionFactory;
        _nestedSectionFactory = nestedSectionFactory;
    }

    public string Title
    {
        get => _title;
        set => _title = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string? Subtitle
    {
        get => _subtitle;
        set => _subtitle = value;
    }

    public string? Description
    {
        get => _description;
        set => _description = value;
    }

    public uint Icon
    {
        get => _icon;
        set => _icon = value;
    }

    public void AddLink(string link, string helpText, uint iconId)
    {
        if (_linksSectionOptions == null)
        {
            this._sections ??= [];
            _linksSectionOptions ??= new LinksSectionOptions()
            {
                SectionName = "Links",
                SectionKey = "links"
            };
            _linksSection ??= _linksSectionFactory.Invoke(_linksSectionOptions);
            this._sections = this._sections.Prepend(_linksSection).ToList();
        }

        _linksSectionOptions.Links.Add(new  (link, helpText, _textureProvider.GetFromGameIcon(iconId)));
    }

    public void AddLink(string link, string helpText, string imageName)
    {
        if (_linksSectionOptions == null)
        {
            this._sections ??= [];
            _linksSectionOptions ??= new LinksSectionOptions()
            {
                SectionName = "Links",
                SectionKey = "links"
            };
            _linksSection ??= _linksSectionFactory.Invoke(_linksSectionOptions);
            this._sections = this._sections.Prepend(_linksSection).ToList();
        }
        _linksSectionOptions.Links.Add(new  (link, helpText, _imGuiService.LoadImage(imageName)));
    }

    public void AddTag(Func<string> tag, Func<string> helpText, Func<Vector4>? color = null)
    {
        _tags ??= new();
        _tags.Add(new (tag, helpText, color));
    }

    public void AddButton(string title, string helpText, Action action)
    {
        _buttons ??= new();
        _buttons.Add(new(title, helpText, action));
    }

    public void AddSection(ICompendiumViewSection section)
    {
        _sections ??= new();
        _sections.Add(section);
    }

    public void AddItemListSection(ItemListSectionOptions options)
    {
        AddSection(_itemListFactory.Invoke(options));
    }

    public void AddInfoTableSection(InfoTableSectionOptions options)
    {
        AddSection(_infoTableFactory.Invoke(options));
    }

    public void AddMapLinkSectionSection(MapLinkViewSectionOptions options)
    {
        AddSection(_mapLinkViewFactory.Invoke(options));
    }

    public void AddMapLinksSectionSection(MapLinksViewSectionOptions options)
    {
        AddSection(_mapLinksViewFactory.Invoke(options));
    }

    public void AddSingleRowRefSection(SingleRowRefSectionOptions options)
    {
        AddSection(_singleRowRefFactory.Invoke(options));
    }

    public void AddCollectionRowRefSection(CollectionRowRefSectionOptions options)
    {
        AddSection(_collectionRowRefFactory.Invoke(options));
    }

    public void AddLevelMapLinkSection(LevelViewSectionOptions options)
    {
        AddSection(_levelViewFactory.Invoke(options));
    }

    public void AddMetadataSection(MetadataSectionOptions options)
    {
        AddSection(_metadataSectionFactory.Invoke(options));
    }

    public void AddItemSourcesSection(ItemSourcesSectionOptions options)
    {
        AddSection(_itemSourcesSectionFactory.Invoke(options));
    }

    public void AddItemFlowSection(ItemFlowSectionOptions options)
    {
        AddSection(_itemFlowSectionFactory.Invoke(options));
    }

    public ItemListSection CreateItemListSection(ItemListSectionOptions options)
    {
        return _itemListFactory.Invoke(options);
    }

    public void AddNestedSection(NestedSectionOptions options)
    {
        AddSection(_nestedSectionFactory.Invoke(options));
    }

    static void DrawTag(string id, string text, Vector4 color)
    {
        var padding = new Vector2(8, 3);
        var rounding = 8f;

        var textSize = ImGui.CalcTextSize(text);
        var size = textSize + padding * 2;

        ImGui.InvisibleButton(id, size);

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();

        var drawList = ImGui.GetWindowDrawList();

        drawList.AddRectFilled(
            min,
            max,
            ImGui.GetColorU32(ImGuiCol.FrameBg),
            rounding
        );

        drawList.AddText(
            min + padding,
            ImGui.GetColorU32(color),
            text
        );
    }

    private void DrawButton(string id, string buttonTitle, Action action)
    {
        var padding = new Vector2(8, 3);

        var textSize = ImGui.CalcTextSize(buttonTitle);
        var size = textSize + padding * 2;

        if (ImGui.InvisibleButton(id, size))
        {
            action();
        }

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();

        var drawList = ImGui.GetWindowDrawList();

        drawList.AddRectFilled(
            min,
            max,
            ImGui.GetColorU32(ImGuiCol.FrameBg),
            0f
        );

        drawList.AddText(
            min + padding,
            ImGui.GetColorU32(ImGuiCol.Text),
            buttonTitle
        );
    }

    static void SameLineWrap(string nextText, float spacing = 6f)
    {
        var nextWidth = ImGui.CalcTextSize(nextText);
        var windowRight = ImGui.GetContentRegionAvail().X - nextWidth.X;
        var itemRight = ImGui.GetItemRectMax().X - ImGui.GetWindowPos().X;

        if (itemRight + spacing < windowRight)
        {
            ImGui.SameLine(0, spacing);
        }
    }

    public void Draw(SectionState sectionState)
    {
        const float iconSize = 64f;
        const float iconTextPadding = 12f;

        var cursorStart = ImGui.GetCursorPos();

        ImGui.Image(_textureProvider.GetFromGameIcon(new GameIconLookup(Icon)).GetWrapOrEmpty().Handle, new Vector2(iconSize, iconSize));

        ImGui.SameLine(0, iconTextPadding);

        using (ImRaii.Group())
        {
            ImGui.TextUnformatted(_title);

            if (!string.IsNullOrEmpty(_subtitle))
            {
                using (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f)))
                {
                    ImGui.TextWrapped(_subtitle);
                }
            }

            if (!string.IsNullOrEmpty(_description))
            {
                using (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1.0f)))
                {
                    ImGui.TextWrapped(_description);
                }
            }
        }

        var textBlockHeight = ImGui.GetItemRectSize().Y;
        if (iconSize > textBlockHeight)
        {
            var offset = Math.Max(iconSize, textBlockHeight);
            ImGui.SetCursorPos(new Vector2(cursorStart.X, cursorStart.Y + offset));
        }

        if (_tags != null)
        {
            ImGui.NewLine();

            for (var i = 0; i < _tags.Count; i++)
            {
                var tag = _tags[i];

                var color = tag.color?.Invoke() ?? ImGuiColors.DalamudWhite;

                var tagText = tag.Tag();
                DrawTag($"tag{i}", tagText, color);

                if (ImGui.IsItemHovered())
                {
                    using (ImRaii.Tooltip())
                    {
                        ImGui.TextUnformatted(tag.HelpText());
                    }
                }

                if (i != _tags.Count - 1)
                {
                    SameLineWrap(_tags[i + 1].Tag());
                }
            }
        }

        if (_buttons != null)
        {
            ImGui.NewLine();

            for (var i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];

                var action = button.action;

                DrawButton($"button{i}", button.Title, action);

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    using (ImRaii.Tooltip())
                    {
                        ImGui.TextUnformatted(button.HelpText);
                    }
                }

                if (i != _buttons.Count - 1)
                {
                    SameLineWrap(_buttons[i + 1].Title);
                }
            }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (_sections != null && _sections.Count != 0)
        {
            var orderedSections = GetOrderedSections(sectionState);
            if (sectionState.EditMode)
            {
                DrawEditMode(sectionState, orderedSections);
            }
            else
            {
                DrawNormal(sectionState, orderedSections);
            }
        }
    }

    private List<ICompendiumViewSection> GetOrderedSections(SectionState sectionState)
    {
        var order = sectionState.SectionOrder;
        if (order == null || order.Count == 0 || _sections == null)
        {
            return _sections ?? new List<ICompendiumViewSection>();
        }

        var ordered = new List<ICompendiumViewSection>(order.Count);
        foreach (var key in order)
        {
            var section = _sections.FirstOrDefault(s => s.SectionOptions.SectionKey == key);
            if (section != null)
            {
                ordered.Add(section);
            }
        }
        foreach (var section in _sections)
        {
            if (!ordered.Contains(section))
            {
                ordered.Add(section);
            }
        }
        return ordered;
    }

    private void DrawNormal(SectionState sectionState, List<ICompendiumViewSection> sections)
    {
        for (var index = 0; index < sections.Count; index++)
        {
            var section = sections[index];
            if (!sectionState.IsSectionVisible(section.SectionOptions.SectionKey))
            {
                continue;
            }

            using (ImRaii.PushId("Section" + index))
            {
                section.Draw(sectionState);
            }
        }
    }

    private void DrawEditMode(SectionState sectionState, List<ICompendiumViewSection> sections)
    {
        for (var index = 0; index < sections.Count; index++)
        {
            var section = sections[index];
            var key = section.SectionOptions.SectionKey;
            var visible = sectionState.IsSectionVisible(key);

            using (ImRaii.PushId("EditSection" + index))
            {
                ImGui.Button("=");
                using (var source = ImRaii.DragDropSource())
                {
                    if (source)
                    {
                        _draggedSectionKey = key;
                        ImGui.SetDragDropPayload("##SectionReorder"u8, []);
                        ImGui.TextUnformatted("Moving: " + section.SectionOptions.SectionName);
                    }
                }

                using (var target = ImRaii.DragDropTarget())
                {
                    if (target)
                    {
                        if (ImGuiUtil.IsDropping("##SectionReorder") && _draggedSectionKey != null)
                        {
                            var newOrder = sections.Select(s => s.SectionOptions.SectionKey).ToList();
                            newOrder.Remove(_draggedSectionKey);
                            newOrder.Insert(index, _draggedSectionKey);
                            sectionState.SectionOrder = newOrder;
                            _draggedSectionKey = null;
                        }
                    }
                }

                ImGui.SameLine();

                var visibleCopy = visible;
                if (ImGui.Checkbox("##vis", ref visibleCopy))
                {
                    sectionState.SetSectionVisible(key, visibleCopy);
                }

                ImGui.SameLine();

                using (ImRaii.Disabled(!visible))
                {
                    ImGui.TextUnformatted(section.SectionOptions.SectionName);
                }
            }
            ImGui.Separator();
        }
    }

    public void SetupDefaults<T>(ICompendiumType<T> compendiumType, T row)
    {
        this.Title = compendiumType.GetName(row) ?? "Unknown";
        var rowSubtitle = compendiumType.GetSubtitle(row);
        if (rowSubtitle != null)
        {
            this.Subtitle = rowSubtitle;
        }

        var rowIcon = compendiumType.GetIcon(row);
        if (rowIcon.Item2 != null)
        {
            this.Icon = rowIcon.Item2.Value;
        }
    }
}