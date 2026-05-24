using System.Windows;

namespace EndOfDayTime.Sample.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ShiftViewModel();
        }
    }
}