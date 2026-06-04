using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using AdminPanel.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AdminPanel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private string _connectionString = "Server=localhost;Database=ragemp;User=ragemp;Password=password123;";

        [HttpGet]
        public async Task<List<PlayerModel>> GetPlayers()
        {
            var players = new List<PlayerModel>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var cmd = new MySqlCommand("SELECT id, login, money, level, admin_level, faction_id, is_muted FROM players", connection);
                using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        players.Add(new PlayerModel
                        {
                            Id = reader.GetInt32("id"),
                            Login = reader.GetString("login"),
                            Money = reader.GetInt32("money"),
                            Level = reader.GetInt32("level"),
                            AdminLevel = reader.GetInt32("admin_level"),
                            FactionId = reader.GetInt32("faction_id"),
                            IsMuted = reader.GetBoolean("is_muted")
                        });
                    }
                }
            }
            return players;
        }
    }
}
