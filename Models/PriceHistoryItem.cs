using System.ComponentModel;

namespace MiniGoods.Inventory.WPF.Models;

/// <summary>
/// 价格历史记录（支持 DataGrid 直接内联编辑）
/// </summary>
public class PriceHistoryItem : INotifyPropertyChanged
{
    private decimal  _price;
    private decimal  _marketPrice;
    private decimal  _costPrice;
    private DateTime _changeDate;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Notify(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public int HistoryID { get; set; }
    public int ProductID { get; set; }

    /// <summary>售价</summary>
    public decimal Price
    {
        get => _price;
        set { _price = value; Notify(nameof(Price)); }
    }

    /// <summary>市均价</summary>
    public decimal MarketPrice
    {
        get => _marketPrice;
        set { _marketPrice = value; Notify(nameof(MarketPrice)); }
    }

    /// <summary>进价（成本价）</summary>
    public decimal CostPrice
    {
        get => _costPrice;
        set { _costPrice = value; Notify(nameof(CostPrice)); }
    }

    public DateTime ChangeDate
    {
        get => _changeDate;
        set
        {
            _changeDate = value;
            Notify(nameof(ChangeDate));
            Notify(nameof(ChangeDateDisplay));
        }
    }

    /// <summary>
    /// DataGrid 可编辑的日期字符串；赋值时自动解析回 ChangeDate
    /// </summary>
    public string ChangeDateDisplay
    {
        get => ChangeDate.ToString("yyyy-MM-dd HH:mm:ss");
        set
        {
            if (DateTime.TryParse(value, out var dt))
                ChangeDate = dt;
        }
    }
}
