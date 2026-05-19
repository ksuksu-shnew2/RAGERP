using System;

namespace MyRageMPServer
{
    public class PlayerData
    {
        public int Id;
        public string Login;
        public string PasswordHash;
        public int Money;
        public float Health = 100f;
        public float PosX, PosY, PosZ;
        public DateTime CreatedAt;
        public DateTime LastLogin;
    }
}
