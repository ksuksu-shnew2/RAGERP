using System.IO;
using System.Text.Json;

namespace MyRageMPServer
{
    public static class Config
    {
        private static string _connectionString;
        private static string _discordWebhook;

        static Config()
        {
            var json = File.ReadAllText("appsettings.json");
            var doc = JsonDocument.Parse(json);
            _connectionString = doc.RootElement.GetProperty("ConnectionString").GetString();
            _discordWebhook = doc.RootElement.GetProperty("DiscordWebhook").GetString();
        }

        public static string GetConnectionString() => _connectionString;
        public static string GetDiscordWebhook() => _discordWebhook;
    }
}