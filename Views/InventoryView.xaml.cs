using System.Windows.Controls;

namespace MiniGoods.Inventory.WPF.Views;

public partial class InventoryView : UserControl
{
    public InventoryView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is ViewModels.InventoryViewModel vm)
                vm.LoadCommand.Execute(null);
        };
    }
}
