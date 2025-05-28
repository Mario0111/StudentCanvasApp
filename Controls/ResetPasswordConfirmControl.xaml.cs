using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace StudentCanvasApp.Controls
{
    public partial class ResetPasswordConfirmControl : UserControl
    {
        private MainWindow _mainWindow;
        private string _email;

        public ResetPasswordConfirmControl(MainWindow mainWindow, string email)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _email = email;
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            string token = TokenBox.Text.Trim();
            string newPassword = NewPasswordBox.Password.Trim();
            string confirmPassword = ConfirmPasswordBox.Password.Trim();

            if (newPassword != confirmPassword)
            {
                StatusText.Text = "Passwords do not match.";
                return;
            }

            using (var conn = new MySqlConnection(App.ConnectionString))
            {
                conn.Open();
                string query = "SELECT ResetToken FROM student WHERE email = @Email";
                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", _email);

                var result = cmd.ExecuteScalar();
                if (result == null || result.ToString() != token)
                {
                    StatusText.Text = "Invalid verification code.";
                    return;
                }

                // Update password and clear token
                string update = "UPDATE student SET password = @Password, ResetToken = NULL WHERE email = @Email";
                var updateCmd = new MySqlCommand(update, conn);
                updateCmd.Parameters.AddWithValue("@Password", newPassword);
                updateCmd.Parameters.AddWithValue("@Email", _email);
                updateCmd.ExecuteNonQuery();
            }

            MessageBox.Show("Password successfully reset!");
            _mainWindow.NavigateTo(new LoginControl(_mainWindow));
        }
    }
}
