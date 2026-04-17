using FleetManager.Models;
using FleetManager.Services;
using System;
using System.Windows;

namespace FleetManager
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseService _dbService;

        public LoginWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        private void BtnConnexion_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string motdepasse = txtPassword.Password.Trim();

            // Validation des champs
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(motdepasse))
            {
                ShowError("Veuillez remplir tous les champs.");
                return;
            }

            // Validation du format email
            if (!IsValidEmail(email))
            {
                ShowError("Format d'email invalide.");
                return;
            }

            try
            {
                // Récupérer l'utilisateur par email
                var user = _dbService.GetUserByEmail(email);
                
                if (user == null)
                {
                    ShowError("Email ou mot de passe incorrect.");
                    return;
                }

                // Vérifier si le compte est bloqué
                if (user.EstBloque)
                {
                    ShowError($"Compte bloqué jusqu'au {user.BloqueJusqu:dd/MM/yyyy HH:mm}");
                    return;
                }

                // Récupérer le mot de passe haché depuis la base de données
                string? hashedPassword = GetHashedPasswordFromDatabase(email);
                
                if (string.IsNullOrEmpty(hashedPassword))
                {
                    // Cas de compatibilité : mot de passe en clair (ancienne méthode)
                    // Vérifier directement avec le mot de passe en clair
                    if (motdepasse == hashedPassword)
                    {
                        LoginUser(user);
                    }
                    else
                    {
                        ShowError("Email ou mot de passe incorrect.");
                    }
                    return;
                }

                // Vérifier le mot de passe avec BCrypt
                if (!PasswordService.VerifyPassword(motdepasse, hashedPassword))
                {
                    ShowError("Email ou mot de passe incorrect.");
                    return;
                }

                // Connexion réussie
                LoginUser(user);
            }
            catch (Exception ex)
            {
                ShowError("Erreur de connexion : " + ex.Message);
            }
        }

        /// <summary>
        /// Connecte l'utilisateur et ouvre le dashboard approprié
        /// </summary>
        private void LoginUser(User user)
        {
            // Enregistrer la session
            SessionService.Instance.Login(user);

            MessageBox.Show($"Bienvenue {user.Prenom} ({user.Role})", "Connexion réussie", 
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Ouvre la bonne fenêtre selon le rôle
            if (user.EstAdministrateur)
            {
                AdminDashboard admin = new AdminDashboard(user.Id);
                admin.Show();
            }
            else
            {
                UserDashboard userDashboard = new UserDashboard(user.Id);
                userDashboard.Show();
            }

            this.Close();
        }

        /// <summary>
        /// Récupère le mot de passe haché depuis la base de données
        /// </summary>
        private string? GetHashedPasswordFromDatabase(string email)
        {
            using var conn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=fleet_manager.db");
            conn.Open();
            
            string query = "SELECT mot_de_passe FROM utilisateurs WHERE email=@email";
            using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            
            var result = cmd.ExecuteScalar();
            return result?.ToString();
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
        /// Affiche un message d'erreur
        /// </summary>
        private void ShowError(string message)
        {
            lblMessage.Text = message;
            ErrorPanel.Visibility = System.Windows.Visibility.Visible;
        }

        // Efface le message d'erreur quand l'utilisateur modifie email ou mot de passe
        private void txtInput_TextChanged(object sender, RoutedEventArgs e)
        {
            lblMessage.Text = "";
            ErrorPanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void BtnInscription_Click(object sender, RoutedEventArgs e)
        {
            // Ouvre la fenêtre de création d'utilisateur (inscription)
            CreateUserWindow createUser = new CreateUserWindow();
            createUser.ShowDialog();
        }
    }
}
