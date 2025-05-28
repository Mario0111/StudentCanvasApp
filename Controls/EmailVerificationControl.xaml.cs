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
using System.Windows.Shapes;

namespace StudentCanvasApp.Controls
{
    /// <summary>
    /// Interaction logic for EmailVerificationControl.xaml
    /// </summary>
    public partial class EmailVerificationControl : UserControl
    {
        private MainWindow _mainWindow;
        public EmailVerificationControl(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void Verify_Click(object sender, RoutedEventArgs e)
        {
            string token = TokenBox.Text.Trim();

            using (var conn = new MySqlConnection(App.ConnectionString))
            {
                conn.Open();
                string query = "SELECT StudentID FROM student WHERE VerificationToken = @token AND IsApproved = 0";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@token", token);

                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    // Token matched, approve account
                    string update = "UPDATE student SET IsApproved = 1 WHERE VerificationToken = @token";
                    var updateCmd = new MySqlCommand(update, conn);
                    updateCmd.Parameters.AddWithValue("@token", token);
                    updateCmd.ExecuteNonQuery();

                    StatusText.Text = "Account verified successfully!";
                    StatusText.Foreground = Brushes.Green;

                    // ✅ Redirect to login
                    _mainWindow.NavigateTo(new LoginControl(_mainWindow));
                }
                else
                {
                    StatusText.Text = "Invalid or already used token.";
                    StatusText.Foreground = Brushes.Red;
                }
            }
        }



    }
}
