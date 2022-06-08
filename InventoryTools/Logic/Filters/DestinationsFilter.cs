using System;
using System.Collections.Generic;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Style;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class DestinationsFilter : DisplayFilter
    {
        public override int Order { get; set; } = 2;
        public override string Key { get; set; } = "Destinations";
        public override string Name { get; set; } = "Destinations";

        public override string HelpText { get; set; } =
            "This lists all the destinations that are applicable given the destinations picked above.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        
        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return false;
        }

        public override FilterType AvailableIn { get; set; } = FilterType.SortingFilter;
        
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
            ImGui.Text("Destination Information: ");
            ImGui.SameLine();
            UiHelpers.HelpMarker(HelpText);
            var allCharacters = PluginService.CharacterMonitor.Characters;
            List<string> destinations = new();
            foreach (var retainerCategory in configuration.DestinationRetainerCategories)
            {
                if (allCharacters.ContainsKey(retainerCategory.Key))
                {
                    var formattedName = allCharacters[retainerCategory.Key].FormattedName + " - " +
                                        retainerCategory.Value.FormattedName();
                    destinations.Add(formattedName);
                }
            }
            ImGui.SetNextItemWidth(LabelSize);
            if (destinations.Count != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            }
            ImGui.LabelText("##" + Key + "RetainerLabel", "Retainer Destinations" + ":");
            if (destinations.Count != 0)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
            ImGui.Text(String.Join(", ", destinations));
            destinations = new();
            foreach (var characterCategory in configuration.DestinationCharacterCategories)
            {
                if (allCharacters.ContainsKey(characterCategory.Key))
                {
                    var formattedName = allCharacters[characterCategory.Key].FormattedName + " - " +
                                        characterCategory.Value.FormattedName();
                    destinations.Add(formattedName);
                }
            }
            ImGui.SetNextItemWidth(LabelSize);
            if (destinations.Count != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            }
            ImGui.LabelText("##" + Key + "CharacterLabel", "Character Destinations" + ":");
            if (destinations.Count != 0)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
            ImGui.Text(String.Join(", ", destinations));
        }
    }
}