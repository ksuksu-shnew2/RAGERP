using System;
using GTANetworkAPI;

namespace MyRageMPServer
{
    public class Main : Script
    {  
        public AuthManager _auth = new AuthManager();
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Util.ConsoleOutput("=== Сервер запущен! ===");
        }
        [Command("register")]
            public void RegisterCommand(Player player, string login, string password)
            {
                if (_auth.Register(player, login, password))
                {
                    player.SendChatMessage("Регистрация успешна! Теперь ты можешь войти с помощью /login.");
                    
                    player.Position = new Vector3(-269.0f, -955.0f, 31.0f);
                }
                else
                {
                    player.SendChatMessage("Ошибка: Логин уже существует.");
                }
            }
        [Command("login")]
            public void LoginCommand(Player player, string login, string password)
            {
                var playerData = _auth.Login(player, login, password);
                if (playerData != null)
                {
                    player.SendChatMessage("Вход успешен!");
                    
                    player.Position = new Vector3(playerData.PosX, playerData.PosY, playerData.PosZ);
                    player.Health = (int)playerData.Health;
                }
                else
                {
                    player.SendChatMessage("Ошибка: Неверный логин или пароль.");
                }
            }
        [Command("balance")]
            public void BalanceCommand(Player player)
            {
                if (_auth.IsAuthorized(player))
                {
                    var playerData = _auth.GetPlayerData(player);
                    player.SendChatMessage($"Твой баланс: ${playerData.Money}");
                }
                else
                {
                    player.SendChatMessage("Ошибка: Ты не авторизован. Введи /login для входа.");
                }
            }
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            
            player.SendChatMessage("Введи /register логин пароль или /login логин пароль для авторизации.");
            player.TriggerEvent("playerJoinedServer", player.Name);
            
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData != null)
            {
                playerData.PosX = player.Position.X;
                playerData.PosY = player.Position.Y;
                playerData.PosZ = player.Position.Z;
                playerData.Health = player.Health;
            }
            
            _auth.Logout(player); 
                
        }
    }
}
