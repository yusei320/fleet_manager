using FleetManager;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager
{
    public partial class MainWindow : Window
    {
        private string connectionString = "server=localhost;Port=3309;database=fleet_managers;uid=root;pwd=;";
        private int currentUserId;

        public MainWindow(int userId)
        {
            InitializeComponent();
            currentUserId = userId;
            RestrictUserAccess();
            LoadDashboard();
        }

        private void RestrictUserAccess()
        {
            // Cache le menu admin pour les utilisateurs non administrateur
            if (currentUserId != 1) // Remplace par un vrai test de rôle plus tard
            {
                if (AdminMenu != null)
                    AdminMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadDashboard()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Kilométrage total
                    string queryKm = @"SELECT SUM(kilometrage) 
                                       FROM vehicules 
                                       WHERE id_utilisateur=@id";
                    MySqlCommand cmdKm = new MySqlCommand(queryKm, conn);
                    cmdKm.Parameters.AddWithValue("@id", currentUserId);
                    object resultKm = cmdKm.ExecuteScalar();
                    txtKilometrageTotal.Text = "Kilométrage total: " + (resultKm != DBNull.Value ? resultKm.ToString() : "0") + " km";

                    // Coût carburant total
                    string queryCout = @"SELECT SUM(cout) 
                                         FROM suivi s
                                         JOIN vehicules v ON s.id_vehicule = v.id
                                         WHERE v.id_utilisateur=@id";
                    MySqlCommand cmdCout = new MySqlCommand(queryCout, conn);
                    cmdCout.Parameters.AddWithValue("@id", currentUserId);
                    object resultCout = cmdCout.ExecuteScalar();
                    txtCoutTotal.Text = "Coût carburant total: " + (resultCout != DBNull.Value ? resultCout.ToString() : "0") + " €";

                    // Suivis récents
                    string querySuivi = @"SELECT v.immatriculation, s.date_suivi, s.carburant_litre, s.cout, s.distance_km
                                          FROM suivi s
                                          JOIN vehicules v ON s.id_vehicule = v.id
                                          WHERE v.id_utilisateur=@id
                                          ORDER BY s.date_suivi DESC
                                          LIMIT 5";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(querySuivi, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@id", currentUserId);

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgRecentSuivi.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur chargement dashboard: " + ex.Message);
                }
            }
        }

        private void Menu_AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            AddVehicleWindow addWindow = new AddVehicleWindow(connectionString, currentUserId);
            addWindow.ShowDialog();
            LoadDashboard();
        }

        private void Menu_ListVehicles_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT id, immatriculation, marque, modele, annee, carburant, kilometrage 
                                     FROM vehicules 
                                     WHERE id_utilisateur=@id";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@id", currentUserId);
                    adapter.Fill(dt);

                    DataGrid grid = new DataGrid
                    {
                        ItemsSource = dt.DefaultView,
                        AutoGenerateColumns = true,
                        IsReadOnly = true
                    };

                    MainContent.Children.Clear();
                    MainContent.Children.Add(grid);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message);
                }
            }
        }

        private void Menu_AddSuivi_Click(object sender, RoutedEventArgs e)
        {
            AddSuiviWindow suiviWindow = new AddSuiviWindow(connectionString, currentUserId);
            suiviWindow.ShowDialog();
            LoadDashboard();
        }

        private void Menu_ViewSuivi_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT s.id, v.immatriculation, s.date_suivi, s.carburant_litre, s.cout, s.distance_km, s.commentaire
                                     FROM suivi s
                                     JOIN vehicules v ON s.id_vehicule = v.id
                                     WHERE v.id_utilisateur=@id";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@id", currentUserId);
                    adapter.Fill(dt);

                    DataGrid grid = new DataGrid
                    {
                        ItemsSource = dt.DefaultView,
                        AutoGenerateColumns = true,
                        IsReadOnly = true
                    };

                    MainContent.Children.Clear();
                    MainContent.Children.Add(grid);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message);
                }
            }
        }

        private void OpenAdmin_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow admin = new AdminWindow(currentUserId);
            admin.Show();
        }
    }
}
