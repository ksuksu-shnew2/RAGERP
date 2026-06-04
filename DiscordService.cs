
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyRageMPServer
{
    public static class DiscordService
    {
        private static readonly HttpClient _client = new HttpClient();
        public static async Task SendMessage(string message)
        {
            var payload = JsonSerializer.Serialize(new { content = message });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            await _client.PostAsync(Config.GetDiscordWebhook(), content);
        }
    }
}