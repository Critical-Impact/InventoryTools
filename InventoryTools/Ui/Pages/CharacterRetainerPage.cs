using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Sections
{
    public class CharacterRetainerPage : IConfigPage
    {
        public string Name { get; } = "Characters/Retainers";
        
        public void Draw()
        {
            if (ImGui.CollapsingHeader("Characters", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
                if (ImGui.BeginTable("CharacterTable", 3, ImGuiTableFlags.BordersV |
                                                             ImGuiTableFlags.BordersOuterV |
                                                             ImGuiTableFlags.BordersInnerV |
                                                             ImGuiTableFlags.BordersH |
                                                             ImGuiTableFlags.BordersOuterH |
                                                             ImGuiTableFlags.BordersInnerH))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 0);
                    ImGui.TableSetupColumn("Display Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 1);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 2);
                    ImGui.TableHeadersRow();
                    var characters = PluginService.CharacterMonitor.GetPlayerCharacters();
                    if (characters.Length == 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.Text("No characters available.");
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
                            ImGui.Text(character.Name);
                            ImGui.SameLine();
                        }

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
                        if (character.CharacterId != Service.ClientState.LocalContentId)
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
                            ImGui.Text(
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
            if (ImGui.CollapsingHeader("Retainers", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
                if (ImGui.BeginTable("RetainerTable", 7, ImGuiTableFlags.BordersV |
                                                             ImGuiTableFlags.BordersOuterV |
                                                             ImGuiTableFlags.BordersInnerV |
                                                             ImGuiTableFlags.BordersH |
                                                             ImGuiTableFlags.BordersOuterH |
                                                             ImGuiTableFlags.BordersInnerH))
                {
                    ImGui.TableSetupColumn("Hire Order", ImGuiTableColumnFlags.WidthStretch, 30.0f, (uint) 0);
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 70.0f, (uint) 1);
                    ImGui.TableSetupColumn("Gil", ImGuiTableColumnFlags.WidthStretch, 30.0f, (uint) 2);
                    ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthStretch, 40.0f, (uint) 3);
                    ImGui.TableSetupColumn("Owner", ImGuiTableColumnFlags.WidthStretch, 60.0f, (uint) 4);
                    ImGui.TableSetupColumn("Display Name", ImGuiTableColumnFlags.WidthStretch, 80.0f, (uint) 5);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 80.0f, (uint) 6);
                    ImGui.TableHeadersRow();
                    var retainers = PluginService.CharacterMonitor.GetRetainerCharacters().OrderBy(c => c.Value.HireOrder).ToList();
                    if (retainers.Count == 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.Text("No retainers available.");
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
                            ImGui.Text((character.HireOrder + 1).ToString());
                            ImGui.SameLine();
                        }
                        
                        ImGui.TableNextColumn();
                        ImGui.Text(character.Name);
                        ImGui.SameLine();
                        
                        ImGui.TableNextColumn();
                        ImGui.Text(character.Gil.ToString());
                        ImGui.SameLine();
                        
                        ImGui.TableNextColumn();
                        ImGui.Text(character.Level.ToString());
                        ImGui.SameLine();
                        
                        ImGui.TableNextColumn();
                        var characterName = "Unknown";
                        if (PluginService.CharacterMonitor.Characters.ContainsKey(character.OwnerId))
                        {
                            var owner = PluginService.CharacterMonitor.Characters[character.OwnerId];
                            characterName = owner.FormattedName;
                        }

                        ImGui.Text(characterName);
                        
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
                        if (character.CharacterId != Service.ClientState.LocalContentId)
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
                            ImGui.Text(
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
        }
    }
}