using System;
using GTANetworkAPI;

namespace MyRageMPServer
{
    public class Main : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Util.ConsoleOutput("=== Сервер запущен! ===");
        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            NAPI.Chat.SendChatMessageToAll("[+] " + player.Name + " зашёл на сервер!");
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            NAPI.Chat.SendChatMessageToAll("[-] " + player.Name + " вышел с сервера.");
        }
    }
}
