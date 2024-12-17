using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Services;
using OtterGui.Raii;

namespace InventoryTools.Logic;

public class ImGuiTooltipService
{

    private readonly IKeyState _keyState;
    private readonly ITextureProvider _textureProvider;
    private readonly ItemInfoRenderService _itemInfoRenderService;
    private readonly TryOn _tryOn;
    private readonly IChatUtilities _chatUtilities;

    public ImGuiTooltipService(
        IKeyState keyState,
        ITextureProvider textureProvider,
        ItemInfoRenderService itemInfoRenderService,
        TryOn tryOn,
        IChatUtilities chatUtilities)
    {
        _keyState = keyState;
        _textureProvider = textureProvider;
        _itemInfoRenderService = itemInfoRenderService;
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
                    ImGui.Image(this._textureProvider.GetFromGameIcon(new(item.Base.Icon)).GetWrapOrEmpty().ImGuiHandle, new Vector2(32, 32));
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


                    if (item.Sources.Count > 0)
                    {
                        ImGui.NewLine();
                        ImGui.TextUnformatted("Available From: ");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        var sources = item.Sources.Select(c => c.Type).Distinct().Select(
                                              c => this._itemInfoRenderService.GetSourceTypeName(c).Singular).Select(c => c!);
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
                                              c => this._itemInfoRenderService.GetUseTypeName(c).Singular).Select(c => c!);
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
}