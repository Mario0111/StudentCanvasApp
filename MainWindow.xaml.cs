using System.Windows;
using StudentCanvasApp.Database; // ⬅️ Make sure this matches your folder

namespace StudentCanvasApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            var db = new DatabaseManager();
            db.InsertStudent(name, email, password);
        }
    }
}
