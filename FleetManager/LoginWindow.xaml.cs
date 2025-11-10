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
                    string query = "SELECT * FROM utilisateurs WHERE email=@Email AND mot_de_passe=SHA2(@Mdp, 256)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Mdp", motdepasse);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string role = reader["role"].ToString();
                            string prenom = reader["prenom"].ToString();

                            MessageBox.Show($"Bienvenue {prenom} ({role})", "Connexion réussie", MessageBoxButton.OK, MessageBoxImage.Information);

                            MainWindow main = new MainWindow();
                            main.Show();
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
    }
}
