using Microsoft.Data.Sqlite;
using MiniGoods.Inventory.WPF.Models;

namespace MiniGoods.Inventory.WPF.DAL;

/// <summary>
/// 库存历史数据访问
/// </summary>
public class StockHistoryDAL
{
    public void Insert(int productId, int quantity)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO StockHistory (ProductID, Quantity, ChangeDate) VALUES (@pid, @qty, @date)";
        cmd.Parameters.AddWithValue("@pid", productId);
        cmd.Parameters.AddWithValue("@qty", quantity);
        cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    public List<StockHistoryItem> GetByProductId(int productId)
    {
        var list = new List<StockHistoryItem>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT HistoryID, ProductID, Quantity, ChangeDate FROM StockHistory WHERE ProductID = @pid ORDER BY ChangeDate DESC";
        cmd.Parameters.AddWithValue("@pid", productId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new StockHistoryItem
            {
                HistoryID = r.GetInt32(0),
                ProductID = r.GetInt32(1),
                Quantity = r.GetInt32(2),
                ChangeDate = DateTime.Parse(r.GetString(3))
            });
        }
        return list;
    }

    public List<StockHistoryItem> GetAll()
    {
        var list = new List<StockHistoryItem>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT HistoryID, ProductID, Quantity, ChangeDate FROM StockHistory ORDER BY ChangeDate DESC";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new StockHistoryItem
            {
                HistoryID = r.GetInt32(0),
                ProductID = r.GetInt32(1),
                Quantity = r.GetInt32(2),
                ChangeDate = DateTime.Parse(r.GetString(3))
            });
        }
        return list;
    }
}
