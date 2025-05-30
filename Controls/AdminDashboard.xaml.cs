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

        private void AddTeacher_Click(object sender, RoutedEventArgs e)
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Enter teacher's name:", "Add Teacher", "");
            var email = Microsoft.VisualBasic.Interaction.InputBox("Enter teacher's email:", "Add Teacher", "");
            var password = Microsoft.VisualBasic.Interaction.InputBox("Enter teacher's password:", "Add Teacher", "");

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Name and email cannot be empty.");
                return;
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("INSERT INTO teacher (Name, Email, Password) VALUES (@name, @email, @password)", conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Teacher added successfully.");
            LoadUsers();
        }

        private void CreateClass_Click(object sender, RoutedEventArgs e)
        {
            // Prompt for class name
            var className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Create Class", "");
            if (string.IsNullOrWhiteSpace(className))
            {
                MessageBox.Show("Class name is required.");
                return;
            }

            // Build teacher selection
            var teachers = new List<(int Id, string Name)>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT TeacherID, Name FROM teacher", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teachers.Add((reader.GetInt32("TeacherID"), reader.GetString("Name")));
                    }
                }
            }

            if (teachers.Count == 0)
            {
                MessageBox.Show("No teachers found.");
                return;
            }

            // Create selection dialog
            string prompt = "Choose a teacher by number:\n";
            for (int i = 0; i < teachers.Count; i++)
                prompt += $"{i + 1}. {teachers[i].Name}\n";

            var input = Microsoft.VisualBasic.Interaction.InputBox(prompt, "Select Teacher", "1");
            if (!int.TryParse(input, out int selectedIndex) || selectedIndex < 1 || selectedIndex > teachers.Count)
            {
                MessageBox.Show("Invalid selection.");
                return;
            }

            int teacherId = teachers[selectedIndex - 1].Id;

            // Create class
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("INSERT INTO class (ClassName, TeacherID) VALUES (@name, @teacherId)", conn);
                cmd.Parameters.AddWithValue("@name", className);
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Class created successfully.");
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new LoginControl(_mainWindow)); // or wherever you want to return
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
