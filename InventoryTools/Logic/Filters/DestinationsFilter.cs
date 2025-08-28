using System;
using System.Collections.Generic;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using InventoryTools.Lists;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class DestinationsFilter : DisplayFilter
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly CraftDestinationInventoriesFilter _craftDestinationInventoriesFilter;
        private readonly DestinationInventoriesFilter _destinationInventoriesFilter;
        private readonly InventoryScopeCalculator _scopeCalculator;

        public DestinationsFilter(ILogger<DestinationsFilter> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, CraftDestinationInventoriesFilter craftDestinationInventoriesFilter, DestinationInventoriesFilter destinationInventoriesFilter, InventoryScopeCalculator scopeCalculator) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
            _craftDestinationInventoriesFilter = craftDestinationInventoriesFilter;
            _destinationInventoriesFilter = destinationInventoriesFilter;
            _scopeCalculator = scopeCalculator;
        }
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

        public override FilterType AvailableIn { get; set; } = FilterType.SortingFilter | FilterType.CraftFilter;

        public override void Draw(FilterConfiguration configuration)
        {
            ImGui.NewLine();
            ImGui.Text("Destination Information: ");
            ImGui.SameLine();
            ImGuiService.HelpMarker(GetHelpText(configuration));
            var allCharacters = _characterMonitor.Characters;

            //Retainers
            List<string> destinations = new();
            var destinationInventories = _destinationInventoriesFilter.CurrentValue(configuration);
            if (configuration.FilterType == FilterType.CraftFilter)
            {
                destinationInventories = _craftDestinationInventoriesFilter.CurrentValue(configuration);
            }

            if (destinationInventories != null)
            {
                foreach (var retainer in allCharacters)
                {
                    foreach (var category in Enum.GetValues<InventoryCategory>())
                    {

                        if (retainer.Value.CharacterType != CharacterType.Retainer ||
                            !category.IsRetainerCategory())
                        {
                            continue;
                        }
                        if (_scopeCalculator.Filter(destinationInventories, retainer.Key, category))
                        {
                            var formattedName = retainer.Value.FormattedName + " - " + category.FormattedName();
                            destinations.Add(formattedName);
                        }
                    }
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
            ImGui.TextWrapped(String.Join(", ", destinations));

            //Characters
            destinations = new();
            if (destinationInventories != null)
            {
                foreach (var retainer in allCharacters)
                {
                    foreach (var category in Enum.GetValues<InventoryCategory>())
                    {
                        if (retainer.Value.CharacterType != CharacterType.Character ||
                            !category.IsCharacterCategory())
                        {
                            continue;
                        }
                        if (_scopeCalculator.Filter(destinationInventories, retainer.Key, category))
                        {
                            var formattedName = retainer.Value.FormattedName + " - " + category.FormattedName();
                            destinations.Add(formattedName);
                        }
                    }
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
            ImGui.TextWrapped(String.Join(", ", destinations));

            //Free Companies
            destinations = new();
            if (destinationInventories != null)
            {
                foreach (var retainer in allCharacters)
                {
                    foreach (var category in Enum.GetValues<InventoryCategory>())
                    {

                        if (retainer.Value.CharacterType != CharacterType.FreeCompanyChest ||
                            !category.IsFreeCompanyCategory())
                        {
                            continue;
                        }
                        if (_scopeCalculator.Filter(destinationInventories, retainer.Key, category))
                        {
                            var formattedName = retainer.Value.FormattedName + " - " + category.FormattedName();
                            destinations.Add(formattedName);
                        }
                    }
                }
            }

            ImGui.SetNextItemWidth(LabelSize);
            if (destinations.Count != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            }
            ImGui.LabelText("##" + Key + "CharacterLabel", "Free Company Destinations" + ":");
            if (destinations.Count != 0)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
            ImGui.TextWrapped(String.Join(", ", destinations));

            //Houses
            destinations = new();
            if (destinationInventories != null)
            {
                foreach (var retainer in allCharacters)
                {
                    foreach (var category in Enum.GetValues<InventoryCategory>())
                    {

                        if (retainer.Value.CharacterType != CharacterType.Housing ||
                            !category.IsHousingCategory())
                        {
                            continue;
                        }
                        if (_scopeCalculator.Filter(destinationInventories, retainer.Key, category))
                        {
                            var formattedName = retainer.Value.FormattedName + " - " + category.FormattedName();
                            destinations.Add(formattedName);
                        }
                    }
                }
            }

            ImGui.SetNextItemWidth(LabelSize);
            if (destinations.Count != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            }
            ImGui.LabelText("##" + Key + "CharacterLabel", "Free Company Destinations" + ":");
            if (destinations.Count != 0)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
            ImGui.TextWrapped(String.Join(", ", destinations));
        }
    }
}