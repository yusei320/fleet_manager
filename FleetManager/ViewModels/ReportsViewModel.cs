using FleetManager.Models;
using FleetManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FleetManager.ViewModels
{
    /// <summary>
    /// ViewModel pour la génération et consultation des rapports (Admin uniquement)
    /// </summary>
    public class ReportsViewModel : ObservableBase
    {
        private readonly DatabaseService _dbService;
        private User? _currentUser;
        private string _selectedReportType = "Dépenses";
        private DateTime _startDate = DateTime.Now.AddMonths(-6);
        private DateTime _endDate = DateTime.Now;

        public ObservableCollection<StatisticsService.MonthlyData> ReportData { get; } = new ObservableCollection<StatisticsService.MonthlyData>();
        public ObservableCollection<string> ReportTypes { get; } = new ObservableCollection<string>
        {
            "Dépenses",
            "Carburant",
            "Distance",
            "Consommation"
        };

        public string SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                SetProperty(ref _selectedReportType, value);
                GenerateReport();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                SetProperty(ref _startDate, value);
                GenerateReport();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                SetProperty(ref _endDate, value);
                GenerateReport();
            }
        }

        // Statistiques du rapport
        private double _totalValue;
        public double TotalValue
        {
            get => _totalValue;
            set => SetProperty(ref _totalValue, value);
        }

        private double _averageValue;
        public double AverageValue
        {
            get => _averageValue;
            set => SetProperty(ref _averageValue, value);
        }

        private int _recordCount;
        public int RecordCount
        {
            get => _recordCount;
            set => SetProperty(ref _recordCount, value);
        }

        // Commandes
        public ICommand GenerateReportCommand { get; }
        public ICommand ExportReportCommand { get; }
        public ICommand BackCommand { get; }

        public ReportsViewModel()
        {
            _dbService = new DatabaseService();
            _currentUser = SessionService.Instance.CurrentUser;

            // Vérifier que l'utilisateur est admin
            if (_currentUser == null || !_currentUser.EstAdministrateur)
            {
                throw new UnauthorizedAccessException("Accès refusé : Réservé aux administrateurs");
            }

            // Initialiser les commandes
            GenerateReportCommand = new RelayCommand(_ => GenerateReport());
            ExportReportCommand = new RelayCommand(_ => ExportReport());
            BackCommand = new RelayCommand(_ => Back());

            // Générer le rapport initial
            GenerateReport();
        }

        /// <summary>
        /// Génère le rapport selon les critères sélectionnés
        /// </summary>
        private void GenerateReport()
        {
            ReportData.Clear();

            List<StatisticsService.MonthlyData> data;

            switch (SelectedReportType)
            {
                case "Dépenses":
                    data = StatisticsService.GetExpensesByMonth();
                    break;
                case "Carburant":
                    data = StatisticsService.GetFuelByMonth();
                    break;
                case "Distance":
                    data = StatisticsService.GetDistanceByMonth();
                    break;
                case "Consommation":
                    var consumptionData = StatisticsService.GetAverageConsumptionPerVehicle();
                    // Convertir en MonthlyData pour affichage
                    data = consumptionData.Select(kvp => new StatisticsService.MonthlyData
                    {
                        Month = kvp.Key,
                        Value = kvp.Value,
                        Percentage = kvp.Value // Simplifié
                    }).ToList();
                    break;
                default:
                    data = new List<StatisticsService.MonthlyData>();
                    break;
            }

            foreach (var item in data)
            {
                ReportData.Add(item);
            }

            // Calculer les statistiques
            CalculateStatistics(data);
        }

        /// <summary>
        /// Calcule les statistiques du rapport
        /// </summary>
        private void CalculateStatistics(List<StatisticsService.MonthlyData> data)
        {
            if (data.Any())
            {
                TotalValue = data.Sum(x => x.Value);
                AverageValue = data.Average(x => x.Value);
                RecordCount = data.Count;
            }
            else
            {
                TotalValue = 0;
                AverageValue = 0;
                RecordCount = 0;
            }
        }

        /// <summary>
        /// Exporte le rapport (simulé pour l'instant)
        /// </summary>
        private void ExportReport()
        {
            try
            {
                var message = $"Rapport {SelectedReportType} généré avec succès !\n\n" +
                            $"Période : {StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}\n" +
                            $"Total : {TotalValue:N2}\n" +
                            $"Moyenne : {AverageValue:N2}\n" +
                            $"Enregistrements : {RecordCount}";

                System.Windows.MessageBox.Show(message, "Export Rapport", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'export : {ex.Message}", "Erreur", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Retourne au dashboard
        /// </summary>
        private void Back()
        {
            // Sera géré par la vue
        }
    }
}
