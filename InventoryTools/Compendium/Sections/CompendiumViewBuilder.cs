using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using Dalamud.Bindings.ImGui;
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
    private string title;
    private string? subtitle;
    private string? description;
    private uint icon;
    private List<(string Link, string HelpText, ISharedImmediateTexture texture)>? links;
    private List<ICompendiumViewSection>? sections;

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
        LevelViewSection.Factory levelViewFactory)
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
    }

    public string Title
    {
        get => title;
        set => title = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string? Subtitle
    {
        get => subtitle;
        set => subtitle = value;
    }

    public string? Description
    {
        get => description;
        set => description = value;
    }

    public uint Icon
    {
        get => icon;
        set => icon = value;
    }

    public void AddLink(string link, string helpText, uint iconId)
    {
        links ??= new();
        links.Add(new  (link, helpText, _textureProvider.GetFromGameIcon(iconId)));
    }

    public void AddLink(string link, string helpText, string imageName)
    {
        links ??= new();
        links.Add(new  (link, helpText, _imGuiService.LoadImage(imageName)));
    }

    public void AddSection(ICompendiumViewSection section)
    {
        sections ??= new();
        sections.Add(section);
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

    public void Draw(SectionState sectionState)
    {
        const float iconSize = 64f;
        const float iconTextPadding = 12f;

        var cursorStart = ImGui.GetCursorPos();

        ImGui.Image(_textureProvider.GetFromGameIcon(new GameIconLookup(Icon)).GetWrapOrEmpty().Handle, new Vector2(iconSize, iconSize));

        ImGui.SameLine(0, iconTextPadding);

        ImGui.BeginGroup();

        ImGui.TextUnformatted(title);

        if (!string.IsNullOrEmpty(subtitle))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
            ImGui.TextWrapped(subtitle);
            ImGui.PopStyleColor();
        }

        if (!string.IsNullOrEmpty(description))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.TextWrapped(description);
            ImGui.PopStyleColor();
        }

        ImGui.EndGroup();

        var textBlockHeight = ImGui.GetItemRectSize().Y;
        if (iconSize > textBlockHeight)
        {
            var offset = Math.Max(iconSize, textBlockHeight);
            ImGui.SetCursorPos(new Vector2(cursorStart.X, cursorStart.Y + offset));
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (links != null && links.Count != 0)
        {
            if (ImGui.CollapsingHeader("Links", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (var index = 0; index < links.Count; index++)
                {
                    var link = links[index];
                    if (ImGui.ImageButton(link.texture.GetWrapOrEmpty().Handle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        link.Link.OpenBrowser();
                    }

                    ImGuiUtil.HoverTooltip(link.HelpText);
                    if (index != links.Count - 1)
                    {
                        ImGui.SameLine();
                    }
                }
            }
        }

        if (sections != null && sections.Count != 0)
        {
            for (var index = 0; index < sections.Count; index++)
            {
                var section = sections[index];
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