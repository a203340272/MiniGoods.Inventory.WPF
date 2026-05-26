using Microsoft.Data.Sqlite;
using MiniGoods.Inventory.WPF.Models;

namespace MiniGoods.Inventory.WPF.DAL;

/// <summary>
/// 价格历史数据访问（含售价 / 市均价 / 进价三种价格）
/// </summary>
public class PriceHistoryDAL
{
    /// <summary>
    /// 新增一条历史记录。changeDate 为 null 时自动取当前时间（兼容旧调用方）。
    /// </summary>
    public void Insert(int productId, decimal price,
                       decimal marketPrice = 0, decimal costPrice = 0,
                       DateTime? changeDate = null)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "INSERT INTO PriceHistory (ProductID, Price, MarketPrice, CostPrice, ChangeDate) " +
            "VALUES (@pid, @price, @mp, @cp, @date)";
        cmd.Parameters.AddWithValue("@pid",   productId);
        cmd.Parameters.AddWithValue("@price", price);
        cmd.Parameters.AddWithValue("@mp",    marketPrice);
        cmd.Parameters.AddWithValue("@cp",    costPrice);
        cmd.Parameters.AddWithValue("@date",  (changeDate ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    /// <summary>按 HistoryID 删除单条历史记录</summary>
    public void Delete(int historyId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM PriceHistory WHERE HistoryID = @id";
        cmd.Parameters.AddWithValue("@id", historyId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>更新已有历史记录（供 DataGrid 内联编辑后保存）</summary>
    public void Update(PriceHistoryItem item)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "UPDATE PriceHistory SET Price=@price, MarketPrice=@mp, CostPrice=@cp, ChangeDate=@date " +
            "WHERE HistoryID=@id";
        cmd.Parameters.AddWithValue("@id",    item.HistoryID);
        cmd.Parameters.AddWithValue("@price", item.Price);
        cmd.Parameters.AddWithValue("@mp",    item.MarketPrice);
        cmd.Parameters.AddWithValue("@cp",    item.CostPrice);
        cmd.Parameters.AddWithValue("@date",  item.ChangeDate.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    public List<PriceHistoryItem> GetByProductId(int productId)
    {
        var list = new List<PriceHistoryItem>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT HistoryID, ProductID, Price, MarketPrice, CostPrice, ChangeDate " +
            "FROM PriceHistory WHERE ProductID = @pid ORDER BY ChangeDate";
        cmd.Parameters.AddWithValue("@pid", productId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new PriceHistoryItem
            {
                HistoryID   = r.GetInt32(0),
                ProductID   = r.GetInt32(1),
                Price       = r.GetDecimal(2),
                MarketPrice = r.IsDBNull(3) ? 0 : r.GetDecimal(3),
                CostPrice   = r.IsDBNull(4) ? 0 : r.GetDecimal(4),
                ChangeDate  = DateTime.Parse(r.GetString(5))
            });
        }
        return list;
    }
}
