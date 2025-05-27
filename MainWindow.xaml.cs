using System.Windows;
using System.Windows.Controls;
using StudentCanvasApp.Controls;

namespace StudentCanvasApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigateTo(new LoginControl(this));
        }

        public void NavigateTo(UserControl nextControl)
        {
            MainContent.Content = nextControl;
        }
    }
}
