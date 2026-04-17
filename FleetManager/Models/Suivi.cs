using System;

namespace FleetManager.Models
{
    /// <summary>
    /// Modèle représentant un suivi de ravitaillement/entretien
    /// </summary>
    public class Suivi
    {
        public int Id { get; set; }
        public int IdVehicule { get; set; }
        public DateTime DateSuivi { get; set; } = DateTime.Now;
        public double? CarburantLitre { get; set; }
        public double? Cout { get; set; }
        public double? DistanceKm { get; set; }
        public string? Commentaire { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;

        /// <summary>
        /// Consommation moyenne calculée (L/100km)
        /// </summary>
        public double? ConsommationMoyenne
        {
            get
            {
                if (CarburantLitre.HasValue && DistanceKm.HasValue && DistanceKm.Value > 0)
                {
                    return (CarburantLitre.Value / DistanceKm.Value) * 100;
                }
                return null;
            }
        }

        /// <summary>
        /// Coût au kilomètre
        /// </summary>
        public double? CoutAuKm
        {
            get
            {
                if (Cout.HasValue && DistanceKm.HasValue && DistanceKm.Value > 0)
                {
                    return Cout.Value / DistanceKm.Value;
                }
                return null;
            }
        }
    }
}
