using FleetManager.ViewModels;
using System.Windows;

namespace FleetManager
{
    /// <summary>
    /// Logique d'interaction pour ReportsWindow.xaml
    /// </summary>
    public partial class ReportsWindow : Window
    {
        private readonly ReportsViewModel _viewModel;

        public ReportsWindow()
        {
            InitializeComponent();
            _viewModel = new ReportsViewModel();
            DataContext = _viewModel;
        }
    }
}
