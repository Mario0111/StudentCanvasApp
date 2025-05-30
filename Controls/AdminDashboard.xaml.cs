using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StudentCanvasApp.Controls
{
    public partial class AdminDashboard : UserControl
    {
        private MainWindow _mainWindow;
        private readonly string connectionString = App.ConnectionString;

        public AdminDashboard(MainWindow mainWindow, int adminId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadUsers();
            LoadClasses();
        }

        private void LoadUsers()
        {
            var students = new List<UserItem>();
            var teachers = new List<UserItem>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Students
                var studentCmd = new MySqlCommand("SELECT StudentID, Name, Email FROM student WHERE IsApproved = 1", conn);
                var reader = studentCmd.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(new UserItem
                    {
                        ID = reader.GetInt32("StudentID"),
                        Name = reader.GetString("Name"),
                        Email = reader.GetString("Email"),
                        Role = "student"
                    });
                }
                reader.Close();

                // Teachers
                var teacherCmd = new MySqlCommand("SELECT TeacherID, Name, Email FROM teacher", conn);
                reader = teacherCmd.ExecuteReader();
                while (reader.Read())
                {
                    teachers.Add(new UserItem
                    {
                        ID = reader.GetInt32("TeacherID"),
                        Name = reader.GetString("Name"),
                        Email = reader.GetString("Email"),
                        Role = "teacher"
                    });
                }
                reader.Close();
            }

            StudentsListBox.ItemsSource = students;
            TeachersListBox.ItemsSource = teachers;
        }

        private void LoadClasses()
        {
            var classes = new List<ClassItem>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT ClassID, ClassName FROM class", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    classes.Add(new ClassItem
                    {
                        ClassID = reader.GetInt32("ClassID"),
                        ClassName = reader.GetString("ClassName")
                    });
                }
            }
            ClassComboBox.ItemsSource = classes;
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is UserItem user)
            {
                var result = MessageBox.Show($"Delete {user.Name} ({user.Role})?", "Confirm", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var table = user.Role == "student" ? "student" : "teacher";
                    var idField = user.Role == "student" ? "StudentID" : "TeacherID";
                    var cmd = new MySqlCommand($"DELETE FROM {table} WHERE {idField} = @id", conn);
                    cmd.Parameters.AddWithValue("@id", user.ID);
                    cmd.ExecuteNonQuery();
                }

                LoadUsers();
            }
        }

        private void AddToClass_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsListBox.SelectedItem is UserItem student &&
                ClassComboBox.SelectedItem is ClassItem classItem)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("INSERT IGNORE INTO enrollment (StudentID, ClassID) VALUES (@sid, @cid)", conn);
                    cmd.Parameters.AddWithValue("@sid", student.ID);
                    cmd.Parameters.AddWithValue("@cid", classItem.ClassID);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show($"{student.Name} added to {classItem.ClassName}");
            }
        }

        private class UserItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }

        private class ClassItem
        {
            public int ClassID { get; set; }
            public string ClassName { get; set; }
        }
    }
}
