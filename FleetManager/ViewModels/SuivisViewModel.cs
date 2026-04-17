using FleetManager.Models;
using FleetManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace FleetManager.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des suivis (ravitaillements/entretiens)
    /// </summary>
    public class SuivisViewModel : ObservableBase
    {
        private readonly DatabaseService _dbService;
        private User? _currentUser;
        private Suivi? _selectedSuivi;
        private bool _isAdmin;

        public ObservableCollection<Suivi> Suivis { get; } = new ObservableCollection<Suivi>();
        public ObservableCollection<Vehicle> Vehicles { get; } = new ObservableCollection<Vehicle>();

        // Propriétés pour l'édition
        private Vehicle? _selectedVehicle;
        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set => SetProperty(ref _selectedVehicle, value);
        }

        private DateTime _editDate = DateTime.Now;
        public DateTime EditDate
        {
            get => _editDate;
            set => SetProperty(ref _editDate, value);
        }

        private double _editCarburantLitre;
        public double EditCarburantLitre
        {
            get => _editCarburantLitre;
            set => SetProperty(ref _editCarburantLitre, value);
        }

        private double _editCout;
        public double EditCout
        {
            get => _editCout;
            set => SetProperty(ref _editCout, value);
        }

        private double _editDistanceKm;
        public double EditDistanceKm
        {
            get => _editDistanceKm;
            set => SetProperty(ref _editDistanceKm, value);
        }

        private string? _editCommentaire;
        public string? EditCommentaire
        {
            get => _editCommentaire;
            set => SetProperty(ref _editCommentaire, value);
        }

        // Propriété sélectionnée
        public Suivi? SelectedSuivi
        {
            get => _selectedSuivi;
            set
            {
                SetProperty(ref _selectedSuivi, value);
                if (value != null)
                {
                    LoadSuiviForEdit(value);
                }
            }
        }

        // Commandes
        public ICommand AddSuiviCommand { get; }
        public ICommand EditSuiviCommand { get; }
        public ICommand DeleteSuiviCommand { get; }
        public ICommand SaveSuiviCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public SuivisViewModel(bool isAdmin = false)
        {
            try
            {
                _dbService = new DatabaseService();
                _currentUser = SessionService.Instance.CurrentUser;
                _isAdmin = isAdmin;

                if (_currentUser == null)
                {
                    // Si pas d'utilisateur connecté, utiliser un utilisateur par défaut pour éviter le crash
                    _currentUser = new Models.User { Id = 1, Email = "admin@fleetmanager.com" };
                }

                // Initialiser les commandes
                AddSuiviCommand = new RelayCommand(_ => AddSuivi());
                EditSuiviCommand = new RelayCommand(_ => EditSuivi(), _ => SelectedSuivi != null);
                DeleteSuiviCommand = new RelayCommand(_ => DeleteSuivi(), _ => SelectedSuivi != null);
                SaveSuiviCommand = new RelayCommand(_ => SaveSuivi());
                CancelEditCommand = new RelayCommand(_ => CancelEdit());
                RefreshCommand = new RelayCommand(_ => LoadSuivis());
                BackCommand = new RelayCommand(_ => Back());

                // Ne pas charger les données dans le constructeur
                // LoadVehicles et LoadSuivis seront appelés après le chargement de la fenêtre
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'initialisation du ViewModel : {ex.Message}", "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Charge la liste des véhicules disponibles
        /// </summary>
        public void LoadVehicles()
        {
            Vehicles.Clear();

            if (_isAdmin)
            {
                // Admin : voir tous les véhicules
                var allUsers = _dbService.GetAllUsers();
                foreach (var user in allUsers)
                {
                    var userVehicles = _dbService.GetVehiclesByUser(user.Id);
                    foreach (var vehicle in userVehicles)
                    {
                        Vehicles.Add(vehicle);
                    }
                }
            }
            else
            {
                // Utilisateur : voir seulement ses véhicules
                var userVehicles = _dbService.GetVehiclesByUser(_currentUser!.Id);
                foreach (var vehicle in userVehicles)
                {
                    Vehicles.Add(vehicle);
                }
            }
        }

        /// <summary>
        /// Charge la liste des suivis
        /// </summary>
        public void LoadSuivis()
        {
            Suivis.Clear();

            if (_isAdmin)
            {
                // Admin : voir tous les suivis
                var allSuivis = _dbService.GetAllSuivis(limit: 100);
                foreach (var suivi in allSuivis)
                {
                    Suivis.Add(suivi);
                }
            }
            else
            {
                // Utilisateur : voir seulement ses suivis
                var userSuivis = _dbService.GetSuivisByUser(_currentUser!.Id, limit: 100);
                foreach (var suivi in userSuivis)
                {
                    Suivis.Add(suivi);
                }
            }
        }

        /// <summary>
        /// Charge les données d'un suivi pour l'édition
        /// </summary>
        private void LoadSuiviForEdit(Suivi suivi)
        {
            var vehicle = Vehicles.FirstOrDefault(v => v.Id == suivi.IdVehicule);
            SelectedVehicle = vehicle;
            EditDate = suivi.DateSuivi;
            EditCarburantLitre = suivi.CarburantLitre ?? 0;
            EditCout = suivi.Cout ?? 0;
            EditDistanceKm = suivi.DistanceKm ?? 0;
            EditCommentaire = suivi.Commentaire;
            IsEditMode = true;
        }

        /// <summary>
        /// Active le mode d'ajout
        /// </summary>
        private void AddSuivi()
        {
            ClearEditFields();
            IsEditMode = true;
        }

        /// <summary>
        /// Active le mode d'édition
        /// </summary>
        private void EditSuivi()
        {
            if (SelectedSuivi != null)
            {
                LoadSuiviForEdit(SelectedSuivi);
                IsEditMode = true;
            }
        }

        /// <summary>
        /// Sauvegarde le suivi (création ou modification)
        /// </summary>
        private void SaveSuivi()
        {
            try
            {
                // Validation
                if (SelectedVehicle == null)
                {
                    System.Windows.MessageBox.Show("Veuillez sélectionner un véhicule.", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (EditCarburantLitre <= 0 && EditCout <= 0 && EditDistanceKm <= 0)
                {
                    System.Windows.MessageBox.Show("Veuillez entrer au moins une valeur (carburant, coût ou distance).", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                Suivi suivi;
                if (SelectedSuivi != null && SelectedSuivi.Id > 0)
                {
                    // Modification
                    suivi = new Suivi
                    {
                        Id = SelectedSuivi.Id,
                        IdVehicule = SelectedVehicle.Id,
                        DateSuivi = EditDate,
                        CarburantLitre = EditCarburantLitre > 0 ? EditCarburantLitre : null,
                        Cout = EditCout > 0 ? EditCout : null,
                        DistanceKm = EditDistanceKm > 0 ? EditDistanceKm : null,
                        Commentaire = EditCommentaire,
                        DateCreation = SelectedSuivi.DateCreation
                    };

                    if (_dbService.UpdateSuivi(suivi))
                    {
                        System.Windows.MessageBox.Show("Suivi modifié avec succès !", "Succès", 
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
                    suivi = new Suivi
                    {
                        IdVehicule = SelectedVehicle.Id,
                        DateSuivi = EditDate,
                        CarburantLitre = EditCarburantLitre > 0 ? EditCarburantLitre : null,
                        Cout = EditCout > 0 ? EditCout : null,
                        DistanceKm = EditDistanceKm > 0 ? EditDistanceKm : null,
                        Commentaire = EditCommentaire
                    };

                    if (_dbService.CreateSuivi(suivi))
                    {
                        System.Windows.MessageBox.Show("Suivi créé avec succès !", "Succès", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Erreur lors de la création.", "Erreur", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }
                }

                LoadSuivis();
                CancelEdit();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur : {ex.Message}", "Erreur", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Supprime le suivi sélectionné
        /// </summary>
        private void DeleteSuivi()
        {
            if (SelectedSuivi == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer ce suivi du {SelectedSuivi.DateSuivi:dd/MM/yyyy} ?",
                "Confirmation",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    if (_dbService.DeleteSuivi(SelectedSuivi.Id))
                    {
                        System.Windows.MessageBox.Show("Suivi supprimé avec succès !", "Succès", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        LoadSuivis();
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
        /// Annule l'édition
        /// </summary>
        private void CancelEdit()
        {
            ClearEditFields();
            IsEditMode = false;
            SelectedSuivi = null;
        }

        /// <summary>
        /// Efface les champs d'édition
        /// </summary>
        private void ClearEditFields()
        {
            SelectedVehicle = null;
            EditDate = DateTime.Now;
            EditCarburantLitre = 0;
            EditCout = 0;
            EditDistanceKm = 0;
            EditCommentaire = null;
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
