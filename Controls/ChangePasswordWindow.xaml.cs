using System.Windows;
using MySql.Data.MySqlClient;

namespace StudentCanvasApp.Controls
{
    public partial class ChangePasswordWindow : Window
    {
        private int _studentId;
        private string _connectionString = "server=localhost;user=root;password=;database=schoolmanagement;";

        public ChangePasswordWindow(int studentId)
        {
            InitializeComponent();
            _studentId = studentId;
        }

        private void SubmitChange_Click(object sender, RoutedEventArgs e)
        {
            string current = CurrentPasswordBox.Password.Trim();
            string newPass = NewPasswordBox.Password.Trim();
            string confirm = ConfirmPasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageText.Text = "All fields are required.";
                return;
            }

            if (newPass != confirm)
            {
                MessageText.Text = "New passwords do not match.";
                return;
            }

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var checkCmd = new MySqlCommand("SELECT password FROM student WHERE StudentID = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", _studentId);
                var currentDbPassword = checkCmd.ExecuteScalar()?.ToString();

                if (currentDbPassword != current)
                {
                    MessageText.Text = "Current password is incorrect.";
                    return;
                }

                var updateCmd = new MySqlCommand("UPDATE student SET password = @newPass WHERE StudentID = @id", conn);
                updateCmd.Parameters.AddWithValue("@newPass", newPass);
                updateCmd.Parameters.AddWithValue("@id", _studentId);
                updateCmd.ExecuteNonQuery();

                MessageBox.Show("Password changed successfully!");
                this.Close();
            }
        }
    }
}
