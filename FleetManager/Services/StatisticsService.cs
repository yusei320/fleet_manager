using FleetManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FleetManager.Services
{
    /// <summary>
    /// Service pour calculer les statistiques et préparer les données pour les graphiques
    /// </summary>
    public static class StatisticsService
    {
        /// <summary>
        /// Classe pour représenter les données mensuelles
        /// </summary>
        public class MonthlyData
        {
            public string Month { get; set; } = string.Empty;
            public double Value { get; set; }
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        /// <summary>
        /// Récupère les dépenses par mois
        /// </summary>
        public static List<MonthlyData> GetExpensesByMonth(int? userId = null)
        {
            var dbService = new DatabaseService();
            var suivis = userId.HasValue 
                ? dbService.GetSuivisByUser(userId.Value, limit: 1000)
                : dbService.GetAllSuivis(limit: 1000);

            var result = suivis
                .Where(s => s.Cout.HasValue)
                .GroupBy(s => new { s.DateSuivi.Year, s.DateSuivi.Month })
                .Select(g => new MonthlyData
                {
                    Month = GetMonthName(g.Key.Month),
                    Value = g.Sum(s => s.Cout ?? 0),
                    Count = g.Count()
                })
                .OrderBy(x => GetMonthOrder(x.Month))
                .ToList();

            // Calculer le pourcentage
            if (result.Any())
            {
                var maxValue = result.Max(x => x.Value);
                foreach (var item in result)
                {
                    item.Percentage = maxValue > 0 ? (item.Value / maxValue) * 100 : 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Récupère le carburant par mois
        /// </summary>
        public static List<MonthlyData> GetFuelByMonth(int? userId = null)
        {
            var dbService = new DatabaseService();
            var suivis = userId.HasValue 
                ? dbService.GetSuivisByUser(userId.Value, limit: 1000)
                : dbService.GetAllSuivis(limit: 1000);

            var result = suivis
                .Where(s => s.CarburantLitre.HasValue)
                .GroupBy(s => new { s.DateSuivi.Year, s.DateSuivi.Month })
                .Select(g => new MonthlyData
                {
                    Month = GetMonthName(g.Key.Month),
                    Value = g.Sum(s => s.CarburantLitre ?? 0),
                    Count = g.Count()
                })
                .OrderBy(x => GetMonthOrder(x.Month))
                .ToList();

            // Calculer le pourcentage
            if (result.Any())
            {
                var maxValue = result.Max(x => x.Value);
                foreach (var item in result)
                {
                    item.Percentage = maxValue > 0 ? (item.Value / maxValue) * 100 : 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Récupère la distance par mois
        /// </summary>
        public static List<MonthlyData> GetDistanceByMonth(int? userId = null)
        {
            var dbService = new DatabaseService();
            var suivis = userId.HasValue 
                ? dbService.GetSuivisByUser(userId.Value, limit: 1000)
                : dbService.GetAllSuivis(limit: 1000);

            var result = suivis
                .Where(s => s.DistanceKm.HasValue)
                .GroupBy(s => new { s.DateSuivi.Year, s.DateSuivi.Month })
                .Select(g => new MonthlyData
                {
                    Month = GetMonthName(g.Key.Month),
                    Value = g.Sum(s => s.DistanceKm ?? 0),
                    Count = g.Count()
                })
                .OrderBy(x => GetMonthOrder(x.Month))
                .ToList();

            // Calculer le pourcentage
            if (result.Any())
            {
                var maxValue = result.Max(x => x.Value);
                foreach (var item in result)
                {
                    item.Percentage = maxValue > 0 ? (item.Value / maxValue) * 100 : 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Obtient le nom du mois en français
        /// </summary>
        private static string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Jan",
                2 => "Fév",
                3 => "Mar",
                4 => "Avr",
                5 => "Mai",
                6 => "Juin",
                7 => "Juil",
                8 => "Août",
                9 => "Sep",
                10 => "Oct",
                11 => "Nov",
                12 => "Déc",
                _ => "Inconnu"
            };
        }

        /// <summary>
        /// Obtient l'ordre du mois pour le tri
        /// </summary>
        private static int GetMonthOrder(string monthName)
        {
            return monthName switch
            {
                "Jan" => 1,
                "Fév" => 2,
                "Mar" => 3,
                "Avr" => 4,
                "Mai" => 5,
                "Juin" => 6,
                "Juil" => 7,
                "Août" => 8,
                "Sep" => 9,
                "Oct" => 10,
                "Nov" => 11,
                "Déc" => 12,
                _ => 0
            };
        }

        /// <summary>
        /// Récupère les données pour les 12 derniers mois
        /// </summary>
        public static List<MonthlyData> GetLast12MonthsExpenses(int? userId = null)
        {
            var dbService = new DatabaseService();
            var suivis = userId.HasValue 
                ? dbService.GetSuivisByUser(userId.Value, limit: 1000)
                : dbService.GetAllSuivis(limit: 1000);

            var twelveMonthsAgo = DateTime.Now.AddMonths(-12);
            var filteredSuivis = suivis.Where(s => s.DateSuivi >= twelveMonthsAgo);

            return filteredSuivis
                .Where(s => s.Cout.HasValue)
                .GroupBy(s => new { s.DateSuivi.Year, s.DateSuivi.Month })
                .Select(g => new MonthlyData
                {
                    Month = $"{g.Key.Month:00}/{g.Key.Year}",
                    Value = g.Sum(s => s.Cout ?? 0),
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();
        }

        /// <summary>
        /// Récupère les données de consommation moyenne par véhicule
        /// </summary>
        public static Dictionary<string, double> GetAverageConsumptionPerVehicle(int? userId = null)
        {
            var dbService = new DatabaseService();
            var suivis = userId.HasValue 
                ? dbService.GetSuivisByUser(userId.Value, limit: 1000)
                : dbService.GetAllSuivis(limit: 1000);

            var vehicles = userId.HasValue 
                ? dbService.GetVehiclesByUser(userId.Value)
                : dbService.GetAllUsers().SelectMany(u => dbService.GetVehiclesByUser(u.Id)).ToList();

            var result = new Dictionary<string, double>();

            foreach (var vehicle in vehicles)
            {
                var vehicleSuivis = suivis.Where(s => s.IdVehicule == vehicle.Id && s.CarburantLitre.HasValue && s.DistanceKm.HasValue).ToList();
                if (vehicleSuivis.Any())
                {
                    var totalFuel = vehicleSuivis.Sum(s => s.CarburantLitre ?? 0);
                    var totalDistance = vehicleSuivis.Sum(s => s.DistanceKm ?? 0);
                    if (totalDistance > 0)
                    {
                        var avgConsumption = (totalFuel / totalDistance) * 100; // L/100km
                        result[vehicle.DescriptionComplet] = avgConsumption;
                    }
                }
            }

            return result;
        }
    }
}
