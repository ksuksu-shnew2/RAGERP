using System;
using System.Collections.Generic;
using GTANetworkAPI;

namespace MyRageMPServer
{
    public class FactionManager
    {
         private AuthManager _auth;

        public FactionManager(AuthManager auth)
            {
                _auth = auth;
            }
        private Dictionary<int, string> _factions = new Dictionary<int, string>
            {
                { 0, "Без фракции" },
                { 1, "Полиция" },
                { 2, "Мафия" },
                { 3, "Медики" }
            };
        public void SetFaction(Player admin, Player target, int factionId)
        {
             if (_auth.IsAdmin(admin,2))
            {
                var targetData = _auth.GetPlayerData(target);
                if (targetData != null)
                {
                    targetData.FactionId = factionId;
                    _auth.UpdatePlayer(targetData);
                }

            }
        }

       public string GetFactionName(int factionId)
            {
                if (_factions.ContainsKey(factionId))
                    return _factions[factionId];
                return "Неизвестно";
            }
        public bool IsInFaction(Player player,int factionId)
        {
            var playerData = _auth.GetPlayerData(player);
            return playerData != null && playerData.FactionId == factionId;
            
        }
        
    }
}