using System.Windows.Controls;

namespace MiniGoods.Inventory.WPF.Views;

public partial class VoiceQueryView : UserControl
{
    public VoiceQueryView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is ViewModels.VoiceQueryViewModel vm)
                vm.SearchByTextCommand.Execute(null);
        };
    }
}
