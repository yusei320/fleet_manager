using Microsoft.Data.Sqlite;
using FleetManager.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace FleetManager.Services
{
    /// <summary>
    /// Service centralisé pour la gestion de la base de données SQLite
    /// Fournit toutes les méthodes nécessaires pour CRUD sur utilisateurs, véhicules et suivis
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString = "Data Source=fleet_manager.db";

        #region Utilisateurs

        /// <summary>
        /// Récupère un utilisateur par son email
        /// </summary>
        public User? GetUserByEmail(string email)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "SELECT * FROM utilisateurs WHERE email=@email";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapUserFromReader(reader);
            }
            return null;
        }

        /// <summary>
        /// Récupère un utilisateur par son ID
        /// </summary>
        public User? GetUserById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "SELECT * FROM utilisateurs WHERE id=@id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapUserFromReader(reader);
            }
            return null;
        }

        /// <summary>
        /// Crée un nouvel utilisateur
        /// </summary>
        public bool CreateUser(User user, string motDePasse)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
                           VALUES (@nom, @prenom, @email, @mdp, @role)";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@nom", user.Nom);
            cmd.Parameters.AddWithValue("@prenom", user.Prenom);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@mdp", motDePasse);
            cmd.Parameters.AddWithValue("@role", user.Role);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Met à jour un utilisateur
        /// </summary>
        public bool UpdateUser(User user)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"UPDATE utilisateurs 
                           SET nom=@nom, prenom=@prenom, email=@email, role=@role, bloque_jusqu=@bloque 
                           WHERE id=@id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@nom", user.Nom);
            cmd.Parameters.AddWithValue("@prenom", user.Prenom);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@role", user.Role);
            cmd.Parameters.AddWithValue("@bloque", user.BloqueJusqu.HasValue ? (object)user.BloqueJusqu.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@id", user.Id);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Met à jour le mot de passe d'un utilisateur
        /// </summary>
        public bool UpdateUserPassword(int userId, string hashedPassword)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"UPDATE utilisateurs 
                           SET mot_de_passe=@mdp 
                           WHERE id=@id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@mdp", hashedPassword);
            cmd.Parameters.AddWithValue("@id", userId);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Supprime un utilisateur
        /// </summary>
        public bool DeleteUser(int userId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "DELETE FROM utilisateurs WHERE id=@id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Récupère tous les utilisateurs
        /// </summary>
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "SELECT * FROM utilisateurs ORDER BY nom, prenom";
            using var cmd = new SqliteCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                users.Add(MapUserFromReader(reader));
            }
            return users;
        }

        /// <summary>
        /// Vérifie si un email existe déjà
        /// </summary>
        public bool EmailExists(string email, int? excludeUserId = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "SELECT COUNT(*) FROM utilisateurs WHERE email=@email";
            if (excludeUserId.HasValue)
            {
                query += " AND id!=@excludeId";
            }
            
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            if (excludeUserId.HasValue)
            {
                cmd.Parameters.AddWithValue("@excludeId", excludeUserId.Value);
            }
            
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        #endregion

        #region Véhicules

        /// <summary>
        /// Récupère tous les véhicules d'un utilisateur
        /// </summary>
        public List<Vehicle> GetVehiclesByUser(int userId)
        {
            var vehicles = new List<Vehicle>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "SELECT * FROM vehicules WHERE id_utilisateur=@userId ORDER BY marque, modele";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                vehicles.Add(MapVehicleFromReader(reader));
            }
            return vehicles;
        }

        /// <summary>
        /// Récupère un véhicule par son ID
        /// </summary>
        public Vehicle? GetVehicleById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "SELECT * FROM vehicules WHERE id=@id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapVehicleFromReader(reader);
            }
            return null;
        }

        /// <summary>
        /// Crée un nouveau véhicule
        /// </summary>
        public bool CreateVehicle(Vehicle vehicle)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"INSERT INTO vehicules (immatriculation, marque, modele, annee, carburant, kilometrage, id_utilisateur) 
                           VALUES (@immat, @marque, @modele, @annee, @carburant, @km, @userId)";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@immat", vehicle.Immatriculation);
            cmd.Parameters.AddWithValue("@marque", vehicle.Marque);
            cmd.Parameters.AddWithValue("@modele", vehicle.Modele);
            cmd.Parameters.AddWithValue("@annee", vehicle.Annee.HasValue ? (object)vehicle.Annee.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@carburant", vehicle.Carburant);
            cmd.Parameters.AddWithValue("@km", vehicle.Kilometrage);
            cmd.Parameters.AddWithValue("@userId", vehicle.IdUtilisateur);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Met à jour un véhicule
        /// </summary>
        public bool UpdateVehicle(Vehicle vehicle)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"UPDATE vehicules 
                           SET immatriculation=@immat, marque=@marque, modele=@modele, annee=@annee, 
                               carburant=@carburant, kilometrage=@km 
                           WHERE id=@id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@immat", vehicle.Immatriculation);
            cmd.Parameters.AddWithValue("@marque", vehicle.Marque);
            cmd.Parameters.AddWithValue("@modele", vehicle.Modele);
            cmd.Parameters.AddWithValue("@annee", vehicle.Annee.HasValue ? (object)vehicle.Annee.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@carburant", vehicle.Carburant);
            cmd.Parameters.AddWithValue("@km", vehicle.Kilometrage);
            cmd.Parameters.AddWithValue("@id", vehicle.Id);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Supprime un véhicule
        /// </summary>
        public bool DeleteVehicle(int vehicleId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "DELETE FROM vehicules WHERE id=@id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", vehicleId);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Vérifie si une immatriculation existe déjà
        /// </summary>
        public bool ImmatriculationExists(string immatriculation, int? excludeVehicleId = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "SELECT COUNT(*) FROM vehicules WHERE immatriculation=@immat";
            if (excludeVehicleId.HasValue)
            {
                query += " AND id!=@excludeId";
            }
            
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@immat", immatriculation);
            if (excludeVehicleId.HasValue)
            {
                cmd.Parameters.AddWithValue("@excludeId", excludeVehicleId.Value);
            }
            
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        #endregion

        #region Suivis

        /// <summary>
        /// Récupère les suivis d'un véhicule
        /// </summary>
        public List<Suivi> GetSuivisByVehicle(int vehicleId, int? limit = null)
        {
            var suivis = new List<Suivi>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "SELECT * FROM suivi WHERE id_vehicule=@vehId ORDER BY date_suivi DESC";
            if (limit.HasValue)
            {
                query += $" LIMIT {limit.Value}";
            }
            
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@vehId", vehicleId);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                suivis.Add(MapSuiviFromReader(reader));
            }
            return suivis;
        }

        /// <summary>
        /// Récupère les suivis d'un utilisateur (tous ses véhicules)
        /// </summary>
        public List<Suivi> GetSuivisByUser(int userId, int? limit = null)
        {
            var suivis = new List<Suivi>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"SELECT s.* FROM suivi s 
                           JOIN vehicules v ON s.id_vehicule = v.id 
                           WHERE v.id_utilisateur=@userId 
                           ORDER BY s.date_suivi DESC";
            if (limit.HasValue)
            {
                query += $" LIMIT {limit.Value}";
            }
            
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                suivis.Add(MapSuiviFromReader(reader));
            }
            return suivis;
        }

        /// <summary>
        /// Récupère tous les suivis (pour admin)
        /// </summary>
        public List<Suivi> GetAllSuivis(int? limit = null)
        {
            var suivis = new List<Suivi>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "SELECT * FROM suivi ORDER BY date_suivi DESC";
            if (limit.HasValue)
            {
                query += $" LIMIT {limit.Value}";
            }
            
            using var cmd = new SqliteCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                suivis.Add(MapSuiviFromReader(reader));
            }
            return suivis;
        }

        /// <summary>
        /// Crée un nouveau suivi
        /// </summary>
        public bool CreateSuivi(Suivi suivi)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"INSERT INTO suivi (id_vehicule, date_suivi, carburant_litre, cout, distance_km, commentaire) 
                           VALUES (@vehId, @date, @carb, @cout, @dist, @comm)";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@vehId", suivi.IdVehicule);
            cmd.Parameters.AddWithValue("@date", suivi.DateSuivi);
            cmd.Parameters.AddWithValue("@carb", suivi.CarburantLitre.HasValue ? (object)suivi.CarburantLitre.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@cout", suivi.Cout.HasValue ? (object)suivi.Cout.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@dist", suivi.DistanceKm.HasValue ? (object)suivi.DistanceKm.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@comm", string.IsNullOrEmpty(suivi.Commentaire) ? DBNull.Value : (object)suivi.Commentaire);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Met à jour un suivi
        /// </summary>
        public bool UpdateSuivi(Suivi suivi)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"UPDATE suivi 
                           SET date_suivi=@date, carburant_litre=@carb, cout=@cout, distance_km=@dist, commentaire=@comm 
                           WHERE id=@id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@date", suivi.DateSuivi);
            cmd.Parameters.AddWithValue("@carb", suivi.CarburantLitre.HasValue ? (object)suivi.CarburantLitre.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@cout", suivi.Cout.HasValue ? (object)suivi.Cout.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@dist", suivi.DistanceKm.HasValue ? (object)suivi.DistanceKm.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@comm", string.IsNullOrEmpty(suivi.Commentaire) ? DBNull.Value : (object)suivi.Commentaire);
            cmd.Parameters.AddWithValue("@id", suivi.Id);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Supprime un suivi
        /// </summary>
        public bool DeleteSuivi(int suiviId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = "DELETE FROM suivi WHERE id=@id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", suiviId);
            
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Récupère les statistiques globales pour un utilisateur
        /// </summary>
        public (double TotalKm, double TotalCout, double TotalLitres) GetUserStatistics(int userId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"SELECT 
                           COALESCE(SUM(s.distance_km), 0) as total_km,
                           COALESCE(SUM(s.cout), 0) as total_cout,
                           COALESCE(SUM(s.carburant_litre), 0) as total_litres
                           FROM suivi s
                           JOIN vehicules v ON s.id_vehicule = v.id
                           WHERE v.id_utilisateur=@userId";
            
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    reader.GetDouble("total_km"),
                    reader.GetDouble("total_cout"),
                    reader.GetDouble("total_litres")
                );
            }
            return (0, 0, 0);
        }

        /// <summary>
        /// Récupère les statistiques globales (admin)
        /// </summary>
        public (double TotalKm, double TotalCout, double TotalLitres) GetAllStatistics()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            
            string query = @"SELECT 
                           COALESCE(SUM(distance_km), 0) as total_km,
                           COALESCE(SUM(cout), 0) as total_cout,
                           COALESCE(SUM(carburant_litre), 0) as total_litres
                           FROM suivi";
            
            using var cmd = new SqliteCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    reader.GetDouble("total_km"),
                    reader.GetDouble("total_cout"),
                    reader.GetDouble("total_litres")
                );
            }
            return (0, 0, 0);
        }

        #endregion

        #region Mapping Methods

        private User MapUserFromReader(SqliteDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("id"),
                Nom = reader.GetString("nom"),
                Prenom = reader.GetString("prenom"),
                Email = reader.GetString("email"),
                Role = reader.GetString("role"),
                BloqueJusqu = reader.IsDBNull(reader.GetOrdinal("bloque_jusqu")) ? null : reader.GetDateTime("bloque_jusqu"),
                DateCreation = reader.GetDateTime("date_creation")
            };
        }

        private Vehicle MapVehicleFromReader(SqliteDataReader reader)
        {
            return new Vehicle
            {
                Id = reader.GetInt32("id"),
                Immatriculation = reader.GetString("immatriculation"),
                Marque = reader.GetString("marque"),
                Modele = reader.GetString("modele"),
                Annee = reader.IsDBNull(reader.GetOrdinal("annee")) ? null : reader.GetInt32("annee"),
                Carburant = reader.IsDBNull(reader.GetOrdinal("carburant")) ? string.Empty : reader.GetString("carburant"),
                Kilometrage = reader.GetInt32("kilometrage"),
                IdUtilisateur = reader.GetInt32("id_utilisateur"),
                DateCreation = reader.GetDateTime("date_creation")
            };
        }

        private Suivi MapSuiviFromReader(SqliteDataReader reader)
        {
            return new Suivi
            {
                Id = reader.GetInt32("id"),
                IdVehicule = reader.GetInt32("id_vehicule"),
                DateSuivi = reader.GetDateTime("date_suivi"),
                CarburantLitre = reader.IsDBNull(reader.GetOrdinal("carburant_litre")) ? null : reader.GetDouble("carburant_litre"),
                Cout = reader.IsDBNull(reader.GetOrdinal("cout")) ? null : reader.GetDouble("cout"),
                DistanceKm = reader.IsDBNull(reader.GetOrdinal("distance_km")) ? null : reader.GetDouble("distance_km"),
                Commentaire = reader.IsDBNull(reader.GetOrdinal("commentaire")) ? null : reader.GetString("commentaire"),
                DateCreation = reader.GetDateTime("date_creation")
            };
        }

        #endregion
    }
}
