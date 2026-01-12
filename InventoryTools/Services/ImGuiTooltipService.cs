using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using InventoryTools.Localizers;
using InventoryTools.Logic;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Settings;
using OtterGui.Extensions;
using OtterGui.Raii;

namespace InventoryTools.Services;

public class ImGuiTooltipService
{
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly InventoryScopeCalculator _inventoryScopeCalculator;
    private readonly IKeyState _keyState;
    private readonly ITextureProvider _textureProvider;
    private readonly ItemLocalizer _itemLocalizer;
    private readonly TryOn _tryOn;
    private readonly IChatUtilities _chatUtilities;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public ItemInfoRenderService InfoRenderService { get; set; }

    public ImGuiTooltipService(
        InventoryToolsConfiguration configuration,
        IInventoryMonitor inventoryMonitor,
        ICharacterMonitor characterMonitor,
        InventoryScopeCalculator inventoryScopeCalculator,
        IKeyState keyState,
        ITextureProvider textureProvider,
        ItemLocalizer itemLocalizer,
        TryOn tryOn,
        IChatUtilities chatUtilities)
    {
        _configuration = configuration;
        _inventoryMonitor = inventoryMonitor;
        _characterMonitor = characterMonitor;
        _inventoryScopeCalculator = inventoryScopeCalculator;
        _keyState = keyState;
        _textureProvider = textureProvider;
        _itemLocalizer = itemLocalizer;
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

                    var sortMode = _configuration.TooltipAmountOwnedSort;

                    var enumerable = _inventoryMonitor.AllItems.Where(item =>
                        item.ItemId == searchResult.ItemId &&
                        _characterMonitor.Characters.ContainsKey(item.RetainerId) &&
                        ((_configuration.TooltipCurrentCharacter &&
                          _characterMonitor.BelongsToActiveCharacter(item.RetainerId)) ||
                         !_configuration.TooltipCurrentCharacter)
                    );
                    if (_configuration.TooltipSearchScope != null && _configuration.TooltipSearchScope.Count != 0)
                    {
                        enumerable = enumerable.Where(c => _inventoryScopeCalculator.Filter(_configuration.TooltipSearchScope, c));
                    }

                    if (sortMode == TooltipAmountOwnedSort.Alphabetically)
                    {
                        var characterNames = _characterMonitor.Characters.OrderBy(c => c.Value.FormattedName).ToList();
                        enumerable = enumerable.OrderBy(c => characterNames.IndexOf(d => d.Key == c.RetainerId));
                    }
                    else if(sortMode == TooltipAmountOwnedSort.Categorically)
                    {
                        var characterNames = _characterMonitor.Characters.OrderBy(c => c.Value.FormattedName).ToList();
                        enumerable = enumerable.OrderBy(c => c.SortedCategory.FormattedName()).ThenBy(c => characterNames.IndexOf(d => d.Key == c.RetainerId));
                    }
                    else if(sortMode == TooltipAmountOwnedSort.Quantity)
                    {
                        enumerable = enumerable.OrderByDescending(c => c.Quantity);
                    }

                    var ownedItems = enumerable
                        .ToList();



                    uint storageCount = 0;
                    List<string> locations = new List<string>();

                    if (_configuration.TooltipLocationDisplayMode ==
                        TooltipLocationDisplayMode.CharacterBagSlotQuality)
                    {
                        foreach (var oItem in ownedItems)
                        {
                            storageCount += oItem.Quantity;

                            if (locations.Count >= _configuration.TooltipLocationLimit)
                                continue;

                            var name = _characterMonitor.GetCharacterNameById(oItem.RetainerId);
                            if (_configuration.TooltipAddCharacterNameOwned)
                            {
                                var owner = _characterMonitor.GetCharacterNameById(
                                    oItem.RetainerId, true);
                                if (owner.Trim().Length != 0)
                                    name += " (" + owner + ")";
                            }

                            var typeIcon = "";
                            if (oItem.IsHQ)
                            {
                                typeIcon = "\uE03c";
                            }
                            else if (oItem.IsCollectible)
                            {
                                typeIcon = "\uE03d";
                            }

                            locations.Add($"{name} - {_itemLocalizer.FormattedBagLocation(oItem)} " + typeIcon);
                        }
                        if (ownedItems.Count > _configuration.TooltipLocationLimit)
                        {
                            locations.Add(ownedItems.Count - _configuration.TooltipLocationLimit + " other locations.");
                        }
                    }
                    if (_configuration.TooltipLocationDisplayMode ==
                        TooltipLocationDisplayMode.CharacterBagSlotQuantity)
                    {
                        foreach (var oItem in ownedItems)
                        {
                            storageCount += oItem.Quantity;

                            if (locations.Count >= _configuration.TooltipLocationLimit)
                                continue;

                            var name = _characterMonitor.GetCharacterNameById(oItem.RetainerId);
                            if (_configuration.TooltipAddCharacterNameOwned)
                            {
                                var owner = _characterMonitor.GetCharacterNameById(
                                    oItem.RetainerId, true);
                                if (owner.Trim().Length != 0)
                                    name += " (" + owner + ")";
                            }

                            locations.Add($"{name} - {_itemLocalizer.FormattedBagLocation(oItem)} - {+ oItem.Quantity} ");
                        }
                        if (ownedItems.Count > _configuration.TooltipLocationLimit)
                        {
                            locations.Add(ownedItems.Count - _configuration.TooltipLocationLimit + " other locations.");
                        }
                    }
                    else if (_configuration.TooltipLocationDisplayMode == TooltipLocationDisplayMode.CharacterCategoryQuantityQuality)
                    {
                        var groupedItems = ownedItems.GroupBy(c => (c.RetainerId, c.SortedCategory, c.Flags)).ToList();
                        foreach (var oGroup in groupedItems)
                        {
                            var quantity = oGroup.Sum(c => c.Quantity);
                            storageCount += (uint)quantity;

                            if (locations.Count >= _configuration.TooltipLocationLimit)
                                continue;

                            var name = _characterMonitor.GetCharacterNameById(oGroup.Key.RetainerId);
                            if (_configuration.TooltipAddCharacterNameOwned)
                            {
                                var owner = _characterMonitor.GetCharacterNameById(
                                    oGroup.Key.RetainerId, true);
                                if (owner.Trim().Length != 0)
                                    name += " (" + owner + ")";
                            }

                            var typeIcon = "";
                            if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality) != 0)
                            {
                                typeIcon = "\uE03c";
                            }
                            else if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable) != 0)
                            {
                                typeIcon = "\uE03d";
                            }

                            locations.Add($"{name} - {oGroup.Key.SortedCategory.FormattedName()} - " + quantity + " " + typeIcon);
                        }
                        if (groupedItems.Count > _configuration.TooltipLocationLimit)
                        {
                            locations.Add(groupedItems.Count - _configuration.TooltipLocationLimit + " other locations.");
                        }
                    }
                    else if (_configuration.TooltipLocationDisplayMode == TooltipLocationDisplayMode.CharacterQuantityQuality)
                    {
                        var groupedItems = ownedItems.GroupBy(c => (c.RetainerId, c.Flags)).ToList();
                        foreach (var oGroup in groupedItems)
                        {
                            var quantity = oGroup.Sum(c => c.Quantity);
                            storageCount += (uint)quantity;

                            if (locations.Count >= _configuration.TooltipLocationLimit)
                                continue;

                            var name = _characterMonitor.GetCharacterNameById(oGroup.Key.RetainerId);
                            if (_configuration.TooltipAddCharacterNameOwned)
                            {
                                var owner = _characterMonitor.GetCharacterNameById(
                                    oGroup.Key.RetainerId, true);
                                if (owner.Trim().Length != 0)
                                    name += " (" + owner + ")";
                            }

                            var typeIcon = "";
                            if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality) != 0)
                            {
                                typeIcon = "\uE03c";
                            }
                            else if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable) != 0)
                            {
                                typeIcon = "\uE03d";
                            }

                            locations.Add($"{name} - " + quantity + " " + typeIcon);
                        }
                        if (groupedItems.Count > _configuration.TooltipLocationLimit)
                        {
                            locations.Add(groupedItems.Count - _configuration.TooltipLocationLimit + " other locations.");
                        }
                    }

                    if (storageCount > 0)
                    {
                        ImGui.Separator();
                        ImGui.TextUnformatted($"Owned: {storageCount}");
                        ImGui.TextUnformatted($"Locations:");
                        using (Dalamud.Interface.Utility.Raii.ImRaii.PushIndent())
                        {
                            for (var index = 0; index < locations.Count; index++)
                            {
                                var location = locations[index];
                                ImGui.TextUnformatted($"{location}\n");
                            }
                        }
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