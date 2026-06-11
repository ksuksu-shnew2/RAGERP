using System;
using System.Collections.Generic;
using GTANetworkAPI;

namespace MyRageMPServer
{
    public class CooldownManager
    {


        private Dictionary<Player, Dictionary<string, DateTime>> cooldowns = new Dictionary<Player, Dictionary<string, DateTime>>();
       
        public bool IsOnCooldown(Player player, string action)
        {
          
             if (cooldowns.ContainsKey(player) && cooldowns[player].ContainsKey(action))
            {
                return cooldowns[player][action] > DateTime.Now;
            }
            
            return false;
        }

        public void SetCooldown(Player player,string action, TimeSpan duration)
        {
            if (!cooldowns.ContainsKey(player))
            {
                cooldowns[player] = new  Dictionary<string, DateTime>();
            }
            cooldowns[player][action] = DateTime.Now.Add(duration);
        }

        public TimeSpan GetRemainingCooldown(Player player, string action)
        {
            if (IsOnCooldown(player, action))
            {
                return cooldowns[player][action] - DateTime.Now;
            }
            return TimeSpan.Zero;
        }
        
    }
}