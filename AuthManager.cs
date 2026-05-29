using System;
using System.Collections.Generic;
using GTANetworkAPI;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;


namespace MyRageMPServer
{
    public class AuthManager
    {
        private Dictionary<Player, PlayerData> _authorizedPlayers = new Dictionary<Player, PlayerData>();
        

        public async Task<bool> Register(Player player, string login, string password)
        {
            if (await FindPlayerAsync(login) != null)
                return false; 

            var playerData = new PlayerData
            {
                
                Login = login,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            await CreatePlayerAsync(playerData); // сохраняем в БД
            _authorizedPlayers[player] = playerData;
            return true;

            
        }

        public async Task<PlayerData> Login(Player player,string login, string password)
        {
            if(await FindPlayerAsync(login) is PlayerData playerData)
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

        public async void Logout(Player player)
        {
           if (_authorizedPlayers.TryGetValue(player, out var playerData))
                {
                    await UpdatePlayerAsync(playerData); // сохраняем в БД
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

        private async Task<PlayerData> FindPlayerAsync(string login)
            {
                using (var connection = new MySqlConnection(Config.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    
                    var cmd = new MySqlCommand("SELECT * FROM players WHERE login = @login", connection);
                    cmd.Parameters.AddWithValue("@login", login); // защита от SQL инъекций
                    
                    using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync()) 
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
                                AdminLevel = reader.GetInt32("admin_level"),
                                IsMuted = reader.GetBoolean("is_muted"),
                                FactionId = reader.GetInt32("faction_id")
                            };
                        }
                    }
                }
                return null; 
            }
        private async Task<PlayerData> CreatePlayerAsync(PlayerData playerData)
        {
            using (var connection = new MySqlConnection(Config.GetConnectionString()))
            {
                await connection.OpenAsync();
                var cmd = new MySqlCommand("INSERT INTO players (login, password_hash, created_at) VALUES (@login, @password_hash, @created_at); SELECT LAST_INSERT_ID();", connection);
                cmd.Parameters.AddWithValue("@login", playerData.Login);
                cmd.Parameters.AddWithValue("@password_hash", playerData.PasswordHash);
                cmd.Parameters.AddWithValue("@created_at", playerData.CreatedAt);

                var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                playerData.Id = id;
                return playerData;
            }
        }

        public async Task<PlayerData> UpdatePlayerAsync(PlayerData playerData)
        {
            using (var connection = new MySqlConnection(Config.GetConnectionString()))
            {
                await connection.OpenAsync();
                var cmd = new MySqlCommand("UPDATE players SET money=@money, health=@health, pos_x=@pos_x, pos_y=@pos_y, pos_z=@pos_z, last_login=@last_login, level=@level, experience=@experience, admin_level=@admin_level, is_muted=@is_muted,faction_id=@faction_id WHERE id=@id", connection);
            
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
                cmd.Parameters.AddWithValue("@is_muted", playerData.IsMuted);
                cmd.Parameters.AddWithValue("@faction_id", playerData.FactionId);
                await cmd.ExecuteNonQueryAsync();
                return playerData;
            }
        }

        public async void AddExperience(Player player, int amount)
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
                await UpdatePlayerAsync(playerData);
            }
        }

        private int GetExperienceForNextLevel(int level)
        {
            return 100 * level; // простая формула для примера
        }

        public async void LevelUp(Player player)
        {
            if (IsAuthorized(player))
            {
                var playerData = GetPlayerData(player);
                playerData.Level++;
                playerData.Experience = 0;
                await UpdatePlayerAsync(playerData);
                player.SendChatMessage($"Поздравляем! Ты достиг уровня {playerData.Level}!");
            }
        }

        public async void GiveMoney(Player player, int amount)
        {
            if (IsAuthorized(player))
            {
                var playerData = GetPlayerData(player);
                playerData.Money += amount;
                await UpdatePlayerAsync(playerData);
            }
        }

        public async Task<bool> TakeMoney(Player player, int amount)
        {
            if (IsAuthorized(player))
            {
                var playerData = GetPlayerData(player);
                if (playerData.Money >= amount)
                {
                    playerData.Money -= amount;
                    await UpdatePlayerAsync(playerData);
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
        public async void SetAdminLevel(Player player, int level)
        {
            if (IsAuthorized(player))
            {
                var playerData = GetPlayerData(player);
                playerData.AdminLevel = level;
                await UpdatePlayerAsync(playerData);
                player.SendChatMessage($"Твой админ уровень был установлен на {level}.");
            }
        }
        public async Task<bool> IsBannedAsync(string login)
        {
            using (var connection = new MySqlConnection(Config.GetConnectionString()))
            {
                await connection.OpenAsync();
                var cmd = new MySqlCommand("SELECT id FROM bans WHERE login = @login and (expires_at > NOW() or expires_at is null)", connection);
                cmd.Parameters.AddWithValue("@login", login);
                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
        }

        public async void BanPlayerAsync(Player admin, Player target, string reason)
        {
            if (IsAdmin(admin))
            {
                var targetData = GetPlayerData(target);
                if (targetData != null)
                {
                    using (var connection = new MySqlConnection(Config.GetConnectionString()))
                    {
                        await connection.OpenAsync();
                        var cmd = new MySqlCommand("INSERT INTO bans (login, reason, banned_by, expires_at) VALUES (@login, @reason, @banned_by, NULL)", connection);
                        cmd.Parameters.AddWithValue("@login", targetData.Login);
                        cmd.Parameters.AddWithValue("@reason", reason);
                        cmd.Parameters.AddWithValue("@banned_by", admin.Name);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    target.SendChatMessage($"Ты был забанен администратором {admin.Name} по причине: {reason}");
                    target.Kick("You have been banned.");
                }
            }
        }

        public async void UnbanPlayerAsync(string login)
        {
            using (var connection = new MySqlConnection(Config.GetConnectionString()))
            {
                await connection.OpenAsync();
                var cmd = new MySqlCommand("DELETE FROM bans WHERE login = @login", connection);
                cmd.Parameters.AddWithValue("@login", login);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async void MutePlayer(Player admin, Player target)
        {
            if (IsAdmin(admin))
            {
                var targetData = GetPlayerData(target);
                if (targetData != null)
                {
                    targetData.IsMuted = true;
                    await UpdatePlayerAsync(targetData);
                    target.SendChatMessage($"Ты был замучен администратором {admin.Name} и не можешь отправлять сообщения в чат.");
                }
            }
        }
        public async void UnmutePlayer(Player admin, Player target)
        {
            if (IsAdmin(admin))
            {
                var targetData = GetPlayerData(target);
                if (targetData != null)
                {
                    targetData.IsMuted = false;
                    await UpdatePlayerAsync(targetData);
                    target.SendChatMessage($"Ты был размучен администратором {admin.Name} и теперь можешь отправлять сообщения в чат.");
                }
            }
        }

    }
}