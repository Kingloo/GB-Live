using System.Windows;

namespace GB_Live
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += async (s, e) => await vm.UpdateAsync();
        }
    }
}
