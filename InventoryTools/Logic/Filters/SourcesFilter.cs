using System;
using System.Collections.Generic;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Filters.Abstract;

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

        public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.CraftFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
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
            foreach (var retainerCategories in configuration.SourceRetainerCategories)
            {
                foreach (var retainerCategory in retainerCategories.Value)
                {
                    if (allCharacters.ContainsKey(retainerCategories.Key) &&
                        retainerCategories.Key.ToString().StartsWith("3"))
                    {
                        var formattedName = allCharacters[retainerCategories.Key].FormattedName + " - " +
                                            retainerCategory.FormattedName();
                        sources.Add(formattedName);
                    }
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
            foreach (var characterCategories in configuration.SourceCharacterCategories)
            {
                foreach (var characterCategory in characterCategories.Value)
                {
                    if (allCharacters.ContainsKey(characterCategories.Key) &&
                        characterCategories.Key.ToString().StartsWith("1"))
                    {
                        var formattedName = allCharacters[characterCategories.Key].FormattedName + " - " +
                                            characterCategory.FormattedName();
                        sources.Add(formattedName);
                    }
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