using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace FleetManager
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnConnexion_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string motdepasse = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(motdepasse))
            {
                lblMessage.Text = "Veuillez remplir tous les champs.";
                return;
            }

            string connectionString = "server=localhost;Port=3309;database=fleet_managers;uid=root;pwd=;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT id, prenom, role, bloque_jusqu FROM utilisateurs WHERE email=@Email AND mot_de_passe=SHA2(@Mdp, 256)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Mdp", motdepasse);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Vérifie si l'utilisateur est bloqué
                            object bloqueObj = reader["bloque_jusqu"];
                            if (bloqueObj != DBNull.Value && DateTime.TryParse(bloqueObj.ToString(), out DateTime bloqueUntil))
                            {
                                if (DateTime.Now < bloqueUntil)
                                {
                                    lblMessage.Text = $"Compte bloqué jusqu'au {bloqueUntil}";
                                    return;
                                }
                            }

                            int userId = Convert.ToInt32(reader["id"]);
                            string role = reader["role"]?.ToString() ?? string.Empty;
                            var prenom = reader["prenom"] != null ? reader["prenom"].ToString() : string.Empty;

                            MessageBox.Show($"Bienvenue {prenom} ({role})", "Connexion réussie", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Ouvre la bonne fenêtre selon le rôle, en passant l'id utilisateur
                            if (role.Trim().Equals("Administrateur", StringComparison.OrdinalIgnoreCase))
                            {
                                AdminWindow admin = new AdminWindow(userId);
                                admin.Show();
                            }
                            else
                            {
                                MainWindow main = new MainWindow(userId);
                                main.Show();
                            }

                            this.Close();
                        }
                        else
                        {
                            lblMessage.Text = "Email ou mot de passe incorrect.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur de connexion : " + ex.Message);
                }
            }
        }

        // Efface le message d'erreur quand l'utilisateur modifie email ou mot de passe
        private void txtInput_TextChanged(object sender, RoutedEventArgs e)
        {
            lblMessage.Text = "";
        }

        private void BtnInscription_Click(object sender, RoutedEventArgs e)
        {
            // Ouvre la fenêtre de création d'utilisateur (inscription)
            CreateUserWindow createUser = new CreateUserWindow();
            createUser.ShowDialog();
        }
    }
}
