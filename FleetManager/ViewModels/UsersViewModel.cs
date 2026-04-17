using FleetManager.Models;
using FleetManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FleetManager.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des utilisateurs (Admin uniquement)
    /// </summary>
    public class UsersViewModel : ObservableBase
    {
        private readonly DatabaseService _dbService;
        private User? _selectedUser;

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        // Propriétés pour l'édition
        private string _editNom = string.Empty;
        public string EditNom
        {
            get => _editNom;
            set => SetProperty(ref _editNom, value);
        }

        private string _editPrenom = string.Empty;
        public string EditPrenom
        {
            get => _editPrenom;
            set => SetProperty(ref _editPrenom, value);
        }

        private string _editEmail = string.Empty;
        public string EditEmail
        {
            get => _editEmail;
            set => SetProperty(ref _editEmail, value);
        }

        private string _editMotDePasse = string.Empty;
        public string EditMotDePasse
        {
            get => _editMotDePasse;
            set => SetProperty(ref _editMotDePasse, value);
        }

        private string _editRole = "Utilisateur";
        public string EditRole
        {
            get => _editRole;
            set => SetProperty(ref _editRole, value);
        }

        private bool _editEstBloque = false;
        public bool EditEstBloque
        {
            get => _editEstBloque;
            set => SetProperty(ref _editEstBloque, value);
        }

        private DateTime? _editBloqueJusqu;
        public DateTime? EditBloqueJusqu
        {
            get => _editBloqueJusqu;
            set => SetProperty(ref _editBloqueJusqu, value);
        }

        // Propriété sélectionnée
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                if (value != null)
                {
                    LoadUserForEdit(value);
                }
            }
        }

        // Commandes
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ToggleBlockUserCommand { get; }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public UsersViewModel()
        {
            try
            {
                _dbService = new DatabaseService();

                // Vérifier que l'utilisateur actuel est admin
                var currentUser = SessionService.Instance.CurrentUser;
                if (currentUser == null || !currentUser.EstAdministrateur)
                {
                    throw new UnauthorizedAccessException("Accès refusé : Réservé aux administrateurs");
                }

                // Initialiser les commandes
                AddUserCommand = new RelayCommand(_ => AddUser());
                EditUserCommand = new RelayCommand(_ => EditUser(), _ => SelectedUser != null);
                DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => SelectedUser != null && SelectedUser.Id != SessionService.Instance.CurrentUser?.Id);
                SaveUserCommand = new RelayCommand(_ => SaveUser());
                CancelEditCommand = new RelayCommand(_ => CancelEdit());
                RefreshCommand = new RelayCommand(_ => LoadUsers());
                BackCommand = new RelayCommand(_ => Back());
                ToggleBlockUserCommand = new RelayCommand(_ => ToggleBlockUser(), _ => SelectedUser != null && SelectedUser.Id != SessionService.Instance.CurrentUser?.Id);

                // Ne pas charger les utilisateurs dans le constructeur
                // LoadUsers sera appelé après le chargement de la fenêtre
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'initialisation du ViewModel : {ex.Message}", "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Charge la liste des utilisateurs
        /// </summary>
        public void LoadUsers()
        {
            Users.Clear();
            var users = _dbService.GetAllUsers();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        /// <summary>
        /// Charge les données d'un utilisateur pour l'édition
        /// </summary>
        private void LoadUserForEdit(User user)
        {
            EditNom = user.Nom;
            EditPrenom = user.Prenom;
            EditEmail = user.Email;
            EditRole = user.Role;
            EditEstBloque = user.EstBloque;
            EditBloqueJusqu = user.BloqueJusqu;
            IsEditMode = true;
        }

        /// <summary>
        /// Active le mode d'ajout
        /// </summary>
        private void AddUser()
        {
            ClearEditFields();
            IsEditMode = true;
        }

        /// <summary>
        /// Active le mode d'édition
        /// </summary>
        private void EditUser()
        {
            if (SelectedUser != null)
            {
                LoadUserForEdit(SelectedUser);
                IsEditMode = true;
            }
        }

        /// <summary>
        /// Sauvegarde l'utilisateur (création ou modification)
        /// </summary>
        private void SaveUser()
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(EditNom))
                {
                    System.Windows.MessageBox.Show("Le nom est obligatoire.", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(EditPrenom))
                {
                    System.Windows.MessageBox.Show("Le prénom est obligatoire.", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(EditEmail) || !IsValidEmail(EditEmail))
                {
                    System.Windows.MessageBox.Show("L'email est invalide.", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Pour la modification, le mot de passe n'est pas obligatoire
                // Pour la création, le mot de passe est obligatoire
                if (SelectedUser == null && string.IsNullOrWhiteSpace(EditMotDePasse))
                {
                    System.Windows.MessageBox.Show("Le mot de passe est obligatoire pour la création.", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Validation du mot de passe si fourni
                if (!string.IsNullOrWhiteSpace(EditMotDePasse))
                {
                    var passwordError = PasswordService.ValidatePasswordStrength(EditMotDePasse);
                    if (passwordError != null)
                    {
                        System.Windows.MessageBox.Show(passwordError, "Erreur", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                }

                User user;
                if (SelectedUser != null && SelectedUser.Id > 0)
                {
                    // Modification
                    user = new User
                    {
                        Id = SelectedUser.Id,
                        Nom = EditNom,
                        Prenom = EditPrenom,
                        Email = EditEmail,
                        Role = EditRole,
                        BloqueJusqu = EditEstBloque ? DateTime.Now.AddYears(10) : null,
                        DateCreation = SelectedUser.DateCreation
                    };

                    if (_dbService.UpdateUser(user))
                    {
                        // Si le mot de passe a été changé, le mettre à jour
                        if (!string.IsNullOrWhiteSpace(EditMotDePasse))
                        {
                            var hashedPassword = PasswordService.HashPassword(EditMotDePasse);
                            _dbService.UpdateUserPassword(user.Id, hashedPassword);
                        }

                        System.Windows.MessageBox.Show("Utilisateur modifié avec succès !", "Succès", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Erreur lors de la modification.", "Erreur", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    // Création
                    user = new User
                    {
                        Nom = EditNom,
                        Prenom = EditPrenom,
                        Email = EditEmail,
                        Role = EditRole,
                        BloqueJusqu = EditEstBloque ? DateTime.Now.AddYears(10) : null
                    };

                    var hashedPassword = PasswordService.HashPassword(EditMotDePasse);
                    if (_dbService.CreateUser(user, hashedPassword))
                    {
                        System.Windows.MessageBox.Show("Utilisateur créé avec succès !", "Succès", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Erreur lors de la création.", "Erreur", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }
                }

                LoadUsers();
                CancelEdit();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur : {ex.Message}", "Erreur", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Supprime l'utilisateur sélectionné
        /// </summary>
        private void DeleteUser()
        {
            if (SelectedUser == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer l'utilisateur {SelectedUser.Prenom} {SelectedUser.Nom} ({SelectedUser.Email}) ?",
                "Confirmation",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    if (_dbService.DeleteUser(SelectedUser.Id))
                    {
                        System.Windows.MessageBox.Show("Utilisateur supprimé avec succès !", "Succès", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        LoadUsers();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Erreur lors de la suppression.", "Erreur", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Erreur : {ex.Message}", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Bloque ou débloque un utilisateur
        /// </summary>
        private void ToggleBlockUser()
        {
            if (SelectedUser == null) return;

            try
            {
                // Inverser l'état de blocage
                if (SelectedUser.EstBloque)
                {
                    SelectedUser.BloqueJusqu = null;
                }
                else
                {
                    SelectedUser.BloqueJusqu = DateTime.Now.AddYears(10);
                }

                if (_dbService.UpdateUser(SelectedUser))
                {
                    string action = SelectedUser.EstBloque ? "bloqué" : "débloqué";
                    System.Windows.MessageBox.Show($"Utilisateur {action} avec succès !", "Succès", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    LoadUsers();
                }
                else
                {
                    System.Windows.MessageBox.Show("Erreur lors de la modification.", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur : {ex.Message}", "Erreur", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Annule l'édition
        /// </summary>
        private void CancelEdit()
        {
            ClearEditFields();
            IsEditMode = false;
            SelectedUser = null;
        }

        /// <summary>
        /// Efface les champs d'édition
        /// </summary>
        private void ClearEditFields()
        {
            EditNom = string.Empty;
            EditPrenom = string.Empty;
            EditEmail = string.Empty;
            EditMotDePasse = string.Empty;
            EditRole = "Utilisateur";
            EditEstBloque = false;
        }

        /// <summary>
        /// Valide le format d'un email
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
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
