using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Types;
using InventoryTools.Services;
using OtterGui;

namespace InventoryTools.Compendium.Sections;

public class CompendiumViewBuilder
{
    private readonly ICompendiumType _compendiumType;
    private readonly ITextureProvider _textureProvider;
    private readonly ImGuiService _imGuiService;
    private readonly CompendiumInfoTableSection.Factory _infoTableFactory;
    private readonly CompendiumItemListSection.Factory _itemListFactory;
    private readonly MapLinkViewSection.Factory _mapLinkViewFactory;
    private readonly MapLinksViewSection.Factory _mapLinksViewFactory;
    private readonly SingleRowRefSection.Factory _singleRowRefFactory;
    private readonly CollectionRowRefSection.Factory _collectionRowRefFactory;
    private readonly LevelViewSection.Factory _levelViewFactory;
    private readonly MetadataSection.Factory _metadataSectionFactory;
    private string _title;
    private string? _subtitle;
    private string? _description;
    private uint _icon;
    private List<(string Link, string HelpText, ISharedImmediateTexture texture)>? _links;
    private List<(string Tag, string HelpText, Func<Vector4>? color)>? _tags;
    private List<ICompendiumViewSection>? _sections;

    public delegate CompendiumViewBuilder Factory(ICompendiumType compendiumType);

    public CompendiumViewBuilder(ICompendiumType compendiumType,
        ITextureProvider textureProvider,
        ImGuiService imGuiService,
        CompendiumInfoTableSection.Factory infoTableFactory,
        CompendiumItemListSection.Factory itemListFactory,
        MapLinkViewSection.Factory mapLinkViewFactory,
        MapLinksViewSection.Factory mapLinksViewFactory,
        SingleRowRefSection.Factory singleRowRefFactory,
        CollectionRowRefSection.Factory collectionRowRefFactory,
        LevelViewSection.Factory levelViewFactory,
        MetadataSection.Factory metadataSectionFactory)
    {
        _compendiumType = compendiumType;
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
        _links ??= new();
        _links.Add(new  (link, helpText, _textureProvider.GetFromGameIcon(iconId)));
    }

    public void AddLink(string link, string helpText, string imageName)
    {
        _links ??= new();
        _links.Add(new  (link, helpText, _imGuiService.LoadImage(imageName)));
    }

    public void AddTag(string tag, string helpText, Func<Vector4>? color = null)
    {
        _tags ??= new();
        _tags.Add(new  (tag, helpText, color));
    }

    public void AddSection(ICompendiumViewSection section)
    {
        _sections ??= new();
        _sections.Add(section);
    }

    public void AddItemListSection(CompendiumItemListSectionOptions options)
    {
        AddSection(_itemListFactory.Invoke(options));
    }

    public void AddInfoTableSection(CompendiumInfoTableSectionOptions options)
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

    static void SameLineWrap(string nextText, float spacing = 6f)
    {
        var nextWidth = ImGui.CalcTextSize(nextText);
        var windowRight = ImGui.GetContentRegionAvail().X - nextWidth.X;
        var itemRight = ImGui.GetItemRectMax().X - ImGui.GetWindowPos().X;

        if (itemRight + spacing < windowRight)
            ImGui.SameLine(0, spacing);
    }

    public void Draw(SectionState sectionState)
    {
        const float iconSize = 64f;
        const float iconTextPadding = 12f;

        var cursorStart = ImGui.GetCursorPos();

        ImGui.Image(_textureProvider.GetFromGameIcon(new GameIconLookup(Icon)).GetWrapOrEmpty().Handle, new Vector2(iconSize, iconSize));

        ImGui.SameLine(0, iconTextPadding);

        ImGui.BeginGroup();

        ImGui.TextUnformatted(_title);

        if (!string.IsNullOrEmpty(_subtitle))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
            ImGui.TextWrapped(_subtitle);
            ImGui.PopStyleColor();
        }

        if (!string.IsNullOrEmpty(_description))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.TextWrapped(_description);
            ImGui.PopStyleColor();
        }

        ImGui.EndGroup();

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

                DrawTag($"tag{i}", tag.Tag, color);

                if (ImGui.IsItemHovered())
                {
                    using var tooltip = ImRaii.Tooltip();
                    if (tooltip)
                        ImGui.TextUnformatted(tag.HelpText);
                }

                if (i != _tags.Count - 1)
                    SameLineWrap(_tags[i + 1].Tag);
            }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (_links != null && _links.Count != 0)
        {
            if (ImGui.CollapsingHeader("Links", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (var index = 0; index < _links.Count; index++)
                {
                    var link = _links[index];
                    if (ImGui.ImageButton(link.texture.GetWrapOrEmpty().Handle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        link.Link.OpenBrowser();
                    }

                    ImGuiUtil.HoverTooltip(link.HelpText);
                    if (index != _links.Count - 1)
                    {
                        ImGui.SameLine();
                    }
                }
            }
        }

        if (_sections != null && _sections.Count != 0)
        {
            for (var index = 0; index < _sections.Count; index++)
            {
                var section = _sections[index];
                using (ImRaii.PushId("Section" + index))
                {
                    section.Draw(sectionState);
                }
            }
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