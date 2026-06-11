using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;

namespace MyRageMPServer

{
    public class CaptchaManager
    {
        private Dictionary<Player, string> activeCaptchas = new Dictionary<Player, string>();
        private HashSet<Player> _verified = new HashSet<Player>();

        public bool IsVerified(Player player) => _verified.Contains(player);

        public void SetVerified(Player player) => _verified.Add(player);

        public void GenerateCaptcha(Player player)
        {
            string captcha = GenerateRandomCaptcha();
            activeCaptchas[player] = captcha;
            player.SendChatMessage($"Пожалуйста, введите капчу: {captcha}");
        }

        public bool VerifyCaptcha(Player player, string input)
        {
            if (activeCaptchas.ContainsKey(player) && activeCaptchas[player] == input)
            {
                activeCaptchas.Remove(player);
                return true;
            }
            return false;
        }

        private string GenerateRandomCaptcha()
        {
            const string chars = "0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public void Remove(Player player)
            {
                activeCaptchas.Remove(player);
                _verified.Remove(player);
            }
    }
}