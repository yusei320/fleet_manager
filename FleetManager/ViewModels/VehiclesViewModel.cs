using FleetManager.Models;
using FleetManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FleetManager.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des véhicules avec CRUD complet
    /// </summary>
    public class VehiclesViewModel : ObservableBase
    {
        private readonly DatabaseService _dbService;
        private User? _currentUser;
        private Vehicle? _selectedVehicle;
        private bool _isAdmin;

        public ObservableCollection<Vehicle> Vehicles { get; } = new ObservableCollection<Vehicle>();

        // Propriétés pour l'édition
        private string _editImmatriculation = string.Empty;
        public string EditImmatriculation
        {
            get => _editImmatriculation;
            set => SetProperty(ref _editImmatriculation, value);
        }

        private string _editMarque = string.Empty;
        public string EditMarque
        {
            get => _editMarque;
            set => SetProperty(ref _editMarque, value);
        }

        private string _editModele = string.Empty;
        public string EditModele
        {
            get => _editModele;
            set => SetProperty(ref _editModele, value);
        }

        private string? _editAnnee;
        public string? EditAnnee
        {
            get => _editAnnee;
            set => SetProperty(ref _editAnnee, value);
        }

        private string _editCarburant = string.Empty;
        public string EditCarburant
        {
            get => _editCarburant;
            set => SetProperty(ref _editCarburant, value);
        }

        private int _editKilometrage;
        public int EditKilometrage
        {
            get => _editKilometrage;
            set => SetProperty(ref _editKilometrage, value);
        }

        // Propriété sélectionnée
        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                SetProperty(ref _selectedVehicle, value);
                if (value != null)
                {
                    LoadVehicleForEdit(value);
                }
            }
        }

        // Commandes
        public ICommand AddVehicleCommand { get; }
        public ICommand EditVehicleCommand { get; }
        public ICommand DeleteVehicleCommand { get; }
        public ICommand SaveVehicleCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public VehiclesViewModel(bool isAdmin = false)
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
                AddVehicleCommand = new RelayCommand(_ => AddVehicle());
                EditVehicleCommand = new RelayCommand(_ => EditVehicle(), _ => SelectedVehicle != null);
                DeleteVehicleCommand = new RelayCommand(_ => DeleteVehicle(), _ => SelectedVehicle != null);
                SaveVehicleCommand = new RelayCommand(_ => SaveVehicle());
                CancelEditCommand = new RelayCommand(_ => CancelEdit());
                RefreshCommand = new RelayCommand(_ => LoadVehicles());
                BackCommand = new RelayCommand(_ => Back());

                // Ne pas charger les véhicules dans le constructeur
                // LoadVehicles sera appelé après le chargement de la fenêtre
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'initialisation du ViewModel : {ex.Message}", "Erreur", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Charge la liste des véhicules
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
        /// Charge les données d'un véhicule pour l'édition
        /// </summary>
        private void LoadVehicleForEdit(Vehicle vehicle)
        {
            EditImmatriculation = vehicle.Immatriculation;
            EditMarque = vehicle.Marque;
            EditModele = vehicle.Modele;
            EditAnnee = vehicle.Annee?.ToString();
            EditCarburant = vehicle.Carburant;
            EditKilometrage = vehicle.Kilometrage;
            IsEditMode = true;
        }

        /// <summary>
        /// Active le mode d'ajout
        /// </summary>
        private void AddVehicle()
        {
            ClearEditFields();
            IsEditMode = true;
        }

        /// <summary>
        /// Active le mode d'édition
        /// </summary>
        private void EditVehicle()
        {
            if (SelectedVehicle != null)
            {
                LoadVehicleForEdit(SelectedVehicle);
                IsEditMode = true;
            }
        }

        /// <summary>
        /// Sauvegarde le véhicule (création ou modification)
        /// </summary>
        private void SaveVehicle()
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(EditImmatriculation))
                {
                    System.Windows.MessageBox.Show("L'immatriculation est obligatoire.", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(EditMarque))
                {
                    System.Windows.MessageBox.Show("La marque est obligatoire.", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(EditModele))
                {
                    System.Windows.MessageBox.Show("Le modèle est obligatoire.", "Erreur", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Parser l'année
                int? annee = null;
                if (!string.IsNullOrWhiteSpace(EditAnnee) && int.TryParse(EditAnnee, out int parsedAnnee))
                {
                    annee = parsedAnnee;
                }

                if (_currentUser == null) return;

                Vehicle vehicle;
                if (SelectedVehicle != null && SelectedVehicle.Id > 0)
                {
                    // Modification
                    vehicle = new Vehicle
                    {
                        Id = SelectedVehicle.Id,
                        Immatriculation = EditImmatriculation,
                        Marque = EditMarque,
                        Modele = EditModele,
                        Annee = annee,
                        Carburant = EditCarburant,
                        Kilometrage = EditKilometrage,
                        IdUtilisateur = SelectedVehicle.IdUtilisateur,
                        DateCreation = SelectedVehicle.DateCreation
                    };

                    if (_dbService.UpdateVehicle(vehicle))
                    {
                        System.Windows.MessageBox.Show("Véhicule modifié avec succès !", "Succès", 
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
                    vehicle = new Vehicle
                    {
                        Immatriculation = EditImmatriculation,
                        Marque = EditMarque,
                        Modele = EditModele,
                        Annee = annee,
                        Carburant = EditCarburant,
                        Kilometrage = EditKilometrage,
                        IdUtilisateur = _currentUser.Id
                    };

                    if (_dbService.CreateVehicle(vehicle))
                    {
                        System.Windows.MessageBox.Show("Véhicule créé avec succès !", "Succès", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Erreur lors de la création.", "Erreur", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }
                }

                LoadVehicles();
                CancelEdit();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur : {ex.Message}", "Erreur", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Supprime le véhicule sélectionné
        /// </summary>
        private void DeleteVehicle()
        {
            if (SelectedVehicle == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer le véhicule {SelectedVehicle.DescriptionComplet} ?",
                "Confirmation",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    if (_dbService.DeleteVehicle(SelectedVehicle.Id))
                    {
                        System.Windows.MessageBox.Show("Véhicule supprimé avec succès !", "Succès", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        LoadVehicles();
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
            SelectedVehicle = null;
        }

        /// <summary>
        /// Efface les champs d'édition
        /// </summary>
        private void ClearEditFields()
        {
            EditImmatriculation = string.Empty;
            EditMarque = string.Empty;
            EditModele = string.Empty;
            EditAnnee = null;
            EditCarburant = string.Empty;
            EditKilometrage = 0;
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
