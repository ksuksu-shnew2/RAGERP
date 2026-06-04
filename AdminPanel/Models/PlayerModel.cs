namespace AdminPanel.Models
{
    public class PlayerModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public int Money { get; set; }
        public int Level { get; set; }
        public int AdminLevel { get; set; }
        public int FactionId { get; set; }
        public bool IsMuted { get; set; }
    }
}
