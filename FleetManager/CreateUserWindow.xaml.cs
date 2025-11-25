using MySql.Data.MySqlClient;
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
            if (string.IsNullOrEmpty(txtNom.Text)
                || string.IsNullOrEmpty(txtPrenom.Text)
                || string.IsNullOrEmpty(txtEmail.Text)
                || string.IsNullOrEmpty(txtPass.Password))
            {
                MessageBox.Show("Veuillez remplir tous les champs.");
                return;
            }

            using MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();

            // 🟦 Vérifier si l'email existe déjà
            string checkEmail = "SELECT COUNT(*) FROM utilisateurs WHERE email=@e";
            MySqlCommand checkCmd = new MySqlCommand(checkEmail, conn);
            checkCmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());

            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (exists > 0)
            {
                MessageBox.Show("Cet email est déjà utilisé.");
                return;
            }

            // 🟦 Ajouter l'utilisateur
            string query = @"INSERT INTO utilisateurs 
                            (nom, prenom, email, mot_de_passe, role) 
                            VALUES (@n, @p, @e, SHA2(@m,256), @r)";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@n", txtNom.Text.Trim());
            cmd.Parameters.AddWithValue("@p", txtPrenom.Text.Trim());
            cmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
            cmd.Parameters.AddWithValue("@m", txtPass.Password.Trim());
            cmd.Parameters.AddWithValue("@r", (cbRole.SelectedItem as ComboBoxItem).Content.ToString());

            cmd.ExecuteNonQuery();

            MessageBox.Show("Utilisateur créé avec succès !");
            this.Close();
        }
    }
}
