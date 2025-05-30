using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StudentCanvasApp.Controls
{
    public partial class StudentDetailControl : UserControl
    {
        private MainWindow _mainWindow;
        private int _studentId;
        private int _teacherId;
        private readonly string connectionString = App.ConnectionString;

        public StudentDetailControl(MainWindow mainWindow, int studentId, int teacherId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _studentId = studentId;
            _teacherId = teacherId;

            LoadStudentDetails();
            LoadClassGrades();
        }


        private void LoadStudentDetails()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new MySqlCommand("SELECT Name, LastName, Email, ProfilePicturePath FROM student WHERE StudentID = @id", conn);
                cmd.Parameters.AddWithValue("@id", _studentId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        StudentNameText.Text = $"{reader.GetString("Name")} {reader.GetString("LastName")}";
                        EmailText.Text = reader.GetString("Email");

                        var path = reader["ProfilePicturePath"]?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            ProfileImage.Source = new BitmapImage(new Uri(path));
                        }
                    }
                }
            }
        }

        private void LoadClassGrades()
        {
            var items = new List<ClassGradeItem>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
                SELECT c.ClassID, c.ClassName, AVG(s.Grade) AS AvgGrade
                FROM class c
                JOIN enrollment e ON e.ClassID = c.ClassID
                LEFT JOIN assignment a ON a.ClassID = c.ClassID
                LEFT JOIN submission s ON s.AssignmentID = a.AssignmentID AND s.StudentID = @studentId
                WHERE e.StudentID = @studentId AND c.TeacherID = @teacherId
                GROUP BY c.ClassID, c.ClassName", conn);

                cmd.Parameters.AddWithValue("@studentId", _studentId);
                cmd.Parameters.AddWithValue("@teacherId", _teacherId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var className = reader.GetString("ClassName");
                        var classId = reader.GetInt32("ClassID");
                        var avgGradeObj = reader["AvgGrade"];

                        string avgGradeText = avgGradeObj != DBNull.Value
                            ? $"Grade: {Math.Round(Convert.ToDouble(avgGradeObj), 2)}/10"
                            : "Grade: N/A";

                        items.Add(new ClassGradeItem
                        {
                            ClassID = classId,
                            ClassName = className,
                            AverageGradeText = avgGradeText
                        });
                    }
                }
            }

            ClassesListBox.ItemsSource = items;
        }


        private void KickFromClass_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int classId)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to remove this student from the class?",
                    "Confirm Removal",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using (var conn = new MySqlConnection(App.ConnectionString))
                    {
                        conn.Open();
                        var cmd = new MySqlCommand(
                            "DELETE FROM enrollment WHERE StudentID = @studentId AND ClassID = @classId", conn);
                        cmd.Parameters.AddWithValue("@studentId", _studentId);
                        cmd.Parameters.AddWithValue("@classId", classId);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Student removed from class.");
                    }

                    LoadClassGrades(); // Refresh list
                }
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new ViewStudentsControl(_mainWindow, _teacherId));
        }


        private class ClassGradeItem
        {
            public int ClassID { get; set; }
            public string ClassName { get; set; }
            public string AverageGradeText { get; set; }
        }
    }
}
