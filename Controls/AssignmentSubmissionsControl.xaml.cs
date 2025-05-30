using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace StudentCanvasApp.Controls
{
    public partial class AssignmentSubmissionsControl : UserControl
    {
        private int _assignmentId;
        private MainWindow _mainWindow;
        private DateTime _dueDate;
        private int _teacherId;
        private readonly string connectionString = App.ConnectionString;

        public AssignmentSubmissionsControl(MainWindow mainWindow, int assignmentId, string assignmentTitle, int teacherId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _assignmentId = assignmentId;
            _teacherId = teacherId;

            AssignmentTitle.Text = assignmentTitle;
            LoadSubmissions();
        }

        private void LoadSubmissions()
        {
            var ungraded = new List<SubmissionItem>();
            var graded = new List<SubmissionItem>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Fetch due date first
                var dueCmd = new MySqlCommand("SELECT DueDate FROM assignment WHERE AssignmentID = @id", conn);
                dueCmd.Parameters.AddWithValue("@id", _assignmentId);
                var result = dueCmd.ExecuteScalar();
                _dueDate = result != DBNull.Value ? Convert.ToDateTime(result) : DateTime.MaxValue;

                // Get only the latest submission per student for this assignment
                var cmd = new MySqlCommand(@"
                    SELECT s.SubmissionID, s.StudentID, st.Name, st.LastName, s.Grade, s.SubmissionText
                    FROM submission s
                    JOIN student st ON s.StudentID = st.StudentID
                    WHERE s.AssignmentID = @assignmentId
                    AND s.SubmissionID IN (
                        SELECT MAX(SubmissionID)
                        FROM submission
                        WHERE AssignmentID = @assignmentId
                        GROUP BY StudentID
                    )
                ", conn);
                cmd.Parameters.AddWithValue("@assignmentId", _assignmentId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new SubmissionItem
                        {
                            SubmissionId = reader.GetInt32("SubmissionID"),
                            StudentName = $"{reader.GetString("Name")} {reader.GetString("LastName")}"
                        };

                        if (reader["Grade"] == DBNull.Value)
                        {
                            if (DateTime.Now >= _dueDate)
                                item.StatusText = "Ready to Grade";
                            else
                                item.StatusText = "Due date has not passed";
                            ungraded.Add(item);
                        }
                        else
                        {
                            item.GradeText = $"Grade: {reader.GetDouble("Grade")}/10";
                            graded.Add(item);
                        }
                    }
                }

            }

            UngradedListBox.ItemsSource = ungraded;
            GradedListBox.ItemsSource = graded;
        }

        private void UngradedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UngradedListBox.SelectedItem is SubmissionItem item)
            {
                //_mainWindow.NavigateTo(new StudentSubmissionDetailControl(_mainWindow, item.SubmissionId));
            }
        }

        private void GradedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GradedListBox.SelectedItem is SubmissionItem item)
            {
                //_mainWindow.NavigateTo(new StudentSubmissionDetailControl(_mainWindow, item.SubmissionId));
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new ViewAssignmentsControl(_mainWindow, _teacherId));
        }

        private class SubmissionItem
        {
            public int SubmissionId { get; set; }
            public string StudentName { get; set; }
            public string StatusText { get; set; } // for ungraded
            public string GradeText { get; set; } // for graded
        }
    }
}
