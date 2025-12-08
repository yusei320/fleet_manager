using MySql.Data.MySqlClient;
using System;

namespace FleetManager
{
    public static class AuthService
    {
        // Centralise ta chaîne, évite la duplication
        private static string connectionString = "server=localhost;Port=3309;database=fleet_managers;uid=root;pwd=;";

        public static string GetUserRole(int userId)
        {
            if (userId <= 0) return string.Empty;

            using MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT role FROM utilisateurs WHERE id=@id";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            object result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? result.ToString() : string.Empty;
        }

        public static bool IsAdmin(int userId)
        {
            var role = GetUserRole(userId);
            return !string.IsNullOrEmpty(role) && role.Equals("Administrateur", StringComparison.OrdinalIgnoreCase);
        }

        // Optionnel : retourner d'autres infos si besoin
        public static bool UserExists(int userId)
        {
            if (userId <= 0) return false;
            using MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();
            string query = "SELECT COUNT(1) FROM utilisateurs WHERE id=@id";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }
}