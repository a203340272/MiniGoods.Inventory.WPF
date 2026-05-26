using Microsoft.Data.Sqlite;
using MiniGoods.Inventory.WPF.Models;

namespace MiniGoods.Inventory.WPF.DAL;

/// <summary>
/// 商品数据访问（含图标 BLOB 字段）
/// </summary>
public class ProductDAL
{
    // ── 私有辅助：从 reader 读取 IconData BLOB ─────────────────────────────
    private static byte[]? ReadIconData(SqliteDataReader r, int ordinal)
    {
        if (r.IsDBNull(ordinal)) return null;
        var raw = r.GetValue(ordinal);
        return raw as byte[];
    }

    // ── 查询 ──────────────────────────────────────────────────────────────
    public List<Product> GetAll()
    {
        var list = new List<Product>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT ProductID, Name, Barcode, Category, Price, Stock, IconData " +
            "FROM Products ORDER BY ProductID";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(MapRow(r));
        return list;
    }

    public Product? GetById(int productId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT ProductID, Name, Barcode, Category, Price, Stock, IconData " +
            "FROM Products WHERE ProductID = @id";
        cmd.Parameters.AddWithValue("@id", productId);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapRow(r) : null;
    }

    public List<Product> SearchByName(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return GetAll();
        var list = new List<Product>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT ProductID, Name, Barcode, Category, Price, Stock, IconData " +
            "FROM Products WHERE Name LIKE @kw ORDER BY ProductID";
        cmd.Parameters.AddWithValue("@kw", "%" + keyword.Trim() + "%");
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(MapRow(r));
        return list;
    }

    private static Product MapRow(SqliteDataReader r) => new()
    {
        ProductID = r.GetInt32(0),
        Name      = r.GetString(1),
        Barcode   = r.IsDBNull(2) ? "" : r.GetString(2),
        Category  = r.IsDBNull(3) ? "" : r.GetString(3),
        Price     = r.GetDecimal(4),
        Stock     = r.GetInt32(5),
        IconData  = ReadIconData(r, 6),
    };

    // ── 写入 ──────────────────────────────────────────────────────────────
    public bool BarcodeExists(string barcode)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Products WHERE Barcode=@barcode";
        cmd.Parameters.AddWithValue("@barcode", barcode);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int Insert(Product p)
    {
        if (!string.IsNullOrEmpty(p.Barcode) && BarcodeExists(p.Barcode))
            throw new Exception("该条码已存在");

        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "INSERT INTO Products (Name, Barcode, Category, Price, Stock, IconData) " +
            "VALUES (@name, @barcode, @cat, @price, @stock, @icon); " +
            "SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@name",    p.Name ?? "");
        cmd.Parameters.AddWithValue("@barcode", p.Barcode ?? "");
        cmd.Parameters.AddWithValue("@cat",     p.Category ?? "");
        cmd.Parameters.AddWithValue("@price",   p.Price);
        cmd.Parameters.AddWithValue("@stock",   p.Stock);
        AddIconParam(cmd, "@icon", p.IconData);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int Update(Product p)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "UPDATE Products SET Name=@name, Barcode=@barcode, Category=@cat, " +
            "Price=@price, Stock=@stock, IconData=@icon WHERE ProductID=@id";
        cmd.Parameters.AddWithValue("@id",      p.ProductID);
        cmd.Parameters.AddWithValue("@name",    p.Name ?? "");
        cmd.Parameters.AddWithValue("@barcode", p.Barcode ?? "");
        cmd.Parameters.AddWithValue("@cat",     p.Category ?? "");
        cmd.Parameters.AddWithValue("@price",   p.Price);
        cmd.Parameters.AddWithValue("@stock",   p.Stock);
        AddIconParam(cmd, "@icon", p.IconData);
        return cmd.ExecuteNonQuery();
    }

    public int Delete(int productId)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Products WHERE ProductID = @id";
        cmd.Parameters.AddWithValue("@id", productId);
        return cmd.ExecuteNonQuery();
    }

    public int UpdateStock(int productId, int newStock)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Products SET Stock = @stock WHERE ProductID = @id";
        cmd.Parameters.AddWithValue("@stock", newStock);
        cmd.Parameters.AddWithValue("@id",    productId);
        return cmd.ExecuteNonQuery();
    }

    // ── 辅助 ──────────────────────────────────────────────────────────────
    /// <summary>BLOB 为 null 时写 DBNull，否则写字节数组</summary>
    private static void AddIconParam(SqliteCommand cmd, string name, byte[]? data)
    {
        if (data is { Length: > 0 })
            cmd.Parameters.AddWithValue(name, data);
        else
            cmd.Parameters.AddWithValue(name, DBNull.Value);
    }
}
