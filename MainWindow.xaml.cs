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
            if (role == "student")
            {
                NavigateTo(new StudentDashboard(this, LoginEmail));
            }
            else
            {
                MessageBox.Show($"Dashboard for role '{role}' not implemented yet.");
            }
        }
        public string LoginEmail { get; set; }
    }
}

