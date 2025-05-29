using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MySql.Data.MySqlClient;

namespace StudentCanvasApp.Controls
{
    public partial class ViewClassesControl : UserControl
    {
        private int _teacherId;
        private MainWindow _mainWindow;
        private string connectionString = App.ConnectionString;

        public ViewClassesControl(MainWindow mainWindow, int teacherId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _teacherId = teacherId;
            LoadClasses();
        }

        private void LoadClasses()
        {
            var classList = new List<dynamic>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT c.ClassID, c.ClassName, 
                           (SELECT COUNT(*) FROM enrollment e WHERE e.ClassID = c.ClassID) AS StudentCount
                    FROM class c
                    WHERE c.TeacherID = @teacherId";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", _teacherId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            classList.Add(new
                            {
                                ClassName = reader.GetString("ClassName"),
                                StudentCountText = $"{reader.GetInt32("StudentCount")} students"
                            });
                        }
                    }
                }
            }

            ClassListBox.ItemsSource = classList;
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new TeacherDashboard(_mainWindow, _teacherId));
        }

    }
}
