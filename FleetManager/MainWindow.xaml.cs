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
            if (!AuthService.IsAdmin(currentUserId))
            {
                if (AdminMenu != null)
                    AdminMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadDashboard()
        {
            // Trouver les contrôles soit du XAML soit créés dynamiquement
            TextBlock txtKm = this.FindName("txtKilometrageTotal") as TextBlock;
            TextBlock txtCout = this.FindName("txtCoutTotal") as TextBlock;
            DataGrid dgSuivi = this.FindName("dgRecentSuivi") as DataGrid;

            if (txtKm == null || txtCout == null || dgSuivi == null)
            {
                // Les contrôles n'existent pas, on ne peut pas charger
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // ✅ Distance totale PARCOURUE (depuis la table suivi)
                    string queryKm = @"SELECT COALESCE(SUM(s.distance_km), 0) 
                                       FROM suivi s
                                       JOIN vehicules v ON s.id_vehicule = v.id
                                       WHERE v.id_utilisateur=@id";
                    MySqlCommand cmdKm = new MySqlCommand(queryKm, conn);
                    cmdKm.Parameters.AddWithValue("@id", currentUserId);
                    object resultKm = cmdKm.ExecuteScalar();

                    double km = resultKm != DBNull.Value ? Convert.ToDouble(resultKm) : 0;
                    txtKm.Text = $"{km:N0} km";

                    // Coût carburant total avec formatage
                    string queryCout = @"SELECT COALESCE(SUM(cout), 0)
                                         FROM suivi s
                                         JOIN vehicules v ON s.id_vehicule = v.id
                                         WHERE v.id_utilisateur=@id";
                    MySqlCommand cmdCout = new MySqlCommand(queryCout, conn);
                    cmdCout.Parameters.AddWithValue("@id", currentUserId);
                    object resultCout = cmdCout.ExecuteScalar();

                    double cout = resultCout != DBNull.Value ? Convert.ToDouble(resultCout) : 0;
                    txtCout.Text = $"{cout:N2} €";

                    // Suivis récents avec colonnes en français
                    string querySuivi = @"SELECT v.immatriculation AS 'Immatriculation', 
                                                 DATE_FORMAT(s.date_suivi, '%d/%m/%Y') AS 'Date', 
                                                 s.carburant_litre AS 'Litres', 
                                                 CONCAT(FORMAT(s.cout, 2), ' €') AS 'Coût', 
                                                 CONCAT(s.distance_km, ' km') AS 'Distance'
                                          FROM suivi s
                                          JOIN vehicules v ON s.id_vehicule = v.id
                                          WHERE v.id_utilisateur=@id
                                          ORDER BY s.date_suivi DESC
                                          LIMIT 5";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(querySuivi, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@id", currentUserId);

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgSuivi.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement du tableau de bord :\n{ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RestoreDashboard()
        {
            // Effacer le contenu actuel
            MainContent.Children.Clear();

            // Recréer l'en-tête du tableau de bord
            Border headerBorder = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(25),
                Margin = new Thickness(0, 0, 0, 25)
            };
            headerBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 10,
                ShadowDepth = 2,
                Opacity = 0.1
            };

            StackPanel headerStack = new StackPanel();
            TextBlock titleBlock = new TextBlock
            {
                Text = "📊 Tableau de Bord",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50")),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 5)
            };
            TextBlock subtitleBlock = new TextBlock
            {
                Text = "Vue d'ensemble de votre flotte de véhicules",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#7F8C8D"))
            };
            headerStack.Children.Add(titleBlock);
            headerStack.Children.Add(subtitleBlock);
            headerBorder.Child = headerStack;

            // Créer la grille des statistiques
            Grid statsGrid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 25)
            };
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Carte Distance Parcourue
            Border kmBorder = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(10)
            };
            kmBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 10,
                ShadowDepth = 2,
                Opacity = 0.1
            };
            Grid.SetColumn(kmBorder, 0);

            StackPanel kmStack = new StackPanel();
            kmStack.Children.Add(new TextBlock
            {
                Text = "🛣️",
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            });
            kmStack.Children.Add(new TextBlock
            {
                Text = "Distance Parcourue",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#7F8C8D")),
                Margin = new Thickness(0, 0, 0, 5)
            });
            TextBlock txtKmTotal = new TextBlock
            {
                Text = "0 km",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50"))
            };
            kmStack.Children.Add(txtKmTotal);
            kmBorder.Child = kmStack;

            // Carte Coût
            Border coutBorder = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(10)
            };
            coutBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 10,
                ShadowDepth = 2,
                Opacity = 0.1
            };
            Grid.SetColumn(coutBorder, 1);

            StackPanel coutStack = new StackPanel();
            coutStack.Children.Add(new TextBlock
            {
                Text = "💰",
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            });
            coutStack.Children.Add(new TextBlock
            {
                Text = "Coût Total Carburant",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#7F8C8D")),
                Margin = new Thickness(0, 0, 0, 5)
            });
            TextBlock txtCTotal = new TextBlock
            {
                Text = "0 €",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50"))
            };
            coutStack.Children.Add(txtCTotal);
            coutBorder.Child = coutStack;

            statsGrid.Children.Add(kmBorder);
            statsGrid.Children.Add(coutBorder);

            // Section Suivis récents
            Border suiviBorder = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(25)
            };
            suiviBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 10,
                ShadowDepth = 2,
                Opacity = 0.1
            };

            StackPanel suiviStack = new StackPanel();

            StackPanel headerSuivi = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 15)
            };
            headerSuivi.Children.Add(new TextBlock
            {
                Text = "📋 ",
                FontSize = 22,
                VerticalAlignment = VerticalAlignment.Center
            });
            headerSuivi.Children.Add(new TextBlock
            {
                Text = "Suivis Récents",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50")),
                VerticalAlignment = VerticalAlignment.Center
            });
            suiviStack.Children.Add(headerSuivi);

            Border gridBorder = new Border
            {
                BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D1D8E0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };
            DataGrid dgRecent = new DataGrid
            {
                Height = 300,
                IsReadOnly = true
            };
            gridBorder.Child = dgRecent;
            suiviStack.Children.Add(gridBorder);
            suiviBorder.Child = suiviStack;

            // Ajouter tous les éléments au MainContent
            MainContent.Children.Add(headerBorder);
            MainContent.Children.Add(statsGrid);
            MainContent.Children.Add(suiviBorder);

            // Enregistrer les références pour LoadDashboard
            // D'abord, désenregistrer si les noms existent déjà
            try
            {
                this.UnregisterName("txtKilometrageTotal");
                this.UnregisterName("txtCoutTotal");
                this.UnregisterName("dgRecentSuivi");
            }
            catch { }

            this.RegisterName("txtKilometrageTotal", txtKmTotal);
            this.RegisterName("txtCoutTotal", txtCTotal);
            this.RegisterName("dgRecentSuivi", dgRecent);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Restaurer le tableau de bord complet
            RestoreDashboard();
            // Recharger les données
            LoadDashboard();
        }

        private void Menu_AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            AddVehicleWindow addWindow = new AddVehicleWindow(connectionString, currentUserId);
            if (addWindow.ShowDialog() == true)
            {
                // Vérifier si on est sur le dashboard avant de recharger
                if (this.FindName("txtKilometrageTotal") != null)
                {
                    LoadDashboard();
                }
            }
        }

        private void Menu_ListVehicles_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT id AS 'ID',
                                            immatriculation AS 'Immatriculation', 
                                            marque AS 'Marque', 
                                            modele AS 'Modèle', 
                                            annee AS 'Année', 
                                            carburant AS 'Carburant', 
                                            CONCAT(FORMAT(kilometrage, 0), ' km') AS 'Kilométrage'
                                     FROM vehicules 
                                     WHERE id_utilisateur=@id
                                     ORDER BY marque, modele";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@id", currentUserId);
                    adapter.Fill(dt);

                    // Créer une nouvelle vue pour afficher les véhicules
                    MainContent.Children.Clear();

                    // Titre
                    Border headerBorder = new Border
                    {
                        Background = System.Windows.Media.Brushes.White,
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(25),
                        Margin = new Thickness(0, 0, 0, 25)
                    };
                    headerBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = System.Windows.Media.Colors.Black,
                        BlurRadius = 10,
                        ShadowDepth = 2,
                        Opacity = 0.1
                    };

                    StackPanel headerStack = new StackPanel();
                    TextBlock titleBlock = new TextBlock
                    {
                        Text = "🚗 Mes Véhicules",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.Black,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    TextBlock subtitleBlock = new TextBlock
                    {
                        Text = $"Total : {dt.Rows.Count} véhicule(s)",
                        FontSize = 14,
                        Foreground = System.Windows.Media.Brushes.Gray
                    };
                    headerStack.Children.Add(titleBlock);
                    headerStack.Children.Add(subtitleBlock);
                    headerBorder.Child = headerStack;

                    // DataGrid
                    Border gridBorder = new Border
                    {
                        Background = System.Windows.Media.Brushes.White,
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(25)
                    };
                    gridBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = System.Windows.Media.Colors.Black,
                        BlurRadius = 10,
                        ShadowDepth = 2,
                        Opacity = 0.1
                    };

                    DataGrid grid = new DataGrid
                    {
                        ItemsSource = dt.DefaultView,
                        AutoGenerateColumns = true,
                        IsReadOnly = true,
                        Height = 400
                    };
                    gridBorder.Child = grid;

                    MainContent.Children.Add(headerBorder);
                    MainContent.Children.Add(gridBorder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des véhicules :\n{ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Menu_AddSuivi_Click(object sender, RoutedEventArgs e)
        {
            AddSuiviWindow suiviWindow = new AddSuiviWindow(connectionString, currentUserId);
            if (suiviWindow.ShowDialog() == true)
            {
                // Vérifier si on est sur le dashboard avant de recharger
                if (this.FindName("txtKilometrageTotal") != null)
                {
                    LoadDashboard();
                }
            }
        }

        private void Menu_ViewSuivi_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT s.id AS 'ID',
                                            v.immatriculation AS 'Immatriculation', 
                                            DATE_FORMAT(s.date_suivi, '%d/%m/%Y') AS 'Date',
                                            CONCAT(s.carburant_litre, ' L') AS 'Carburant', 
                                            CONCAT(FORMAT(s.cout, 2), ' €') AS 'Coût', 
                                            CONCAT(s.distance_km, ' km') AS 'Distance',
                                            s.commentaire AS 'Commentaire'
                                     FROM suivi s
                                     JOIN vehicules v ON s.id_vehicule = v.id
                                     WHERE v.id_utilisateur=@id
                                     ORDER BY s.date_suivi DESC";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@id", currentUserId);
                    adapter.Fill(dt);

                    // Créer une nouvelle vue pour afficher les suivis
                    MainContent.Children.Clear();

                    // Titre
                    Border headerBorder = new Border
                    {
                        Background = System.Windows.Media.Brushes.White,
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(25),
                        Margin = new Thickness(0, 0, 0, 25)
                    };
                    headerBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = System.Windows.Media.Colors.Black,
                        BlurRadius = 10,
                        ShadowDepth = 2,
                        Opacity = 0.1
                    };

                    StackPanel headerStack = new StackPanel();
                    TextBlock titleBlock = new TextBlock
                    {
                        Text = "⛽ Historique des Suivis",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.Black,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    TextBlock subtitleBlock = new TextBlock
                    {
                        Text = $"Total : {dt.Rows.Count} suivi(s) enregistré(s)",
                        FontSize = 14,
                        Foreground = System.Windows.Media.Brushes.Gray
                    };
                    headerStack.Children.Add(titleBlock);
                    headerStack.Children.Add(subtitleBlock);
                    headerBorder.Child = headerStack;

                    // DataGrid
                    Border gridBorder = new Border
                    {
                        Background = System.Windows.Media.Brushes.White,
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(25)
                    };
                    gridBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = System.Windows.Media.Colors.Black,
                        BlurRadius = 10,
                        ShadowDepth = 2,
                        Opacity = 0.1
                    };

                    DataGrid grid = new DataGrid
                    {
                        ItemsSource = dt.DefaultView,
                        AutoGenerateColumns = true,
                        IsReadOnly = true,
                        Height = 400
                    };
                    gridBorder.Child = grid;

                    MainContent.Children.Add(headerBorder);
                    MainContent.Children.Add(gridBorder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des suivis :\n{ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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