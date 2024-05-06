using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class TeamCraftImportWindow : GenericWindow
{
    private readonly ExcelCache _excelCache;
    private string _importListItems = "";
    private bool _hasError;
    private List<(uint, uint)>? _parseResult;

    public TeamCraftImportWindow(ILogger<TeamCraftImportWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ExcelCache excelCache, string name = "Teamcraft Import") : base(logger, mediator, imGuiService, configuration, name)
    {
        _excelCache = excelCache;
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
            var importedList = ParseImport();
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

    private List<(uint, uint)>? ParseImport()
    {
        if (string.IsNullOrEmpty(_importListItems)) return null;
        List<(uint, uint)> output = new List<(uint, uint)>();
        using (System.IO.StringReader reader = new System.IO.StringReader(_importListItems))
        {
            string line;
            while ((line = reader.ReadLine()!) != null)
            {
                var parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    continue;

                if (parts[0][^1] == 'x')
                {
                    int numberOfItem = int.Parse(parts[0].Substring(0, parts[0].Length - 1));
                    var builder = new StringBuilder();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        builder.Append(parts[i]);
                        builder.Append(" ");
                    }
                    var item = builder.ToString().Trim();

                    var itemEx = _excelCache.GetItemExSheet().FirstOrDefault(c => c!.NameString == item, null);

                    if (itemEx != null && _excelCache.CanCraftItem(itemEx.RowId))
                    {
                        output.Add((itemEx.RowId, (uint)numberOfItem));
                    }
                }

            }
        }

        if (output.Count == 0) return null;

        return output;
    }
}