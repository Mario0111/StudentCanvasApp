using MySql.Data.MySqlClient;
using StudentCanvasApp.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StudentCanvasApp.Controls
{
    public partial class ViewStudentsControl : UserControl
    {
        private MainWindow _mainWindow;
        private int _teacherId;

        public ViewStudentsControl(MainWindow mainWindow, int teacherId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _teacherId = teacherId;

            LoadStudents();
        }

        private void LoadStudents()
        {
            var students = new List<StudentOverview>();
            using (var conn = new MySqlConnection(App.ConnectionString))
            {
                conn.Open();
                string query = @"
                    SELECT s.StudentID, s.Name, s.LastName, GROUP_CONCAT(c.ClassName SEPARATOR ', ') AS ClassNames
                    FROM student s
                    JOIN enrollment e ON s.StudentID = e.StudentID
                    JOIN class c ON e.ClassID = c.ClassID
                    WHERE c.TeacherID = @teacherId
                    GROUP BY s.StudentID, s.Name, s.LastName";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@teacherId", _teacherId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(new StudentOverview
                    {
                        StudentID = reader.GetInt32("StudentID"),
                        FullName = reader.GetString("Name") + " " + reader.GetString("LastName"),
                        ClassNames = reader.GetString("ClassNames")
                    });
                }
            }

            StudentListBox.ItemsSource = students;
        }

        private void StudentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = StudentListBox.SelectedItem as StudentOverview;
            if (selected != null)
            {
                MessageBox.Show($"Future: Show details for {selected.FullName}"); // Replace later with detailed view
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new TeacherDashboard(_mainWindow, _teacherId));
        }
    }
}

