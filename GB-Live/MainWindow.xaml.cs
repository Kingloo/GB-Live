using System.Windows;

namespace GB_Live
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += async (sender, e) => await vm.UpdateAllAsync();
        }
    }
}
