using FleetManager.ViewModels;
using FleetManager.Services;
using System.Windows;

namespace FleetManager
{
    /// <summary>
    /// Logique d'interaction pour UserDashboard.xaml
    /// </summary>
    public partial class UserDashboard : Window
    {
        private readonly UserDashboardViewModel _viewModel;

        public UserDashboard()
        {
            InitializeComponent();
            _viewModel = new UserDashboardViewModel();
            DataContext = _viewModel;
        }

        public UserDashboard(int userId) : this()
        {
            // Constructeur avec ID utilisateur pour compatibilité
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Si l'utilisateur se déconnecte, fermer l'application ou retourner au login
            if (!SessionService.Instance.IsLoggedIn)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
        }
    }
}
