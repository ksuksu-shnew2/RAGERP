using System;
using System.Collections.Generic;
using GTANetworkAPI;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace MyRageMPServer
{
    public class ItemDefinition
    {
        public string Name;
        public int HealthRestore;
        public string Description;

        public int Price;
    }
}