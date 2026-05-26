using System.Windows;

namespace MiniGoods.Inventory.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ((App)Application.Current).MainViewModel;
    }
}
