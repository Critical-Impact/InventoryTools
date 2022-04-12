using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Services.Ui;
using FFXIVClientStructs.FFXIV.Client.Game;
using InventoryTools.Logic;

namespace InventoryTools.Ui.GameUi
{
    public class RetainerInventoryCounts : AtkRetainerList, AtkState<FilterConfiguration?> 
    {
        public override bool ShouldDraw { get; set; }

        public override unsafe bool Draw()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null && State != null)
            {
                this.SetNames(atkUnitBase, RetainerNames, State.HighlightColor ?? PluginLogic.PluginConfiguration.HighlightColor);
                return true;
            }

            return false;
        }

        public override void Setup()
        {
            
        }

        public FilterConfiguration? State { get; set; }

        public Dictionary<ulong, string> RetainerNames = new Dictionary<ulong, string>();
        
        public void UpdateState(FilterConfiguration? newState)
        {
            State = newState;
            if (State != null)
            {
                var activeFilter = State;
                if (activeFilter.FilterResult.HasValue)
                {
                    var filteredList = activeFilter.FilterResult.Value;
                    var currentCharacterId = Service.ClientState.LocalContentId;
                    if (activeFilter.FilterType == FilterType.SortingFilter)
                    {
                        RetainerNames = filteredList.SortedItems.Where(c => c.SourceRetainerId == currentCharacterId && c.DestinationRetainerId != null)
                            .GroupBy(c => c.DestinationRetainerId!.Value).Where(c => c.Any()).ToDictionary(c => c.Key, GenerateNewName);
                    }
                    else
                    {
                        RetainerNames = filteredList.SortedItems.GroupBy(c => c.SourceRetainerId).Where(c => c.Any()).ToDictionary(c => c.Key, GenerateNewName);
                    }
                }
            }
        }

        private unsafe string GenerateNewName(IGrouping<ulong, SortingResult> c)
        {
            if (PluginService.CharacterMonitor.Characters.ContainsKey(c.Key))
            {
                return PluginService.CharacterMonitor.Characters[c.Key].Name + " (" + c.Count() + ")";
            }
            return "Unknown "  + "(" + c.Count() + ")";
        }
    }
}