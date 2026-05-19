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
                }
                else
                {
                    player.SendChatMessage("Ошибка: Логин уже существует.");
                }
            }
        [Command("login")]
            public void LoginCommand(Player player, string login, string password)
            {
                if (_auth.Login(player, login, password) != null)
                {
                    player.SendChatMessage("Вход успешен!");
                }
                else
                {
                    player.SendChatMessage("Ошибка: Неверный логин или пароль.");
                }
            }
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            player.SendChatMessage("Введи /register логин пароль или /login логин пароль для авторизации.");
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            _auth.Logout(player);     
        }
    }
}
