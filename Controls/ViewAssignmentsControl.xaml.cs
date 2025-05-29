using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Windows.Controls;

namespace StudentCanvasApp.Controls
{
    public partial class ViewAssignmentsControl : UserControl
    {
        private int _teacherId;
        private readonly string connectionString = App.ConnectionString;
        private MainWindow _mainWindow;

        public ViewAssignmentsControl(MainWindow mainWindow, int teacherId)
        {
            InitializeComponent();
            _teacherId = teacherId;
            _mainWindow = mainWindow;
            LoadAssignments();
        }

        private void LoadAssignments()
        {
            var items = new List<AssignmentItem>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new MySqlCommand(@"
                SELECT a.AssignmentID, a.Title, c.ClassName, 
                COUNT(DISTINCT s.StudentID) AS SubmissionCount
                FROM assignment a
                JOIN class c ON a.ClassID = c.ClassID
                LEFT JOIN submission s ON a.AssignmentID = s.AssignmentID
                WHERE c.TeacherID = @teacherId
                GROUP BY a.AssignmentID, a.Title, c.ClassName", conn);


                cmd.Parameters.AddWithValue("@teacherId", _teacherId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new AssignmentItem
                    {
                        AssignmentID = reader.GetInt32("AssignmentID"),
                        Title = reader.GetString("Title"),
                        ClassName = reader.GetString("ClassName"),
                        SubmissionCountText = $"Submissions: {reader.GetInt32("SubmissionCount")}"
                    });
                }
            }

            AssignmentListBox.ItemsSource = items;
        }

        private class AssignmentItem
        {
            public int AssignmentID { get; set; }
            public string Title { get; set; }
            public string ClassName { get; set; }
            public string SubmissionCountText { get; set; }
        }
    }
}
