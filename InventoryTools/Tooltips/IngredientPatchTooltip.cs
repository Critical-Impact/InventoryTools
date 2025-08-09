using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic.Settings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public class IngredientPatchTooltip : BaseTooltip
{
    private readonly IngredientPatchService _ingredientPatchService;
    private readonly TooltipIngredientPatchTooltipColorSetting _colorSetting;
    private readonly TooltipDisplayIngredientPatchSetting _tooltipIngredientPatchTooltipSetting;
    private readonly ShowTooltipsSetting _showTooltipsSetting;

    public IngredientPatchTooltip(ILogger<IngredientPatchTooltip> logger, IngredientPatchService ingredientPatchService, TooltipIngredientPatchTooltipColorSetting colorSetting, TooltipDisplayIngredientPatchSetting tooltipIngredientPatchTooltipSetting, ShowTooltipsSetting showTooltipsSetting, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui, IChatGui chatGui) : base(6908, logger, itemSheet, configuration, gameGui, chatGui)
    {
        _ingredientPatchService = ingredientPatchService;
        _colorSetting = colorSetting;
        _tooltipIngredientPatchTooltipSetting = tooltipIngredientPatchTooltipSetting;
        _showTooltipsSetting = showTooltipsSetting;
    }

    public override bool IsEnabled => _showTooltipsSetting.CurrentValue(Configuration) && _tooltipIngredientPatchTooltipSetting.CurrentValue(Configuration);
    public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (!ShouldShow()) return;
        var item = HoverItem;
        if (item != null && item.HasUsesByType(ItemInfoType.CraftRecipe))
        {
            if (!_ingredientPatchService.IngredientPatches.TryGetValue(item.RowId, out var patch))
            {
                return;
            }

            TooltipService.ItemTooltipField itemTooltipField = TooltipService.ItemTooltipField.ItemDescription;
            SeString? seStr = null;
            if (GetTooltipVisibility(ItemTooltipFieldVisibility.Description))
            {
                itemTooltipField = TooltipService.ItemTooltipField.ItemDescription;
                seStr = GetTooltipString(stringArrayData, itemTooltipField);
            }

            if (seStr == null && GetTooltipVisibility(ItemTooltipFieldVisibility.Effects))
            {
                itemTooltipField = TooltipService.ItemTooltipField.Effects;
                seStr = GetTooltipString(stringArrayData, itemTooltipField);
            }

            if (seStr == null && GetTooltipVisibility(ItemTooltipFieldVisibility.Levels))
            {
                itemTooltipField = TooltipService.ItemTooltipField.Levels;
                seStr = GetTooltipString(stringArrayData, itemTooltipField);
            }

            if(seStr == null)
            {
                return;
            }

            if (seStr.Payloads.Any(payload =>
                    payload is DalamudLinkPayload linkPayload && linkPayload.CommandId == TooltipIdentifier))
            {
                return;
            }
            seStr.Payloads.Add(GetLinkPayload());
            seStr.Payloads.Add(RawPayload.LinkTerminator);

            var newText = $"\nIngredient Patch: {patch.ToString(CultureInfo.InvariantCulture)}";

            newText = newText.TrimEnd('\n');
            if (newText != "")
            {
                var lines = new List<Payload>()
                {
                    new UIForegroundPayload((ushort)(_colorSetting.CurrentValue(Configuration) ?? Configuration.TooltipColor ?? 1)),
                    new UIGlowPayload(0),
                    new TextPayload(newText),
                    new UIGlowPayload(0),
                    new UIForegroundPayload(0),

                };
                foreach (var line in lines)
                {
                    seStr.Payloads.Add(line);
                }
                SetTooltipString(stringArrayData, itemTooltipField, seStr);
            }
        }
    }

    public override uint Order => 1;
}