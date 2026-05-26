using System.Collections.ObjectModel;
using System.Windows.Input;
using MiniGoods.Inventory.WPF.DAL;
using MiniGoods.Inventory.WPF.Models;
using MiniGoods.Inventory.WPF.Services;

namespace MiniGoods.Inventory.WPF.ViewModels;

/// <summary>
/// 上货/入库：商品列表、选中商品、进货数量、确认入库
/// </summary>
public class ReceivingViewModel : ViewModelBase
{
    private readonly ProductDAL _productDal = new();
    private readonly StockHistoryDAL _stockHistoryDal = new();
    private readonly IMessageService _msg;

    private Product? _selectedProduct;
    private int _receiveQuantity = 1;

    public ObservableCollection<Product> Products { get; } = new();
    public Product? SelectedProduct { get => _selectedProduct; set => SetProperty(ref _selectedProduct, value); }
    public int ReceiveQuantity { get => _receiveQuantity; set => SetProperty(ref _receiveQuantity, value); }

    public ICommand LoadCommand { get; }
    public ICommand ConfirmReceiveCommand { get; }

    public ReceivingViewModel(IMessageService msg)
    {
        _msg = msg;
        LoadCommand = new RelayCommand(_ => Load());
        ConfirmReceiveCommand = new RelayCommand(_ => ConfirmReceive());
    }

    private void Load()
    {
        Products.Clear();
        foreach (var p in _productDal.GetAll())
            Products.Add(p);
    }

    private void ConfirmReceive()
    {
        if (SelectedProduct == null) { _msg.ShowError("请先选择商品"); return; }
        if (ReceiveQuantity <= 0) { _msg.ShowError("入库数量必须大于 0"); return; }
        int newStock = SelectedProduct.Stock + ReceiveQuantity;
        _productDal.UpdateStock(SelectedProduct.ProductID, newStock);
        _stockHistoryDal.Insert(SelectedProduct.ProductID, ReceiveQuantity);
        _msg.ShowSuccess($"已入库 {ReceiveQuantity} 件，当前库存 {newStock}");
        ReceiveQuantity = 1;
        Load();
    }
}
