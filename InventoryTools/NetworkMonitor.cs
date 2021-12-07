using System;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Game.Network;
using Dalamud.Logging;

namespace InventoryTools
{
    public class NetworkMonitor : IDisposable
    {
        private GameNetwork _gameNetwork;
        
        public NetworkMonitor(GameNetwork gameNetwork)
        {
            _gameNetwork = gameNetwork;
            _gameNetwork.NetworkMessage +=OnNetworkMessage;
        }
        
        public delegate void RetainerInformationUpdatedDelegate(NetworkRetainerInformation retainerInformation);
        public event RetainerInformationUpdatedDelegate OnRetainerInformationUpdated;

        private void OnNetworkMessage(IntPtr dataptr, ushort opcode, uint sourceactorid, uint targetactorid, NetworkMessageDirection direction)
        {
            if (opcode == 0x129 && direction == NetworkMessageDirection.ZoneDown) //Retainer Info - 0x02E3?
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