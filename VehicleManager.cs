using System;
using System.Collections.Generic;
using GTANetworkAPI;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace MyRageMPServer
{
    public class VehicleManager
    {
        private AuthManager _auth;
    
        private string _connectionString = "Server=localhost;Database=ragemp;User=ragemp;Password=password123;";


        private Dictionary<string, int> _carPrices = new Dictionary<string, int>
        {
            { "sultan", 5000 },
            { "zentorno", 50000 },
            { "adder", 100000 },
            { "elegy", 10000 }
        };

         public VehicleManager(AuthManager auth)
            {
                _auth = auth;
            }

        public Dictionary<string, int> GetPlayerCars(Player player)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return new Dictionary<string, int>();

            var cars = new Dictionary<string, int>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand("SELECT model FROM vehicles WHERE player_id = @player_id", connection);
                cmd.Parameters.AddWithValue("@player_id", playerData.Id);
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string model = reader.GetString("model");
                        if (_carPrices.ContainsKey(model))
                        {
                            cars.Add(model, _carPrices[model]);
                        }
                    }
                }
            }
            return cars;
        }
        public void BuyCar(Player player, string model)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return;

            if (!_carPrices.ContainsKey(model))
            {
                player.SendChatMessage("Ошибка: Такой модели автомобиля нет в продаже.");
                return;
            }

            int price = _carPrices[model];
            if (playerData.Money < price)
            {
                player.SendChatMessage("Ошибка: Недостаточно денег для покупки этого автомобиля.");
                return;
            }

            _auth.TakeMoney(player, price);

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand("INSERT INTO vehicles (player_id, model) VALUES (@player_id, @model)", connection);
                cmd.Parameters.AddWithValue("@player_id", playerData.Id);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.ExecuteNonQuery();
            }

            player.SendChatMessage($"Поздравляем! Вы купили {model} за ${price}.");
        }

        public void SpawnCar(Player player, string model)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand("SELECT COUNT(*) FROM vehicles WHERE player_id = @player_id AND model = @model", connection);
                cmd.Parameters.AddWithValue("@player_id", playerData.Id);
                cmd.Parameters.AddWithValue("@model", model);
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count > 0)
                {
                    // Здесь можно добавить код для спавна автомобиля в игре
                    NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(model), player.Position, player.Heading, 0, 0);
                    player.SendChatMessage($"Ваш {model} был заспавнен.");
                }
                else
                {
                    player.SendChatMessage("Ошибка: У вас нет такого автомобиля.");
                }
        
            }
        }
    }
}
