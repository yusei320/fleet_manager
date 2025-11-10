using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager
{
    public partial class AddSuiviWindow : Window
    {
        private string connectionString;

        public AddSuiviWindow(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            LoadVehicules();
        }

        private void LoadVehicules()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT id, immatriculation FROM vehicules";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbVehicule.ItemsSource = dt.DefaultView;
                    cmbVehicule.DisplayMemberPath = "immatriculation";
                    cmbVehicule.SelectedValuePath = "id";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors du chargement des véhicules : " + ex.Message);
                }
            }
        }

        private void BtnAddSuivi_Click(object sender, RoutedEventArgs e)
        {
            if (cmbVehicule.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un véhicule.");
                return;
            }

            int idVehicule = Convert.ToInt32(cmbVehicule.SelectedValue);
            DateTime dateSuivi = dpDateSuivi.SelectedDate ?? DateTime.Now;

            decimal carburant = 0;
            decimal.TryParse(txtCarburant.Text.Trim(), out carburant);

            decimal cout = 0;
            decimal.TryParse(txtCout.Text.Trim(), out cout);

            int distance = 0;
            int.TryParse(txtDistance.Text.Trim(), out distance);

            string commentaire = txtCommentaire.Text.Trim();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"INSERT INTO suivi 
                                     (id_vehicule, date_suivi, carburant_litre, cout, distance_km, commentaire)
                                     VALUES (@id_vehicule, @date_suivi, @carburant, @cout, @distance, @commentaire)";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id_vehicule", idVehicule);
                    cmd.Parameters.AddWithValue("@date_suivi", dateSuivi);
                    cmd.Parameters.AddWithValue("@carburant", carburant);
                    cmd.Parameters.AddWithValue("@cout", cout);
                    cmd.Parameters.AddWithValue("@distance", distance);
                    cmd.Parameters.AddWithValue("@commentaire", commentaire);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Suivi ajouté avec succès !");
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
