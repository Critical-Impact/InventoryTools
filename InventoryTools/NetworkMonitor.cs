using System;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Data;
using Dalamud.Game.Network;
using Dalamud.Logging;

namespace InventoryTools
{
    public class NetworkMonitor : IDisposable
    {
        private GameNetwork _gameNetwork;
        private DataManager _dataManager;
        private ushort _retainerInformationOpcode;

        
        public NetworkMonitor(GameNetwork gameNetwork, DataManager dataManager)
        {
            _gameNetwork = gameNetwork;
            _dataManager = dataManager;
            _retainerInformationOpcode = (ushort)(_dataManager.ServerOpCodes.TryGetValue("RetainerInformation", out var code) ? code : 0x0318);

            _gameNetwork.NetworkMessage +=OnNetworkMessage;
        }
        
        public delegate void RetainerInformationUpdatedDelegate(NetworkRetainerInformation retainerInformation);
        public event RetainerInformationUpdatedDelegate OnRetainerInformationUpdated;

        private void OnNetworkMessage(IntPtr dataptr, ushort opcode, uint sourceactorid, uint targetactorid, NetworkMessageDirection direction)
        {
            if (opcode == _retainerInformationOpcode && direction == NetworkMessageDirection.ZoneDown) //Retainer Info - 0x02E3?
            {
                var retainerInformation = NetworkDecoder.DecodeRetainerInformation(dataptr);
                OnRetainerInformationUpdated?.Invoke(retainerInformation);
            }
        }

        public void Dispose()
        {
            _gameNetwork.NetworkMessage -= OnNetworkMessage;
        }
    }
}