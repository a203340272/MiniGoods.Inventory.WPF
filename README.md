# 迷你上货 · 单机库存管理系统

> C# + .NET 8 + WPF + SQLite + MVVM 架构，完全离线单机运行

---

## 项目结构

```
MiniGoods.Inventory.WPF/
├── App.xaml / App.xaml.cs              # 应用入口：初始化数据库、注入服务与 ViewModel；全局 UI 样式
├── MainWindow.xaml / .cs               # 主窗口：左侧导航菜单 + 右侧 ContentControl
│
├── Models/                             # 数据模型层（纯 POCO）
│   ├── Product.cs                      # 商品：ProductID, Name, Barcode, Category, Price, Stock
│   ├── PriceHistoryItem.cs             # 价格历史：HistoryID, ProductID, Price, ChangeDate
│   └── StockHistoryItem.cs             # 库存历史：HistoryID, ProductID, Quantity, ChangeDate
│
├── DAL/                                # 数据访问层（所有 SQL 操作集中于此，View 不直接访问数据库）
│   ├── DatabaseHelper.cs               # 连接字符串、InitDatabase() 建表、示例数据（含阶梯价格历史）
│   ├── ProductDAL.cs                   # 商品 CRUD + UpdateStock + SearchByName
│   ├── PriceHistoryDAL.cs              # 价格历史 Insert + GetByProductId
│   └── StockHistoryDAL.cs              # 库存历史 Insert + GetByProductId + GetAll
│
├── Services/                           # 服务层
│   ├── IMessageService.cs              # 消息提示接口
│   ├── MessageService.cs               # MessageBox 实现
│   ├── IVoiceRecognitionService.cs     # 语音识别接口
│   └── WindowsVoiceRecognitionService.cs  # System.Speech 中文语音识别
│
├── ViewModels/                         # MVVM 视图模型
│   ├── ViewModelBase.cs                # INotifyPropertyChanged 基类 + SetProperty
│   ├── MainViewModel.cs                # 菜单切换、CurrentView、RelayCommand 定义
│   ├── ProductManageViewModel.cs       # 商品管理（新增/修改/删除），改价时自动写价格历史
│   ├── ReceivingViewModel.cs           # 上货入库，更新库存 + 写库存历史
│   ├── InventoryViewModel.cs           # 库存总览（InventoryItemViewModel 含 IsLowStock）
│   ├── PriceHistoryViewModel.cs        # 价格历史 + LiveCharts 折线图（含 X 轴日期标签）
│   └── VoiceQueryViewModel.cs          # 语音查询：语音识别→文本清洗→模糊匹配
│
├── Views/                              # WPF 视图（UserControl）
│   ├── ProductManageView.xaml/.cs      # 商品管理页
│   ├── ReceivingView.xaml/.cs          # 上货入库页
│   ├── InventoryView.xaml/.cs          # 库存总览页（低库存红色高亮 + 状态标签）
│   ├── PriceHistoryView.xaml/.cs       # 价格历史页（折线图 + 记录表 + 改价栏）
│   └── VoiceQueryView.xaml/.cs         # 语音查询页（可语音也可手动输入）
│
├── Converters/
│   └── InverseBooleanConverter.cs      # 布尔取反（语音按钮禁用状态）
│
└── README.md
```

---

## 数据库表结构

| 表名           | 字段说明 |
|---------------|---------|
| **Products**  | `ProductID`（主键 AUTOINCREMENT）, `Name`, `Barcode`, `Category`, `Price`, `Stock` |
| **PriceHistory** | `HistoryID`（主键）, `ProductID`（外键）, `Price`, `ChangeDate` |
| **StockHistory** | `HistoryID`（主键）, `ProductID`（外键）, `Quantity`, `ChangeDate` |

数据库文件自动创建于程序运行目录：`minigoods.db`

---

## 如何运行

### 前置条件

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)（x64，Windows）
- Visual Studio 2022+ 或 VS Code + C# 扩展
- **语音查询**需确保 Windows 已安装中文语音识别语言包（控制面板 → 语音识别）

### 步骤

```bash
# 克隆/进入项目目录
cd MiniGoods.Inventory.WPF

# 还原包
dotnet restore

# 构建
dotnet build

# 运行
dotnet run
```

或在 Visual Studio 中打开 `MiniGoods.Inventory.WPF.sln`，按 **F5** 运行。

首次运行会自动创建 `minigoods.db` 并插入 8 种示例商品及阶梯价格历史数据。

---

## 功能说明

### 📦 商品管理
- DataGrid 展示商品列表，点击行自动填充右侧表单
- 支持**新增**（同时写入初始价格历史）、**修改**（价格变动自动记录价格历史）、**删除**
- 字段校验：名称不能为空，价格/库存不能为负

### 🚚 上货入库
- 列表选中商品后填写入库数量，点击「确认入库」
- 自动累加库存 (`UpdateStock`) 并向 `StockHistory` 写入一条记录

### 📊 库存总览
- 展示所有商品当前库存
- 库存 ≤ 10 件：数量**红色加粗**，整行浅红背景，状态列显示「库存不足」标签

### 💰 价格历史
- 选择商品后，表格展示历史价格记录，折线图（LiveCharts2）显示价格走势
- X 轴为日期标签（30天前、15天前、今天示例数据），Y 轴显示价格（¥）
- 底部操作栏可修改当前价格，变动后实时刷新图表

### 🎤 语音查询
1. 点击「🎙️ 开始语音查询」，系统调用 Windows 中文语音识别
2. 识别完成后文本写入搜索框，自动清洗关键词（去除"多少钱""价格"等词）
3. 模糊匹配商品名称（SQLite `LIKE %keyword%`），结果展示在下方列表
4. **也可直接在搜索框输入文字**，点击「🔍 按文字搜索」进行查询
5. 语音识别失败时有错误提示，不影响手动搜索功能

---

## 依赖包

| 包名 | 版本 | 用途 |
|-----|------|------|
| `Microsoft.Data.Sqlite` | 8.0.0 | SQLite 数据库访问 |
| `LiveChartsCore.SkiaSharpView.WPF` | 2.0.0-rc2 | 价格折线图 |
| `System.Speech` | 8.0.0 | Windows 语音识别 |

---

## 架构说明

```
View  ──绑定──▶  ViewModel  ──调用──▶  DAL  ──操作──▶  SQLite
       (XAML)    (INotifyPropertyChanged)   (ADO.NET)
                      │
                      └──调用──▶  Services（消息提示 / 语音识别）
```

- **View** 不直接操作数据库，只通过 Command/Binding 与 ViewModel 交互
- **DAL** 所有 SQL 集中管理，使用参数化查询防止注入
- **Services** 接口化设计，便于后续替换实现（如接入云端语音识别）
