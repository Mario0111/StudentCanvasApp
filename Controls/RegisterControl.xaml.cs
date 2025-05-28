using System;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace StudentCanvasApp.Controls
{
    public partial class RegisterControl : UserControl
    {
        private MainWindow _mainWindow;

        public RegisterControl(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void SubmitRegister_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string confirm = ConfirmBox.Password.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
            {
                MessageText.Text = "All fields are required.";
                return;
            }

            if (password != confirm)
            {
                MessageText.Text = "Passwords do not match.";
                return;
            }

            string token = new Random().Next(100000, 999999).ToString();

            try
            {
                using (var conn = new MySqlConnection(App.ConnectionString))
                {
                    conn.Open();

                    var cmd = new MySqlCommand("INSERT INTO student (Name, Email, Password, IsApproved, VerificationToken) VALUES (@Name, @Email, @Password, 0, @Token)", conn);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.ExecuteNonQuery();
                }

                // Send verification token via email
                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("canvasstudent70@gmail.com", "bfzl jzpx vnnc kigz"),
                    EnableSsl = true
                };

                var message = new MailMessage("canvasstudent70@gmail.com", email)
                {
                    Subject = "Verify your student account",
                    Body = $"Hello {name},\n\nYour verification code is: {token}\n\nPlease enter this code in the application to activate your account."
                };

                smtp.Send(message);

                MessageBox.Show("Account created! A verification code has been sent to your email.");

                // Navigate to verification page
                _mainWindow.NavigateTo(new EmailVerificationControl(_mainWindow));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to register: " + ex.Message);
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new LoginControl(_mainWindow));
        }
    }
}
