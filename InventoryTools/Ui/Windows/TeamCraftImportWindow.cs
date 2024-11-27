using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class TeamCraftImportWindow : GenericWindow
{
    private readonly ListImportExportService _importExportService;
    private string _importListItems = "";
    private bool _hasError;
    private List<(uint, uint)>? _parseResult;

    public TeamCraftImportWindow(ILogger<TeamCraftImportWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ListImportExportService importExportService, string name = "Teamcraft Import") : base(logger, mediator, imGuiService, configuration, name)
    {
        _importExportService = importExportService;
        Flags = ImGuiWindowFlags.NoCollapse;
    }

    public List<(uint, uint)>? ParseResult => _parseResult;


    public override string GenericKey { get; } = "tcimport";
    public override string GenericName { get; } = "Teamcraft Import";
    public override bool DestroyOnClose { get; }
    public override bool SaveState { get; } = false;
    public override Vector2? DefaultSize { get; } = new Vector2(300, 300);
    public override Vector2? MaxSize { get; }
    public override Vector2? MinSize { get; }
    public override void Initialize()
    {
    }

    public override void Draw()
    {
        ImGui.Text("Import to Craft List: ");
        ImGui.SameLine();
        ImGuiService.HelpMarker("Guide to importing lists.\r\n\r\n" +
                                "Step 1. Open a list on Teamcraft with the items you wish to craft.\r\n\r\n" +
                                "Step 2. Find the 'Items' \"Copy as Text\" button. You only want to copy the output items.\r\n\r\n" +
                                "Step 3. Paste into the text box below in this window.\r\n\r\n" +
                                "Step 4. Click import.");
        ImGui.Text("Paste text here");
        ImGui.InputTextMultiline("###FinalItems", ref _importListItems, 10000000, new Vector2(ImGui.GetContentRegionAvail().X, 100));


        if (ImGui.Button("Import"))
        {
            var importedList = _importExportService.FromTCString(_importListItems ?? "");
            if (importedList is not null)
            {
                Close();
                MediatorService.Publish(new TeamCraftDataImported(importedList));
            }

        }
        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            Close();
        }
    }

    public override void Invalidate()
    {
        
    }

    public override FilterConfiguration? SelectedConfiguration { get; }
}