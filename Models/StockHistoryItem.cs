namespace MiniGoods.Inventory.WPF.Models;

/// <summary>
/// 库存历史记录
/// </summary>
public class StockHistoryItem
{
    public int HistoryID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public DateTime ChangeDate { get; set; }
}
