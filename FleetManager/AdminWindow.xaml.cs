using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FleetManager
{
    public partial class AdminWindow : Window
    {
        string connectionString = "server=localhost;Port=3309;database=fleet_managers;uid=root;pwd=;";
        private int adminId; // ID de l'admin connecté

        public AdminWindow(int userId)
        {
            InitializeComponent();

            // Vérifie que l'utilisateur connecté est bien admin
            if (!AuthService.IsAdmin(userId))
            {
                MessageBox.Show("Accès refusé : vous n'avez pas les droits d'administration.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }

            adminId = userId;
            LoadUsers();
        }


        private void LoadUsers()
        {
            UserList.Items.Clear();

            using MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT id, nom, prenom, email, role, bloque_jusqu FROM utilisateurs";
            MySqlCommand cmd = new MySqlCommand(query, conn);

            using MySqlDataReader rd = cmd.ExecuteReader()
            {
                while (rd.Read())
                {
                    string bloque = rd["bloque_jusqu"] != DBNull.Value
                                    ? rd["bloque_jusqu"].ToString()
                                    : "Actif";

                    UserList.Items.Add(
                        $"{rd["id"]} - {rd["prenom"]} {rd["nom"]} | {rd["role"]} | {rd["email"]} | {bloque}"
                    );
                }
            }
        }

        private int GetSelectedUserId()
        {
            if (UserList.SelectedItem == null) return -1;

            string str = UserList.SelectedItem.ToString();
            return int.Parse(str.Split('-')[0].Trim());
        }

        private void LoadVehicules(int idUser)
        {
            VehiculeList.Items.Clear();

            using MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT immatriculation, marque, modele FROM vehicules WHERE id_utilisateur=@id";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", idUser);

            using MySqlDataReader rd = cmd.ExecuteReader()
            {
                while (rd.Read())
                {
                    VehiculeList.Items.Add(
                        $"{rd["marque"]} {rd["modele"]} - {rd["immatriculation"]}"
                    );
                }
            }
        }

        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int id = GetSelectedUserId();
            if (id != -1)
                LoadVehicules(id);
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateUserWindow wnd = new CreateUserWindow();
            wnd.ShowDialog();
            LoadUsers();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            int id = GetSelectedUserId();
            if (id == -1)
            {
                MessageBox.Show("Sélectionnez un utilisateur");
                return;
            }

            if (id == adminId)
            {
                MessageBox.Show("Vous ne pouvez pas vous supprimer vous-même !");
                return;
            }

            if (id == 1) // Super admin root
            {
                MessageBox.Show("Impossible de supprimer le super administrateur !");
                return;
            }

            if (MessageBox.Show("Voulez-vous vraiment supprimer cet utilisateur ?",
                "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using MySqlConnection conn = new MySqlConnection(connectionString);
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("DELETE FROM utilisateurs WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            LoadUsers();
        }

        private void BtnBlock_Click(object sender, RoutedEventArgs e)
        {
            int id = GetSelectedUserId();
            if (id == -1)
            {
                MessageBox.Show("Sélectionnez un utilisateur");
                return;
            }

            if (id == adminId)
            {
                MessageBox.Show("Vous ne pouvez pas vous bloquer vous-même !");
                return;
            }

            using MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();

            // Vérifie si l'utilisateur est déjà bloqué
            string checkQuery = "SELECT bloque_jusqu FROM utilisateurs WHERE id=@id";
            MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            object result = checkCmd.ExecuteScalar();

            DateTime? bloqueUntil = result != DBNull.Value ? (DateTime?)result : null;

            if (bloqueUntil == null || bloqueUntil < DateTime.Now)
            {
                // Bloquer 7 jours
                string blockQuery = "UPDATE utilisateurs SET bloque_jusqu=@date WHERE id=@id";
                MySqlCommand blockCmd = new MySqlCommand(blockQuery, conn);
                blockCmd.Parameters.AddWithValue("@date", DateTime.Now.AddDays(7));
                blockCmd.Parameters.AddWithValue("@id", id);
                blockCmd.ExecuteNonQuery();

                MessageBox.Show("Utilisateur bloqué pendant 7 jours.");
            }
            else
            {
                // Débloquer
                string unblockQuery = "UPDATE utilisateurs SET bloque_jusqu=NULL WHERE id=@id";
                MySqlCommand unblockCmd = new MySqlCommand(unblockQuery, conn);
                unblockCmd.Parameters.AddWithValue("@id", id);
                unblockCmd.ExecuteNonQuery();

                MessageBox.Show("Utilisateur débloqué.");
            }

            LoadUsers();
        }
    }
}
