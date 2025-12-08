using FleetManager;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager
{
    public partial class CreateUserWindow : Window
    {
        string connStr = "server=localhost;Port=3309;database=fleet_managers;uid=root;pwd=;";

        public CreateUserWindow()
        {
            InitializeComponent();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier que tous les champs sont remplis
            if (string.IsNullOrWhiteSpace(txtNom.Text) ||
                string.IsNullOrWhiteSpace(txtPrenom.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPass.Password) ||
                cbRole.SelectedItem == null)
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();

                // Vérifier si l'email existe déjà
                string checkEmail = "SELECT COUNT(*) FROM utilisateurs WHERE email=@e";
                using MySqlCommand checkCmd = new MySqlCommand(checkEmail, conn);
                checkCmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (exists > 0)
                {
                    MessageBox.Show("Cet email est déjà utilisé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Ajouter l'utilisateur
                string query = @"INSERT INTO utilisateurs 
                            (nom, prenom, email, mot_de_passe, role) 
                            VALUES (@n, @p, @e, SHA2(@m,256), @r)";

                using MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@n", txtNom.Text.Trim());
                cmd.Parameters.AddWithValue("@p", txtPrenom.Text.Trim());
                cmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@m", txtPass.Password.Trim());
                cmd.Parameters.AddWithValue("@r", (cbRole.SelectedItem as ComboBoxItem).Content.ToString());

                cmd.ExecuteNonQuery();

                MessageBox.Show("Utilisateur créé avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Erreur de base de données : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}