using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using InventoryTools.Enums;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Services;

public class ShopTrackerService : IDisposable
{
    private readonly ITargetManager targetManager;
    private readonly GilShopSheet _gilShopSheet;
    private readonly SpecialShopSheet _specialShopSheet;
    private readonly InclusionShopSheet _inclusionShopSheet;
    private readonly FccShopSheet _fccShopSheet;
    private readonly ENpcBaseSheet _enpcBaseSheet;
    private readonly IAddonLifecycle _addonLifecycle;
    private HashSet<uint> specialShops = new();
    private HashSet<uint> gilShops = new();
    private HashSet<uint> inclusionShops = new();
    private HashSet<uint> collectableShops = new();
    private HashSet<uint> fccShops = new();

    private Dictionary<uint, uint> gilShopPreHandlers = new();
    private Dictionary<uint, uint> specialShopPreHandlers = new();
    private Dictionary<uint, uint> collectableShopPreHandlers = new();
    private Dictionary<uint, uint> inclusionShopPreHandlers = new();

    private Dictionary<uint, HashSet<uint>> gilShopCustomTalk = new();
    private Dictionary<uint, HashSet<uint>> specialShopCustomTalk = new();
    private Dictionary<uint, HashSet<uint>> collectableShopCustomTalk = new();
    private Dictionary<uint, HashSet<uint>> inclusionShopCustomTalk = new();
    private Dictionary<uint, HashSet<uint>> fccShopCustomTalk = new();

    private Dictionary<uint, HashSet<uint>> gilShopTopicSelect = new();
    private Dictionary<uint, HashSet<uint>> specialShopTopicSelect = new();
    private Dictionary<uint, HashSet<uint>> collectableShopTopicSelect = new();
    private Dictionary<uint, HashSet<uint>> inclusionShopTopicSelect = new();
    private Dictionary<uint, HashSet<uint>> fccShopTopicSelect = new();

    public delegate void ShopChangedDelegate();

    public event ShopChangedDelegate? OnShopChanged;

    public ShopTrackerService(IDataManager dataManager, ITargetManager targetManager, GilShopSheet gilShopSheet, SpecialShopSheet specialShopSheet, InclusionShopSheet inclusionShopSheet, FccShopSheet fccShopSheet, ENpcBaseSheet enpcBaseSheet, IAddonLifecycle addonLifecycle)
    {
        this.targetManager = targetManager;
        _gilShopSheet = gilShopSheet;
        _specialShopSheet = specialShopSheet;
        _inclusionShopSheet = inclusionShopSheet;
        _fccShopSheet = fccShopSheet;
        _enpcBaseSheet = enpcBaseSheet;
        _addonLifecycle = addonLifecycle;
        _addonLifecycle.RegisterListener(AddonEvent.PostSetup, ["Shop", "FreeCompanyCreditShop", "ShopExchangeItem", "ShopExchangeCurrency", "InclusionShop", "CollectablesShop"], AddonPostSetup);
        foreach (var item in dataManager.GetExcelSheet<SpecialShop>())
        {
            specialShops.Add(item.RowId);
        }

        foreach (var item in dataManager.GetExcelSheet<GilShop>())
        {
            gilShops.Add(item.RowId);
        }

        foreach (var item in dataManager.GetExcelSheet<InclusionShop>())
        {
            inclusionShops.Add(item.RowId);
        }

        foreach (var item in dataManager.GetExcelSheet<Lumina.Excel.Sheets.CollectablesShop>())
        {
            collectableShops.Add(item.RowId);
        }

        foreach (var item in dataManager.GetExcelSheet<Lumina.Excel.Sheets.FccShop>())
        {
            fccShops.Add(item.RowId);
        }

        ReadOnlySpan<Type> readOnlySpan = [typeof(Lumina.Excel.Sheets.CollectablesShop), typeof(InclusionShop), typeof(GilShop), typeof(SpecialShop), typeof(FccShop)];
        var typeHash = RowRef.CreateTypeHash(readOnlySpan);

        foreach (var customTalk in dataManager.GetExcelSheet<CustomTalk>())
        {
            foreach (var scriptStruct in customTalk.Script)
            {
                var rowRef = RowRef.GetFirstValidRowOrUntyped(dataManager.Excel, scriptStruct.ScriptArg, readOnlySpan, typeHash, dataManager.GameData.Options.DefaultExcelLanguage);
                if (rowRef.Is<Lumina.Excel.Sheets.CollectablesShop>())
                {
                    collectableShops.Add(rowRef.RowId);
                    collectableShopCustomTalk.TryAdd(customTalk.RowId, new());
                    collectableShopCustomTalk[customTalk.RowId].Add(rowRef.RowId);
                }
                else if (rowRef.Is<Lumina.Excel.Sheets.InclusionShop>())
                {
                    inclusionShops.Add(rowRef.RowId);
                    inclusionShopCustomTalk.TryAdd(customTalk.RowId, new());
                    inclusionShopCustomTalk[customTalk.RowId].Add(rowRef.RowId);
                }
                else if (rowRef.Is<Lumina.Excel.Sheets.GilShop>())
                {
                    gilShops.Add(rowRef.RowId);
                    gilShopCustomTalk.TryAdd(customTalk.RowId, new());
                    gilShopCustomTalk[customTalk.RowId].Add(rowRef.RowId);
                }
                else if (rowRef.Is<Lumina.Excel.Sheets.SpecialShop>())
                {
                    specialShops.Add(rowRef.RowId);
                    specialShopCustomTalk.TryAdd(customTalk.RowId, new());
                    specialShopCustomTalk[customTalk.RowId].Add(rowRef.RowId);
                }
                else if (rowRef.Is<Lumina.Excel.Sheets.FccShop>())
                {
                    fccShops.Add(rowRef.RowId);
                    fccShopCustomTalk.TryAdd(customTalk.RowId, new());
                    fccShopCustomTalk[customTalk.RowId].Add(rowRef.RowId);
                }
            }
        }

        foreach (var prehandler in dataManager.GetExcelSheet<PreHandler>())
        {
            if (prehandler.Target.Is<Lumina.Excel.Sheets.CollectablesShop>())
            {
                collectableShopPreHandlers[prehandler.RowId] = prehandler.Target.RowId;
            }
            else if (prehandler.Target.Is<Lumina.Excel.Sheets.InclusionShop>())
            {
                inclusionShopPreHandlers[prehandler.RowId] = prehandler.Target.RowId;
            }
            else if (prehandler.Target.Is<Lumina.Excel.Sheets.GilShop>())
            {
                gilShopPreHandlers[prehandler.RowId] = prehandler.Target.RowId;
            }
            else if (prehandler.Target.Is<Lumina.Excel.Sheets.SpecialShop>())
            {
                specialShopPreHandlers[prehandler.RowId] = prehandler.Target.RowId;
            }
        }

        foreach (var topicSelect in dataManager.GetExcelSheet<TopicSelect>())
        {
            foreach (var shop in topicSelect.Shop)
            {
                if (shop.Is<Lumina.Excel.Sheets.PreHandler>())
                {
                    var prehandler = shop.GetValueOrDefault<PreHandler>();
                    if (prehandler != null)
                    {
                        if (prehandler.Value.Target.Is<Lumina.Excel.Sheets.CollectablesShop>())
                        {
                            collectableShopTopicSelect.TryAdd(prehandler.Value.RowId, new());
                            collectableShopTopicSelect[prehandler.Value.RowId].Add(prehandler.Value.Target.RowId);
                        }
                        else if (prehandler.Value.Target.Is<Lumina.Excel.Sheets.InclusionShop>())
                        {
                            inclusionShopTopicSelect.TryAdd(prehandler.Value.RowId, new());
                            inclusionShopTopicSelect[prehandler.Value.RowId].Add(prehandler.Value.Target.RowId);
                        }
                        else if (prehandler.Value.Target.Is<Lumina.Excel.Sheets.GilShop>())
                        {
                            gilShopTopicSelect.TryAdd(prehandler.Value.RowId, new());
                            gilShopTopicSelect[prehandler.Value.RowId].Add(prehandler.Value.Target.RowId);
                        }
                        else if (prehandler.Value.Target.Is<Lumina.Excel.Sheets.SpecialShop>())
                        {
                            specialShopTopicSelect.TryAdd(prehandler.Value.RowId, new());
                            specialShopTopicSelect[prehandler.Value.RowId].Add(prehandler.Value.Target.RowId);
                        }
                    }
                }
                else if (shop.Is<Lumina.Excel.Sheets.GilShop>())
                {
                    gilShopTopicSelect.TryAdd(topicSelect.RowId, new());
                    gilShopTopicSelect[topicSelect.RowId].Add(shop.RowId);
                }
                else if (shop.Is<Lumina.Excel.Sheets.SpecialShop>())
                {
                    specialShopTopicSelect.TryAdd(topicSelect.RowId, new());
                    specialShopTopicSelect[topicSelect.RowId].Add(shop.RowId);
                }
            }
        }
    }

    private void AddonPostSetup(AddonEvent type, AddonArgs args)
    {
        OnShopChanged?.Invoke();
    }

    public (ENpcBaseRow npc, List<IShop> shops, IShop? activeShop)? GetCurrentShopType()
    {
        var currentShopIds = GetCurrentShopTypeIds();
        if (currentShopIds == null)
        {
            return null;
        }

        var shopIds = currentShopIds.Value.shops;
        var activeShopId = currentShopIds.Value.activeShopId;

        var shops = new List<IShop>();
        IShop? activeShop = null;
        var enpcBaseRow = _enpcBaseSheet.GetRowOrDefault(currentShopIds.Value.npcId);
        if (enpcBaseRow == null)
        {
            return null;
        }
        foreach (var shopId in shopIds)
        {
            var shop = GetShopByIdAndType(shopId.Item2, shopId.Item1);
            if (shop != null)
            {
                shops.Add(shop);
            }
        }

        if (activeShopId != null)
        {
            var shop = GetShopByIdAndType(activeShopId.Value.Item2, activeShopId.Value.Item1);
            if (shop != null)
            {
                activeShop = shop;
            }
        }


        return (enpcBaseRow, shops, activeShop);
    }

    public IShop? GetShopByIdAndType(uint shopId, ShopType type)
    {
        switch (type)
        {
            case ShopType.Gil:
                var gilShop = _gilShopSheet.GetRowOrDefault(shopId);
                if (gilShop != null)
                {
                    return gilShop;
                }
                break;
            case ShopType.SpecialShop:
                var specialShop = _specialShopSheet.GetRowOrDefault(shopId);
                if (specialShop != null)
                {
                    return specialShop;
                }
                break;
            case ShopType.Collectable:
                //TODO
                break;
            case ShopType.InclusionShop:
                var inclusionShop = _inclusionShopSheet.GetRowOrDefault(shopId);
                if (inclusionShop != null)
                {
                    //TODO
                    //shops.Add(inclusionShop);
                }
                break;
            case ShopType.FreeCompanyShop:
                var fccShop = _inclusionShopSheet.GetRowOrDefault(shopId);
                if (fccShop != null)
                {
                    //shops.Add(fccShop);
                }
                break;
        }

        return null;
    }

    public unsafe (uint npcId, List<(ShopType, uint)> shops, (ShopType, uint)? activeShopId)? GetCurrentShopTypeIds()
    {
        var eventFramework = EventFramework.Instance();
        if (eventFramework != null)
        {
            uint? npcId = null;
            List<(ShopType,uint)> shops = new();
            (ShopType, uint)? shopId = null;

            foreach (var eventHandler in eventFramework->EventHandlerModule.EventHandlerMap)
            {
                if (eventHandler.Item2.Value != null)
                {
                    var activeTarget = false;
                    foreach (var eventObject in eventHandler.Item2.Value->EventObjects)
                    {
                        if (targetManager.Target?.DataId == eventObject.Value->BaseId)
                        {
                            npcId = targetManager.Target?.DataId;
                            activeTarget = true;
                        }
                    }

                    if (!activeTarget)
                    {
                        continue;
                    }
                    if (collectableShops.Contains(eventHandler.Item1))
                    {
                        shops.Add((ShopType.Collectable,eventHandler.Item1));
                    }
                    else if (inclusionShops.Contains(eventHandler.Item1))
                    {
                        shops.Add((ShopType.InclusionShop,eventHandler.Item1));
                    }
                    else if (gilShops.Contains(eventHandler.Item1))
                    {
                        shops.Add((ShopType.Gil,eventHandler.Item1));
                    }
                    else if (specialShops.Contains(eventHandler.Item1))
                    {
                        shops.Add((ShopType.SpecialShop,eventHandler.Item1));
                    }
                    else if (fccShops.Contains(eventHandler.Item1))
                    {
                        shops.Add((ShopType.FreeCompanyShop,eventHandler.Item1));
                    }
                    else if (collectableShopCustomTalk.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var customTalkShopId in collectableShopCustomTalk[eventHandler.Item1])
                        {
                            shops.Add((ShopType.Collectable,customTalkShopId));
                        }
                    }
                    else if (inclusionShopCustomTalk.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var customTalkShopId in inclusionShopCustomTalk[eventHandler.Item1])
                        {
                            shops.Add((ShopType.InclusionShop,customTalkShopId));
                        }
                    }
                    else if (gilShopCustomTalk.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var customTalkShopId in gilShopCustomTalk[eventHandler.Item1])
                        {
                            shops.Add((ShopType.Gil,customTalkShopId));
                        }
                    }
                    else if (specialShopCustomTalk.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var customTalkShopId in specialShopCustomTalk[eventHandler.Item1])
                        {
                            shops.Add((ShopType.SpecialShop,customTalkShopId));
                        }
                    }
                    else if (fccShopCustomTalk.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var customTalkShopId in fccShopCustomTalk[eventHandler.Item1])
                        {
                            shops.Add((ShopType.FreeCompanyShop,customTalkShopId));
                        }
                    }
                    else if (collectableShopPreHandlers.ContainsKey(eventHandler.Item1))
                    {
                        shops.Add((ShopType.Collectable,collectableShopPreHandlers[eventHandler.Item1]));
                    }
                    else if (inclusionShopPreHandlers.ContainsKey(eventHandler.Item1))
                    {
                        shops.Add((ShopType.InclusionShop,inclusionShopPreHandlers[eventHandler.Item1]));
                    }
                    else if (gilShopPreHandlers.ContainsKey(eventHandler.Item1))
                    {
                        shops.Add((ShopType.Gil,gilShopPreHandlers[eventHandler.Item1]));
                    }
                    else if (specialShopPreHandlers.ContainsKey(eventHandler.Item1))
                    {
                        shops.Add((ShopType.SpecialShop,specialShopPreHandlers[eventHandler.Item1]));
                    }
                    else if (collectableShopTopicSelect.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var topicSelectShopId in collectableShopTopicSelect[eventHandler.Item1])
                        {
                            shops.Add((ShopType.Collectable,topicSelectShopId));
                        }
                    }
                    else if (inclusionShopTopicSelect.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var topicSelectShopId in inclusionShopTopicSelect[eventHandler.Item1])
                        {
                            shops.Add((ShopType.InclusionShop,topicSelectShopId));
                        }
                    }
                    else if (gilShopTopicSelect.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var topicSelectShopId in gilShopTopicSelect[eventHandler.Item1])
                        {
                            shops.Add((ShopType.Gil,topicSelectShopId));
                        }
                    }
                    else if (specialShopTopicSelect.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var topicSelectShopId in specialShopTopicSelect[eventHandler.Item1])
                        {
                            shops.Add((ShopType.SpecialShop,topicSelectShopId));
                        }
                    }
                    else if (fccShopTopicSelect.ContainsKey(eventHandler.Item1))
                    {
                        foreach (var topicSelectShopId in fccShopTopicSelect[eventHandler.Item1])
                        {
                            shops.Add((ShopType.FreeCompanyShop,topicSelectShopId));
                        }
                    }

                    if (activeTarget)
                    {
                         var freeCompanyShopAgent = UIModule.Instance()->GetAgentModule()->GetAgentByInternalId(AgentId.FreeCompanyCreditShop);
                         if (freeCompanyShopAgent != null && freeCompanyShopAgent->IsAgentActive() && eventHandler.Item2.Value != null && eventHandler.Item2.Value->Info.EventId.ContentId == EventHandlerType.FreeCompanyCreditShop)
                         {
                             shopId = shops.FirstOrDefault(c => c.Item2 == eventHandler.Item1);
                         }
                         else
                         {
                             var agent = (AgentShop*)UIModule.Instance()->GetAgentModule()->GetAgentByInternalId(AgentId.Shop);
                             if (agent != null && agent->IsAgentActive() && agent->EventReceiver != null &&
                                 agent->IsAddonReady())
                             {
                                 var proxy = (ShopEventHandler.AgentProxy*)agent->EventReceiver;
                                 if (proxy != null && proxy->Handler != null)
                                 {
                                     shopId = shops.FirstOrDefault(c => c.Item2 == proxy->Handler->Info.EventId.Id);
                                 }
                             }

                             var agentProxy = ShopEventHandler.AgentProxy.Instance();
                             if (agentProxy != null && agentProxy->Handler != null)
                             {
                                 if (agentProxy->Handler == eventHandler.Item2.Value)
                                 {
                                     shopId = shops.FirstOrDefault(c => c.Item2 == eventHandler.Item1);
                                 }
                             }
                         }
                    }
                }
            }

            if (npcId != null)
            {
                return (npcId.Value, shops, shopId);
            }
        }

        return null;
    }

    public void Dispose()
    {
        _addonLifecycle.UnregisterListener(AddonEvent.PostSetup, ["Shop", "FreeCompanyCreditShop", "ShopExchangeItem", "ShopExchangeCurrency", "InclusionShop", "CollectablesShop"], AddonPostSetup);
    }
}