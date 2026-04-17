using FleetManager.Models;
using FleetManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FleetManager.ViewModels
{
    /// <summary>
    /// ViewModel pour le dashboard utilisateur
    /// Gère l'affichage des statistiques et données personnelles de l'utilisateur
    /// </summary>
    public class UserDashboardViewModel : ObservableBase
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

        // Statistiques personnelles
        private int _totalVehicles;
        public int TotalVehicles
        {
            get => _totalVehicles;
            set => SetProperty(ref _totalVehicles, value);
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

        // Collections pour les DataGrids
        public ObservableCollection<Vehicle> Vehicles { get; } = new ObservableCollection<Vehicle>();
        public ObservableCollection<Suivi> Suivis { get; } = new ObservableCollection<Suivi>();

        // Commandes de navigation
        public ICommand NavigateToDashboard { get; }
        public ICommand NavigateToVehicles { get; }
        public ICommand NavigateToSuivis { get; }
        public ICommand LogoutCommand { get; }
        public ICommand AddVehicleCommand { get; }
        public ICommand AddSuiviCommand { get; }
        public ICommand OpenVehiclesCommand { get; }
        public ICommand OpenSuivisCommand { get; }

        public UserDashboardViewModel()
        {
            _dbService = new DatabaseService();
            _currentUser = SessionService.Instance.CurrentUser;

            if (_currentUser == null)
                throw new InvalidOperationException("Aucun utilisateur connecté");

            // Initialiser les commandes
            NavigateToDashboard = new RelayCommand(_ => CurrentView = "Dashboard");
            NavigateToVehicles = new RelayCommand(_ => CurrentView = "Vehicles");
            NavigateToSuivis = new RelayCommand(_ => CurrentView = "Suivis");
            LogoutCommand = new RelayCommand(_ => Logout());
            AddVehicleCommand = new RelayCommand(_ => AddVehicle());
            AddSuiviCommand = new RelayCommand(_ => AddSuivi());
            OpenVehiclesCommand = new RelayCommand(_ => OpenVehiclesWindow());
            OpenSuivisCommand = new RelayCommand(_ => OpenSuivisWindow());

            // Charger les données initiales
            LoadDashboardData();
        }

        /// <summary>
        /// Charge les données du dashboard utilisateur
        /// </summary>
        public void LoadDashboardData()
        {
            if (_currentUser == null) return;

            // Charger les statistiques personnelles
            var stats = _dbService.GetUserStatistics(_currentUser.Id);
            TotalKilometres = stats.TotalKm;
            TotalDepenses = stats.TotalCout;
            TotalCarburant = stats.TotalLitres;

            // Charger les véhicules de l'utilisateur
            var userVehicles = _dbService.GetVehiclesByUser(_currentUser.Id);
            TotalVehicles = userVehicles.Count;

            // Charger les suivis de l'utilisateur
            var userSuivis = _dbService.GetSuivisByUser(_currentUser.Id);
            TotalSuivis = userSuivis.Count;

            // Charger les collections
            LoadVehicles();
            LoadSuivis();
        }

        /// <summary>
        /// Charge la liste des véhicules de l'utilisateur
        /// </summary>
        public void LoadVehicles()
        {
            Vehicles.Clear();
            if (_currentUser == null) return;

            var vehicles = _dbService.GetVehiclesByUser(_currentUser.Id);
            foreach (var vehicle in vehicles)
            {
                Vehicles.Add(vehicle);
            }
        }

        /// <summary>
        /// Charge la liste des suivis de l'utilisateur
        /// </summary>
        public void LoadSuivis()
        {
            Suivis.Clear();
            if (_currentUser == null) return;

            var suivis = _dbService.GetSuivisByUser(_currentUser.Id, limit: 50);
            foreach (var suivi in suivis)
            {
                Suivis.Add(suivi);
            }
        }

        /// <summary>
        /// Ouvre la fenêtre d'ajout de véhicule
        /// </summary>
        private void AddVehicle()
        {
            if (_currentUser == null) return;

            var addVehicleWindow = new AddVehicleWindow("Data Source=fleet_manager.db", _currentUser.Id);
            if (addVehicleWindow.ShowDialog() == true)
            {
                LoadVehicles();
                LoadDashboardData();
            }
        }

        /// <summary>
        /// Ouvre la fenêtre d'ajout de suivi
        /// </summary>
        private void AddSuivi()
        {
            if (_currentUser == null) return;

            var addSuiviWindow = new AddSuiviWindow("Data Source=fleet_manager.db", _currentUser.Id);
            if (addSuiviWindow.ShowDialog() == true)
            {
                LoadSuivis();
                LoadDashboardData();
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
        /// Ouvre la fenêtre de gestion des véhicules
        /// </summary>
        private void OpenVehiclesWindow()
        {
            var vehiclesWindow = new VehiclesWindow(isAdmin: false);
            vehiclesWindow.Show();
        }

        /// <summary>
        /// Ouvre la fenêtre de gestion des suivis
        /// </summary>
        private void OpenSuivisWindow()
        {
            var suivisWindow = new SuivisWindow(isAdmin: false);
            suivisWindow.Show();
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
