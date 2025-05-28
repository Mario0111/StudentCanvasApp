using MySql.Data.MySqlClient;
using StudentCanvasApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace StudentCanvasApp.Controls
{
    public partial class StudentDashboard : UserControl
    {
        private MainWindow _mainWindow;
        private int _studentId;
        private string _studentName;
        private ObservableCollection<Class> _classes = new ObservableCollection<Class>();
        private ObservableCollection<Assignment> _assignments = new ObservableCollection<Assignment>();
        private string _selectedFilePath = null;
        private string _submittedFilePath = null;


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

                // 1. Load all classes for the student
                string query = @"
                    SELECT c.ClassID, c.ClassName, t.name AS TeacherName
                    FROM enrollment e
                    JOIN class c ON e.ClassID = c.ClassID
                    JOIN teacher t ON c.TeacherID = t.TeacherID
                    WHERE e.StudentID = @StudentID";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentID", _studentId);

                var reader = cmd.ExecuteReader();
                var loadedClasses = new List<Class>();

                while (reader.Read())
                {
                    loadedClasses.Add(new Class
                    {
                        ClassID = reader.GetInt32("ClassID"),
                        ClassName = reader.GetString("ClassName"),
                        TeacherName = reader.GetString("TeacherName")
                    });
                }

                reader.Close();

                // 2. For each class, fetch the average grade
                foreach (var cls in loadedClasses)
                {
                    string avgQuery = @"
                        SELECT AVG(grade)
                        FROM submission s
                        JOIN assignment a ON a.AssignmentID = s.AssignmentID
                        WHERE a.ClassID = @ClassID AND s.StudentID = @StudentID AND s.Grade IS NOT NULL";

                    MySqlCommand avgCmd = new MySqlCommand(avgQuery, conn);
                    avgCmd.Parameters.AddWithValue("@ClassID", cls.ClassID);
                    avgCmd.Parameters.AddWithValue("@StudentID", _studentId);

                    object result = avgCmd.ExecuteScalar();
                    cls.AverageGradeText = result != DBNull.Value
                        ? $"Total Grade: {Math.Round(Convert.ToDouble(result), 2)}/10"
                        : "Total Grade: N/A";

                    _classes.Add(cls);
                }

                ClassListBox.ItemsSource = null;
                ClassListBox.ItemsSource = _classes;
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
            if (selectedAssignment == null)
                return;

            // Show basic assignment info
            AssignmentTitle.Text = $"Title: {selectedAssignment.Title}";
            AssignmentDue.Text = $"Due: {(selectedAssignment.DueDate.HasValue ? selectedAssignment.DueDate.Value.ToString("dd MMM yyyy") : "No due date")}";
            AssignmentDescription.Text = $"Description: {selectedAssignment.Description}";

            // Reset UI
            SubmissionTextBox.Clear();
            SubmissionTextBox.IsReadOnly = false;
            SubmitAssignmentButton.IsEnabled = true;
            SubmissionStatus.Text = "Not yet submitted.";
            SelectedFileLabel.Text = "No file selected";
            DownloadFileButton.Visibility = Visibility.Collapsed;
            _submittedFilePath = null;
            _selectedFilePath = null;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string checkQuery = @"
                    SELECT SubmissionText, SubmittedAt, FilePath, grade
                    FROM submission
                    WHERE AssignmentID = @AssignmentID AND StudentID = @StudentID
                    ORDER BY SubmittedAt DESC
                    LIMIT 1";

                MySqlCommand cmd = new MySqlCommand(checkQuery, conn);
                cmd.Parameters.AddWithValue("@AssignmentID", selectedAssignment.AssignmentID);
                cmd.Parameters.AddWithValue("@StudentID", _studentId);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string submittedText = reader.GetString("SubmissionText");
                    DateTime submittedAt = reader.GetDateTime("SubmittedAt");
                    string filePath = reader.IsDBNull(reader.GetOrdinal("FilePath")) ? null : reader.GetString("FilePath");

                    SubmissionTextBox.Text = submittedText;
                    SubmissionStatus.Text = $"✅ Submitted at {submittedAt:dd MMM yyyy HH:mm}";
                    SelectedFileLabel.Text = filePath != null ? System.IO.Path.GetFileName(filePath) : "No file submitted";
                    DownloadFileButton.Visibility = filePath != null ? Visibility.Visible : Visibility.Collapsed;
                    _submittedFilePath = filePath;

                    // 🔒 Allow resubmission if still before due date
                    if (selectedAssignment.DueDate.HasValue && selectedAssignment.DueDate.Value > DateTime.Now)
                    {
                        SubmissionTextBox.IsReadOnly = false;
                        SubmitAssignmentButton.IsEnabled = true;
                    }
                    else
                    {
                        SubmissionTextBox.IsReadOnly = true;
                        SubmitAssignmentButton.IsEnabled = false;
                    }
                    if (reader["grade"] != DBNull.Value)
                    {
                        GradeTextBlock.Text = $"Grade: {reader["grade"]}/10";
                    }
                    else
                    {
                        GradeTextBlock.Text = "Grade: Not yet assigned";
                    }
                }
                else
                {
                    // No previous submission
                    SubmissionStatus.Text = "❌ Not yet submitted.";
                }
            }
        }


        private void SubmitAssignment_Click(object sender, RoutedEventArgs e)
        {
            // Get selected assignment
            var selectedAssignment = AssignmentListBox.SelectedItem as Assignment;

            if (selectedAssignment == null)
            {
                MessageBox.Show("Please select an assignment.");
                return;
            }

            // Check if due date has passed
            if (selectedAssignment.DueDate.HasValue && selectedAssignment.DueDate.Value < DateTime.Now)
            {
                MessageBox.Show("You cannot submit this assignment after the due date.");
                return;
            }

            string submissionText = SubmissionTextBox.Text.Trim();
            string fileName = _selectedFilePath != null ? System.IO.Path.GetFileName(_selectedFilePath) : null;
            string targetPath = null;

            // Optional: save uploaded file to a "submissions" folder
            if (_selectedFilePath != null)
            {
                string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "submissions");
                System.IO.Directory.CreateDirectory(folder);
                targetPath = System.IO.Path.Combine(folder, fileName);
                System.IO.File.Copy(_selectedFilePath, targetPath, true);
            }

            // Insert or update submission
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string upsert = @"
                    INSERT INTO submission (AssignmentID, StudentID, SubmissionText, FilePath, SubmittedAt)
                    VALUES (@AssignmentID, @StudentID, @SubmissionText, @FilePath, NOW())
                    ON DUPLICATE KEY UPDATE
                    SubmissionText = @SubmissionText,
                    FilePath = @FilePath,
                    SubmittedAt = NOW()";

                MySqlCommand cmd = new MySqlCommand(upsert, conn);
                cmd.Parameters.AddWithValue("@AssignmentID", selectedAssignment.AssignmentID);
                cmd.Parameters.AddWithValue("@StudentID", _studentId);
                cmd.Parameters.AddWithValue("@SubmissionText", submissionText);
                cmd.Parameters.AddWithValue("@FilePath", targetPath ?? (object)DBNull.Value);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    MessageBox.Show("Submission saved.");
                    var current = AssignmentListBox.SelectedItem;
                    AssignmentListBox.SelectedItem = null;
                    AssignmentListBox.SelectedItem = current;
                }
                else
                {
                    MessageBox.Show("Submission failed.");
                }
            }

        }

        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All Files|*.*";

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                _selectedFilePath = dlg.FileName;
                SelectedFileLabel.Text = System.IO.Path.GetFileName(_selectedFilePath);
            }
        }

        private void DownloadFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_submittedFilePath) || !File.Exists(_submittedFilePath))
            {
                MessageBox.Show("The submitted file could not be found.");
                return;
            }

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", _submittedFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open file: " + ex.Message);
            }
        }


        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new LoginControl(_mainWindow));
        }
    }
}

