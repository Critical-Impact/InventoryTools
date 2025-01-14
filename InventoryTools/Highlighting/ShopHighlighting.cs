using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace InventoryTools.Highlighting;

public class ShopHighlighting : IDisposable
{
    private readonly IGameGui gameGui;
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IPluginLog pluginLog;
    private uint shopItemsAtkIndex = 441;
    private uint shopCountAtkIndex = 2;
    private HashSet<uint> highlightedItems = new HashSet<uint>();
    private Dictionary<int, uint>? itemIndexMap = null;

    public ShopHighlighting(IGameGui gameGui, IAddonLifecycle addonLifecycle, IPluginLog pluginLog)
    {
        this.gameGui = gameGui;
        this.addonLifecycle = addonLifecycle;
        this.pluginLog = pluginLog;
        addonLifecycle.RegisterListener(AddonEvent.PostSetup, "Shop", AddonSetup);
        addonLifecycle.RegisterListener(AddonEvent.PostDraw, "Shop", AddonPostDraw);
    }

    public void AddItem(uint itemId)
    {
        highlightedItems.Add(itemId);
    }

    public void RemoveItem(uint itemId)
    {
        highlightedItems.Remove(itemId);
    }

    public void SetItems(List<uint> items)
    {
        highlightedItems = [..items];
    }

    public void SetItems(HashSet<uint> items)
    {
        highlightedItems = items;
    }

    public void ClearItems()
    {
        highlightedItems.Clear();
    }

    private string itemIdString = "";
    private uint itemId;

    public unsafe void DrawDebug()
    {
        var addon = gameGui.GetAddonByName("Shop");
        if (addon != IntPtr.Zero)
        {
            var atkUnitBase = (AtkUnitBase*)addon;
            var atkComponentBase = atkUnitBase->GetComponentByNodeId(16);
            if (atkComponentBase != null)
            {
                var listNode = (AtkComponentList*)atkComponentBase;
                var listItemIndex = listNode->ItemRendererList->AtkComponentListItemRenderer->ListItemIndex;
                ImGui.TextUnformatted($"List Item Index: {listItemIndex}");
                if (itemIndexMap != null)
                {
                    foreach (var item in itemIndexMap)
                    {
                        ImGui.TextUnformatted(item.Key + ": " + item.Value);
                    }
                }

                if (ImGui.InputText("Item", ref itemIdString, 128))
                {
                    if (uint.TryParse(itemIdString, out itemId))
                    {
                        itemId = itemId;
                    }
                    itemIdString = itemId.ToString();
                }
                if (ImGui.Button("Add Item"))
                {
                    if (uint.TryParse(itemIdString, out itemId))
                    {
                        highlightedItems.Add(itemId);
                    }
                }
                if (ImGui.Button("Remove Item"))
                {
                    if (uint.TryParse(itemIdString, out itemId))
                    {
                        highlightedItems.Remove(itemId);
                    }
                }
            }
        }
    }


    private unsafe void AddonPostDraw(AddonEvent type, AddonArgs args)
    {
        if (args.Addon != IntPtr.Zero)
        {
            var atkUnitBase = (AtkUnitBase*)args.Addon;
            var atkComponentBase = atkUnitBase->GetComponentByNodeId(16);
            if (atkComponentBase != null)
            {
                var listNode = (AtkComponentList*)atkComponentBase;
                if (this.itemIndexMap == null)
                {
                    CalculateItemIndexMap(atkUnitBase);
                }

                for (int i = 0; i < listNode->ListLength; i++)
                {
                    if (!highlightedItems.Any())
                    {
                        listNode->SetItemDisabledState(i, false);
                        listNode->SetItemHighlightedState(i, false);
                    }
                    else if (itemIndexMap!.ContainsKey(i))
                    {
                        if (highlightedItems.Contains(itemIndexMap[i]))
                        {
                            if (!listNode->GetItemHighlightedState(i))
                            {
                                listNode->SetItemHighlightedState(i, true);
                            }
                            if (listNode->GetItemDisabledState(i))
                            {
                                listNode->SetItemDisabledState(i, false);
                            }
                        }
                        else
                        {
                            if (!listNode->GetItemDisabledState(i))
                            {
                                listNode->SetItemDisabledState(i, true);
                            }
                            if (listNode->GetItemHighlightedState(i))
                            {
                                listNode->SetItemHighlightedState(i, false);
                            }
                        }
                    }
                }
            }
        }
    }

    private unsafe void CalculateItemIndexMap(AtkUnitBase* atkUnitBase)
    {
        var itemIndexMap = new Dictionary<int, uint>();
        var shopLength = atkUnitBase->AtkValues[shopCountAtkIndex].UInt;
        for (var i = shopItemsAtkIndex; i < shopItemsAtkIndex + shopLength; i++)
        {
            var atkValue = atkUnitBase->AtkValues[i];
            if (atkValue.Type != ValueType.UInt)
            {
                break;
            }

            itemIndexMap[(int)(i - shopItemsAtkIndex)] = atkValue.UInt;
        }
        this.itemIndexMap = itemIndexMap;
    }

    private unsafe void AddonSetup(AddonEvent type, AddonArgs args)
    {
        if (args.Addon != IntPtr.Zero)
        {
            var atkUnitBase = (AtkUnitBase*)args.Addon;
            CalculateItemIndexMap(atkUnitBase);
        }
    }

    public void Dispose()
    {
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "Shop", AddonSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostDraw, "Shop", AddonPostDraw);
    }
}