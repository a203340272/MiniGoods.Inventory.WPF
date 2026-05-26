using System.Windows.Controls;

namespace MiniGoods.Inventory.WPF.Views;

public partial class ProductManageView : UserControl
{
    public ProductManageView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is ViewModels.ProductManageViewModel vm)
                vm.LoadCommand.Execute(null);
        };
    }
}
