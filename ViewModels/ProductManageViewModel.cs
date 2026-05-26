using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using MiniGoods.Inventory.WPF.DAL;
using MiniGoods.Inventory.WPF.Models;
using MiniGoods.Inventory.WPF.Services;

namespace MiniGoods.Inventory.WPF.ViewModels;

/// <summary>
/// 商品管理：列表、新增/编辑/删除、表单绑定，新增/改价时同步写入价格历史
/// </summary>
public class ProductManageViewModel : ViewModelBase
{
    private readonly ProductDAL      _dal            = new();
    private readonly PriceHistoryDAL _priceHistoryDal = new();
    private readonly IMessageService _msg;

    private string   _name     = "";
    private string   _barcode  = "";
    private string   _category = "";
    private decimal  _price;
    private int      _stock;
    private byte[]?  _iconData;
    private Product? _selectedProduct;

    public ObservableCollection<Product> Products { get; } = new();

    public string  Name     { get => _name;     set => SetProperty(ref _name, value); }
    public string  Barcode  { get => _barcode;  set => SetProperty(ref _barcode, value); }
    public string  Category { get => _category; set => SetProperty(ref _category, value); }
    public decimal Price    { get => _price;    set => SetProperty(ref _price, value); }
    public int     Stock    { get => _stock;    set => SetProperty(ref _stock, value); }

    /// <summary>表单区当前图标字节（null = 未设置）</summary>
    public byte[]? IconData { get => _iconData; set => SetProperty(ref _iconData, value); }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value) && value != null)
            {
                Name     = value.Name;
                Barcode  = value.Barcode;
                Category = value.Category;
                Price    = value.Price;
                Stock    = value.Stock;
                IconData = value.IconData;
            }
        }
    }

    public ICommand LoadCommand       { get; }
    public ICommand AddCommand        { get; }
    public ICommand UpdateCommand     { get; }
    public ICommand DeleteCommand     { get; }
    public ICommand ClearFormCommand  { get; }
    public ICommand UploadIconCommand { get; }
    public ICommand ClearIconCommand  { get; }

    public ProductManageViewModel(IMessageService msg)
    {
        _msg = msg;
        LoadCommand       = new RelayCommand(_ => Load());
        AddCommand        = new RelayCommand(_ => Add());
        UpdateCommand     = new RelayCommand(_ => Update());
        DeleteCommand     = new RelayCommand(_ => Delete());
        ClearFormCommand  = new RelayCommand(_ => ClearForm());
        UploadIconCommand = new RelayCommand(_ => UploadIcon());
        ClearIconCommand  = new RelayCommand(_ => IconData = null);
    }

    private void Load()
    {
        Products.Clear();
        foreach (var p in _dal.GetAll())
            Products.Add(p);
    }

    private void Add()
    {
        if (string.IsNullOrWhiteSpace(Name)) { _msg.ShowError("商品名称不能为空"); return; }
        if (Price < 0)                       { _msg.ShowError("价格不能为负数");   return; }
        if (Stock < 0)                       { _msg.ShowError("库存不能为负数");   return; }
        try
        {
            var p = new Product
            {
                Name     = Name.Trim(),
                Barcode  = Barcode.Trim(),
                Category = Category.Trim(),
                Price    = Price,
                Stock    = Stock,
                IconData = IconData,
            };
            int newId = _dal.Insert(p);
            _priceHistoryDal.Insert(newId, Price);
            _msg.ShowSuccess($"商品「{p.Name}」添加成功");
            ClearForm();
            Load();
        }
        catch (Exception ex)
        {
            _msg.ShowError(ex.Message);
        }
    }

    private void Update()
    {
        if (SelectedProduct == null) { _msg.ShowError("请先在列表中选择要修改的商品"); return; }
        if (string.IsNullOrWhiteSpace(Name)) { _msg.ShowError("商品名称不能为空"); return; }
        if (Price < 0)                       { _msg.ShowError("价格不能为负数");   return; }
        if (Stock < 0)                       { _msg.ShowError("库存不能为负数");   return; }

        bool priceChanged = SelectedProduct.Price != Price;

        var p = new Product
        {
            ProductID = SelectedProduct.ProductID,
            Name      = Name.Trim(),
            Barcode   = Barcode.Trim(),
            Category  = Category.Trim(),
            Price     = Price,
            Stock     = Stock,
            IconData  = IconData,
        };
        _dal.Update(p);

        if (priceChanged)
            _priceHistoryDal.Insert(p.ProductID, Price);

        _msg.ShowSuccess($"商品「{p.Name}」修改成功");
        ClearForm();
        Load();
    }

    private void Delete()
    {
        if (SelectedProduct == null) { _msg.ShowError("请先在列表中选择要删除的商品"); return; }
        var name = SelectedProduct.Name;
        var id   = SelectedProduct.ProductID;
        try
        {
            _dal.Delete(id);
            ClearForm();
            Load();
            _msg.ShowSuccess($"商品「{name}」已删除");
        }
        catch (Exception ex)
        {
            _msg.ShowError($"删除失败：{ex.Message}");
        }
    }

    private void ClearForm()
    {
        SelectedProduct = null;
        Name = Barcode = Category = "";
        Price    = 0;
        Stock    = 0;
        IconData = null;
    }

    /// <summary>打开文件对话框，读取图片为字节数组</summary>
    private void UploadIcon()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "选择商品图标",
            Filter = "图像文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|所有文件|*.*",
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            IconData = File.ReadAllBytes(dlg.FileName);
        }
        catch (Exception ex)
        {
            _msg.ShowError($"读取图片失败：{ex.Message}");
        }
    }
}
