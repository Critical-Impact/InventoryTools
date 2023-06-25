using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Logic;
using OtterGui.Raii;

namespace InventoryTools.Ui;

public class InventoryHistoryWindow : Window
{
    private IEnumerable<IGrouping<uint, InventoryChange>> _changes;
    public InventoryHistoryWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
        _changes = UpdateChanges();
    }

    public InventoryHistoryWindow() : base("Inventory History Window")
    {
        _changes = UpdateChanges();
    }

    private IEnumerable<IGrouping<uint, InventoryChange>> UpdateChanges()
    {
        var changes = PluginService.InventoryHistory.GetHistory().GroupBy(c => c.ChangeSetId);
        return changes;
    }

    public override void Draw()
    {
        if (ImGui.Button("Refresh history"))
        {
            _changes = UpdateChanges();
        }
        using (var table = ImRaii.Table("InventoryHistory", 4 ,ImGuiTableFlags.None))
        {
            if (table.Success)
            {
                ImGui.TableNextColumn();
                ImGui.TableHeader("Change #");
                ImGui.TableNextColumn();
                ImGui.TableHeader("Reason");
                ImGui.TableNextColumn();
                ImGui.TableHeader("Date");
                ImGui.TableNextColumn();
                ImGui.TableHeader("Change");

                foreach (var changeSet in _changes)
                {
                    foreach (var change in changeSet)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("#" + change.ChangeSetId);
                        ImGui.PopTextWrapPos();
                        ImGui.TableNextColumn();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted(change.InventoryChangeReason.FormattedName());
                        ImGui.PopTextWrapPos();
                        ImGui.TableNextColumn();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted(change.ChangeDate?.ToString() ?? "Unknown");
                        ImGui.PopTextWrapPos();
                        ImGui.TableNextColumn();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted(change.GetFormattedChange());
                        ImGui.PopTextWrapPos();
                    }
                    ImGui.TableNextRow();
                }
            }
        }
    }

    public override void Invalidate()
    {
       
    }

    public override FilterConfiguration? SelectedConfiguration { get; } = null;
    public override string Key { get; } = AsKey;
    public static string AsKey = "inventoryhistory";
    public override bool DestroyOnClose { get; } = false;
    public override bool SaveState { get; } = true;
    public override Vector2 DefaultSize { get; } = new Vector2(600, 600);
    public override Vector2 MaxSize { get; } = new Vector2(2000, 2000);
    public override Vector2 MinSize { get; } = new Vector2(100, 100);
}