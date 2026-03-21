using System.Linq;
using System.Numerics;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Sections;

public class ItemSourcesSection : ViewSection
{
    private readonly ItemSourcesSectionOptions _options;
    private readonly ItemInfoRenderService _itemInfoRenderService;
    private readonly MediatorService _mediatorService;

    public delegate ItemSourcesSection Factory(ItemSourcesSectionOptions options);

    public ItemSourcesSection(ItemSourcesSectionOptions options, ItemInfoRenderService itemInfoRenderService, MediatorService mediatorService, ImGuiService imGuiService) : base(imGuiService)
    {
        _options = options;
        _itemInfoRenderService = itemInfoRenderService;
        _mediatorService = mediatorService;
    }

    public override string SectionName => _options.SectionName;
    public override void DrawSection(SectionState sectionState)
    {
        if (_options.Sources.Any())
        {
            _mediatorService.Publish(_itemInfoRenderService.DrawItemSourceIconsContainer("ItemSources",
                32 * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, new Vector2(32, 32),
                _options.Sources));
        }
    }
}