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

        // Validation pour n'accepter que des chiffres dans le champ Année
        private void TxtAnnee_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Vérifie si l'entrée est un chiffre
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string immat = txtImmatriculation.Text.Trim();
            string marque = txtMarque.Text.Trim();
            string modele = txtModele.Text.Trim();
            string annee = txtAnnee.Text.Trim();

            // Extraire uniquement le texte du carburant sans l'emoji
            string carburantComplet = (cmbCarburant.SelectedItem as ComboBoxItem)?.Content.ToString();
            string carburant = null;

            if (!string.IsNullOrEmpty(carburantComplet))
            {
                // Retirer l'emoji (tout ce qui est avant le premier espace)
                int espaceIndex = carburantComplet.IndexOf(' ');
                if (espaceIndex > 0)
                {
                    carburant = carburantComplet.Substring(espaceIndex + 1).Trim();
                }
                else
                {
                    carburant = carburantComplet;
                }
            }

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
                    cmd.Parameters.AddWithValue("@carburant", carburant ?? (object)DBNull.Value);
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