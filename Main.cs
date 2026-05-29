using System;
using System.Threading.Tasks;
using GTANetworkAPI;

namespace MyRageMPServer
{
    public class Main : Script
    {  
        public AuthManager _auth = new AuthManager();
        public InventoryManager _inventory;

        public FactionManager _faction;
        public VehicleManager _vehicle;

        public Main()
        {
            _inventory = new InventoryManager(_auth);
            _vehicle = new VehicleManager(_auth);
            _faction = new FactionManager(_auth);
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Util.ConsoleOutput("=== Сервер запущен! ===");
        }
        [Command("register")]
            public async Task RegisterCommand(Player player, string login, string password)
            {
                if (await _auth.Register(player, login, password))
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
            public async Task LoginCommand(Player player, string login, string password)
            {
                var playerData = await _auth.Login(player, login, password);
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
        [Command("stats")]
            public void StatsCommand(Player player)
            {
                if (_auth.IsAuthorized(player))
                {
                    var playerData = _auth.GetPlayerData(player);
                    player.SendChatMessage($"Уровень: {playerData.Level}, Опыт: {playerData.Experience}");
                }
                else
                {
                    player.SendChatMessage("Ошибка: Ты не авторизован. Введи /login для входа.");
                }
            }
        [Command("givemoney")]
            public void GiveMoneyCommand(Player player, int amount)
            {
                if (_auth.IsAuthorized(player))
                {
                    _auth.GiveMoney(player, amount);
                    player.SendChatMessage($"Тебе было добавлено ${amount}.");
                }
                else
                {
                    player.SendChatMessage("Ошибка: Ты не авторизован. Введи /login для входа.");
                }
            }
        [Command("takemoney")]
            public async Task TakeMoneyCommand(Player player, int amount)
            {
                if (_auth.IsAuthorized(player))
                {
                    if (await _auth.TakeMoney(player, amount))
                    {
                        player.SendChatMessage($"У тебя было снято ${amount}.");
                    }
                    else                    {
                        player.SendChatMessage("Ошибка: Недостаточно средств.");
                    }
                }
                else                {
                    player.SendChatMessage("Ошибка: Ты не авторизован. Введи /login для входа.");
                }
            }
        [Command("pay")]
        public async Task PayCommand(Player player, Player target, int amount)
            {
                if (_auth.IsAuthorized(player))
                {
                    if (await _auth.TakeMoney(player, amount))
                    {
                        _auth.GiveMoney(target, amount);
                        player.SendChatMessage($"Ты заплатил ${amount} игроку {target.Name}.");
                        target.SendChatMessage($"Ты получил ${amount} от игрока {player.Name}.");
                    }
                    else
                    {
                        player.SendChatMessage("Ошибка: Недостаточно средств для оплаты.");
                    }
                }
                else
                {
                    player.SendChatMessage("Ошибка: Ты не авторизован. Введи /login для входа.");
                }
            }
        [ServerEvent(Event.PlayerConnected)]
        public async void OnPlayerConnected(Player player)
        {
            if (await _auth.IsBannedAsync(player.Name))
            {
                player.Kick("Вы забанены на этом сервере.");
                return;
            }
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
        [Command("kick")]
            public void KickCommand(Player player, Player target, string reason)
            {
                if (_auth.IsAdmin(player))
                {
                    target.Kick(reason);
                    player.SendChatMessage($"Игрок {target.Name} был кикнут. Причина: {reason}");
                }
                else
                {
                    player.SendChatMessage("Ошибка: У тебя нет прав для выполнения этой команды.");
                }
            }
        [Command("setadmin")]
            public void SetAdminCommand(Player player, Player target, int level)
            {
                if (_auth.IsAdmin(player, 3))
                {
                    _auth.SetAdminLevel(target, level);
                    player.SendChatMessage($"У игрока {target.Name} теперь уровень администратора: {level}");
                }
                else
                {
                    player.SendChatMessage("Ошибка: У тебя нет прав для выполнения этой команды.");
                }
            }
        [Command("ban")]
            public async Task BanCommand(Player player, Player target, string reason)
            {
                if (_auth.IsAdmin(player, 2))
                {
                    await _auth.BanPlayerAsync(player, target, reason);
                    target.Kick(reason);
                    player.SendChatMessage($"Игрок {target.Name} был забанен. Причина: {reason}");
                }
                else
                {
                    player.SendChatMessage("Ошибка: У тебя нет прав для выполнения этой команды.");
                }
            }
        [Command("unban")]
            public async Task UnbanCommand(Player player, string targetName)
            {
                if (_auth.IsAdmin(player, 2))
                {
                    await _auth.UnbanPlayerAsync(targetName);
                    player.SendChatMessage($"Игрок {targetName} был разбанен.");
                }
                else
                {
                    player.SendChatMessage("Ошибка: У тебя нет прав для выполнения этой команды.");
                }
            }
        [Command("mute")]
            public void MuteCommand(Player player, Player target)
            {
                if (_auth.IsAdmin(player))
                {
                    _auth.MutePlayer(player, target);
                }
                else
                {
                    player.SendChatMessage("Ошибка: У тебя нет прав для выполнения этой команды.");
                }
            }
        [Command("unmute")]
            public void UnmuteCommand(Player player, Player target)
            {
                if (_auth.IsAdmin(player))
                {
                    _auth.UnmutePlayer(player, target);
                }
                else
                {
                    player.SendChatMessage("Ошибка: У тебя нет прав для выполнения этой команды.");
                }
            }
        [Command("inventory")]
            public async Task InventoryCommand(Player player)
            {
                var inventory = await _inventory.GetInventory(player);
                if (inventory.Count == 0)
                    {
                        player.SendChatMessage("Твой инвентарь пуст.");
                        return;
                    }
                    foreach (var item in inventory)
                        player.SendChatMessage($"{item.Key}: {item.Value} шт.");
            }
        [Command("giveitem")]
            public async Task AddItemCommand(Player player, string item, int quantity)
            {
                await _inventory.AddItem(player, item, quantity);
                player.SendChatMessage($"Ты получил {quantity}x {item}.");
            }
        [Command("useitem")]
            public async Task UseItemCommand(Player player, string item)
            {
                await _inventory.UseItem(player, item);
            }
        [Command("buycar")]
            public async Task BuyCarCommand(Player player, string model)
            {
               await _vehicle.BuyCar(player, model);
            }
        [Command("spawncar")]
            public void SpawnCarCommand(Player player, string model)
            {                
                _vehicle.SpawnCar(player, model);  
            }
        [Command("mycars")]
        public async Task MyCarsCommand(Player player)
        {
            var cars = await _vehicle.GetPlayerCars(player);
            if (cars.Count == 0)
            {
                player.SendChatMessage("У тебя нет автомобилей.");
                return;         
            }
            foreach (var car in cars)
            {
                player.SendChatMessage($"{car.Key} - ${car.Value}");    
            }
        }
        [Command("shop")]
        public void ShopItem(Player player)
        {
            if (_auth.IsAuthorized(player))
            {player.SendChatMessage("=== Магазин ===");
             foreach(var item in _inventory._items)
             {
                player.SendChatMessage($"{item.Value.Name} - ${item.Value.Price}");    
             }}
            else
            player.SendChatMessage("Ошибка: У тебя нет прав для выполнения этой команды.");

        }
        [Command("buy")]
            public async Task BuyCommand(Player player, string itemKey, int quantity)
            {
                if (!_auth.IsAuthorized(player))
                {
                    player.SendChatMessage("Ошибка: Ты не авторизован.");
                    return;
                }

                if (!_inventory._items.ContainsKey(itemKey))
                {
                    player.SendChatMessage("Ошибка: Такого предмета нет в магазине.");
                    return;
                }

                var item = _inventory._items[itemKey];
                int amount = item.Price * quantity;

                if (await _auth.TakeMoney(player, amount))
                {
                    await _inventory.AddItem(player, itemKey, quantity);
                    player.SendChatMessage($"Вы купили {item.Name} x{quantity} за ${amount}.");
                }
                else
                {
                    player.SendChatMessage("Ошибка: Недостаточно денег.");
                }
            }
        [Command("setfaction")]
            public async Task SetFactionCommand(Player player, Player target, int id_factionId)
                {
                    await _faction.SetFaction(player,target,id_factionId);
                }
        [Command("myfaction")]
            public void MyfactionCommand(Player player)
                {
                    var playerData = _auth.GetPlayerData(player);
                    player.SendChatMessage(_faction.GetFactionName(playerData.FactionId));
                }
        [Command("arrest")]
            public void ArrestCommand(Player player,Player target)
                {
                    if (!_faction.IsInFaction(player, 1))
                    {
                        player.SendChatMessage("Ошибка: Эта команда только для полиции.");
                        return;
                    }
                    string reason = "Арест";
                    target.Kick(reason);
                    player.SendChatMessage($"Игрок {target.Name} был арестован.");
                }

            [Command("heal")]
            public void HealCommand(Player player,Player target)
                {
                    if (!_faction.IsInFaction(player, 3))
                    {
                        player.SendChatMessage("Ошибка: Эта команда только для медиков.");
                        return;
                    }
                    target.Health = 100;
                    player.SendChatMessage($"Игрок {target.Name} восстановлен до 100% здоровья.");
                    target.SendChatMessage($"Медик {player.Name} восстановил твоё здоровье.");
                }
    }
}
