using FleetManager.Models;
using FleetManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace FleetManager.ViewModels
{
    /// <summary>
    /// ViewModel pour le dashboard administrateur
    /// Gère l'affichage des statistiques, graphiques et navigation
    /// </summary>
    public class AdminDashboardViewModel : ObservableBase
    {
        private readonly DatabaseService _dbService;
        private string _currentView = "Dashboard";
        private User? _currentUser;

        // Propriétés de navigation
        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // Statistiques
        private int _totalVehicles;
        public int TotalVehicles
        {
            get => _totalVehicles;
            set => SetProperty(ref _totalVehicles, value);
        }

        private int _totalUsers;
        public int TotalUsers
        {
            get => _totalUsers;
            set => SetProperty(ref _totalUsers, value);
        }

        private int _totalSuivis;
        public int TotalSuivis
        {
            get => _totalSuivis;
            set => SetProperty(ref _totalSuivis, value);
        }

        private double _TotalKilometres;
        public double TotalKilometres
        {
            get => _TotalKilometres;
            set => SetProperty(ref _TotalKilometres, value);
        }

        private double _TotalDepenses;
        public double TotalDepenses
        {
            get => _TotalDepenses;
            set => SetProperty(ref _TotalDepenses, value);
        }

        private double _TotalCarburant;
        public double TotalCarburant
        {
            get => _TotalCarburant;
            set => SetProperty(ref _TotalCarburant, value);
        }

        // Données des graphiques
        public ObservableCollection<StatisticsService.MonthlyData> ExpensesByMonth { get; } = new ObservableCollection<StatisticsService.MonthlyData>();
        public ObservableCollection<StatisticsService.MonthlyData> FuelByMonth { get; } = new ObservableCollection<StatisticsService.MonthlyData>();
        public ObservableCollection<StatisticsService.MonthlyData> DistanceByMonth { get; } = new ObservableCollection<StatisticsService.MonthlyData>();

        // Collections pour les DataGrids
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<Vehicle> Vehicles { get; } = new ObservableCollection<Vehicle>();
        public ObservableCollection<Suivi> Suivis { get; } = new ObservableCollection<Suivi>();

        // Commandes de navigation
        public ICommand NavigateToDashboard { get; }
        public ICommand NavigateToUsers { get; }
        public ICommand NavigateToVehicles { get; }
        public ICommand NavigateToSuivis { get; }
        public ICommand LogoutCommand { get; }
        public ICommand OpenVehiclesCommand { get; }
        public ICommand OpenSuivisCommand { get; }
        public ICommand OpenUsersCommand { get; }
        public ICommand OpenReportsCommand { get; }

        public AdminDashboardViewModel()
        {
            _dbService = new DatabaseService();
            _currentUser = SessionService.Instance.CurrentUser;

            // Initialiser les commandes
            NavigateToDashboard = new RelayCommand(_ => CurrentView = "Dashboard");
            NavigateToUsers = new RelayCommand(_ => CurrentView = "Users");
            NavigateToVehicles = new RelayCommand(_ => CurrentView = "Vehicles");
            NavigateToSuivis = new RelayCommand(_ => CurrentView = "Suivis");
            LogoutCommand = new RelayCommand(_ => Logout());
            OpenVehiclesCommand = new RelayCommand(_ => OpenVehiclesWindow());
            OpenSuivisCommand = new RelayCommand(_ => OpenSuivisWindow());
            OpenUsersCommand = new RelayCommand(_ => OpenUsersWindow());
            OpenReportsCommand = new RelayCommand(_ => OpenReportsWindow());

            // Charger les données initiales
            LoadDashboardData();
        }

        /// <summary>
        /// Charge les données du dashboard
        /// </summary>
        public void LoadDashboardData()
        {
            // Charger les statistiques globales
            var stats = _dbService.GetAllStatistics();
            TotalKilometres = stats.TotalKm;
            TotalDepenses = stats.TotalCout;
            TotalCarburant = stats.TotalLitres;

            // Charger les comptes
            var allUsers = _dbService.GetAllUsers();
            TotalUsers = allUsers.Count;
            
            var allVehicles = allUsers.SelectMany(u => _dbService.GetVehiclesByUser(u.Id)).ToList();
            TotalVehicles = allVehicles.Count;

            var allSuivis = _dbService.GetAllSuivis();
            TotalSuivis = allSuivis.Count;

            // Charger les collections
            LoadUsers();
            LoadVehicles();
            LoadSuivis();

            // Charger les données des graphiques
            LoadChartData();
        }

        /// <summary>
        /// Charge les données des graphiques
        /// </summary>
        private void LoadChartData()
        {
            ExpensesByMonth.Clear();
            FuelByMonth.Clear();
            DistanceByMonth.Clear();

            var expenses = StatisticsService.GetExpensesByMonth();
            foreach (var item in expenses)
            {
                ExpensesByMonth.Add(item);
            }

            var fuel = StatisticsService.GetFuelByMonth();
            foreach (var item in fuel)
            {
                FuelByMonth.Add(item);
            }

            var distance = StatisticsService.GetDistanceByMonth();
            foreach (var item in distance)
            {
                DistanceByMonth.Add(item);
            }
        }

        /// <summary>
        /// Charge la liste des utilisateurs
        /// </summary>
        public void LoadUsers()
        {
            Users.Clear();
            var users = _dbService.GetAllUsers();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        /// <summary>
        /// Charge la liste des véhicules
        /// </summary>
        public void LoadVehicles()
        {
            Vehicles.Clear();
            var users = _dbService.GetAllUsers();
            foreach (var user in users)
            {
                var userVehicles = _dbService.GetVehiclesByUser(user.Id);
                foreach (var vehicle in userVehicles)
                {
                    Vehicles.Add(vehicle);
                }
            }
        }

        /// <summary>
        /// Charge la liste des suivis
        /// </summary>
        public void LoadSuivis()
        {
            Suivis.Clear();
            var suivis = _dbService.GetAllSuivis(limit: 50);
            foreach (var suivi in suivis)
            {
                Suivis.Add(suivi);
            }
        }

        /// <summary>
        /// Déconnecte l'utilisateur
        /// </summary>
        private void Logout()
        {
            SessionService.Instance.Logout();
            // Fermer la fenêtre sera géré par la vue
        }

        /// <summary>
        /// Ouvre la fenêtre de gestion des utilisateurs
        /// </summary>
        private void OpenUsersWindow()
        {
            var usersWindow = new UsersWindow();
            usersWindow.Show();
        }

        /// <summary>
        /// Ouvre la fenêtre des rapports
        /// </summary>
        private void OpenReportsWindow()
        {
            var reportsWindow = new ReportsWindow();
            reportsWindow.Show();
        }

        /// <summary>
        /// Ouvre la fenêtre de gestion des véhicules
        /// </summary>
        private void OpenVehiclesWindow()
        {
            try
            {
                var vehiclesWindow = new VehiclesWindow(isAdmin: true);
                vehiclesWindow.Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'ouverture de la fenêtre véhicules : {ex.Message}", "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Ouvre la fenêtre de gestion des suivis
        /// </summary>
        private void OpenSuivisWindow()
        {
            try
            {
                var suivisWindow = new SuivisWindow(isAdmin: true);
                suivisWindow.Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'ouverture de la fenêtre suivis : {ex.Message}", "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Rafraîchit les données actuelles
        /// </summary>
        public void RefreshData()
        {
            LoadDashboardData();
        }
    }
}
