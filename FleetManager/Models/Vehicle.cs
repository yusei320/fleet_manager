using System;

namespace FleetManager.Models
{
    /// <summary>
    /// Modèle représentant un véhicule de la flotte
    /// </summary>
    public class Vehicle
    {
        public int Id { get; set; }
        public string Immatriculation { get; set; } = string.Empty;
        public string Marque { get; set; } = string.Empty;
        public string Modele { get; set; } = string.Empty;
        public int? Annee { get; set; }
        public string Carburant { get; set; } = string.Empty;
        public int Kilometrage { get; set; } = 0;
        public int IdUtilisateur { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;

        /// <summary>
        /// Description complète du véhicule
        /// </summary>
        public string DescriptionComplet => $"{Immatriculation} - {Marque} {Modele}";

        /// <summary>
        /// Description avec année si disponible
        /// </summary>
        public string DescriptionAvecAnnee => Annee.HasValue 
            ? $"{Immatriculation} - {Marque} {Modele} ({Annee.Value})" 
            : DescriptionComplet;
    }
}
