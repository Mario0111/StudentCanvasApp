using MySql.Data.MySqlClient;
using StudentCanvasApp.Models;
using System;
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
                        SELECT class.ClassID, class.ClassName
                        FROM enrollment
                        JOIN class ON enrollment.ClassID = class.ClassID
                        WHERE enrollment.StudentID = @StudentID";

                    MySqlCommand classCmd = new MySqlCommand(classQuery, conn);
                    classCmd.Parameters.AddWithValue("@StudentID", studentId);

                    var classReader = classCmd.ExecuteReader();
                    while (classReader.Read())
                    {
                        string className = classReader.GetString("ClassName");
                        ClassListBox.Items.Add(new Class
                        {
                            ClassName = classReader.GetString("ClassName"),
                            ClassID = classReader.IsDBNull(classReader.GetOrdinal("ClassID")) ? 0 : classReader.GetInt32("ClassID")
                        });
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

        private void ClassListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AssignmentListBox.Items.Clear();

            if (ClassListBox.SelectedItem is Class selectedClass)
            {
                string connectionString = "server=localhost;port=3306;user=student;password=1234;database=schoolmanagement;";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();

                        string assignmentQuery = @"
                    SELECT AssignmentID, Title, Description, DueDate
                    FROM assignment
                    WHERE ClassID = @ClassID";

                        MySqlCommand cmd = new MySqlCommand(assignmentQuery, conn);
                        cmd.Parameters.AddWithValue("@ClassID", selectedClass.ClassID);

                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            AssignmentListBox.Items.Add(new Assignment
                            {
                                AssignmentID = reader.GetInt32("AssignmentID"),
                                Title = reader.GetString("Title"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                                DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? (DateTime?)null : reader.GetDateTime("DueDate")

                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to load assignments: " + ex.Message);
                    }
                }
            }
        }

    }
}
