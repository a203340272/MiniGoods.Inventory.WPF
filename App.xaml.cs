using System.Windows;
using MiniGoods.Inventory.WPF.DAL;
using MiniGoods.Inventory.WPF.Services;
using MiniGoods.Inventory.WPF.ViewModels;

namespace MiniGoods.Inventory.WPF;

public partial class App : Application
{
    public MainViewModel MainViewModel { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DatabaseHelper.InitDatabase();

        IMessageService messageService = new MessageService();
        IVoiceRecognitionService voiceService = new WindowsVoiceRecognitionService();

        var productManageVm = new ProductManageViewModel(messageService);
        var receivingVm = new ReceivingViewModel(messageService);
        var inventoryVm = new InventoryViewModel();
        var priceHistoryVm = new PriceHistoryViewModel(messageService, voiceService);
        var voiceQueryVm = new VoiceQueryViewModel(messageService, voiceService);

        MainViewModel = new MainViewModel(productManageVm, receivingVm, inventoryVm, priceHistoryVm, voiceQueryVm);
    }
}
