using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StudentCanvasApp.Controls
{
    public partial class CreateAssignmentControl : UserControl
    {
        private readonly MainWindow _mainWindow;
        private readonly int _teacherId;
        private readonly string connectionString = App.ConnectionString;

        private List<ClassItem> _classItems = new List<ClassItem>();

        public CreateAssignmentControl(MainWindow mainWindow, int teacherId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _teacherId = teacherId;

            LoadClasses();
        }

        private void LoadClasses()
        {
            _classItems.Clear();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT ClassID, ClassName FROM class WHERE TeacherID = @id", conn);
                cmd.Parameters.AddWithValue("@id", _teacherId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ClassItem
                        {
                            ClassID = reader.GetInt32("ClassID"),
                            ClassName = reader.GetString("ClassName")
                        };
                        _classItems.Add(item);
                    }
                }
            }

            ClassComboBox.ItemsSource = _classItems;
            ClassComboBox.DisplayMemberPath = "ClassName";
            ClassComboBox.SelectedValuePath = "ClassID";
        }

        private void CreateAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionBox.Text) ||
                ClassComboBox.SelectedItem == null ||
                DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please fill out all fields.");
                return;
            }

            var title = TitleBox.Text.Trim();
            var description = DescriptionBox.Text.Trim();
            var classId = ((ClassItem)ClassComboBox.SelectedItem).ClassID;
            var dueDate = DueDatePicker.SelectedDate.Value;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "INSERT INTO assignment (Title, Description, ClassID, DueDate) VALUES (@title, @desc, @classId, @dueDate)", conn);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@desc", description);
                cmd.Parameters.AddWithValue("@classId", classId);
                cmd.Parameters.AddWithValue("@dueDate", dueDate);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Assignment created successfully!");
            _mainWindow.NavigateTo(new ViewAssignmentsControl(_mainWindow, _teacherId));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new ViewAssignmentsControl(_mainWindow, _teacherId));
        }

        private class ClassItem
        {
            public int ClassID { get; set; }
            public string ClassName { get; set; }
        }
    }
}
