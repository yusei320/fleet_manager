using Microsoft.Data.Sqlite;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Annotations;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FleetManager
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string connectionString = "Data Source=fleet_manager.db";
        private int currentUserId;
        private string currentView = "Dashboard";
        private List<VehicleOption> vehicleOptions = new List<VehicleOption>();
        private bool isAdmin;

        public PlotModel? MonthlyDistanceModel { get; private set; }
        public PlotModel? MonthlyCostModel { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow(int userId)
        {
            InitializeComponent();
            currentUserId = userId;
            isAdmin = AuthService.IsAdmin(currentUserId);
            DataContext = this;
            RestrictUserAccess();
            ApplyAdminToggleVisibility();
            LoadFilterOptions();
            LoadDashboard();
            currentView = "Dashboard";
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

        private void ApplyAdminToggleVisibility()
        {
            CheckBox? chk = this.FindName("chkAdminAll") as CheckBox;
            Button? btn = this.FindName("btnToggleAdmin") as Button;
            var visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            if (chk != null) chk.Visibility = visibility;
            if (btn != null) btn.Visibility = visibility;
        }

        private void LoadDashboard()
        {
            TextBlock? txtKm = this.FindName("txtKilometrageTotal") as TextBlock;
            TextBlock? txtCout = this.FindName("txtCoutTotal") as TextBlock;
            TextBlock? txtLitres = this.FindName("txtLitresTotal") as TextBlock;
            DataGrid? dgSuivi = this.FindName("dgRecentSuivi") as DataGrid;

            if (txtKm == null || txtCout == null || txtLitres == null || dgSuivi == null)
            {
                return;
            }

            var (vehicleId, startDate, endDate, adminAll) = GetFilters();

            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string filterClause = BuildFilterClause(vehicleId, startDate, endDate, adminAll);

                    string queryKm = $@"SELECT COALESCE(SUM(s.distance_km), 0) 
                                       FROM suivi s
                                       JOIN vehicules v ON s.id_vehicule = v.id
                                       WHERE 1=1 {filterClause}";
                    using SqliteCommand cmdKm = new SqliteCommand(queryKm, conn);
                    ApplyFilterParameters(cmdKm, vehicleId, startDate, endDate, adminAll);
                    object resultKm = cmdKm.ExecuteScalar();

                    double km = resultKm != DBNull.Value ? Convert.ToDouble(resultKm) : 0;
                    txtKm.Text = $"{km:N0} km";

                    string queryCout = $@"SELECT COALESCE(SUM(cout), 0)
                                         FROM suivi s
                                         JOIN vehicules v ON s.id_vehicule = v.id
                                         WHERE 1=1 {filterClause}";
                    using SqliteCommand cmdCout = new SqliteCommand(queryCout, conn);
                    ApplyFilterParameters(cmdCout, vehicleId, startDate, endDate, adminAll);
                    object resultCout = cmdCout.ExecuteScalar();

                    double cout = resultCout != DBNull.Value ? Convert.ToDouble(resultCout) : 0;
                    txtCout.Text = $"{cout:N2} €";

                    string queryLitres = $@"SELECT COALESCE(SUM(s.carburant_litre), 0)
                                            FROM suivi s
                                            JOIN vehicules v ON s.id_vehicule = v.id
                                            WHERE 1=1 {filterClause}";
                    using SqliteCommand cmdLitres = new SqliteCommand(queryLitres, conn);
                    ApplyFilterParameters(cmdLitres, vehicleId, startDate, endDate, adminAll);
                    object resultLitres = cmdLitres.ExecuteScalar();

                    double litres = resultLitres != DBNull.Value ? Convert.ToDouble(resultLitres) : 0;
                    txtLitres.Text = $"{litres:N1} L";

                    string querySuivi = $@"SELECT v.immatriculation AS 'Immatriculation', 
                                                 STRFTIME('%d/%m/%Y', s.date_suivi) AS 'Date', 
                                                 s.carburant_litre AS 'Litres', 
                                                PRINTF('%.2f €', s.cout) AS 'Coût', 
                                                 s.distance_km || ' km' AS 'Distance'
                                          FROM suivi s
                                          JOIN vehicules v ON s.id_vehicule = v.id
                                          WHERE 1=1 {filterClause}
                                          ORDER BY s.date_suivi DESC
                                          LIMIT 5";
                    DataTable dt = new DataTable();
                    using (var cmd = new SqliteCommand(querySuivi, conn))
                    {
                        ApplyFilterParameters(cmd, vehicleId, startDate, endDate, adminAll);
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                    dgSuivi.ItemsSource = dt.DefaultView;

            LoadCharts(conn, vehicleId, startDate, endDate, adminAll);
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

            // Actions
            DockPanel actionsPanel = new DockPanel { Margin = new Thickness(0, 0, 0, 15) };
            StackPanel actionsStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };
            Button btnRefresh = new Button
            {
                Content = "🔄 Rafraîchir",
                Style = (Style)this.FindResource("ActionButton"),
                Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3498DB")),
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0)
            };
            btnRefresh.Click += BtnRefresh_Click;
            Button btnHome = new Button
            {
                Content = "🏠 Accueil",
                Style = (Style)this.FindResource("ActionButton"),
                Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#95A5A6")),
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(8, 0, 0, 0)
            };
            btnHome.Click += BtnHome_Click;
            actionsStack.Children.Add(btnRefresh);
            actionsStack.Children.Add(btnHome);
            actionsPanel.Children.Add(actionsStack);

            // Recréer l'en-tête du tableau de bord
            Border headerBorder = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(25),
                Margin = new Thickness(0, 0, 0, 18)
            };
            headerBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 12,
                ShadowDepth = 2,
                Opacity = 0.12
            };

            StackPanel headerStack = new StackPanel();
            TextBlock titleBlock = new TextBlock
            {
                Text = "📊 Tableau de Bord",
                FontSize = 30,
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50")),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 6)
            };
            TextBlock subtitleBlock = new TextBlock
            {
                Text = "Vue d'ensemble dynamique de votre flotte",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#7F8C8D"))
            };
            headerStack.Children.Add(titleBlock);
            headerStack.Children.Add(subtitleBlock);
            headerBorder.Child = headerStack;

            // Filtres
            Border filterBorder = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 18)
            };
            filterBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 12,
                ShadowDepth = 2,
                Opacity = 0.12
            };

            StackPanel filterStack = new StackPanel();
            filterStack.Children.Add(new TextBlock
            {
                Text = "Filtres",
                FontSize = 16,
                FontWeight = System.Windows.FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50")),
                Margin = new Thickness(0, 0, 0, 10)
            });

            Grid filterGrid = new Grid();
            filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            ComboBox cbFilter = new ComboBox
            {
                DisplayMemberPath = "Label",
                SelectedValuePath = "Id",
                Height = 34,
                MinWidth = 180,
                Margin = new Thickness(0, 0, 12, 0)
            };
            cbFilter.ItemsSource = vehicleOptions;
            cbFilter.SelectedIndex = 0;
            Grid.SetColumn(cbFilter, 0);
            filterGrid.Children.Add(cbFilter);

            DatePicker dpStart = new DatePicker
            {
                Height = 34,
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(dpStart, 1);
            filterGrid.Children.Add(dpStart);

            DatePicker dpEnd = new DatePicker
            {
                Height = 34,
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(dpEnd, 2);
            filterGrid.Children.Add(dpEnd);

            Button btnApply = new Button
            {
                Content = "Appliquer",
                Style = (Style)this.FindResource("ActionButton"),
                Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#27AE60")),
                Margin = new Thickness(0, 0, 6, 0)
            };
            btnApply.Click += BtnApplyFilters_Click;
            Grid.SetColumn(btnApply, 3);
            filterGrid.Children.Add(btnApply);

            CheckBox chkAdminAll = new CheckBox
            {
                Content = "Mode admin : tous les utilisateurs",
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
                Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed
            };
            Grid.SetColumn(chkAdminAll, 4);
            filterGrid.Children.Add(chkAdminAll);

            Button btnToggleAdmin = new Button
            {
                Content = "🔀 Basculer admin",
                Style = (Style)this.FindResource("ActionButton"),
                Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#8E44AD")),
                Foreground = System.Windows.Media.Brushes.White,
                Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed
            };
            btnToggleAdmin.Click += BtnToggleAdmin_Click;
            Grid.SetColumn(btnToggleAdmin, 5);
            filterGrid.Children.Add(btnToggleAdmin);

            Button btnReset = new Button
            {
                Content = "Réinitialiser",
                Style = (Style)this.FindResource("ActionButton"),
                Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#95A5A6")),
                Margin = new Thickness(0, 0, 0, 0)
            };
            btnReset.Click += BtnResetFilters_Click;
            Grid.SetColumn(btnReset, 6);
            filterGrid.Children.Add(btnReset);

            filterStack.Children.Add(filterGrid);
            filterBorder.Child = filterStack;

            // Créer la grille des statistiques
            Grid statsGrid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 25)
            };
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
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
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
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
                FontWeight = System.Windows.FontWeights.Bold,
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
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
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
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50"))
            };
            coutStack.Children.Add(txtCTotal);
            coutBorder.Child = coutStack;

            statsGrid.Children.Add(kmBorder);
            statsGrid.Children.Add(coutBorder);
            // Carte Litres
            Border litresBorder = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(10)
            };
            litresBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 10,
                ShadowDepth = 2,
                Opacity = 0.1
            };
            Grid.SetColumn(litresBorder, 2);

            StackPanel litresStack = new StackPanel();
            litresStack.Children.Add(new TextBlock
            {
                Text = "⛽",
                FontSize = 32,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            });
            litresStack.Children.Add(new TextBlock
            {
                Text = "Carburant consommé",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#7F8C8D")),
                Margin = new Thickness(0, 0, 0, 5)
            });
            TextBlock txtLitresTotal = new TextBlock
            {
                Text = "0 L",
                FontSize = 28,
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50"))
            };
            litresStack.Children.Add(txtLitresTotal);
            litresBorder.Child = litresStack;
            statsGrid.Children.Add(litresBorder);

            // Section graphiques
            Border chartsBorder = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(25),
                Margin = new Thickness(0, 0, 0, 25)
            };
            chartsBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 10,
                ShadowDepth = 2,
                Opacity = 0.1
            };

            StackPanel chartsStack = new StackPanel();
            StackPanel chartsHeader = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 15)
            };
            chartsHeader.Children.Add(new TextBlock
            {
                Text = "📈",
                FontSize = 22,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            });
            chartsHeader.Children.Add(new TextBlock
            {
                Text = "Tendances récentes",
                FontSize = 20,
                FontWeight = System.Windows.FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50")),
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            });
            chartsStack.Children.Add(chartsHeader);

            Grid chartsGrid = new Grid();
            chartsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            chartsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel distancePanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 12, 0)
            };
            distancePanel.Children.Add(new TextBlock
            {
                Text = "Distance parcourue (6 derniers mois)",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#7F8C8D")),
                Margin = new Thickness(0, 0, 0, 8)
            });
            PlotView pvDistance = new PlotView
            {
                Height = 280
            };
            BindingOperations.SetBinding(pvDistance, PlotView.ModelProperty, new Binding(nameof(MonthlyDistanceModel)));
            distancePanel.Children.Add(pvDistance);
            Grid.SetColumn(distancePanel, 0);

            StackPanel costPanel = new StackPanel
            {
                Margin = new Thickness(12, 0, 0, 0)
            };
            costPanel.Children.Add(new TextBlock
            {
                Text = "Coût carburant (6 derniers mois)",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#7F8C8D")),
                Margin = new Thickness(0, 0, 0, 8)
            });
            PlotView pvCost = new PlotView
            {
                Height = 280
            };
            BindingOperations.SetBinding(pvCost, PlotView.ModelProperty, new Binding(nameof(MonthlyCostModel)));
            costPanel.Children.Add(pvCost);
            Grid.SetColumn(costPanel, 1);

            chartsGrid.Children.Add(distancePanel);
            chartsGrid.Children.Add(costPanel);

            chartsStack.Children.Add(chartsGrid);
            chartsBorder.Child = chartsStack;

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
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            });
            headerSuivi.Children.Add(new TextBlock
            {
                Text = "Suivis Récents",
                FontSize = 20,
                FontWeight = System.Windows.FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50")),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
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
            MainContent.Children.Add(actionsPanel);
            MainContent.Children.Add(headerBorder);
            MainContent.Children.Add(filterBorder);
            MainContent.Children.Add(statsGrid);
            MainContent.Children.Add(chartsBorder);
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
            GoHome();
        }

        private void Menu_AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            AddVehicleWindow addWindow = new AddVehicleWindow(connectionString, currentUserId);
            if (addWindow.ShowDialog() == true)
            {
                // Vérifier si on est sur le dashboard avant de recharger
                RefreshCurrentView();
            }
        }

        private void Menu_ListVehicles_Click(object sender, RoutedEventArgs e)
        {
            LoadVehiclesList();
        }

        private void Menu_AddSuivi_Click(object sender, RoutedEventArgs e)
        {
            AddSuiviWindow suiviWindow = new AddSuiviWindow(connectionString, currentUserId);
            if (suiviWindow.ShowDialog() == true)
            {
                // Vérifier si on est sur le dashboard avant de recharger
                RefreshCurrentView();
            }
        }

        private void Menu_ViewSuivi_Click(object sender, RoutedEventArgs e)
        {
            LoadSuiviList();
        }

        private void LoadCharts(SqliteConnection conn, int? vehicleId, DateTime? startDate, DateTime? endDate, bool adminAll)
        {
            MonthlyDistanceModel = BuildMonthlyDistanceModel(conn, vehicleId, startDate, endDate, adminAll);
            OnPropertyChanged(nameof(MonthlyDistanceModel));

            MonthlyCostModel = BuildMonthlyCostModel(conn, vehicleId, startDate, endDate, adminAll);
            OnPropertyChanged(nameof(MonthlyCostModel));
        }

        private PlotModel BuildMonthlyDistanceModel(SqliteConnection conn, int? vehicleId, DateTime? startDate, DateTime? endDate, bool adminAll)
        {
            string filter = BuildFilterClause(vehicleId, startDate, endDate, adminAll);
            string query = $@"SELECT STRFTIME('%Y-%m-01', s.date_suivi) AS mois,
                                    SUM(s.distance_km) AS total_km
                             FROM suivi s
                             JOIN vehicules v ON s.id_vehicule = v.id
                             WHERE 1=1
                               AND s.date_suivi >= DATE('now', '-6 months')
                               {filter}
                             GROUP BY mois
                             ORDER BY mois;";

            using SqliteCommand cmd = new SqliteCommand(query, conn);
            ApplyFilterParameters(cmd, vehicleId, startDate, endDate, adminAll);

            List<(DateTime Month, double Value)> data = new List<(DateTime, double)>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string monthString = reader.GetString("mois");
                    double value = reader.IsDBNull(reader.GetOrdinal("total_km")) ? 0 : reader.GetDouble("total_km");

                    if (DateTime.TryParseExact(monthString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime monthDate))
                    {
                        data.Add((monthDate, value));
                    }
                }
            }

            if (data.Count == 0)
            {
                return BuildEmptyModel("Distance parcourue", "Aucune donnée récente");
            }

            return BuildLineModel(data, "Distance parcourue", "km");
        }

        private PlotModel BuildMonthlyCostModel(SqliteConnection conn, int? vehicleId, DateTime? startDate, DateTime? endDate, bool adminAll)
        {
            string filter = BuildFilterClause(vehicleId, startDate, endDate, adminAll);
            string query = $@"SELECT STRFTIME('%Y-%m-01', s.date_suivi) AS mois,
                                    SUM(s.cout) AS total_cout
                             FROM suivi s
                             JOIN vehicules v ON s.id_vehicule = v.id
                             WHERE 1=1
                               AND s.date_suivi >= DATE('now', '-6 months')
                               {filter}
                             GROUP BY mois
                             ORDER BY mois;";

            using SqliteCommand cmd = new SqliteCommand(query, conn);
            ApplyFilterParameters(cmd, vehicleId, startDate, endDate, adminAll);

            List<(DateTime Month, double Value)> data = new List<(DateTime, double)>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string monthString = reader.GetString("mois");
                    double value = reader.IsDBNull(reader.GetOrdinal("total_cout")) ? 0 : reader.GetDouble("total_cout");

                    if (DateTime.TryParseExact(monthString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime monthDate))
                    {
                        data.Add((monthDate, value));
                    }
                }
            }

            if (data.Count == 0)
            {
                return BuildEmptyModel("Coût carburant", "Aucune donnée récente");
            }

            return BuildColumnModel(data, "Coût carburant", "€");
        }

        private PlotModel BuildLineModel(List<(DateTime Month, double Value)> data, string title, string unit)
        {
            var model = CreateBaseModel(title);

            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MMM yy",
                IntervalType = DateTimeIntervalType.Months,
                MinorIntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                TextColor = OxyColor.FromRgb(44, 62, 80)
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Title = unit,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                TextColor = OxyColor.FromRgb(44, 62, 80)
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            var series = new LineSeries
            {
                StrokeThickness = 2,
                Color = OxyColor.FromRgb(46, 134, 193),
                MarkerType = MarkerType.Circle,
                MarkerSize = 3.5,
                MarkerFill = OxyColor.FromRgb(52, 152, 219)
            };

            foreach (var point in data)
            {
                series.Points.Add(DateTimeAxis.CreateDataPoint(point.Month, point.Value));
            }

            model.Series.Add(series);
            return model;
        }

        private PlotModel BuildColumnModel(List<(DateTime Month, double Value)> data, string title, string unit)
        {
            var model = CreateBaseModel(title);
            var culture = CultureInfo.GetCultureInfo("fr-FR");

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                TextColor = OxyColor.FromRgb(44, 62, 80)
            };

            foreach (var point in data)
            {
                categoryAxis.Labels.Add(point.Month.ToString("MMM yy", culture));
            }

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Title = unit,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                TextColor = OxyColor.FromRgb(44, 62, 80)
            };

            var series = new LineSeries
            {
                StrokeThickness = 2,
                Color = OxyColor.FromRgb(46, 204, 113),
                MarkerType = MarkerType.Circle,
                MarkerSize = 3.5,
                MarkerFill = OxyColor.FromRgb(46, 204, 113)
            };

            for (int i = 0; i < data.Count; i++)
            {
                series.Points.Add(new DataPoint(i, data[i].Value));
            }

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);
            model.Series.Add(series);
            return model;
        }

        private PlotModel BuildEmptyModel(string title, string message)
        {
            var model = CreateBaseModel(title);
            model.Subtitle = message;

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 1,
                IsAxisVisible = false
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 1,
                IsAxisVisible = false
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation
            {
                Text = message,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Middle,
                TextColor = OxyColor.FromRgb(127, 140, 141),
                Stroke = OxyColors.Transparent,
                FontSize = 12,
                TextPosition = new DataPoint(0.5, 0.5)
            });

            return model;
        }

        private PlotModel CreateBaseModel(string title)
        {
            return new PlotModel
            {
                Title = title,
                Background = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Transparent,
                TextColor = OxyColor.FromRgb(44, 62, 80),
                TitleColor = OxyColor.FromRgb(44, 62, 80),
                SubtitleColor = OxyColor.FromRgb(127, 140, 141)
            };
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private (int? vehicleId, DateTime? startDate, DateTime? endDate, bool adminAll) GetFilters()
        {
            ComboBox? cb = this.FindName("cbVehicleFilter") as ComboBox;
            DatePicker? dpStart = this.FindName("dpStartDate") as DatePicker;
            DatePicker? dpEnd = this.FindName("dpEndDate") as DatePicker;
            CheckBox? chkAdminAll = this.FindName("chkAdminAll") as CheckBox;

            int? vehicleId = cb != null && cb.SelectedValue != null && (int)cb.SelectedValue != 0
                ? (int?)cb.SelectedValue
                : null;
            DateTime? startDate = dpStart?.SelectedDate;
            DateTime? endDate = dpEnd?.SelectedDate;
            bool adminAll = chkAdminAll != null && chkAdminAll.IsChecked == true;
            return (vehicleId, startDate, endDate, adminAll);
        }

        private string BuildFilterClause(int? vehicleId, DateTime? startDate, DateTime? endDate, bool adminAll = false)
        {
            string clause = "";
            if (!(isAdmin && adminAll))
            {
                clause += " AND v.id_utilisateur=@id";
            }
            if (vehicleId.HasValue)
            {
                clause += " AND v.id=@veh";
            }
            clause += " AND (@start IS NULL OR s.date_suivi >= @start)";
            clause += " AND (@end IS NULL OR s.date_suivi <= @end)";
            return clause;
        }

        private void ApplyFilterParameters(SqliteCommand cmd, int? vehicleId, DateTime? startDate, DateTime? endDate, bool adminAll = false)
        {
            cmd.Parameters.AddWithValue("@veh", vehicleId.HasValue ? vehicleId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@start", startDate.HasValue ? startDate.Value.Date : DBNull.Value);
            cmd.Parameters.AddWithValue("@end", endDate.HasValue ? endDate.Value.Date : DBNull.Value);
            if (!(isAdmin && adminAll))
            {
                cmd.Parameters.AddWithValue("@id", currentUserId);
            }
        }

        private void ResetFilters()
        {
            ComboBox? cb = this.FindName("cbVehicleFilter") as ComboBox;
            DatePicker? dpStart = this.FindName("dpStartDate") as DatePicker;
            DatePicker? dpEnd = this.FindName("dpEndDate") as DatePicker;
            CheckBox? chkAdminAll = this.FindName("chkAdminAll") as CheckBox;
            if (cb != null)
            {
                cb.SelectedIndex = 0;
            }
            if (dpStart != null) dpStart.SelectedDate = null;
            if (dpEnd != null) dpEnd.SelectedDate = null;
            if (chkAdminAll != null && isAdmin) chkAdminAll.IsChecked = false;
        }

        private void LoadFilterOptions()
        {
            vehicleOptions = new List<VehicleOption>
            {
                new VehicleOption { Id = 0, Label = "Tous les véhicules" }
            };

            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT id, immatriculation, marque, modele 
                                     FROM vehicules 
                                     WHERE id_utilisateur=@id
                                     ORDER BY marque, modele";

                    using SqliteCommand cmd = new SqliteCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", currentUserId);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        vehicleOptions.Add(new VehicleOption
                        {
                            Id = reader.GetInt32("id"),
                            Label = $"{reader.GetString("immatriculation")} - {reader.GetString("marque")} {reader.GetString("modele")}"
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des véhicules :\n{ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            ComboBox? cb = this.FindName("cbVehicleFilter") as ComboBox;
            if (cb != null)
            {
                cb.ItemsSource = vehicleOptions;
                cb.SelectedIndex = 0;
            }
        }

        private class VehicleOption
        {
            public int Id { get; set; }
            public string Label { get; set; } = string.Empty;
        }

        private void OpenAdmin_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow admin = new AdminWindow(currentUserId);
            admin.Show();
        }

        private void GoHome()
        {
            RestoreDashboard();
            LoadDashboard();
            currentView = "Dashboard";
        }

        private void RefreshCurrentView()
        {
            switch (currentView)
            {
                case "Vehicles":
                    LoadVehiclesList();
                    break;
                case "Suivis":
                    LoadSuiviList();
                    break;
                default:
                    GoHome();
                    break;
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshCurrentView();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            GoHome();
        }

        private void BtnApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            GoHome();
        }

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            ResetFilters();
            GoHome();
        }

        private void BtnToggleAdmin_Click(object sender, RoutedEventArgs e)
        {
            CheckBox? chk = this.FindName("chkAdminAll") as CheckBox;
            if (chk != null && isAdmin)
            {
                chk.IsChecked = !(chk.IsChecked ?? false);
                GoHome();
            }
        }

        private void LoadVehiclesList()
        {
            DataTable dt = new DataTable();
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var (_, _, _, adminAll) = GetFilters();
                    string query = @"SELECT id AS 'ID',
                                            immatriculation AS 'Immatriculation', 
                                            marque AS 'Marque', 
                                            modele AS 'Modèle', 
                                            annee AS 'Année', 
                                            carburant AS 'Carburant', 
                                            kilometrage || ' km' AS 'Kilométrage'
                                     FROM vehicules 
                                     WHERE 1=1 " + (isAdmin && adminAll ? "" : "AND id_utilisateur=@id") + @"
                                     ORDER BY marque, modele";

                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        if (!(isAdmin && adminAll))
                        {
                            cmd.Parameters.AddWithValue("@id", currentUserId);
                        }
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }

                    MainContent.Children.Clear();

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
                        FontWeight = System.Windows.FontWeights.Bold,
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

                    currentView = "Vehicles";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des véhicules :\n{ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    currentView = "Dashboard";
                }
            }
        }

        private void LoadSuiviList()
        {
            DataTable dt = new DataTable();
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var (_, _, _, adminAll) = GetFilters();
                    string query = @"SELECT s.id AS 'ID',
                                            v.immatriculation AS 'Immatriculation', 
                                            STRFTIME('%d/%m/%Y', s.date_suivi) AS 'Date',
                                            s.carburant_litre || ' L' AS 'Carburant', 
                                            PRINTF('%.2f €', s.cout) AS 'Coût', 
                                            s.distance_km || ' km' AS 'Distance',
                                            s.commentaire AS 'Commentaire'
                                     FROM suivi s
                                     JOIN vehicules v ON s.id_vehicule = v.id
                                     WHERE 1=1 " + (isAdmin && adminAll ? "" : "AND v.id_utilisateur=@id") + @"
                                     ORDER BY s.date_suivi DESC";

                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        if (!(isAdmin && adminAll))
                        {
                            cmd.Parameters.AddWithValue("@id", currentUserId);
                        }
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }

                    MainContent.Children.Clear();

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
                        FontWeight = System.Windows.FontWeights.Bold,
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

                    currentView = "Suivis";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des suivis :\n{ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    currentView = "Dashboard";
                }
            }
        }
    }
}