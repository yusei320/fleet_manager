using FleetManager.ViewModels;
using System.Windows;

namespace FleetManager
{
    /// <summary>
    /// Logique d'interaction pour UsersWindow.xaml
    /// </summary>
    public partial class UsersWindow : Window
    {
        private readonly UsersViewModel _viewModel;

        public UsersWindow()
        {
            InitializeComponent();
            _viewModel = new UsersViewModel();
            DataContext = _viewModel;
            Loaded += UsersWindow_Loaded;
        }

        private void UsersWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Charger les utilisateurs après le chargement de la fenêtre
            try
            {
                _viewModel.LoadUsers();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du chargement des utilisateurs : {ex.Message}", "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
