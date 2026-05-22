using System;
using System.Collections.Generic;
using GTANetworkAPI;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace MyRageMPServer
{
    public class AuthManager
    {
        private Dictionary<Player, PlayerData> _authorizedPlayers = new Dictionary<Player, PlayerData>();
        private string _connectionString = "Server=localhost;Database=ragemp;User=ragemp;Password=password123;";

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

            CreatePlayer(playerData); // сохраняем в БД
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
           if (_authorizedPlayers.TryGetValue(player, out var playerData))
                {
                    UpdatePlayer(playerData); // сохраняем в БД
                    _authorizedPlayers.Remove(player);
                }
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
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open(); // открываем соединение
                    
                    var cmd = new MySqlCommand("SELECT * FROM players WHERE login = @login", connection);
                    cmd.Parameters.AddWithValue("@login", login); // защита от SQL инъекций
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // если нашли запись
                        {
                            return new PlayerData
                            {
                                Id = reader.GetInt32("id"),
                                Login = reader.GetString("login"),
                                PasswordHash = reader.GetString("password_hash"),
                                Money = reader.GetInt32("money"),
                                Health = reader.GetFloat("health"),
                                PosX = reader.GetFloat("pos_x"),
                                PosY = reader.GetFloat("pos_y"),
                                PosZ = reader.GetFloat("pos_z"),
                                Level = reader.GetInt32("level"),
                                Experience = reader.GetInt32("experience"),
                                AdminLevel = reader.GetInt32("admin_level")
                            };
                        }
                    }
                }
                return null; // не нашли
            }
        private PlayerData CreatePlayer(PlayerData playerData)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand("INSERT INTO players (login, password_hash, created_at) VALUES (@login, @password_hash, @created_at); SELECT LAST_INSERT_ID();", connection);
                cmd.Parameters.AddWithValue("@login", playerData.Login);
                cmd.Parameters.AddWithValue("@password_hash", playerData.PasswordHash);
                cmd.Parameters.AddWithValue("@created_at", playerData.CreatedAt);

                var id = Convert.ToInt32(cmd.ExecuteScalar());
                playerData.Id = id;
                return playerData;
            }
        }

        private PlayerData UpdatePlayer(PlayerData playerData)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand("UPDATE players SET money=@money, health=@health, pos_x=@pos_x, pos_y=@pos_y, pos_z=@pos_z, last_login=@last_login, level=@level, experience=@experience, admin_level=@admin_level WHERE id=@id", connection);
            
                cmd.Parameters.AddWithValue("@id", playerData.Id);
                cmd.Parameters.AddWithValue("@money", playerData.Money);
                cmd.Parameters.AddWithValue("@health", playerData.Health);
                cmd.Parameters.AddWithValue("@pos_x", playerData.PosX);
                cmd.Parameters.AddWithValue("@pos_y", playerData.PosY);
                cmd.Parameters.AddWithValue("@pos_z", playerData.PosZ);
                cmd.Parameters.AddWithValue("@last_login", playerData.LastLogin);
                cmd.Parameters.AddWithValue("@level", playerData.Level);
                cmd.Parameters.AddWithValue("@experience", playerData.Experience);
                cmd.Parameters.AddWithValue("@admin_level", playerData.AdminLevel);
                cmd.ExecuteNonQuery();
                return playerData;
            }
        }

        public void AddExperience(Player player, int amount)
        {
            if (IsAuthorized(player))
            {
                var playerData = GetPlayerData(player);
                playerData.Experience += amount;
                if (playerData.Experience >= GetExperienceForNextLevel(playerData.Level))
                {
                    playerData.Level++;
                    playerData.Experience = 0;
                    player.SendChatMessage($"Поздравляем! Ты достиг уровня {playerData.Level}!");
                }
                UpdatePlayer(playerData);
            }
        }

        private int GetExperienceForNextLevel(int level)
        {
            return 100 * level; // простая формула для примера
        }

        public void LevelUp(Player player)
        {
            if (IsAuthorized(player))
            {
                var playerData = GetPlayerData(player);
                playerData.Level++;
                playerData.Experience = 0;
                UpdatePlayer(playerData);
                player.SendChatMessage($"Поздравляем! Ты достиг уровня {playerData.Level}!");
            }
        }

        public void GiveMoney(Player player, int amount)
        {
            if (IsAuthorized(player))
            {
                var playerData = GetPlayerData(player);
                playerData.Money += amount;
                UpdatePlayer(playerData);
            }
        }

        public bool TakeMoney(Player player, int amount)
        {
            if (IsAuthorized(player))
            {
                var playerData = GetPlayerData(player);
                if (playerData.Money >= amount)
                {
                    playerData.Money -= amount;
                    UpdatePlayer(playerData);
                    return true;
                }
            }
            return false;
        }

        public bool IsAdmin(Player player, int minLevel = 1)
            {
                var playerData = GetPlayerData(player);
                return playerData != null && playerData.AdminLevel >= minLevel;
            }
        public void SetAdminLevel(Player player, int level)
        {
            if (IsAuthorized(player))
            {
                var playerData = GetPlayerData(player);
                playerData.AdminLevel = level;
                UpdatePlayer(playerData);
                player.SendChatMessage($"Твой админ уровень был установлен на {level}.");
            }
        }

    }
}