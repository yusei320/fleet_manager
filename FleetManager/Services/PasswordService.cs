namespace FleetManager.Services
{
    /// <summary>
    /// Service pour le hachage et la vérification des mots de passe
    /// Utilise BCrypt pour un hachage sécurisé
    /// </summary>
    public static class PasswordService
    {
        /// <summary>
        /// Hache un mot de passe en clair
        /// </summary>
        /// <param name="plainPassword">Mot de passe en clair</param>
        /// <returns>Mot de passe haché</returns>
        public static string HashPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
        }

        /// <summary>
        /// Vérifie si un mot de passe correspond au hachage stocké
        /// </summary>
        /// <param name="plainPassword">Mot de passe en clair à vérifier</param>
        /// <param name="hashedPassword">Hachage stocké</param>
        /// <returns>True si le mot de passe correspond, false sinon</returns>
        public static bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }

        /// <summary>
        /// Valide la force d'un mot de passe
        /// </summary>
        /// <param name="password">Mot de passe à valider</param>
        /// <returns>Message d'erreur si le mot de passe est faible, null si le mot de passe est fort</returns>
        public static string? ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Le mot de passe ne peut pas être vide.";

            if (password.Length < 8)
                return "Le mot de passe doit contenir au moins 8 caractères.";

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
                return "Le mot de passe doit contenir au moins une majuscule.";

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
                return "Le mot de passe doit contenir au moins une minuscule.";

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
                return "Le mot de passe doit contenir au moins un chiffre.";

            // Optionnel: exiger un caractère spécial
            // if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,\\-]"))
            //     return "Le mot de passe doit contenir au moins un caractère spécial.";

            return null; // Mot de passe valide
        }
    }
}
