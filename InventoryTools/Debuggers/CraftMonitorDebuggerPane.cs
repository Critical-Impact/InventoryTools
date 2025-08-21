using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Interfaces;
using CriticalCommonLib.Crafting;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Debuggers;

public class CraftMonitorDebuggerPane : IDebugPane
{
    private readonly ICraftMonitor _craftMonitor;
    private readonly ItemSheet _itemSheet;

    public CraftMonitorDebuggerPane(ICraftMonitor craftMonitor, ItemSheet itemSheet)
    {
        _craftMonitor = craftMonitor;
        _itemSheet = itemSheet;
    }
    public string Name =>  "Craft Monitor";
    public unsafe void Draw()
    {
        var craftMonitorAgent = _craftMonitor.Agent;
        var simpleCraftMonitorAgent = _craftMonitor.SimpleAgent;
        if (craftMonitorAgent != null)
        {
            ImGui.Text($"Craft Monitor Pointer: {(ulong)craftMonitorAgent.Agent:X}");
            ImGui.TextUnformatted("Is Trial Synthesis: " + craftMonitorAgent.IsTrialSynthesis);
            ImGui.TextUnformatted("Progress: " + craftMonitorAgent.Progress);
            ImGui.TextUnformatted("Total Progress Required: " +
                _craftMonitor.RecipeLevelTable?.ProgressRequired(_craftMonitor
                    .CurrentRecipe) ?? "Unknown");
            ImGui.TextUnformatted("Quality: " + craftMonitorAgent.Quality);
            ImGui.TextUnformatted("Status: " + craftMonitorAgent.Status);
            ImGui.TextUnformatted("Step: " + craftMonitorAgent.Step);
            ImGui.TextUnformatted("Durability: " + craftMonitorAgent.Durability);
            ImGui.TextUnformatted("HQ Chance: " + craftMonitorAgent.HqChance);
            ImGui.TextUnformatted("Item: " +
                                  (_itemSheet.GetRow(craftMonitorAgent.ResultItemId)
                                      ?.NameString ?? "Unknown"));
            ImGui.TextUnformatted(
                "Current Recipe: " + _craftMonitor.CurrentRecipe?.RowId ?? "Unknown");
            ImGui.TextUnformatted(
                "Recipe Difficulty: " + _craftMonitor.RecipeLevelTable?.Base.Difficulty ??
                "Unknown");
            ImGui.TextUnformatted(
                "Recipe Difficulty Factor: " +
                _craftMonitor.CurrentRecipe?.Base.DifficultyFactor ??
                "Unknown");
            ImGui.TextUnformatted(
                "Recipe Durability: " + _craftMonitor.RecipeLevelTable?.Base.Durability ??
                "Unknown");
            ImGui.TextUnformatted("Suggested Craftsmanship: " +
                _craftMonitor.RecipeLevelTable?.Base.SuggestedCraftsmanship ?? "Unknown");
            ImGui.TextUnformatted(
                "Current Craft Type: " + _craftMonitor.CraftType ?? "Unknown");
        }
        else if (simpleCraftMonitorAgent != null)
        {
            ImGui.Text($"Simple Craft Monitor Pointer: {(ulong)simpleCraftMonitorAgent.Agent:X}");
            ImGui.TextUnformatted("NQ Complete: " + simpleCraftMonitorAgent.NqCompleted);
            ImGui.TextUnformatted("HQ Complete: " + simpleCraftMonitorAgent.HqCompleted);
            ImGui.TextUnformatted("Failed: " + simpleCraftMonitorAgent.TotalFailed);
            ImGui.TextUnformatted("Total Completed: " + simpleCraftMonitorAgent.TotalCompleted);
            ImGui.TextUnformatted("Total: " + simpleCraftMonitorAgent.Total);
            ImGui.TextUnformatted("Item: " + _itemSheet
                .GetRowOrDefault(simpleCraftMonitorAgent.ResultItemId)?.NameString.ToString() ?? "Unknown");
            ImGui.TextUnformatted(
                "Current Recipe: " + _craftMonitor.CurrentRecipe?.RowId ?? "Unknown");
            ImGui.TextUnformatted(
                "Current Craft Type: " + _craftMonitor.CraftType ?? "Unknown");
        }
        else
        {
            ImGui.TextUnformatted("Not crafting.");
        }
    }
}