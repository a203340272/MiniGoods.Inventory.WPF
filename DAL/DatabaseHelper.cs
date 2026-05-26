using System.IO;
using Microsoft.Data.Sqlite;

namespace MiniGoods.Inventory.WPF.DAL;

/// <summary>
/// SQLite 数据库连接与初始化（建表 + 示例数据）
/// </summary>
public static class DatabaseHelper
{
    private static string DbPath       => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "minigoods.db");
    public  static string ConnectionString => $"Data Source={DbPath}";

    public static SqliteConnection GetConnection() => new(ConnectionString);

    public static void InitDatabase()
    {
        using var conn = GetConnection();
        conn.Open();

        //启动外键
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();

        // 商品表
        ExecuteNonQuery(conn, @"
            CREATE TABLE IF NOT EXISTS Products (
                ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                Name      TEXT    NOT NULL,
                Barcode   TEXT UNIQUE,
                Category  TEXT,
                Price     REAL    NOT NULL DEFAULT 0,
                Stock     INTEGER NOT NULL DEFAULT 0
            );");

        // 价格历史表
        ExecuteNonQuery(conn, @"
            CREATE TABLE IF NOT EXISTS PriceHistory (
                HistoryID   INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductID   INTEGER NOT NULL,
                Price       REAL    NOT NULL,
                MarketPrice REAL    NOT NULL DEFAULT 0,
                CostPrice   REAL    NOT NULL DEFAULT 0,
                ChangeDate  TEXT    NOT NULL,
                FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
            );");

        // 兼容旧数据库：按需添加新列（列已存在时静默忽略）
        TryAddColumn(conn, "PriceHistory", "MarketPrice", "REAL NOT NULL DEFAULT 0");
        TryAddColumn(conn, "PriceHistory", "CostPrice",   "REAL NOT NULL DEFAULT 0");
        TryAddColumn(conn, "Products",     "IconData",    "BLOB");

        // 库存历史表
        ExecuteNonQuery(conn, @"
            CREATE TABLE IF NOT EXISTS StockHistory (
                HistoryID  INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductID  INTEGER NOT NULL,
                Quantity   INTEGER NOT NULL,
                ChangeDate TEXT    NOT NULL,
                FOREIGN KEY (ProductID) REFERENCES Products(ProductID) 
            );");

        // 插入示例数据（仅当 Products 表为空时）
        using var countCmd = conn.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM Products";
        if (Convert.ToInt32(countCmd.ExecuteScalar()) > 0) return;

        SeedProducts(conn);
    }

    /// <summary>
    /// 插入示例商品及历史价格数据
    /// </summary>
    private static void SeedProducts(SqliteConnection conn)
    {
        // 商品列表：(名称, 条码, 分类, 当前价, 库存, 历史价格列表[30天前→15天前→现在])
        var products = new[]
        {
            ("可口可乐 330ml",    "6901234567890", "饮料",   3.50m, 120, new[] { 3.00m, 3.20m, 3.50m }),
            ("康师傅红烧牛肉面", "6901234567891", "方便食品", 4.00m,  80, new[] { 3.50m, 3.80m, 4.00m }),
            ("农夫山泉 550ml",    "6901234567892", "饮料",   2.00m, 200, new[] { 1.80m, 1.90m, 2.00m }),
            ("奥利奥饼干",        "6901234567893", "零食",   9.90m,  50, new[] { 8.80m, 9.50m, 9.90m }),
            ("伊利纯牛奶 250ml", "6901234567894", "乳品",   3.20m,  60, new[] { 2.90m, 3.00m, 3.20m }),
            ("洗洁精 500ml",      "6901234567895", "日用品", 6.80m,   8, new[] { 6.00m, 6.50m, 6.80m }),
            ("牙刷（软毛）",      "6901234567896", "日用品", 12.0m,   5, new[] { 10.0m, 11.0m, 12.0m }),
            ("卫龙辣条",          "6901234567897", "零食",   2.50m, 300, new[] { 2.00m, 2.20m, 2.50m }),
        };

        var now = DateTime.Now;

        foreach (var (name, barcode, category, price, stock, history) in products)
        {
            // 插入商品
            using var insertProd = conn.CreateCommand();
            insertProd.CommandText =
                "INSERT INTO Products (Name, Barcode, Category, Price, Stock) " +
                "VALUES (@name, @barcode, @cat, @price, @stock); SELECT last_insert_rowid();";
            insertProd.Parameters.AddWithValue("@name",    name);
            insertProd.Parameters.AddWithValue("@barcode", barcode);
            insertProd.Parameters.AddWithValue("@cat",     category);
            insertProd.Parameters.AddWithValue("@price",   price);
            insertProd.Parameters.AddWithValue("@stock",   stock);
            var productId = Convert.ToInt32(insertProd.ExecuteScalar());

            // 插入阶梯价格历史（30天前 → 15天前 → 今天）
            var offsets = new[] { -30, -15, 0 };
            for (int i = 0; i < history.Length && i < offsets.Length; i++)
            {
                using var insertHist = conn.CreateCommand();
                insertHist.CommandText =
                    "INSERT INTO PriceHistory (ProductID, Price, ChangeDate) VALUES (@pid, @price, @date)";
                insertHist.Parameters.AddWithValue("@pid",   productId);
                insertHist.Parameters.AddWithValue("@price", history[i]);
                insertHist.Parameters.AddWithValue("@date",  now.AddDays(offsets[i]).ToString("yyyy-MM-dd HH:mm:ss"));
                insertHist.ExecuteNonQuery();
            }
        }
    }

    private static void ExecuteNonQuery(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 若列不存在则添加，已存在时静默跳过（旧数据库迁移）
    /// </summary>
    private static void TryAddColumn(SqliteConnection conn, string table, string column, string definition)
    {
        try
        {
            ExecuteNonQuery(conn, $"ALTER TABLE {table} ADD COLUMN {column} {definition}");
        }
        catch { /* 列已存在，忽略 */ }
    }
}
