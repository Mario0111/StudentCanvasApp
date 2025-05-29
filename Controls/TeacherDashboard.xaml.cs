using System.Windows;
using System.Windows.Controls;

namespace StudentCanvasApp.Controls
{
    public partial class TeacherDashboard : UserControl
    {
        private MainWindow _mainWindow;
        private int _teacherId;


        public TeacherDashboard(MainWindow mainWindow, int teacherId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _teacherId = teacherId;
        }

        private void ViewClasses_Click(object sender, RoutedEventArgs e)
        {
            // Load the ViewClassesControl into the main content area
            _mainWindow.NavigateTo(new ViewClassesControl(_mainWindow, _teacherId));

        }

        private void ManageStudents_Click(object sender, RoutedEventArgs e)
        {
            // Load the ManageStudentsControl into the main content area
            MainContent.Content = new ViewStudentsControl(_mainWindow, _teacherId);
        }

        private void Assignments_Click(object sender, RoutedEventArgs e)
        {
            // Load the AssignmentsControl into the main content area
            MainContent.Content = new ViewAssignmentsControl(_mainWindow, _teacherId);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the login screen
            _mainWindow.NavigateTo(new LoginControl(_mainWindow));
        }
    }
}
