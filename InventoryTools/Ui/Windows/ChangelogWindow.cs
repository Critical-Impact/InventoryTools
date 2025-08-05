using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using SemVersion;

namespace InventoryTools.Ui;

public class ChangelogWindow : GenericWindow, IMenuWindow
{
    private readonly ChangelogService _changelogService;
    private SemanticVersion? scrollTo;
    private SemanticVersion? viewingVersion;

    public ChangelogWindow(ILogger<ChangelogWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ChangelogService changelogService) : base(logger, mediator, imGuiService, configuration, "Changelog")
    {
        _changelogService = changelogService;
    }

    public override void Draw()
    {
        ImGui.SetNextWindowSize(new Vector2(600, 400), ImGuiCond.FirstUseEver);
        ImGui.Begin("Changelog");

        // Sidebar and Main Panel
        var sidebarWidth = 150f;
        using (var sideBar = ImRaii.Child("Sidebar", new Vector2(sidebarWidth, 0), true))
        {
            if (sideBar)
            {
                using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(4, 6));
                int? currentGroup = null;
                foreach (var changelog in _changelogService.ChangeLogs.Select(c => c.Item1).OrderByDescending(c => c))
                {
                    var minorGroup = changelog.Major;

                    if (currentGroup != minorGroup)
                    {
                        if (currentGroup != null)
                            ImGui.Separator();

                        ImGui.TextColored(new Vector4(1f, 0.8f, 0.3f, 1f), $"1.{minorGroup}.x");
                        currentGroup = minorGroup;
                    }

                    bool isSelected = viewingVersion == changelog;

                    using var color = ImRaii.PushColor(ImGuiCol.Text, new Vector4(1f, 0.8f, 0.3f, 1f), isSelected);

                    if (ImGui.Selectable("  1." + changelog, isSelected))
                    {
                        scrollTo = changelog;
                    }
                }
            }
        }

        ImGui.SameLine();

        using (var main = ImRaii.Child("Main", new Vector2(0, 0), true))
        {
            if (main)
            {
                bool currentVersionSet = false;
                foreach (var changelog in _changelogService.ChangeLogs)
                {
                    if (ImGui.CollapsingHeader("1." + changelog.Item1.ToString(), ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen))
                    {
                        if (ImGui.IsItemVisible() && !currentVersionSet)
                        {
                            viewingVersion = changelog.Item1;
                            currentVersionSet = true;
                        }
                        if (scrollTo != null && scrollTo == changelog.Item1)
                        {
                            ImGui.SetScrollHereY(0);
                            scrollTo = null;
                        }
                        DrawSection("Added", changelog.Item3.Added);
                        DrawSection("Fixed", changelog.Item3.Fixed);
                        DrawSection("Changed", changelog.Item3.Changed);
                        DrawSection("Removed", changelog.Item3.Removed);
                    }
                }
            }
        }

        ImGui.End();
    }

    private void DrawSection(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        ImGui.Spacing();
        ImGui.TextColored(new Vector4(1f, 0.8f, 0.3f, 1f), $"{title}:");

        using var indent = ImRaii.PushIndent();

        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.TrimStart();

            if (trimmed.StartsWith("- "))
            {
                ImGui.Text("-");
                ImGui.SameLine();
                ImGui.PushTextWrapPos();
                ImGui.TextUnformatted(trimmed.Substring(2).Trim());
                ImGui.PopTextWrapPos();

            }
            else
            {
                ImGui.PushTextWrapPos();
                ImGui.TextUnformatted(trimmed);
                ImGui.PopTextWrapPos();
            }
        }

    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration { get; } = null;
    public override string GenericKey { get; } = "changelog";
    public override string GenericName { get; } = "Changelog";
    public override bool DestroyOnClose { get; } = true;
    public override bool SaveState { get; } = true;
    public override Vector2? DefaultSize { get; } = new Vector2(500, 500);
    public override Vector2? MaxSize { get; } = null;
    public override Vector2? MinSize { get; } = null;
    public override void Initialize()
    {

    }
}