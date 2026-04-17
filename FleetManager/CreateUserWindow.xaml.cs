using FleetManager.Models;
using FleetManager.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager
{
    public partial class CreateUserWindow : Window
    {
        private readonly DatabaseService _dbService;

        public CreateUserWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer les valeurs des champs
            string nom = txtNom.Text.Trim();
            string prenom = txtPrenom.Text.Trim();
            string email = txtEmail.Text.Trim();
            string motDePasse = txtPass.Password.Trim();

            // Validation des champs obligatoires
            if (string.IsNullOrWhiteSpace(nom) ||
                string.IsNullOrWhiteSpace(prenom) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(motDePasse) ||
                cbRole.SelectedItem == null)
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validation du format email
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Format d'email invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validation de la force du mot de passe
            string? passwordError = PasswordService.ValidatePasswordStrength(motDePasse);
            if (passwordError != null)
            {
                MessageBox.Show($"Mot de passe trop faible : {passwordError}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Vérifier si l'email existe déjà
                if (_dbService.EmailExists(email))
                {
                    MessageBox.Show("Cet email est déjà utilisé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Récupérer le rôle
                if (cbRole.SelectedItem is not ComboBoxItem roleItem || roleItem.Content == null)
                {
                    MessageBox.Show("Veuillez choisir un rôle.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                string role = roleItem.Content.ToString() ?? "Utilisateur";

                // Créer l'utilisateur
                var user = new User
                {
                    Nom = nom,
                    Prenom = prenom,
                    Email = email,
                    Role = role
                };

                // Hacher le mot de passe
                string hashedPassword = PasswordService.HashPassword(motDePasse);

                // Insérer dans la base de données
                if (_dbService.CreateUser(user, hashedPassword))
                {
                    MessageBox.Show("Utilisateur créé avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Erreur lors de la création de l'utilisateur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    }
}