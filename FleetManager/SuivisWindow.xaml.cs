using FleetManager.ViewModels;
using System.Windows;

namespace FleetManager
{
    /// <summary>
    /// Logique d'interaction pour SuivisWindow.xaml
    /// </summary>
    public partial class SuivisWindow : Window
    {
        private readonly SuivisViewModel _viewModel;

        public SuivisWindow()
        {
            InitializeComponent();
            _viewModel = new SuivisViewModel();
            DataContext = _viewModel;
            Loaded += SuivisWindow_Loaded;
        }

        public SuivisWindow(bool isAdmin) : this()
        {
            // Constructeur avec paramètre admin
            _viewModel = new SuivisViewModel(isAdmin);
            DataContext = _viewModel;
        }

        public SuivisWindow(string connectionString, int userId) : this()
        {
            // Constructeur pour compatibilité avec AddSuiviWindow
        }

        private void SuivisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Charger les données après le chargement de la fenêtre
            try
            {
                _viewModel.LoadVehicles();
                _viewModel.LoadSuivis();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}", "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
