using System;
using System.Collections.Generic;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class SourcesFilter : DisplayFilter
    {
        public override int Order { get; set; } = 1;
        public override string Key { get; set; } = "Sources";
        public override string Name { get; set; } = "Sources";

        public override string HelpText { get; set; } =
            "This lists all the sources that are applicable given the sources picked above.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        
        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return false;
        }

        public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter;
        
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return true;
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            return true;
        }

        public override void Draw(FilterConfiguration configuration)
        {
            ImGui.NewLine();
            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Text("Source Information: ");
            ImGui.SameLine();
            UiHelpers.HelpMarker(HelpText);
            var allCharacters = PluginService.CharacterMonitor.Characters;
            List<string> sources = new();
            foreach (var retainerCategory in configuration.SourceRetainerCategories)
            {
                if (allCharacters.ContainsKey(retainerCategory.Key))
                {
                    var formattedName = allCharacters[retainerCategory.Key].FormattedName + " - " +
                                        retainerCategory.Value.FormattedName();
                    sources.Add(formattedName);
                }
            }
            ImGui.SetNextItemWidth(LabelSize);
            if (sources.Count != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            }
            ImGui.LabelText("##" + Key + "RetainerLabel", "Retainer Sources" + ":");
            if (sources.Count != 0)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
            ImGui.Text(String.Join(", ", sources));
            sources = new();
            foreach (var characterCategory in configuration.SourceCharacterCategories)
            {
                if (allCharacters.ContainsKey(characterCategory.Key))
                {
                    var formattedName = allCharacters[characterCategory.Key].FormattedName + " - " +
                                        characterCategory.Value.FormattedName();
                    sources.Add(formattedName);
                }
            }
            ImGui.SetNextItemWidth(LabelSize);
            if (sources.Count != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            }
            ImGui.LabelText("##" + Key + "CharacterLabel", "Character Sources" + ":");
            if (sources.Count != 0)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
            ImGui.Text(String.Join(", ", sources));
        }
    }
}