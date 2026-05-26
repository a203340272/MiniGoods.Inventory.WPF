using System.Windows.Controls;

namespace MiniGoods.Inventory.WPF.Views;

public partial class ReceivingView : UserControl
{
    public ReceivingView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is ViewModels.ReceivingViewModel vm)
                vm.LoadCommand.Execute(null);
        };
    }
}
