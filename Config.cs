using System.IO;
using System.Text.Json;

namespace MyRageMPServer
{
    public static class Config
    {
        private static string _connectionString;

        static Config()
        {
            var json = File.ReadAllText("appsettings.json");
            var doc = JsonDocument.Parse(json);
            _connectionString = doc.RootElement.GetProperty("ConnectionString").GetString();
        }

        public static string GetConnectionString() => _connectionString;
    }
}