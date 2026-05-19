using System;
using System.Collections.Generic;
using GTANetworkAPI;
using System.Security.Cryptography;

namespace MyRageMPServer
{
    public class AuthManager
    {
        private Dictionary<Player, PlayerData> _authorizedPlayers = new Dictionary<Player, PlayerData>();

        public bool Register(Player player, string login, string password)
        {
            if (FindPlayer(login) != null)
                return false; // Login already exists

            var playerData = new PlayerData
            {
                Id = _authorizedPlayers.Count + 1,
                Login = login,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            _authorizedPlayers[player] = playerData;
            return true;
        }

        public PlayerData Login(Player player,string login, string password)
        {
            if(FindPlayer(login) is PlayerData playerData)
            {
                if (VerifyPassword(password, playerData.PasswordHash))
                {
                    playerData.LastLogin = DateTime.UtcNow;
                    _authorizedPlayers[player] = playerData;
                    return playerData;
                }
            }
            return null; // Invalid login or password
            
        }

        public void Logout(Player player)
        {
            _authorizedPlayers.Remove(player);
        } 

        public bool IsAuthorized(Player player)

        {
            return _authorizedPlayers.ContainsKey(player); 
        }

        public PlayerData GetPlayerData(Player player)
        {
           _authorizedPlayers.TryGetValue(player, out var playerData);
           return playerData;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private PlayerData FindPlayer(string login)
            {
                return null; // реализуем когда подключим MySQL
            }
    }
}