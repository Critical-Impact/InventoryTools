using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using OtterGui.Raii;

namespace InventoryTools.Services;

public class ImGuiTooltipService
{
    private readonly IKeyState _keyState;
    private readonly ITextureProvider _textureProvider;
    private readonly TryOn _tryOn;
    private readonly IChatUtilities _chatUtilities;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public ItemInfoRenderService InfoRenderService { get; set; }

    public ImGuiTooltipService(
        IKeyState keyState,
        ITextureProvider textureProvider,
        TryOn tryOn,
        IChatUtilities chatUtilities)
    {
        _keyState = keyState;
        _textureProvider = textureProvider;
        _tryOn = tryOn;
        _chatUtilities = chatUtilities;
    }

    public void DrawItemTooltip(SearchResult searchResult)
    {
        //need to include setting inside this instead of other way around
        var item = searchResult.Item;
        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                if (this._keyState[VirtualKey.CONTROL])
                {
                    this._chatUtilities.LinkItem(searchResult.Item);
                }
                else if (this._keyState[VirtualKey.SHIFT])
                {
                    this._tryOn.TryOnItem(searchResult.Item);
                }
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("rMenu");
            }
            ImGui.SetNextWindowSizeConstraints(new Vector2(200,100), new Vector2(600,600));
            using (var tooltip = ImRaii.Tooltip())
            {
                if (tooltip)
                {
                    var availableWidth = ImGui.GetContentRegionAvail().X;
                    float imageStartX = availableWidth - 32;
                    ImGui.PushTextWrapPos(imageStartX);
                    ImGui.TextUnformatted(item.NameString);
                    ImGui.PopTextWrapPos();
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - 32);
                    ImGui.Image(this._textureProvider.GetFromGameIcon(new(item.Base.Icon)).GetWrapOrEmpty().Handle, new Vector2(32, 32));
                    ImGui.TextUnformatted(item.Base.ItemUICategory.Value.Name.ExtractText());
                    ImGui.Separator();
                    if (item.ClassJobCategory != null)
                    {
                        var classJobCategory = item.ClassJobCategory.Base.Name.ExtractText();
                        if (classJobCategory != string.Empty)
                        {
                            ImGui.TextUnformatted(classJobCategory);
                        }
                    }

                    if (searchResult.Item.Base.BaseParamValue.All(c => c == 0))
                    {
                        DrawBaseAttributes(item);
                    }
                    else
                    {

                        using (var table = ImRaii.Table("StatsTable", 2,
                                   ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg))
                        {
                            if (table)
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                {
                                    DrawBaseAttributes(item);
                                }

                                ImGui.TableSetColumnIndex(1);
                                {
                                    for (var index = 0; index < searchResult.Item.Base.BaseParam.Count; index++)
                                    {
                                        var baseParam = searchResult.Item.Base.BaseParam[index];
                                        if (baseParam.RowId == 0)
                                        {
                                            continue;
                                        }

                                        var baseParamValue = searchResult.Item.Base.BaseParamValue[index];
                                        if (baseParamValue == 0)
                                        {
                                            continue;
                                        }

                                        ImGui.Text(baseParam.Value.Name.ToImGuiString() + ": " +
                                                   baseParamValue);
                                    }

                                    if (searchResult.Item.Base.BaseParamValueSpecial.Any(c => c != 0))
                                    {
                                        ImGui.NewLine();
                                        ImGui.Separator();
                                        ImGui.Text("When HQ:");
                                        for (var index = 0; index < searchResult.Item.Base.BaseParamSpecial.Count; index++)
                                        {
                                            var baseParamSpecial = searchResult.Item.Base.BaseParamSpecial[index];
                                            if (baseParamSpecial.RowId == 0)
                                            {
                                                continue;
                                            }

                                            var baseParamValue = searchResult.Item.Base.BaseParamValueSpecial[index];
                                            if (baseParamValue == 0)
                                            {
                                                continue;
                                            }

                                            for (var baseParamIndex = 0; baseParamIndex < searchResult.Item.Base.BaseParam.Count; baseParamIndex++)
                                            {
                                                var baseParam = searchResult.Item.Base.BaseParam[baseParamIndex];

                                                if (baseParam.RowId == baseParamSpecial.RowId)
                                                {
                                                    baseParamValue += searchResult.Item.Base.BaseParamValue[baseParamIndex];
                                                }

                                            }

                                            ImGui.Text(baseParamSpecial.Value.Name.ToImGuiString() + ": " +
                                                       baseParamValue);
                                        }
                                    }

                                }
                            }
                        }
                    }

                    if (item.Sources.Count > 0)
                    {
                        ImGui.NewLine();
                        ImGui.TextUnformatted("Available From: ");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        var sources = item.Sources.Select(c => c.Type).Distinct().Select(
                                              c => this.InfoRenderService.GetSourceTypeName(c).Singular).Select(c => c!);
                        ImGui.TextUnformatted(string.Join(", ", sources));
                        ImGui.PopTextWrapPos();
                    }


                    if (item.Uses.Count > 0)
                    {
                        ImGui.NewLine();
                        ImGui.TextUnformatted("Used In: ");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        var uses = item.Uses.Select(c => c.Type).Distinct().Select(
                                              c => this.InfoRenderService.GetUseTypeName(c).Singular).Select(c => c!);
                        ImGui.TextUnformatted(string.Join(", ", uses));
                        ImGui.PopTextWrapPos();
                    }

                    ImGui.Separator();
                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
                    {
                        ImGui.TextUnformatted("Ctrl: Link");
                        if (item.CanTryOn)
                        {
                            ImGui.TextUnformatted("Shift: Try on");
                        }
                    }
                }
            }
        }
    }

    private static void DrawBaseAttributes(ItemRow item)
    {
        ImGui.TextUnformatted($"Item Level {item.Base.LevelItem.RowId}");
        if (item.ClassJobCategory != null)
        {
            ImGui.TextUnformatted($"Equip Level {item.Base.LevelEquip}");
        }

        ImGui.TextUnformatted(item.FormattedRarity);

        if (item.EquipRace != CharacterRace.Any && item.EquipRace != CharacterRace.None)
        {
            ImGui.TextUnformatted($"Only equippable by {item.EquipRace}");
        }

        if (item.EquippableByGender != CharacterSex.Both && item.EquippableByGender != CharacterSex.NotApplicable)
        {
            ImGui.TextUnformatted($"Only equippable by {item.EquippableByGender.ToString()}");
        }

        if (item.Base.CanBeHq)
        {
            ImGui.TextUnformatted("Can be HQ");
        }

        if (item.Base.IsUnique)
        {
            ImGui.TextUnformatted("Unique");
        }

        if (item.Base.IsUntradable)
        {
            ImGui.TextUnformatted("Untradable");
        }
    }
}