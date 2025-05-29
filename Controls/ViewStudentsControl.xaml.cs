using MySql.Data.MySqlClient;
using StudentCanvasApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StudentCanvasApp.Controls
{
    public partial class ViewStudentsControl : UserControl
    {
        private MainWindow _mainWindow;
        private int _teacherId;
        private List<Student> _allStudents = new List<Student>();


        public ViewStudentsControl(MainWindow mainWindow, int teacherId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _teacherId = teacherId;

            LoadStudents();
        }

        private void LoadStudents()
        {
            _allStudents.Clear();

            using (var conn = new MySqlConnection(App.ConnectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
                    SELECT s.StudentID, s.Name, s.LastName, GROUP_CONCAT(c.ClassName SEPARATOR ', ') AS Classes
                    FROM student s
                    JOIN enrollment e ON s.StudentID = e.StudentID
                    JOIN class c ON e.ClassID = c.ClassID
                    WHERE c.TeacherID = @teacherId
                    GROUP BY s.StudentID, s.Name, s.LastName
                    ORDER BY s.LastName ASC, s.Name ASC;", conn);

                cmd.Parameters.AddWithValue("@teacherId", _teacherId); // make sure you have this

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _allStudents.Add(new Student
                    {
                        StudentID = reader.GetInt32("StudentID"),
                        Name = reader.GetString("Name"),
                        LastName = reader.GetString("LastName"),
                        Classes = reader.GetString("Classes").Split(',').Select(x => x.Trim()).ToList()
                    });
                }
            }

            FilterStudents(); // apply filter (initially all)
        }

        private void StudentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = StudentsListBox.SelectedItem as StudentOverview;
            if (selected != null)
            {
                MessageBox.Show($"Future: Show details for {selected.FullName}"); // Replace later with detailed view
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterStudents();
        }

        private void FilterStudents()
        {
            string query = SearchTextBox.Text.Trim().ToLower();

            var filtered = _allStudents
                .Where(s => s.FullName.ToLower().Contains(query))
                .ToList();

            StudentsListBox.ItemsSource = filtered;
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new TeacherDashboard(_mainWindow, _teacherId));
        }
    }
}

