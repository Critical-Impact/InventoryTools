using System;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Ui.Widgets;

namespace InventoryTools.Logic.Columns
{
    public class CraftSimpleColumn : TextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Crafting;
        public override string? CurrentValue(CraftItem currentValue)
        {
            return "";
        }

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            var nextStep = GetNextStep(configuration, item);
            ImGuiUtil.VerticalAlignTextColored(nextStep.Item2, nextStep.Item1, configuration.TableHeight, true);
        }

        public (Vector4, string) GetNextStep(FilterConfiguration configuration, CraftItem item)
        {
            var unavailable = Math.Max(0, (int)item.QuantityMissingOverall);
            
            if (configuration.CraftList.RetainerRetrieveOrder == RetainerRetrieveOrder.RetrieveFirst)
            {
                var retrieve = (int)item.QuantityWillRetrieve;
                if (retrieve != 0)
                {
                    return (ImGuiColors.DalamudOrange, "Retrieve " + retrieve);
                }
            }

            var ingredientPreference = configuration.CraftList.GetIngredientPreference(item.ItemId);

            if (ingredientPreference == null)
            {
                foreach (var defaultPreference in configuration.CraftList.IngredientPreferenceTypeOrder)
                {
                    if (item.Item.GetIngredientPreference(defaultPreference.Item1, defaultPreference.Item2,
                            out ingredientPreference))
                    {
                        break;
                    }
                }
            }

            if (ingredientPreference != null)
            {
                string nextStepString = "";
                Vector4 stepColour = ImGuiColors.DalamudYellow;
                if (unavailable != 0)
                {

                    switch (ingredientPreference.Type)
                    {
                        case IngredientPreferenceType.Botany:
                        case IngredientPreferenceType.Mining:
                            nextStepString = "Gather " + unavailable;
                            break;
                        case IngredientPreferenceType.Buy:
                            nextStepString = "Buy " + unavailable + " (Vendor)";
                            break;
                        case IngredientPreferenceType.Marketboard:
                            nextStepString = "Buy " + unavailable + " (MB)";
                            break;
                        case IngredientPreferenceType.Crafting:
                            if (item.QuantityCanCraft >= unavailable)
                            {
                                if (item.QuantityCanCraft != 0)
                                {
                                    if (item.Item.CanBeCrafted)
                                    {
                                        nextStepString = "Craft " + item.CraftOperationsRequired;
                                        stepColour = ImGuiColors.ParsedBlue;
                                    }
                                }
                            }
                            else
                            {
                                stepColour = ImGuiColors.DalamudRed;
                                nextStepString = "Craft Ingredients Missing";
                            }

                            break;
                        case IngredientPreferenceType.Fishing:
                            nextStepString = "Fish for " + unavailable;
                            break;
                        case IngredientPreferenceType.Item:
                            if (ingredientPreference.LinkedItemId != null &&
                                ingredientPreference.LinkedItemQuantity != null)
                            {
                                if (item.QuantityCanCraft >= unavailable)
                                {
                                    if (item.QuantityCanCraft != 0)
                                    {
                                        var linkedItem = Service.ExcelCache.GetItemExSheet()
                                            .GetRow(item.IngredientPreference.ItemId);
                                        nextStepString = "Purchase " + item.QuantityCanCraft + " " + linkedItem?.NameString ?? "Unknown";
                                        stepColour = ImGuiColors.DalamudYellow;
                                    }
                                }
                                else
                                {
                                    if (item.IngredientPreference.LinkedItemId != null)
                                    {
                                        var linkedItem = Service.ExcelCache.GetItemExSheet()
                                            .GetRow(item.IngredientPreference.LinkedItemId.Value);
                                        stepColour = ImGuiColors.DalamudRed;
                                        nextStepString = "Not enough " + (linkedItem?.NameString ?? "Unknown");
                                    }
                                }
                                break;
                            }

                            nextStepString = "No item selected";
                            break;
                        case IngredientPreferenceType.Venture:
                            nextStepString = "Venture: " + item.Item.RetainerTaskNames;
                            ;
                            break;
                        case IngredientPreferenceType.Gardening:
                            nextStepString = "Harvest(Gardening): " + unavailable;
                            break;
                        case IngredientPreferenceType.ResourceInspection:
                            nextStepString = "Resource Inspection: " + unavailable;
                            break;
                        case IngredientPreferenceType.Reduction:
                            nextStepString = "Reduce: " + unavailable;
                            break;
                        case IngredientPreferenceType.Desynthesis:
                            nextStepString = "Desynthesize: " + unavailable;
                            break;
                        case IngredientPreferenceType.Mobs:
                            nextStepString = "Hunt: " + unavailable;
                            break;
                    }

                    if (nextStepString != "")
                    {
                        return (stepColour, nextStepString);
                    }
                }
            }
            if (unavailable != 0)
            {
                if (item.Item.ObtainedGathering)
                {
                    return (ImGuiColors.DalamudYellow, "Gather " + unavailable);
                }
                else if (item.Item.ObtainedGil)
                {
                    return (ImGuiColors.DalamudYellow, "Buy " + unavailable);

                }
                return (ImGuiColors.DalamudRed, "Missing " + unavailable);
            }
            var canCraft = item.QuantityCanCraft;
            if (canCraft != 0)
            {
                return (ImGuiColors.ParsedBlue, "Craft " + (uint)Math.Ceiling((double)canCraft / item.Yield));
            }

            if (configuration.CraftList.RetainerRetrieveOrder == RetainerRetrieveOrder.RetrieveLast)
            {
                var retrieve = (int)item.QuantityWillRetrieve;
                if (retrieve != 0)
                {
                    return (ImGuiColors.DalamudOrange, "Retrieve " + retrieve);
                }
            }



            if (item.IsOutputItem)
            {
                return (ImGuiColors.DalamudWhite, "Waiting");
            }
            return (ImGuiColors.HealerGreen, "Done");
        }

        public override string? CurrentValue(InventoryItem item)
        {
            return "";
        }

        public override string? CurrentValue(ItemEx item)
        {
            return "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return "";
        }

        public override string Name { get; set; } = "Next Step in Craft";
        public override string RenderName => "Next Step";

        public override float Width { get; set; } = 100;
        public override bool? CraftOnly => true;

        public override string HelpText { get; set; } =
            "Shows a simplified version of what you should do next in your craft";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
    }
}