using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StudentCanvasApp.Helpers;


namespace StudentCanvasApp.Controls
{
    
    public partial class PasswordResetControl : UserControl
    {
        private MainWindow _mainWindow;
        public PasswordResetControl(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void SendResetCode_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                StatusText.Text = "Please enter your email.";
                return;
            }

            using (var conn = new MySqlConnection(App.ConnectionString))
            {
                conn.Open();
                string token = Guid.NewGuid().ToString();
                var cmd = new MySqlCommand("UPDATE student SET ResetToken = @token WHERE email = @Email", conn);
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@Email", email);

                int affected = cmd.ExecuteNonQuery();
                if (affected > 0)
                {
                    EmailUtils.SendEmail(email, "Password Reset", $"Use this code to reset your password:\n\n{token}");
                    _mainWindow.NavigateTo(new ResetPasswordConfirmControl(_mainWindow, email));
                    StatusText.Foreground = Brushes.Green;

                    // Optionally navigate to a "ResetFormControl" to enter token + new password
                    //_mainWindow.NavigateTo(new PasswordResetFormControl(_mainWindow, email));
                }
                else
                {
                    StatusText.Text = "No account with that email.";
                }
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new LoginControl(_mainWindow));
        }

    }
}
