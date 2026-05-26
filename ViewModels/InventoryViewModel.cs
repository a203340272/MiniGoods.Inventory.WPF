using System.Collections.ObjectModel;
using System.Windows.Input;
using MiniGoods.Inventory.WPF.DAL;

namespace MiniGoods.Inventory.WPF.ViewModels;

/// <summary>
/// 库存列表，低阈值（如 10）高亮
/// </summary>
public class InventoryViewModel : ViewModelBase
{
    private readonly ProductDAL _dal = new();
    public const int LowStockThreshold = 10;

    public ObservableCollection<InventoryItemViewModel> Items { get; } = new();

    public ICommand LoadCommand { get; }

    public InventoryViewModel()
    {
        LoadCommand = new RelayCommand(_ => Load());
    }

    public void Load()
    {
        Items.Clear();
        foreach (var p in _dal.GetAll())
            Items.Add(new InventoryItemViewModel(p.ProductID, p.Name, p.Barcode, p.Category, p.Price, p.Stock, p.Stock <= LowStockThreshold));
    }
}

public class InventoryItemViewModel : ViewModelBase
{
    public int ProductID { get; }
    public string Name { get; }
    public string Barcode { get; }
    public string Category { get; }
    public decimal Price { get; }
    public int Stock { get; }
    public bool IsLowStock { get; }

    public InventoryItemViewModel(int productId, string name, string barcode, string category, decimal price, int stock, bool isLowStock)
    {
        ProductID = productId;
        Name = name;
        Barcode = barcode;
        Category = category;
        Price = price;
        Stock = stock;
        IsLowStock = isLowStock;
    }
}
