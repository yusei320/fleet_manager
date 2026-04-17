using FleetManager.Models;

namespace FleetManager.Services
{
    /// <summary>
    /// Service singleton pour gérer la session utilisateur connecté
    /// Permet d'accéder à l'utilisateur actuel depuis n'importe où dans l'application
    /// </summary>
    public class SessionService
    {
        private static SessionService? _instance;
        private static readonly object _lock = new object();

        private User? _currentUser;

        /// <summary>
        /// Instance singleton du service de session
        /// </summary>
        public static SessionService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new SessionService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Utilisateur actuellement connecté
        /// </summary>
        public User? CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value;
        }

        /// <summary>
        /// Indique si un utilisateur est connecté
        /// </summary>
        public bool IsLoggedIn => CurrentUser != null;

        /// <summary>
        /// Indique si l'utilisateur connecté est un administrateur
        /// </summary>
        public bool IsAdmin => CurrentUser?.EstAdministrateur ?? false;

        /// <summary>
        /// ID de l'utilisateur connecté
        /// </summary>
        public int? CurrentUserId => CurrentUser?.Id;

        /// <summary>
        /// Connecte un utilisateur
        /// </summary>
        public void Login(User user)
        {
            CurrentUser = user;
        }

        /// <summary>
        /// Déconnecte l'utilisateur actuel
        /// </summary>
        public void Logout()
        {
            CurrentUser = null;
        }

        /// <summary>
        /// Met à jour les informations de l'utilisateur connecté
        /// </summary>
        public void UpdateCurrentUser(User user)
        {
            if (CurrentUser != null && CurrentUser.Id == user.Id)
            {
                CurrentUser = user;
            }
        }

        /// <summary>
        /// Vérifie si l'utilisateur a accès à une ressource spécifique
        /// </summary>
        public bool HasAccessToResource(int resourceUserId)
        {
            // Les admins ont accès à tout
            if (IsAdmin)
                return true;

            // Les utilisateurs normaux n'accèdent qu'à leurs propres ressources
            return CurrentUserId == resourceUserId;
        }

        private SessionService()
        {
            // Constructeur privé pour le pattern singleton
        }
    }
}
