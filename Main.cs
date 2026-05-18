using AlternateLife.RageMP.Net.Scripting;
using AlternateLife.RageMP.Net.EventArgs;

namespace MyRageMPServer
{
    public class Main
    {
        public Main()
        {
            MP.Logger.Info("=== Сервер запущен! ===");
            MP.Events.PlayerJoin += OnPlayerJoin;
            MP.Events.PlayerQuit += OnPlayerQuit;
        }

        private async System.Threading.Tasks.Task OnPlayerJoin(object sender, PlayerEventArgs e)
        {
            var name = await e.Player.GetNameAsync();
            await e.Player.OutputChatBoxAsync("Добро пожаловать, " + name + "!");
            MP.Logger.Info(name + " зашёл на сервер!");
        }

        private void OnPlayerQuit(object sender, PlayerQuitEventArgs e)
        {
            MP.Logger.Info("Игрок вышел: " + e.Reason);
        }
    }
}
