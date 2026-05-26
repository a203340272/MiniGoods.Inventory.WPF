namespace MiniGoods.Inventory.WPF.Models;

/// <summary>
/// 商品模型
/// </summary>
public class Product
{
    public int     ProductID { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public string  Barcode   { get; set; } = string.Empty;
    public string  Category  { get; set; } = string.Empty;
    public decimal Price     { get; set; }
    public int     Stock     { get; set; }
    /// <summary>商品图标，以 BLOB 形式存入数据库，为 null 时表示未设置</summary>
    public byte[]? IconData  { get; set; }
}
