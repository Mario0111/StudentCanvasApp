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
                        if (role == "student" && reader["IsApproved"] is bool isApproved && !isApproved)
                        {
                            MessageBox.Show("Account not yet approved. Please wait for admin confirmation.");
                            return;
                        }

                        MessageTextBlock.Text = "";

                        if (role == "student")
                        {
                            int studentId = reader.GetInt32("StudentID");
                            string name = reader.GetString("Name");
                            _mainWindow.NavigateTo(new StudentDashboard(_mainWindow, studentId, name));
                        }
                        if (role == "teacher")
                        {
                            string name = reader.GetString("Name");
                            int teacherId = reader.GetInt32("TeacherID");
                            _mainWindow.NavigateTo(new TeacherDashboard(_mainWindow, teacherId));
                        }
                        if (role == "admin")
                        {
                            MessageBox.Show($"{role} login successful (dashboard not yet implemented).");
                        }
                    }
                    else
                    {
                        MessageTextBlock.Text = "User Credentials are not correct.";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new RegisterControl(_mainWindow));
        }
        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new PasswordResetControl(_mainWindow));
        }

    }
}
