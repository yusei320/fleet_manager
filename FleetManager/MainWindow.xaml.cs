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

        public MainWindow()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Kilométrage total
                    string queryKm = "SELECT SUM(kilometrage) FROM vehicules";
                    MySqlCommand cmdKm = new MySqlCommand(queryKm, conn);
                    object resultKm = cmdKm.ExecuteScalar();
                    txtKilometrageTotal.Text = "Kilométrage total: " + (resultKm != DBNull.Value ? resultKm.ToString() : "0") + " km";

                    // Coût carburant total
                    string queryCout = "SELECT SUM(cout) FROM suivi";
                    MySqlCommand cmdCout = new MySqlCommand(queryCout, conn);
                    object resultCout = cmdCout.ExecuteScalar();
                    txtCoutTotal.Text = "Coût carburant total: " + (resultCout != DBNull.Value ? resultCout.ToString() : "0") + " €";

                    // Suivis récents (derniers 5)
                    string querySuivi = @"SELECT v.immatriculation, s.date_suivi, s.carburant_litre, s.cout, s.distance_km
                                          FROM suivi s
                                          JOIN vehicules v ON s.id_vehicule = v.id
                                          ORDER BY s.date_suivi DESC
                                          LIMIT 5";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(querySuivi, conn);
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

        // --- Menu Event Handlers (identiques à avant) ---
        private void Menu_AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            AddVehicleWindow addWindow = new AddVehicleWindow(connectionString);
            addWindow.ShowDialog();
            LoadDashboard(); // Refresh dashboard après ajout
        }

        private void Menu_ListVehicles_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT id, immatriculation, marque, modele, annee, carburant, kilometrage FROM vehicules";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
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
            AddSuiviWindow suiviWindow = new AddSuiviWindow(connectionString);
            suiviWindow.ShowDialog();
            LoadDashboard(); // Refresh dashboard après ajout
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
                                     JOIN vehicules v ON s.id_vehicule = v.id";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Efface tout le contenu et recharge le tableau de bord
            MainContent.Children.Clear();

            // Recrée le tableau de bord
            StackPanel dashboardPanel = new StackPanel();

            TextBlock title = new TextBlock
            {
                Text = "Tableau de bord Fleet Manager",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            StackPanel statsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            TextBlock kmText = new TextBlock
            {
                Name = "txtKilometrageTotal",
                FontSize = 16,
                Margin = new Thickness(10)
            };

            TextBlock coutText = new TextBlock
            {
                Name = "txtCoutTotal",
                FontSize = 16,
                Margin = new Thickness(10)
            };

            statsPanel.Children.Add(kmText);
            statsPanel.Children.Add(coutText);

            TextBlock suivisTitle = new TextBlock
            {
                Text = "Suivis récents:",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            DataGrid suivisGrid = new DataGrid
            {
                Name = "dgRecentSuivi",
                AutoGenerateColumns = true,
                Height = 250,
                IsReadOnly = true
            };

            dashboardPanel.Children.Add(title);
            dashboardPanel.Children.Add(statsPanel);
            dashboardPanel.Children.Add(suivisTitle);
            dashboardPanel.Children.Add(suivisGrid);

            MainContent.Children.Add(dashboardPanel);

            // Recharge les données du tableau de bord
            LoadDashboard();
        }

    }
}
