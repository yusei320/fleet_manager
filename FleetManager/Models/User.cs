using System;

namespace FleetManager.Models
{
    /// <summary>
    /// Modèle représentant un utilisateur de l'application
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Utilisateur";
        public DateTime? BloqueJusqu { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;

        /// <summary>
        /// Nom complet de l'utilisateur
        /// </summary>
        public string NomComplet => $"{Prenom} {Nom}";

        /// <summary>
        /// Indique si l'utilisateur est un administrateur
        /// </summary>
        public bool EstAdministrateur => Role.Equals("Administrateur", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indique si le compte est actuellement bloqué
        /// </summary>
        public bool EstBloque => BloqueJusqu.HasValue && DateTime.Now < BloqueJusqu.Value;
    }
}
