using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager
{
    public partial class CreateUserWindow : Window
    {
        string connStr = "server=localhost;database=fleet_managers;uid=admin_fleet;pwd=AdminFleet2024!;";

        public CreateUserWindow()
        {
            InitializeComponent();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNom.Text) || string.IsNullOrEmpty(txtPrenom.Text) ||
                string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPass.Password))
            {
                MessageBox.Show("Veuillez remplir tous les champs");
                return;
            }

            using MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) " +
                           "VALUES (@n,@p,@e,SHA2(@m,256),@r)";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@n", txtNom.Text);
            cmd.Parameters.AddWithValue("@p", txtPrenom.Text);
            cmd.Parameters.AddWithValue("@e", txtEmail.Text);
            cmd.Parameters.AddWithValue("@m", txtPass.Password);
            cmd.Parameters.AddWithValue("@r", (cbRole.SelectedItem as ComboBoxItem).Content.ToString());

            cmd.ExecuteNonQuery();

            MessageBox.Show("Utilisateur créé !");
            this.Close();
        }
    }
}
