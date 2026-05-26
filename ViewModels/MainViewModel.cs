using System.Windows;
using System.Windows.Input;
using MiniGoods.Inventory.WPF.Views;

namespace MiniGoods.Inventory.WPF.ViewModels;

/// <summary>
/// 主窗口：左侧菜单切换，右侧显示当前子页面
/// </summary>
public class MainViewModel : ViewModelBase
{
    private object? _currentView;

    public object? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public ICommand ShowProductManageCommand { get; }
    public ICommand ShowReceivingCommand { get; }
    public ICommand ShowInventoryCommand { get; }
    public ICommand ShowPriceHistoryCommand { get; }
    public ICommand ShowVoiceQueryCommand { get; }

    public MainViewModel(
        ProductManageViewModel productManageVm,
        ReceivingViewModel receivingVm,
        InventoryViewModel inventoryVm,
        PriceHistoryViewModel priceHistoryVm,
        VoiceQueryViewModel voiceQueryVm)
    {
        ShowProductManageCommand = new RelayCommand(_ => CurrentView = new ProductManageView { DataContext = productManageVm });
        ShowReceivingCommand = new RelayCommand(_ => CurrentView = new ReceivingView { DataContext = receivingVm });
        ShowInventoryCommand = new RelayCommand(_ => CurrentView = new InventoryView { DataContext = inventoryVm });
        ShowPriceHistoryCommand = new RelayCommand(_ => CurrentView = new PriceHistoryView { DataContext = priceHistoryVm });
        ShowVoiceQueryCommand = new RelayCommand(_ => CurrentView = new VoiceQueryView { DataContext = voiceQueryVm });

        CurrentView = new ProductManageView { DataContext = productManageVm };
    }
}

/// <summary>
/// 简单 RelayCommand
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
