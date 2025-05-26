using StudentCanvasApp.Controls;
using System.Windows;
using System.Windows.Controls;

namespace StudentCanvasApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigateTo(new LoginControl(this));
        }

        public void NavigateTo(UserControl control)
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(control);
        }

        public void NavigateToRole(string role)
        {
            // You can create role-based dashboards here
            MessageBox.Show($"Loading {role} panel...");
            // Example: NavigateTo(new StudentDashboard(this));
        }
    }
}

