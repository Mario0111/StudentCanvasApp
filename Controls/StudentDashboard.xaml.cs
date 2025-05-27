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
                    SELECT c.ClassID, c.ClassName, t.name AS TeacherName
                    FROM enrollment e
                    JOIN class c ON e.ClassID = c.ClassID
                    JOIN teacher t ON c.TeacherID = t.TeacherID
                    WHERE e.StudentID = @StudentID";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentID", _studentId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _classes.Add(new Class
                    {
                        ClassID = reader.GetInt32("ClassID"),
                        ClassName = reader.GetString("ClassName"),
                        TeacherName = reader.GetString("TeacherName")
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
            var selectedAssignment = AssignmentListBox.SelectedItem as Assignment;
            if (selectedAssignment == null) return;

            AssignmentTitle.Text = $"Title: {selectedAssignment.Title}";
            AssignmentDue.Text = $"Due: {(selectedAssignment.DueDate.HasValue ? selectedAssignment.DueDate.Value.ToString("dd MMM yyyy") : "No due date")}";
            AssignmentDescription.Text = $"Description: {selectedAssignment.Description}";

            SubmissionTextBox.Clear();
            SubmissionTextBox.IsReadOnly = false;
            SubmitAssignmentButton.IsEnabled = true;
            SubmissionStatus.Text = "Not yet submitted.";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string checkQuery = @"
            SELECT SubmissionText, SubmittedAt
            FROM submission
            WHERE AssignmentID = @AssignmentID AND StudentID = @StudentID
            LIMIT 1";

                MySqlCommand cmd = new MySqlCommand(checkQuery, conn);
                cmd.Parameters.AddWithValue("@AssignmentID", selectedAssignment.AssignmentID);
                cmd.Parameters.AddWithValue("@StudentID", _studentId);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string text = reader.GetString("SubmissionText");
                    DateTime submittedAt = reader.GetDateTime("SubmittedAt");

                    SubmissionTextBox.Text = text;
                    SubmissionTextBox.IsReadOnly = true;
                    SubmitAssignmentButton.IsEnabled = false;
                    SubmissionStatus.Text = $"✅ Submitted at {submittedAt:dd MMM yyyy HH:mm}";
                }
            }
        }


        private void SubmitAssignment_Click(object sender, RoutedEventArgs e)
        {
            var selectedAssignment = AssignmentListBox.SelectedItem as Assignment;
            if (selectedAssignment == null)
            {
                MessageBox.Show("Please select an assignment first.");
                return;
            }


            string submissionText = SubmissionTextBox.Text.Trim();
            if (string.IsNullOrEmpty(submissionText))
            {
                MessageBox.Show("Submission cannot be empty.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
            INSERT INTO submission (AssignmentID, StudentID, SubmissionText, SubmittedAt)
            VALUES (@AssignmentID, @StudentID, @SubmissionText, NOW())";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AssignmentID", selectedAssignment.AssignmentID);
                cmd.Parameters.AddWithValue("@StudentID", _studentId);
                cmd.Parameters.AddWithValue("@SubmissionText", submissionText);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    MessageBox.Show("Assignment submitted successfully!");

                    // Trigger the assignment selection again to refresh status
                    AssignmentListBox_SelectionChanged(null, null);

                }
                else
                {
                    MessageBox.Show("Submission failed. Please try again.");
                }
            }
        }


        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new LoginControl(_mainWindow));
        }
    }
}

