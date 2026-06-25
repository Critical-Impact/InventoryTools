using System.Collections.Generic;
using CriticalCommonLib.Services.Ui;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace InventoryTools.Services.GameCraftSources
{
    public class GrandCompanySupplyCraftSource : IGameCraftSource
    {
        private const int FirstProvisioningPosition = 8;
        private const int FirstExpertDeliveryPosition = 11;

        private const int ContentsInfoDetailSupplyFirstItemIdIndex = 233;
        private const int ContentsInfoDetailSupplyLastItemIdIndex = 240;
        private const int ContentsInfoDetailSupplyRequestedOffset = 16;
        private const int ContentsInfoDetailProvisioningFirstItemIdIndex = 292;
        private const int ContentsInfoDetailProvisioningLastItemIdIndex = 294;
        private const int ContentsInfoDetailProvisioningRequestedOffset = 6;

        private readonly IGameUiManager _gameUiManager;

        public GrandCompanySupplyCraftSource(IGameUiManager gameUiManager)
        {
            _gameUiManager = gameUiManager;
        }

        public IReadOnlyList<GameCraftCategory> GetAvailableCategories()
        {
            if (_gameUiManager.IsWindowVisible(WindowName.GrandCompanySupplyList))
            {
                var categories = new List<GameCraftCategory>();
                if (AgentHasItems(0, FirstProvisioningPosition))
                {
                    categories.Add(new GameCraftCategory("Supply Missions",
                        () => GetAgentItems(0, FirstProvisioningPosition)));
                }

                if (AgentHasItems(FirstProvisioningPosition, FirstExpertDeliveryPosition))
                {
                    categories.Add(new GameCraftCategory("Provisioning Missions",
                        () => GetAgentItems(FirstProvisioningPosition, FirstExpertDeliveryPosition)));
                }

                if (categories.Count > 0)
                {
                    return categories;
                }
            }

            if (_gameUiManager.IsWindowVisible(WindowName.ContentsInfoDetail))
            {
                var categories = new List<GameCraftCategory>();
                if (ContentsInfoDetailHasItems(ContentsInfoDetailSupplyFirstItemIdIndex,
                        ContentsInfoDetailSupplyLastItemIdIndex, ContentsInfoDetailSupplyRequestedOffset))
                {
                    categories.Add(new GameCraftCategory("Supply Missions",
                        () => GetContentsInfoDetailItems(ContentsInfoDetailSupplyFirstItemIdIndex,
                            ContentsInfoDetailSupplyLastItemIdIndex, ContentsInfoDetailSupplyRequestedOffset)));
                }

                if (ContentsInfoDetailHasItems(ContentsInfoDetailProvisioningFirstItemIdIndex,
                        ContentsInfoDetailProvisioningLastItemIdIndex, ContentsInfoDetailProvisioningRequestedOffset))
                {
                    categories.Add(new GameCraftCategory("Provisioning Missions",
                        () => GetContentsInfoDetailItems(ContentsInfoDetailProvisioningFirstItemIdIndex,
                            ContentsInfoDetailProvisioningLastItemIdIndex,
                            ContentsInfoDetailProvisioningRequestedOffset)));
                }

                return categories;
            }

            return [];
        }

        private static unsafe bool AgentHasItems(int minPosition, int maxPosition)
        {
            var agent = AgentGrandCompanySupply.Instance();
            if (agent == null || agent->ItemArray == null)
            {
                return false;
            }

            for (var i = 0; i < agent->NumItems; i++)
            {
                var item = agent->ItemArray[i];
                if (item.Position >= minPosition && item.Position < maxPosition && item.ItemId != 0 &&
                    item.NumRequested > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static unsafe IReadOnlyList<GameCraftItem> GetAgentItems(int minPosition, int maxPosition)
        {
            var items = new List<GameCraftItem>();
            var agent = AgentGrandCompanySupply.Instance();
            if (agent == null || agent->ItemArray == null)
            {
                return items;
            }

            for (var i = 0; i < agent->NumItems; i++)
            {
                var item = agent->ItemArray[i];
                if (item.Position >= minPosition && item.Position < maxPosition && item.ItemId != 0 &&
                    item.NumRequested > 0)
                {
                    items.Add(new GameCraftItem(item.ItemId, (uint)item.NumRequested));
                }
            }

            return items;
        }

        private unsafe bool ContentsInfoDetailHasItems(int firstItemIdIndex, int lastItemIdIndex, int requestedOffset)
        {
            var addon = _gameUiManager.GetWindow("ContentsInfoDetail");
            if (!HasContentsInfoDetailValues(addon, lastItemIdIndex + requestedOffset))
            {
                return false;
            }

            for (var i = firstItemIdIndex; i <= lastItemIdIndex; i++)
            {
                if (GetUInt(addon->AtkValues[i]) is > 0 && GetUInt(addon->AtkValues[i + requestedOffset]) is > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private unsafe IReadOnlyList<GameCraftItem> GetContentsInfoDetailItems(int firstItemIdIndex,
            int lastItemIdIndex, int requestedOffset)
        {
            var items = new List<GameCraftItem>();
            var addon = _gameUiManager.GetWindow("ContentsInfoDetail");
            if (!HasContentsInfoDetailValues(addon, lastItemIdIndex + requestedOffset))
            {
                return items;
            }

            for (var i = firstItemIdIndex; i <= lastItemIdIndex; i++)
            {
                var itemId = GetUInt(addon->AtkValues[i]);
                var requested = GetUInt(addon->AtkValues[i + requestedOffset]);
                if (itemId is > 0 && requested is > 0)
                {
                    items.Add(new GameCraftItem(itemId.Value, requested.Value));
                }
            }

            return items;
        }

        private static unsafe bool HasContentsInfoDetailValues(AtkUnitBase* addon, int maxIndex)
        {
            return addon != null && addon->AtkValues != null && addon->AtkValuesCount > maxIndex;
        }

        private static uint? GetUInt(AtkValue value)
        {
            return value.Type switch
            {
                AtkValueType.Int => (uint)value.Int,
                AtkValueType.UInt => value.UInt,
                _ => null,
            };
        }
    }
}