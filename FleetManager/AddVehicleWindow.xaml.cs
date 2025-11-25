using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager
{
    public partial class AddVehicleWindow : Window
    {
        private string connectionString;
        private int currentUserId;

        public AddVehicleWindow(string connStr, int userId)
        {
            InitializeComponent();
            connectionString = connStr;
            currentUserId = userId;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string immat = txtImmatriculation.Text.Trim();
            string marque = txtMarque.Text.Trim();
            string modele = txtModele.Text.Trim();
            string annee = txtAnnee.Text.Trim();
            string carburant = (cmbCarburant.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(immat) || string.IsNullOrEmpty(marque) || string.IsNullOrEmpty(modele))
            {
                MessageBox.Show("Veuillez remplir les champs obligatoires.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"INSERT INTO vehicules 
                                    (immatriculation, marque, modele, annee, carburant, id_utilisateur) 
                                     VALUES (@immat,@marque,@modele,@annee,@carburant,@idUser)";

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@immat", immat);
                    cmd.Parameters.AddWithValue("@marque", marque);
                    cmd.Parameters.AddWithValue("@modele", modele);
                    cmd.Parameters.AddWithValue("@annee", annee != "" ? Convert.ToInt32(annee) : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@carburant", carburant);
                    cmd.Parameters.AddWithValue("@idUser", currentUserId);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Véhicule ajouté avec succès !");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message);
                }
            }
        }
    }
}
