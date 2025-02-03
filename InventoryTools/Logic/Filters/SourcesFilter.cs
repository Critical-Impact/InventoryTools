using System;
using System.Collections.Generic;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Lists;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class SourcesFilter : DisplayFilter
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly ListCategoryService _listCategoryService;

        public SourcesFilter(ILogger<SourcesFilter> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, ListCategoryService listCategoryService) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
            _listCategoryService = listCategoryService;
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

        public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.CraftFilter | FilterType.HistoryFilter;

        public override void Draw(FilterConfiguration configuration)
        {
            ImGui.NewLine();
            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Text("Source Information: ");
            ImGui.SameLine();
            ImGuiService.HelpMarker(HelpText);
            var allCharacters = _characterMonitor.Characters;
            
            //Retainer Sources
            List<string> sources = new();
            foreach (var retainerCategories in _listCategoryService.SourceRetainerCategories(configuration))
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
            ImGui.TextWrapped(String.Join(", ", sources));
            
            //Character Sources
            sources = new();
            foreach (var characterCategories in _listCategoryService.SourceCharacterCategories(configuration))
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
            ImGui.TextWrapped(String.Join(", ", sources));

            //Free Company Sources
            sources = new();
            foreach (var characterCategories in _listCategoryService.SourceFreeCompanyCategories(configuration))
            {
                foreach (var characterCategory in characterCategories.Value)
                {
                    if (allCharacters.ContainsKey(characterCategories.Key))
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
            ImGui.LabelText("##" + Key + "CharacterLabel", "Free Company Sources" + ":");
            if (sources.Count != 0)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
            ImGui.TextWrapped(String.Join(", ", sources));

            //House Sources
            sources = new();
            foreach (var characterCategories in _listCategoryService.SourceHouseCategories(configuration))
            {
                foreach (var characterCategory in characterCategories.Value)
                {
                    if (allCharacters.TryGetValue(characterCategories.Key, out var character))
                    {
                        var formattedName = character.FormattedName + " - " +
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