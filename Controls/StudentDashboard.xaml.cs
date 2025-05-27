using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using StudentCanvasApp.Models;

namespace StudentCanvasApp.Controls
{
    public partial class StudentDashboard : UserControl
    {
        private MainWindow _mainWindow;
        private int _studentId;
        private string _studentName;
        private ObservableCollection<Class> _classes = new ObservableCollection<Class>();
        private ObservableCollection<Assignment> _assignments = new ObservableCollection<Assignment>();

        private string connectionString = "server=localhost;port=3306;user=student;password=1234;database=schoolmanagement;";

        public StudentDashboard(MainWindow mainWindow, int studentId, string studentName)
        {
            InitializeComponent();
            ClassListBox.ItemsSource = _classes;
            AssignmentListBox.ItemsSource = _assignments;

            _mainWindow = mainWindow;
            _studentId = studentId;
            _studentName = studentName;

            LoadStudentData();
        }

        private void LoadStudentData()
        {
            WelcomeText.Text = $"Welcome, {_studentName}!";

            _classes.Clear();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT c.ClassID, c.ClassName
                    FROM enrollment e
                    JOIN class c ON e.ClassID = c.ClassID
                    WHERE e.StudentID = @StudentID";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentID", _studentId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _classes.Add(new Class
                    {
                        ClassID = reader.GetInt32("ClassID"),
                        ClassName = reader.GetString("ClassName")
                    });
                }
            }
        }

        private void ClassListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassListBox.SelectedItem is Class selectedClass)
            {
                LoadAssignmentsForClass(selectedClass.ClassID);
            }
        }

        private void LoadAssignmentsForClass(int classId)
        {
            _assignments.Clear();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT AssignmentID, Title, Description, DueDate
                    FROM assignment
                    WHERE ClassID = @ClassID";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClassID", classId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _assignments.Add(new Assignment
                    {
                        AssignmentID = reader.GetInt32("AssignmentID"),
                        Title = reader.GetString("Title"),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                        DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? (DateTime?)null : reader.GetDateTime("DueDate")
                    });
                }
            }
        }

        private void AssignmentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssignmentListBox.SelectedItem is Assignment assignment)
            {
                AssignmentTitle.Text = $"Title: {assignment.Title}";
                AssignmentDue.Text = $"Due: {(assignment.DueDate.HasValue ? assignment.DueDate.Value.ToString("dd MMM yyyy") : "No due date")}";
                AssignmentDescription.Text = $"Description: {assignment.Description}";
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new LoginControl(_mainWindow));
        }
    }
}

