using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StudentCanvasApp.Controls
{
    public partial class AdminDashboard : UserControl
    {
        private readonly string connectionString = App.ConnectionString;
        private MainWindow _mainWindow;
        private int _adminId;

        public AdminDashboard(MainWindow mainWindow, int adminId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _adminId = adminId;

            LoadPendingStudents(); // o cualquier método de inicialización
        }

        private void LoadPendingStudents()
        {
            var pending = new List<StudentItem>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT StudentID, Name, LastName, Email FROM student WHERE IsApproved = 0", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pending.Add(new StudentItem
                    {
                        StudentID = reader.GetInt32("StudentID"),
                        FullName = $"{reader.GetString("Name")} {reader.GetString("LastName")}",
                        Email = reader.GetString("Email")
                    });
                }
                reader.Close(); // 👈 importante cerrar el reader
            }

            PendingListBox.ItemsSource = pending;
        }


        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentId)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("UPDATE student SET IsApproved = 1 WHERE StudentID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", studentId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Student approved.");
                LoadPendingStudents();
            }
        }

        private class StudentItem
        {
            public int StudentID { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
        }
    }
}
