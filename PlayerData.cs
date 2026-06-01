using System;

namespace MyRageMPServer
{
    public class PlayerData
    {
        public int Id{ get; set; }
        public string Login{ get; set; }
        public string PasswordHash{ get; set; }
        public int Money{ get; set; }
        public float Health{ get; set; }
        public float PosX{ get; set; }
        public float PosY{ get; set; }
        public float PosZ{ get; set; }
        public DateTime CreatedAt{ get; set; }
        public DateTime LastLogin{ get; set; }
        public int Level{ get; set; }
        public int Experience{ get; set; }
        public int AdminLevel{ get; set; }
        public bool IsMuted{ get; set; }
        public int FactionId{ get; set; }
    }
}
