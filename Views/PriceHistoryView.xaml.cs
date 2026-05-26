using System.Windows.Controls;

namespace MiniGoods.Inventory.WPF.Views;

public partial class PriceHistoryView : UserControl
{
    public PriceHistoryView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is ViewModels.PriceHistoryViewModel vm)
                vm.LoadCommand.Execute(null);
        };
    }
}
