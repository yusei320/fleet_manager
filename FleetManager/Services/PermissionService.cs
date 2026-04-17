using FleetManager.Models;

namespace FleetManager.Services
{
    /// <summary>
    /// Service pour la vérification des permissions
    /// Assure que les utilisateurs ne peuvent accéder qu'aux fonctionnalités autorisées
    /// </summary>
    public static class PermissionService
    {
        /// <summary>
        /// Vérifie si l'utilisateur actuel est un administrateur
        /// </summary>
        public static bool IsAdmin()
        {
            var currentUser = SessionService.Instance.CurrentUser;
            return currentUser != null && currentUser.EstAdministrateur;
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut gérer les utilisateurs (Admin uniquement)
        /// </summary>
        public static bool CanManageUsers()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut gérer tous les véhicules (Admin uniquement)
        /// </summary>
        public static bool CanManageAllVehicles()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut gérer tous les suivis (Admin uniquement)
        /// </summary>
        public static bool CanManageAllSuivis()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut accéder aux rapports globaux (Admin uniquement)
        /// </summary>
        public static bool CanAccessReports()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut supprimer des données globales (Admin uniquement)
        /// </summary>
        public static bool CanDeleteGlobalData()
        {
            return IsAdmin();
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut gérer ses propres véhicules
        /// </summary>
        public static bool CanManageOwnVehicles()
        {
            return SessionService.Instance.IsLoggedIn;
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut gérer ses propres suivis
        /// </summary>
        public static bool CanManageOwnSuivis()
        {
            return SessionService.Instance.IsLoggedIn;
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut accéder à un véhicule spécifique
        /// </summary>
        public static bool CanAccessVehicle(int vehicleUserId)
        {
            var currentUser = SessionService.Instance.CurrentUser;
            if (currentUser == null) return false;

            // Admin peut accéder à tous les véhicules
            if (currentUser.EstAdministrateur) return true;

            // Utilisateur ne peut accéder qu'à ses propres véhicules
            return currentUser.Id == vehicleUserId;
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut modifier un suivi spécifique
        /// </summary>
        public static bool CanModifySuivi(int suiviUserId)
        {
            var currentUser = SessionService.Instance.CurrentUser;
            if (currentUser == null) return false;

            // Admin peut modifier tous les suivis
            if (currentUser.EstAdministrateur) return true;

            // Utilisateur ne peut modifier que ses propres suivis
            return currentUser.Id == suiviUserId;
        }

        /// <summary>
        /// Vérifie si l'utilisateur est connecté
        /// </summary>
        public static bool IsLoggedIn()
        {
            return SessionService.Instance.IsLoggedIn;
        }

        /// <summary>
        /// Vérifie si le compte de l'utilisateur est bloqué
        /// </summary>
        public static bool IsAccountBlocked()
        {
            var currentUser = SessionService.Instance.CurrentUser;
            return currentUser != null && currentUser.EstBloque;
        }

        /// <summary>
        /// Lance une exception si l'utilisateur n'est pas admin
        /// </summary>
        public static void RequireAdmin()
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("Accès refusé : Cette fonctionnalité est réservée aux administrateurs");
            }
        }

        /// <summary>
        /// Lance une exception si l'utilisateur n'est pas connecté
        /// </summary>
        public static void RequireLogin()
        {
            if (!IsLoggedIn())
            {
                throw new UnauthorizedAccessException("Accès refusé : Vous devez être connecté pour accéder à cette fonctionnalité");
            }
        }

        /// <summary>
        /// Lance une exception si le compte est bloqué
        /// </summary>
        public static void RequireNotBlocked()
        {
            if (IsAccountBlocked())
            {
                throw new UnauthorizedAccessException("Accès refusé : Votre compte est bloqué");
            }
        }

        /// <summary>
        /// Vérifie toutes les conditions nécessaires pour une action admin
        /// </summary>
        public static void RequireAdminAccess()
        {
            RequireLogin();
            RequireNotBlocked();
            RequireAdmin();
        }

        /// <summary>
        /// Vérifie toutes les conditions nécessaires pour une action utilisateur
        /// </summary>
        public static void RequireUserAccess()
        {
            RequireLogin();
            RequireNotBlocked();
        }
    }
}
