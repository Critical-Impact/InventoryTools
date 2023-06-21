using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CriticalCommonLib;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;
using InventoryTools.Logic;
using OtterGui.Raii;

namespace InventoryTools.Ui;

public class TeamCraftImportWindow
{
    private string _windowName;
    private string _importListItems = "";
    private bool _hasError = false;
    private bool _hasResult = false;
    private List<(uint, uint)>? _parseResult = null;
    private bool _openImportWindow = false;

    public TeamCraftImportWindow(string windowName)
    {
        _windowName = windowName;
    }

    public bool OpenImportWindow
    {
        get => _openImportWindow;
        set
        {
            if (value != _openImportWindow && _openImportWindow == false)
            {
                _importListItems = "";
            }
            _openImportWindow = value;
        }
    }

    public bool HasError => _hasError;
    public bool HasResult => _hasResult;

    public List<(uint, uint)>? ParseResult => _parseResult;


    public void Draw()
    {
        if (!OpenImportWindow) return;
        bool isOpen = OpenImportWindow;
        using var id = ImRaii.PushId("TCImport");

        if (ImGui.Begin("Teamcraft Import", ref isOpen, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("List Name");
            ImGui.SameLine();
            ImGuiComponents.HelpMarker("Guide to importing lists.\r\n\r\n" +
                "Step 1. Open a list on Teamcraft with the items you wish to craft.\r\n\r\n" +
                "Step 2. Find the 'Items' \"Copy as Text\" button.\r\n\r\n" +
                "Step 3. Paste into the Items box in this window.\r\n\r\n" +
                "Step 4. Click import.");
            ImGui.Text("Items");
            ImGui.InputTextMultiline("###FinalItems", ref _importListItems, 10000000, new Vector2(ImGui.GetContentRegionAvail().X, 100));


            if (ImGui.Button("Import"))
            {
                var importedList = ParseImport();
                if (importedList is not null)
                {
                    isOpen = false;
                    _parseResult = importedList;
                    _hasResult = true;
                }
                else
                {
                    _hasError = false;
                }

            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                isOpen = false;
            }
            ImGui.End();
        }

        if (OpenImportWindow != isOpen)
        {
            OpenImportWindow = isOpen;
        }
    }

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

                    var itemEx = Service.ExcelCache.GetItemExSheet().FirstOrDefault(c => c.NameString == item, null);

                    if (itemEx != null && Service.ExcelCache.CanCraftItem(itemEx.RowId))
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