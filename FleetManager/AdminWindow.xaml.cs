using Microsoft.Data.Sqlite;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager
{
    public partial class AdminWindow : Window
    {
        string connectionString = "Data Source=fleet_manager.db";
        private int adminId;

        public AdminWindow(int userId)
        {
            InitializeComponent();

            // Vérifie que l'utilisateur connecté est bien admin
            if (!AuthService.IsAdmin(userId))
            {
                MessageBox.Show("Accès refusé : vous n'avez pas les droits d'administration.",
                    "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }

            adminId = userId;
            LoadUsers();
        }

        private void LoadUsers()
        {
            UserList.Items.Clear();

            try
            {
                using SqliteConnection conn = new SqliteConnection(connectionString);
                conn.Open();

                string query = "SELECT id, nom, prenom, email, role, bloque_jusqu FROM utilisateurs ORDER BY id";
                SqliteCommand cmd = new SqliteCommand(query, conn);

                using SqliteDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    string bloque = rd["bloque_jusqu"] != DBNull.Value
                                    ? $"🔒 Bloqué jusqu'au {Convert.ToDateTime(rd["bloque_jusqu"]):dd/MM/yyyy}"
                                    : "✅ Actif";

                    UserList.Items.Add(
                        $"{rd["id"]} - {rd["prenom"]} {rd["nom"]} | {rd["role"]} | {rd["email"]} | {bloque}"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des utilisateurs : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetSelectedUserId()
        {
            if (UserList.SelectedItem == null) return -1;

            string? str = UserList.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(str)) return -1;
            return int.Parse(str.Split('-')[0].Trim());
        }

        private void LoadVehicules(int idUser)
        {
            VehiculeList.Items.Clear();

            try
            {
                using SqliteConnection conn = new SqliteConnection(connectionString);
                conn.Open();

                // Récupérer le nom de l'utilisateur
                string userQuery = "SELECT prenom, nom FROM utilisateurs WHERE id=@id";
                SqliteCommand userCmd = new SqliteCommand(userQuery, conn);
                userCmd.Parameters.AddWithValue("@id", idUser);

                string userName = "";
                using (SqliteDataReader userRd = userCmd.ExecuteReader())
                {
                    if (userRd.Read())
                    {
                        userName = $"{userRd["prenom"]} {userRd["nom"]}";
                    }
                }

                // Récupérer tous les véhicules avec détails
                string query = @"SELECT immatriculation, marque, modele, annee, carburant, kilometrage 
                                FROM vehicules 
                                WHERE id_utilisateur=@id
                                ORDER BY marque, modele";

                SqliteCommand cmd = new SqliteCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", idUser);

                using SqliteDataReader rd = cmd.ExecuteReader();

                if (!rd.HasRows)
                {
                    VehiculeList.Items.Add($"📋 {userName} n'a aucun véhicule enregistré");
                }
                else
                {
                    VehiculeList.Items.Add($"👤 Véhicules de {userName} :");
                    VehiculeList.Items.Add("═══════════════════════════════════════");

                    int count = 0;
                    while (rd.Read())
                    {
                        count++;
                        VehiculeList.Items.Add(
                            $"🚗 {count}. {rd["marque"]} {rd["modele"]} ({rd["annee"]})"
                        );
                        VehiculeList.Items.Add(
                            $"    📍 Immat: {rd["immatriculation"]} | ⛽ {rd["carburant"]} | 📊 {rd["kilometrage"]:N0} km"
                        );
                        VehiculeList.Items.Add("─────────────────────────────────────");
                    }

                    VehiculeList.Items.Add($"✔ Total : {count} véhicule(s)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des véhicules : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int id = GetSelectedUserId();
            if (id != -1)
                LoadVehicules(id);
            else
                VehiculeList.Items.Clear();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateUserWindow wnd = new CreateUserWindow();
            if (wnd.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            int id = GetSelectedUserId();
            if (id == -1)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (id == adminId)
            {
                MessageBox.Show("Vous ne pouvez pas vous supprimer vous-même !", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (id == 1)
            {
                MessageBox.Show("Impossible de supprimer le super administrateur !", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                "Voulez-vous vraiment supprimer cet utilisateur ?\n\n" +
                "⚠️ ATTENTION : Tous ses véhicules et suivis seront également supprimés de façon définitive !",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using SqliteConnection conn = new SqliteConnection(connectionString);
                    conn.Open();

                    // Supprimer d'abord les suivis liés aux véhicules
                    SqliteCommand deleteSuiviCmd = new SqliteCommand(
                        @"DELETE FROM suivi WHERE id_vehicule IN 
                          (SELECT id FROM vehicules WHERE id_utilisateur=@id)", conn);
                    deleteSuiviCmd.Parameters.AddWithValue("@id", id);
                    int suiviDeleted = deleteSuiviCmd.ExecuteNonQuery();

                    // Supprimer les véhicules
                    SqliteCommand deleteVehiclesCmd = new SqliteCommand(
                        "DELETE FROM vehicules WHERE id_utilisateur=@id", conn);
                    deleteVehiclesCmd.Parameters.AddWithValue("@id", id);
                    int vehiclesDeleted = deleteVehiclesCmd.ExecuteNonQuery();

                    // Supprimer l'utilisateur
                    SqliteCommand deleteUserCmd = new SqliteCommand(
                        "DELETE FROM utilisateurs WHERE id=@id", conn);
                    deleteUserCmd.Parameters.AddWithValue("@id", id);
                    deleteUserCmd.ExecuteNonQuery();

                    MessageBox.Show(
                        $"✔ Utilisateur supprimé avec succès !\n\n" +
                        $"📊 Éléments supprimés :\n" +
                        $"   • Véhicules : {vehiclesDeleted}\n" +
                        $"   • Suivis : {suiviDeleted}",
                        "Suppression réussie",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    LoadUsers();
                    VehiculeList.Items.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Erreur lors de la suppression : {ex.Message}", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnBlock_Click(object sender, RoutedEventArgs e)
        {
            int id = GetSelectedUserId();
            if (id == -1)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (id == adminId)
            {
                MessageBox.Show("Vous ne pouvez pas vous bloquer vous-même !", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (id == 1)
            {
                MessageBox.Show("Impossible de bloquer le super administrateur !", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using SqliteConnection conn = new SqliteConnection(connectionString);
                conn.Open();

                // Vérifie si l'utilisateur est déjà bloqué
                string checkQuery = "SELECT bloque_jusqu FROM utilisateurs WHERE id=@id";
                SqliteCommand checkCmd = new SqliteCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@id", id);
                object result = checkCmd.ExecuteScalar();

                DateTime? bloqueUntil = result != DBNull.Value ? (DateTime?)result : null;

                if (bloqueUntil == null || bloqueUntil < DateTime.Now)
                {
                    // Bloquer pour 7 jours
                    DateTime dateBloquage = DateTime.Now.AddDays(7);
                    string blockQuery = "UPDATE utilisateurs SET bloque_jusqu=@date WHERE id=@id";
                    SqliteCommand blockCmd = new SqliteCommand(blockQuery, conn);
                    blockCmd.Parameters.AddWithValue("@date", dateBloquage);
                    blockCmd.Parameters.AddWithValue("@id", id);
                    blockCmd.ExecuteNonQuery();

                    MessageBox.Show(
                        $"🔒 Utilisateur bloqué jusqu'au {dateBloquage:dd/MM/yyyy à HH:mm}",
                        "Blocage effectué",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    // Débloquer
                    string unblockQuery = "UPDATE utilisateurs SET bloque_jusqu=NULL WHERE id=@id";
                    SqliteCommand unblockCmd = new SqliteCommand(unblockQuery, conn);
                    unblockCmd.Parameters.AddWithValue("@id", id);
                    unblockCmd.ExecuteNonQuery();

                    MessageBox.Show(
                        "✅ Utilisateur débloqué avec succès !",
                        "Déblocage effectué",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}