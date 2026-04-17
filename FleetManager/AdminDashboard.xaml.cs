using FleetManager.ViewModels;
using System.Windows;

namespace FleetManager
{
    /// <summary>
    /// Logique d'interaction pour AdminDashboard.xaml
    /// </summary>
    public partial class AdminDashboard : Window
    {
        private readonly AdminDashboardViewModel _viewModel;

        public AdminDashboard()
        {
            InitializeComponent();
            _viewModel = new AdminDashboardViewModel();
            DataContext = _viewModel;
        }

        public AdminDashboard(int userId) : this()
        {
            // Constructeur avec ID utilisateur pour compatibilité
        }
    }
}
