using System;
using System.Collections.Generic;
using GTANetworkAPI;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;


namespace MyRageMPServer
{
    public class InventoryManager
    {
        private AuthManager _auth;
    
        

         public InventoryManager(AuthManager auth)
            {
                _auth = auth;
            }

        public Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>
            {
                { "bread", new ItemDefinition { Name = "Хлеб", HealthRestore = 20, Description = "Восстанавливает 20 здоровья", Price = 50 }},
                { "bandage", new ItemDefinition { Name = "Бинт", HealthRestore = 30, Description = "Восстанавливает 30 здоровья", Price = 100 }},
                { "water", new ItemDefinition { Name = "Вода", HealthRestore = 10, Description = "Восстанавливает 10 здоровья", Price = 30 }}            
            };
        public async Task<Dictionary<string, int>> GetInventory(Player player)
        {
            var inventory = new Dictionary<string, int>();
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return inventory;
            // Получаем инвентарь игрока из базы данных
                
                using (var connection = new MySqlConnection(Config.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    
                    var cmd = new MySqlCommand("SELECT item_name, quantity FROM inventory WHERE player_id = @player_id", connection);
                    cmd.Parameters.AddWithValue("@player_id", playerData.Id); // защита от SQL инъекций
                    
                    using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string itemName = reader.GetString("item_name");
                            int quantity = reader.GetInt32("quantity");
                            inventory[itemName] = quantity;
                        }
                    }
                }
                return inventory;
            
        
        }
        public async Task AddItem(Player player, string item, int quantity)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return;

            using (var connection = new MySqlConnection(Config.GetConnectionString()))
            {
                await connection.OpenAsync();

                var subject = new MySqlCommand("SELECT item_name, quantity FROM inventory WHERE player_id = @player_id and item_name = @item_name", connection);
                subject.Parameters.AddWithValue("@player_id", playerData.Id); // защита от SQL инъекций
                subject.Parameters.AddWithValue("@item_name", item);

                if (await subject.ExecuteScalarAsync() != null)
                {
                    var updateCmd = new MySqlCommand("UPDATE inventory SET quantity = quantity + @quantity WHERE player_id = @player_id AND item_name = @item_name", connection);
                    updateCmd.Parameters.AddWithValue("@quantity", quantity);
                    updateCmd.Parameters.AddWithValue("@player_id", playerData.Id);
                    updateCmd.Parameters.AddWithValue("@item_name", item);
                    await updateCmd.ExecuteNonQueryAsync();
                }
                else
                {
                    var insertCmd = new MySqlCommand("INSERT INTO inventory (player_id, item_name, quantity) VALUES (@player_id, @item_name, @quantity)", connection);
                    insertCmd.Parameters.AddWithValue("@player_id", playerData.Id);
                    insertCmd.Parameters.AddWithValue("@item_name", item);
                    insertCmd.Parameters.AddWithValue("@quantity", quantity);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task RemoveItem(Player player, string item, int quantity)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return;

            using (var connection = new MySqlConnection(Config.GetConnectionString()))
            {
                await connection.OpenAsync();

                var subject = new MySqlCommand("SELECT quantity FROM inventory WHERE player_id = @player_id and item_name = @item_name", connection);
                subject.Parameters.AddWithValue("@player_id", playerData.Id); // защита от SQL инъекций
                subject.Parameters.AddWithValue("@item_name", item);

                var currentQuantityObj = await subject.ExecuteScalarAsync();
                if (currentQuantityObj != null)
                {
                    int currentQuantity = Convert.ToInt32(currentQuantityObj);
                    if (currentQuantity > quantity)
                    {
                        var updateCmd = new MySqlCommand("UPDATE inventory SET quantity = quantity - @quantity WHERE player_id = @player_id AND item_name = @item_name", connection);
                        updateCmd.Parameters.AddWithValue("@quantity", quantity);
                        updateCmd.Parameters.AddWithValue("@player_id", playerData.Id);
                        updateCmd.Parameters.AddWithValue("@item_name", item);
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        var deleteCmd = new MySqlCommand("DELETE FROM inventory WHERE player_id = @player_id AND item_name = @item_name", connection);
                        deleteCmd.Parameters.AddWithValue("@player_id", playerData.Id);
                        deleteCmd.Parameters.AddWithValue("@item_name", item);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
        public async Task UseItem(Player player, string item)
        {
            var playerData = _auth.GetPlayerData(player);
            if (playerData == null) return;

            if (_items.ContainsKey(item))
            {
                var itemDef = _items[item];
                
                player.Health = Math.Min(100, player.Health + itemDef.HealthRestore);
                await RemoveItem(player, item, 1);
                player.SendChatMessage($"Ты использовал {itemDef.Name}. {itemDef.Description}");
            }
            else
            {
                player.SendChatMessage("Ошибка: Предмет не найден.");
            }
        }
    }
}