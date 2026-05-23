using System;
using System.Collections.Generic;
using GTANetworkAPI;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;


namespace MyRageMPServer
{
    public class InventoryManager
    {
        private AuthManager _auth;
    
        private string _connectionString = "Server=localhost;Database=ragemp;User=ragemp;Password=password123;";

         public InventoryManager(AuthManager auth)
            {
                _auth = auth;
            }

        private Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>
            {
                { "bread", new ItemDefinition { Name = "Хлеб", HealthRestore = 20, Description = "Восстанавливает 20 здоровья" }},
                { "bandage", new ItemDefinition { Name = "Бинт", HealthRestore = 30, Description = "Восстанавливает 30 здоровья" }},
                { "water", new ItemDefinition { Name = "Вода", HealthRestore = 10, Description = "Восстанавливает 10 здоровья" }}
            };
        public Dictionary<string, int> GetInventory(Player player)
        {
            var inventory = new Dictionary<string, int>();
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return inventory;
            // Получаем инвентарь игрока из базы данных
                
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open(); 
                    
                    var cmd = new MySqlCommand("SELECT item_name, quantity FROM inventory WHERE player_id = @player_id", connection);
                    cmd.Parameters.AddWithValue("@player_id", playerData.Id); // защита от SQL инъекций
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string itemName = reader.GetString("item_name");
                            int quantity = reader.GetInt32("quantity");
                            inventory[itemName] = quantity;
                        }
                    }
                }
                return inventory;
            
        
        }
        public void AddItem(Player player, string item, int quantity)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var subject = new MySqlCommand("SELECT item_name, quantity FROM inventory WHERE player_id = @player_id and item_name = @item_name", connection);
                subject.Parameters.AddWithValue("@player_id", playerData.Id); // защита от SQL инъекций
                subject.Parameters.AddWithValue("@item_name", item);

                if (subject.ExecuteScalar() != null)
                {
                    var updateCmd = new MySqlCommand("UPDATE inventory SET quantity = quantity + @quantity WHERE player_id = @player_id AND item_name = @item_name", connection);
                    updateCmd.Parameters.AddWithValue("@quantity", quantity);
                    updateCmd.Parameters.AddWithValue("@player_id", playerData.Id);
                    updateCmd.Parameters.AddWithValue("@item_name", item);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    var insertCmd = new MySqlCommand("INSERT INTO inventory (player_id, item_name, quantity) VALUES (@player_id, @item_name, @quantity)", connection);
                    insertCmd.Parameters.AddWithValue("@player_id", playerData.Id);
                    insertCmd.Parameters.AddWithValue("@item_name", item);
                    insertCmd.Parameters.AddWithValue("@quantity", quantity);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        public void RemoveItem(Player player, string item, int quantity)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var subject = new MySqlCommand("SELECT quantity FROM inventory WHERE player_id = @player_id and item_name = @item_name", connection);
                subject.Parameters.AddWithValue("@player_id", playerData.Id); // защита от SQL инъекций
                subject.Parameters.AddWithValue("@item_name", item);

                var currentQuantityObj = subject.ExecuteScalar();
                if (currentQuantityObj != null)
                {
                    int currentQuantity = Convert.ToInt32(currentQuantityObj);
                    if (currentQuantity > quantity)
                    {
                        var updateCmd = new MySqlCommand("UPDATE inventory SET quantity = quantity - @quantity WHERE player_id = @player_id AND item_name = @item_name", connection);
                        updateCmd.Parameters.AddWithValue("@quantity", quantity);
                        updateCmd.Parameters.AddWithValue("@player_id", playerData.Id);
                        updateCmd.Parameters.AddWithValue("@item_name", item);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var deleteCmd = new MySqlCommand("DELETE FROM inventory WHERE player_id = @player_id AND item_name = @item_name", connection);
                        deleteCmd.Parameters.AddWithValue("@player_id", playerData.Id);
                        deleteCmd.Parameters.AddWithValue("@item_name", item);
                        deleteCmd.ExecuteNonQuery();
                    }
                }
            }
        }
        public void UseItem(Player player, string item)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return;

            if (_items.ContainsKey(item))
            {
                var itemDef = _items[item];
                
                player.Health = Math.Min(100, player.Health + itemDef.HealthRestore);
                RemoveItem(player, item, 1);
                player.SendChatMessage($"Ты использовал {itemDef.Name}. {itemDef.Description}");
            }
            else
            {
                player.SendChatMessage("Ошибка: Предмет не найден.");
            }
        }
    }
}