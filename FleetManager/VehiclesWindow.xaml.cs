using FleetManager.ViewModels;
using System.Windows;

namespace FleetManager
{
    /// <summary>
    /// Logique d'interaction pour VehiclesWindow.xaml
    /// </summary>
    public partial class VehiclesWindow : Window
    {
        private readonly VehiclesViewModel _viewModel;

        public VehiclesWindow()
        {
            InitializeComponent();
            _viewModel = new VehiclesViewModel();
            DataContext = _viewModel;
            Loaded += VehiclesWindow_Loaded;
        }

        public VehiclesWindow(bool isAdmin) : this()
        {
            // Constructeur avec paramètre admin
            _viewModel = new VehiclesViewModel(isAdmin);
            DataContext = _viewModel;
        }

        public VehiclesWindow(string connectionString, int userId) : this()
        {
            // Constructeur pour compatibilité avec AddVehicleWindow
        }

        private void VehiclesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Charger les véhicules après le chargement de la fenêtre
            try
            {
                _viewModel.LoadVehicles();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du chargement des véhicules : {ex.Message}", "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
