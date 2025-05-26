using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;

namespace StudentCanvasApp.Controls
{
    public partial class StudentDashboard : UserControl
    {
        private MainWindow _mainWindow;
        private string _studentEmail;

        public StudentDashboard(MainWindow mainWindow, string studentEmail)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _studentEmail = studentEmail;

            LoadStudentData();
        }

        private void LoadStudentData()
        {
            string connectionString = "server=localhost;port=3306;user=student;password=1234;database=schoolmanagement;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Get student info
                    string infoQuery = "SELECT StudentID, Name FROM student WHERE Email = @Email LIMIT 1";
                    MySqlCommand cmd = new MySqlCommand(infoQuery, conn);
                    cmd.Parameters.AddWithValue("@Email", _studentEmail);
                    var reader = cmd.ExecuteReader();

                    int studentId = -1;
                    string name = "";

                    if (reader.Read())
                    {
                        studentId = reader.GetInt32("StudentID");
                        name = reader.GetString("Name");
                    }
                    reader.Close();

                    WelcomeText.Text = $"Welcome, {name}!";

                    // Get enrolled classes
                    string classQuery = @"
                        SELECT class.ClassName
                        FROM enrollment
                        JOIN class ON enrollment.ClassID = class.ClassID
                        WHERE enrollment.StudentID = @StudentID";

                    MySqlCommand classCmd = new MySqlCommand(classQuery, conn);
                    classCmd.Parameters.AddWithValue("@StudentID", studentId);

                    var classReader = classCmd.ExecuteReader();
                    while (classReader.Read())
                    {
                        string className = classReader.GetString("ClassName");
                        ClassListBox.Items.Add(className);
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Failed to load student data: " + ex.Message);
                }
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new LoginControl(_mainWindow));
        }
    }
}
