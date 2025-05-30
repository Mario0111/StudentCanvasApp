using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace StudentCanvasApp.Controls
{
    public partial class StudentSubmissionDetailControl : UserControl
    {
        private readonly int _submissionId;
        private readonly MainWindow _mainWindow;
        private readonly string connectionString = App.ConnectionString;
        private bool _canGrade = false;

        private string _filePath;
        private int _teacherId;

        public StudentSubmissionDetailControl(MainWindow mainWindow, int submissionId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _submissionId = submissionId;

            LoadSubmissionDetails();
        }

        private void LoadSubmissionDetails()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
                    SELECT s.SubmissionText, s.FilePath, s.Grade, s.SubmittedAt,
                           a.Title, a.DueDate, c.TeacherID
                    FROM submission s
                    JOIN assignment a ON s.AssignmentID = a.AssignmentID
                    JOIN class c ON a.ClassID = c.ClassID
                    WHERE s.SubmissionID = @id", conn);

                cmd.Parameters.AddWithValue("@id", _submissionId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        _filePath = reader["FilePath"]?.ToString();
                        _teacherId = Convert.ToInt32(reader["TeacherID"]);

                        SubmissionTitleText.Text = reader["Title"]?.ToString() ?? "No title";

                        SubmissionTextBlock.Text = reader["SubmissionText"] != DBNull.Value
                            ? reader["SubmissionText"].ToString()
                            : "(No text submitted)";

                        if (reader["SubmittedAt"] != DBNull.Value)
                        {
                            DateTime submittedAt = Convert.ToDateTime(reader["SubmittedAt"]);
                            SubmittedAtText.Text = $"Submitted on: {submittedAt:dd MMM yyyy HH:mm}";
                        }
                        else
                        {
                            SubmittedAtText.Text = "(No submission time)";
                        }

                        GradeText.Text = reader["Grade"] != DBNull.Value
                            ? $"Grade: {reader["Grade"]}/10"
                            : "Not yet graded";

                        DateTime dueDate = reader["DueDate"] != DBNull.Value
                            ? Convert.ToDateTime(reader["DueDate"])
                            : DateTime.MinValue;

                        _canGrade = DateTime.Now > dueDate && reader["Grade"] == DBNull.Value;

                        GradeBox.Visibility = _canGrade ? Visibility.Visible : Visibility.Collapsed;
                        SubmitGradeButton.Visibility = _canGrade ? Visibility.Visible : Visibility.Collapsed;
                    }

                }
            }
        }

        private void SubmitGradeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(GradeBox.Text, out int grade) || grade < 0 || grade > 10)
            {
                MessageBox.Show("Enter a grade between 0 and 10.");
                return;
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("UPDATE submission SET Grade = @grade WHERE SubmissionID = @id", conn);
                cmd.Parameters.AddWithValue("@grade", grade);
                cmd.Parameters.AddWithValue("@id", _submissionId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Grade submitted.");
            _mainWindow.NavigateTo(new ViewAssignmentsControl(_mainWindow, _teacherId));
        }

        private void DownloadFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_filePath) && File.Exists(_filePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", _filePath);
            }
            else
            {
                MessageBox.Show("File not found.");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new ViewAssignmentsControl(_mainWindow, _teacherId));
        }
    }
}
