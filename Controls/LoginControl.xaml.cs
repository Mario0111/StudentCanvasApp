using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace StudentCanvasApp.Controls
{
    public partial class LoginControl : UserControl
    {
        private MainWindow _mainWindow;

        public LoginControl(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageTextBlock.Text = "Please enter email and password.";
                return;
            }

            string connectionString = "server=localhost;port=3306;user=student;password=1234;database=schoolmanagement;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT 'student' AS role FROM student WHERE email = @Email AND password = @Password
                        UNION
                        SELECT 'teacher' AS role FROM teacher WHERE email = @Email AND password = @Password
                        UNION
                        SELECT 'admin' AS role FROM admin WHERE email = @Email AND password = @Password
                        LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    var role = cmd.ExecuteScalar() as string;

                    if (role != null)
                    {
                        MessageBox.Show($"Login successful as {role}!");
                        _mainWindow.NavigateToRole(role); // 🚨 You’ll add this method next
                    }
                    else
                    {
                        MessageTextBlock.Text = "Invalid credentials.";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Login failed: " + ex.Message);
                }
            }
        }
    }
}
