using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Logic;
using OtterGui.Raii;

namespace InventoryTools.Sections
{
    public class CharacterRetainerPage : IConfigPage
    {
        private bool _isSeparator;
        public string Name { get; } = "Characters/Retainers";
        
        public void Draw()
        {
            if (ImGui.CollapsingHeader("Characters", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
                if (ImGui.BeginTable("CharacterTable", 5, ImGuiTableFlags.BordersV |
                                                             ImGuiTableFlags.BordersOuterV |
                                                             ImGuiTableFlags.BordersInnerV |
                                                             ImGuiTableFlags.BordersH |
                                                             ImGuiTableFlags.BordersOuterH |
                                                             ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.Resizable))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 0);
                    ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 1);
                    ImGui.TableSetupColumn("Free Company", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 2);
                    ImGui.TableSetupColumn("Display Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 3);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 4);
                    ImGui.TableHeadersRow();
                    var characters = PluginService.CharacterMonitor.GetPlayerCharacters();
                    if (characters.Length == 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TextUnformatted("No characters available.");
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                    }

                    for (var index = 0; index < characters.Length; index++)
                    {
                        ImGui.TableNextRow();
                        var character = characters[index].Value;
                        ImGui.TableNextColumn();
                        if (character.Name != "")
                        {
                            ImGui.TextUnformatted(character.Name);
                            ImGui.SameLine();
                        }

                        ImGui.TableNextColumn();
                        if (character.WorldId != 0)
                        {
                            ImGui.TextUnformatted(character.World?.Name ?? "Unknown");
                            ImGui.SameLine();
                        }

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(character.FreeCompanyName);

                        ImGui.TableNextColumn();
                        var value = character.AlternativeName ?? "";
                        if (ImGui.InputText("##"+index+"Input", ref value, 150))
                        {
                            if (value == "")
                            {
                                character.AlternativeName = null;
                                PluginService.CharacterMonitor.UpdateCharacter(character);
                            }
                            else
                            {
                                character.AlternativeName = value;
                                PluginService.CharacterMonitor.UpdateCharacter(character);
                            }
                        }
                        ImGui.TableNextColumn();
                        if (character.CharacterId != PluginService.CharacterMonitor.ActiveCharacterId)
                        {
                            if (ImGui.SmallButton("Remove##" + index))
                            {
                                PluginService.CharacterMonitor.RemoveCharacter(character.CharacterId);
                            }

                        }

                        if (ImGui.SmallButton("Clear All Bags##" + index))
                        {
                            ImGui.OpenPopup("Are you sure?##" + index);
                        }
                        if (ImGui.BeginPopupModal("Are you sure?##" + index))
                        {
                            ImGui.TextUnformatted(
                                "Are you sure you want to clear all the bags stored for this character?.\nThis operation cannot be undone!\n\n");
                            ImGui.Separator();

                            if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                            {
                                PluginService.InventoryMonitor.ClearCharacterInventories(character.CharacterId);
                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.SetItemDefaultFocus();
                            ImGui.SameLine();
                            if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                            {
                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.EndPopup();
                        }
                    }

                    ImGui.EndTable();
                }

                ImGui.PopStyleVar();
            }
            ImGui.NewLine();
            if (ImGui.CollapsingHeader("Free Companies", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                RenderFreeCompanyTable();
            }
            ImGui.NewLine();
            if (ImGui.CollapsingHeader("Houses", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                RenderHouseTable();
            }
            ImGui.NewLine();
            if (ImGui.CollapsingHeader("Retainers", ImGuiTreeNodeFlags.DefaultOpen))
            {
                using (var tabBar = ImRaii.TabBar("Retainers", ImGuiTabBarFlags.TabListPopupButton))
                {
                    if (tabBar.Success)
                    {
                        foreach (var retainerGroup in PluginService.CharacterMonitor.GetRetainerCharacters().GroupBy(c => c.Value.OwnerId))
                        {
                            var mainCharacter = PluginService.CharacterMonitor.GetCharacterById(retainerGroup.Key);
                            if (mainCharacter != null)
                            {
                                using (var tabItem = ImRaii.TabItem(mainCharacter.FormattedName))
                                {
                                    if (tabItem.Success)
                                    {
                                        RenderRetainerTable(retainerGroup.ToList());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void RenderFreeCompanyTable()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.BeginTable("FreeCompanyTable", 4, ImGuiTableFlags.BordersV |
                                                        ImGuiTableFlags.BordersOuterV |
                                                        ImGuiTableFlags.BordersInnerV |
                                                        ImGuiTableFlags.BordersH |
                                                        ImGuiTableFlags.BordersOuterH |
                                                        ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)0);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)1);
                ImGui.TableSetupColumn("Display Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)3);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)4);
                ImGui.TableHeadersRow();
                var characters = PluginService.CharacterMonitor.GetFreeCompanies();
                if (characters.Length == 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TextUnformatted("No free companies available.");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                }

                for (var index = 0; index < characters.Length; index++)
                {
                    ImGui.TableNextRow();
                    var character = characters[index].Value;
                    ImGui.TableNextColumn();
                    if (character.Name != "")
                    {
                        ImGui.TextUnformatted(character.Name);
                        ImGui.SameLine();
                    }

                    ImGui.TableNextColumn();
                    if (character.WorldId != 0)
                    {
                        ImGui.TextUnformatted(character.World?.Name ?? "Unknown");
                        ImGui.SameLine();
                    }

                    ImGui.TableNextColumn();
                    var value = character.AlternativeName ?? "";
                    if (ImGui.InputText("##" + index + "Input", ref value, 150))
                    {
                        if (value == "")
                        {
                            character.AlternativeName = null;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                        else
                        {
                            character.AlternativeName = value;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                    }

                    ImGui.TableNextColumn();
                    if (character.CharacterId != PluginService.CharacterMonitor.ActiveCharacterId)
                    {
                        if (ImGui.SmallButton("Remove##" + index))
                        {
                            PluginService.CharacterMonitor.RemoveCharacter(character.CharacterId);
                        }

                    }

                    if (ImGui.SmallButton("Clear All Bags##" + index))
                    {
                        ImGui.OpenPopup("Are you sure?##" + index);
                    }

                    if (ImGui.BeginPopupModal("Are you sure?##" + index))
                    {
                        ImGui.TextUnformatted(
                            "Are you sure you want to clear all the bags stored for this free company?.\nThis operation cannot be undone!\n\n");
                        ImGui.Separator();

                        if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            PluginService.InventoryMonitor.ClearCharacterInventories(character.CharacterId);
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.SetItemDefaultFocus();
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.EndPopup();
                    }
                }

                ImGui.EndTable();
            }
            ImGui.PopStyleVar();
        }
        private static void RenderHouseTable()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.BeginTable("HouseTable", 6, ImGuiTableFlags.BordersV |
                                                        ImGuiTableFlags.BordersOuterV |
                                                        ImGuiTableFlags.BordersInnerV |
                                                        ImGuiTableFlags.BordersH |
                                                        ImGuiTableFlags.BordersOuterH |
                                                        ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)0);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)1);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)2);
                ImGui.TableSetupColumn("Owners", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)3);
                ImGui.TableSetupColumn("Display Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)4);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)5);
                ImGui.TableHeadersRow();
                var houses = PluginService.CharacterMonitor.GetCharacterHouses();
                if (houses.Length == 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TextUnformatted("No houses available.");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                }

                for (var index = 0; index < houses.Length; index++)
                {
                    ImGui.TableNextRow();
                    var character = houses[index].Value;
                    ImGui.TableNextColumn();
                    if (character.HousingName != "")
                    {
                        ImGui.TextUnformatted(character.HousingName);
                        ImGui.SameLine();
                    }

                    ImGui.TableNextColumn();
                    if (character.WorldId != 0)
                    {
                        ImGui.TextUnformatted(character.World?.Name ?? "Unknown");
                        ImGui.SameLine();
                    }
                    
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(character.GetPlotSize().ToString());
                    ImGui.SameLine();

                    ImGui.TableNextColumn();
                    if (character.Owners.Count != 0)
                    {
                        ImGui.TextUnformatted(String.Join(",",character.Owners.Select(c => PluginService.CharacterMonitor.GetCharacterById(c)).Where(c => c != null).Select(c => c!.FormattedName).ToList()));
                        ImGui.SameLine();
                    }
                    
                    ImGui.TableNextColumn();
                    var value = character.AlternativeName ?? "";
                    if (ImGui.InputText("##" + index + "Input", ref value, 150))
                    {
                        if (value == "")
                        {
                            character.AlternativeName = null;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                        else
                        {
                            character.AlternativeName = value;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                    }

                    ImGui.TableNextColumn();
                    if (character.CharacterId != PluginService.CharacterMonitor.ActiveCharacterId)
                    {
                        if (ImGui.SmallButton("Remove##" + index))
                        {
                            PluginService.CharacterMonitor.RemoveCharacter(character.CharacterId);
                        }

                        ImGui.SameLine();
                    }

                    if (ImGui.SmallButton("Clear All Bags##" + index))
                    {
                        ImGui.OpenPopup("Are you sure?##" + index);
                    }

                    if (ImGui.BeginPopupModal("Are you sure?##" + index))
                    {
                        ImGui.TextUnformatted(
                            "Are you sure you want to clear all storage for this house?.\nThis operation cannot be undone!\n\n");
                        ImGui.Separator();

                        if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            PluginService.InventoryMonitor.ClearCharacterInventories(character.CharacterId);
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.SetItemDefaultFocus();
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.EndPopup();
                    }
                }

                ImGui.EndTable();
            }
            ImGui.PopStyleVar();
        }

        public void RenderRetainerTable(List<KeyValuePair<ulong, Character>> retainers)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.BeginTable("RetainerTable", 8, ImGuiTableFlags.BordersV |
                                                         ImGuiTableFlags.BordersOuterV |
                                                         ImGuiTableFlags.BordersInnerV |
                                                         ImGuiTableFlags.BordersH |
                                                         ImGuiTableFlags.BordersOuterH |
                                                         ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Hire Order", ImGuiTableColumnFlags.WidthStretch, 30.0f, (uint) 0);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 70.0f, (uint) 1);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthStretch, 70.0f, (uint) 2);
                ImGui.TableSetupColumn("Gil", ImGuiTableColumnFlags.WidthStretch, 30.0f, (uint) 3);
                ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthStretch, 40.0f, (uint) 4);
                ImGui.TableSetupColumn("Owner", ImGuiTableColumnFlags.WidthStretch, 60.0f, (uint) 5);
                ImGui.TableSetupColumn("Display Name", ImGuiTableColumnFlags.WidthStretch, 80.0f, (uint) 6);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 80.0f, (uint) 7);
                ImGui.TableHeadersRow();
                if (retainers.Count == 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TextUnformatted("No retainers available.");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                }

                for (var index = 0; index < retainers.Count; index++)
                {
                    ImGui.TableNextRow();
                    var character = retainers[index].Value;
                    
                    ImGui.TableNextColumn();
                    {
                        ImGui.TextUnformatted((character.HireOrder + 1).ToString());
                        ImGui.SameLine();
                    }
                    
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(character.Name);
                    ImGui.SameLine();
                    
                    ImGui.TableNextColumn();
                    if (character.WorldId != 0)
                    {
                        ImGui.TextUnformatted(character.World?.Name ?? "Unknown");
                        ImGui.SameLine();
                    }
                    
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(character.Gil.ToString());
                    ImGui.SameLine();
                    
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(character.Level.ToString());
                    ImGui.SameLine();
                    
                    ImGui.TableNextColumn();
                    var characterName = "Unknown";
                    if (PluginService.CharacterMonitor.Characters.ContainsKey(character.OwnerId))
                    {
                        var owner = PluginService.CharacterMonitor.Characters[character.OwnerId];
                        characterName = owner.FormattedName;
                    }

                    ImGui.TextUnformatted(characterName);
                    
                    ImGui.TableNextColumn();
                    var value = character.AlternativeName ?? "";
                    if (ImGui.InputText("##"+index+"Input", ref value, 150))
                    {
                        if (value == "")
                        {
                            character.AlternativeName = null;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                        else
                        {
                            character.AlternativeName = value;
                            PluginService.CharacterMonitor.UpdateCharacter(character);
                        }
                    }
                    ImGui.TableNextColumn();
                    if (character.CharacterId != PluginService.CharacterMonitor.LocalContentId)
                    {
                        if (ImGui.SmallButton("Remove##" + index))
                        {
                            PluginService.CharacterMonitor.RemoveCharacter(character.CharacterId);
                        }

                    }

                    if (ImGui.SmallButton("Clear All Bags##" + index))
                    {
                        ImGui.OpenPopup("Are you sure?##" + index);
                    }
                    if (ImGui.BeginPopupModal("Are you sure?##" + index))
                    {
                        ImGui.TextUnformatted(
                            "Are you sure you want to clear all the bags stored for this retainer?.\nThis operation cannot be undone!\n\n");
                        ImGui.Separator();

                        if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            PluginService.InventoryMonitor.ClearCharacterInventories(character.CharacterId);
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.SetItemDefaultFocus();
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                        {
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.EndPopup();
                    }
                }

                ImGui.EndTable();
            }

            ImGui.PopStyleVar();            
        }

        public bool IsMenuItem => _isSeparator;
    }
}