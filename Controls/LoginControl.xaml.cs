using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using StudentCanvasApp.Controls; // Assuming StudentDashboard is here

namespace StudentCanvasApp.Controls
{
    public partial class LoginControl : UserControl
    {
        private MainWindow _mainWindow;
        private string connectionString = "server=localhost;port=3306;user=student;password=1234;database=schoolmanagement;";

        public LoginControl(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString().ToLower();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                MessageTextBlock.Text = "Please fill in all fields and select a role.";
                return;
            }

            string tableName;
            if (role == "admin") tableName = "admin";
            else if (role == "teacher") tableName = "teacher";
            else if (role == "student") tableName = "student";
            else tableName = null;


            if (tableName == null)
            {
                MessageTextBlock.Text = "Invalid role selected.";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = $"SELECT * FROM {tableName} WHERE email = @Email AND password = @Password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        MessageTextBlock.Text = "";
                        if (role == "student")
                        {
                            int studentId = reader.GetInt32("StudentID");
                            string name = reader.GetString("Name");
                            _mainWindow.NavigateTo(new StudentDashboard(_mainWindow, studentId, name));
                        }
                        else
                        {
                            MessageBox.Show($"{role} login successful (dashboard not yet implemented).");
                            // You can later redirect to TeacherDashboard or AdminDashboard here
                        }
                    }
                    else
                    {
                        MessageTextBlock.Text = "Login failed. Check your credentials.";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
    }
}
