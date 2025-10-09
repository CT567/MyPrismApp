using PrismDemo.ViewModels;
using System.Windows;

namespace PrismDemo.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm; //绑定 ViewModel
        }
    }
}
