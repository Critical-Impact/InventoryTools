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
    public class SourcesFilter : DisplayFilter
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly CraftSourceInventoriesFilter _craftSourceInventoriesFilter;
        private readonly SourceInventoriesFilter _sourceInventoriesFilter;
        private readonly InventoryScopeCalculator _scopeCalculator;

        public SourcesFilter(ILogger<SourcesFilter> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, CraftSourceInventoriesFilter craftSourceInventoriesFilter, SourceInventoriesFilter sourceInventoriesFilter, InventoryScopeCalculator scopeCalculator) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
            _craftSourceInventoriesFilter = craftSourceInventoriesFilter;
            _sourceInventoriesFilter = sourceInventoriesFilter;
            _scopeCalculator = scopeCalculator;
        }
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

        public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.CraftFilter | FilterType.HistoryFilter | FilterType.GroupedList;

        public override void Draw(FilterConfiguration configuration)
        {
            ImGui.NewLine();
            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Text("Source Information: ");
            ImGui.SameLine();
            ImGuiService.HelpMarker(GetHelpText(configuration));
            var allCharacters = _characterMonitor.Characters;

            //Retainer Sources
            List<string> sources = new();
            var sourceInventories = _sourceInventoriesFilter.CurrentValue(configuration);
            if (configuration.FilterType == FilterType.CraftFilter)
            {
                sourceInventories = _craftSourceInventoriesFilter.CurrentValue(configuration);
            }


            if (sourceInventories != null)
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
                        if (_scopeCalculator.Filter(sourceInventories, retainer.Key, category))
                        {
                            var formattedName = retainer.Value.FormattedName + " - " + category.FormattedName();
                            sources.Add(formattedName);
                        }
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
            ImGui.TextWrapped(String.Join(", ", sources));

            //Character Sources
            sources = new();

            if (sourceInventories != null)
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
                        if (_scopeCalculator.Filter(sourceInventories, retainer.Key, category))
                        {
                            var formattedName = retainer.Value.FormattedName + " - " + category.FormattedName();
                            sources.Add(formattedName);
                        }
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
            ImGui.TextWrapped(String.Join(", ", sources));

            //Free Company Sources
            sources = new();

            if (sourceInventories != null)
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
                        if (_scopeCalculator.Filter(sourceInventories, retainer.Key, category))
                        {
                            var formattedName = retainer.Value.FormattedName + " - " + category.FormattedName();
                            sources.Add(formattedName);
                        }
                    }
                }
            }


            ImGui.SetNextItemWidth(LabelSize);
            if (sources.Count != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            }
            ImGui.LabelText("##" + Key + "CharacterLabel", "Free Company Sources" + ":");
            if (sources.Count != 0)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
            ImGui.TextWrapped(String.Join(", ", sources));

            //House Sources
            sources = new();

            if (sourceInventories != null)
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
                        if (_scopeCalculator.Filter(sourceInventories, retainer.Key, category))
                        {
                            var formattedName = retainer.Value.FormattedName + " - " + category.FormattedName();
                            sources.Add(formattedName);
                        }
                    }
                }
            }


            ImGui.SetNextItemWidth(LabelSize);
            if (sources.Count != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            }
            ImGui.LabelText("##" + Key + "CharacterLabel", "Housing Sources" + ":");
            if (sources.Count != 0)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
            ImGui.TextWrapped(String.Join(", ", sources));
        }
    }
}