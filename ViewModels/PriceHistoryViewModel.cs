using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using MiniGoods.Inventory.WPF.DAL;
using MiniGoods.Inventory.WPF.Models;
using MiniGoods.Inventory.WPF.Services;
using System.IO;
using LiveChartsCore.Drawing;

namespace MiniGoods.Inventory.WPF.ViewModels;

/// <summary>
/// 价格历史：语音/文字搜索商品、展示三价折线图（售价/市均价/进价）、
/// DataGrid 内联编辑历史条目、修改当前价格并写历史
/// </summary>
public class PriceHistoryViewModel : ViewModelBase
{
    // 使用支持中文的字体，修复 SkiaSharp 默认字体无法显示汉字的问题
    private static readonly SKTypeface ChineseTypeface =
    SKTypeface.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts/msyh.ttc"));
    /*private static readonly SKTypeface ChineseTypeface =*/
    /*    SKTypeface.FromFamilyName("Microsoft YaHei") ??
        SKTypeface.FromFamilyName("SimHei")          ??
        SKTypeface.Default;*/

    private readonly ProductDAL _productDal = new();
    private readonly PriceHistoryDAL _priceHistoryDal = new();
    private readonly IMessageService _msg;
    private readonly IVoiceRecognitionService _voice;

    private Product? _selectedProduct;
    private decimal  _newPrice;
    private decimal  _newMarketPrice;
    private decimal  _newCostPrice;
    private DateTime _newChangeDate    = DateTime.Now;
    private string   _newChangeTimeStr = DateTime.Now.ToString("HH:mm");
    private string   _searchText       = "";
    private bool     _isRecognizing;
    private ISeries[] _chartSeries  = Array.Empty<ISeries>();
    private Axis[]    _xAxes        = [new Axis { Name = "日期" }];
    private Axis[]    _yAxes        = [new Axis { Name = "价格（元）" }];

    public SolidColorPaint LegendTextPaint { get; set; }
    public SolidColorPaint TooltipTextPaint { get; set; }

    public ObservableCollection<Product>          Products     { get; } = new();
    public ObservableCollection<PriceHistoryItem> HistoryItems { get; } = new();

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value))
            {
                NewPrice       = value?.Price ?? 0;
                NewMarketPrice = 0;
                NewCostPrice   = 0;
                LoadHistoryAndChart();
            }
        }
    }

    public decimal NewPrice
    {
        get => _newPrice;
        set => SetProperty(ref _newPrice, value);
    }

    public decimal NewMarketPrice
    {
        get => _newMarketPrice;
        set => SetProperty(ref _newMarketPrice, value);
    }

    public decimal NewCostPrice
    {
        get => _newCostPrice;
        set => SetProperty(ref _newCostPrice, value);
    }

    /// <summary>新增记录的日期部分，绑定 DatePicker.SelectedDate</summary>
    public DateTime NewChangeDate
    {
        get => _newChangeDate;
        set => SetProperty(ref _newChangeDate, value);
    }

    /// <summary>新增记录的时间部分（HH:mm），绑定文字输入框</summary>
    public string NewChangeTimeStr
    {
        get => _newChangeTimeStr;
        set => SetProperty(ref _newChangeTimeStr, value);
    }

    public string SearchText    { get => _searchText;    set => SetProperty(ref _searchText, value); }
    public bool   IsRecognizing { get => _isRecognizing; set => SetProperty(ref _isRecognizing, value); }

    public ISeries[] ChartSeries
    {
        get => _chartSeries;
        private set { _chartSeries = value; RaisePropertyChanged(); }
    }

    public Axis[] XAxes
    {
        get => _xAxes;
        private set { _xAxes = value; RaisePropertyChanged(); }
    }

    public Axis[] YAxes
    {
        get => _yAxes;
        private set { _yAxes = value; RaisePropertyChanged(); }
    }

    public ICommand LoadCommand              { get; }
    public ICommand UpdatePriceCommand       { get; }
    public ICommand SaveChangesCommand       { get; }
    public ICommand DeleteHistoryItemCommand { get; }
    public ICommand StartVoiceCommand        { get; }
    public ICommand SearchByTextCommand      { get; }

    public PriceHistoryViewModel(IMessageService msg, IVoiceRecognitionService voice)
    {
        _msg   = msg;
        _voice = voice;
        LoadCommand              = new RelayCommand(_ => LoadProducts());
        UpdatePriceCommand       = new RelayCommand(_ => UpdatePrice());
        SaveChangesCommand       = new RelayCommand(_ => SaveHistoryChanges());
        DeleteHistoryItemCommand = new RelayCommand(p => DeleteHistoryItem(p as PriceHistoryItem));
        StartVoiceCommand        = new RelayCommand(_ => StartVoice(), _ => !IsRecognizing);
        SearchByTextCommand      = new RelayCommand(_ => SearchByText());

        LegendTextPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = ChineseTypeface };
        TooltipTextPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = ChineseTypeface };
    }

    public void LoadProducts()
    {
        Products.Clear();
        foreach (var p in _productDal.GetAll())
            Products.Add(p);
    }

    /// <summary>从外部导航：加载列表并选中指定商品</summary>
    public void NavigateToProduct(int productId)
    {
        LoadProducts();
        SelectedProduct = Products.FirstOrDefault(p => p.ProductID == productId);
    }

    private void LoadHistoryAndChart()
    {

        HistoryItems.Clear();
        if (SelectedProduct == null)
        {
            ChartSeries = Array.Empty<ISeries>();
            return;
        }

        var list = _priceHistoryDal.GetByProductId(SelectedProduct.ProductID);
        foreach (var h in list)
            HistoryItems.Add(h);

        RebuildChart();

       
    }

    /// <summary>仅根据内存中的 HistoryItems 重建图表（不回数据库，供删除后即时刷新）</summary>
    private void RebuildChart()
    {
        if (SelectedProduct == null || HistoryItems.Count == 0)
        {
            ChartSeries = Array.Empty<ISeries>();
            return;
        }

        var saleVals   = new List<decimal>();
        var marketVals = new List<decimal>();
        var costVals   = new List<decimal>();
        var labels     = new List<string>();

        


        foreach (var h in HistoryItems)
        {
            saleVals  .Add(h.Price);
            marketVals.Add(h.MarketPrice);
            costVals  .Add(h.CostPrice);
            labels    .Add(h.ChangeDate.ToString("MM-dd HH:mm"));
        }

        // 通用文字画笔工厂（使用支持中文的字体）
        SolidColorPaint TextPaint(SKColor color) =>
            new(color) { SKTypeface = ChineseTypeface };

        // 折线系列工厂
        LineSeries<decimal> MakeSeries(string name, SKColor color,
                                       List<decimal> values) =>
            new()
            {
                Name             = name,
                Values           = values,
                Fill             = null,
                Stroke           = new SolidColorPaint(color, 2),
                GeometryFill     = new SolidColorPaint(color),
                GeometryStroke = new SolidColorPaint(color),
                GeometrySize     = 8,
                LineSmoothness   = 0.4,
                // 每个数据点上方显示价格数值
                DataLabelsPaint     = TextPaint(color),
                DataLabelsSize      = 11,
                DataLabelsPosition  = DataLabelsPosition.Top,
                DataLabelsFormatter = pt => $"¥{pt.PrimaryValue:F2}",
            };

        ChartSeries = new ISeries[]
        {
           
            MakeSeries("售价",   SKColors.SteelBlue, saleVals),
            MakeSeries("市均价", SKColors.OrangeRed, marketVals),
            MakeSeries("进价",   SKColors.SeaGreen,  costVals),
        };

        XAxes = new[]
        {
            new Axis
            {
                Labels         = labels,
                Name           = "日期",
                LabelsRotation = -30,
                TextSize       = 11,
                LabelsPaint    = TextPaint(SKColors.DimGray),
                NamePaint      = TextPaint(SKColors.DimGray),
            }
        };

        var allValues = saleVals.Concat(marketVals).Concat(costVals).ToList();
        var min = allValues.Count >= 0 ? allValues.Min() : 0;
        var max = allValues.Count >= 0 ? allValues.Max() : 0;
        // 给上下都留一点空间
        var padding = ((double)max - (double)min) * 0.4; // 20%缓冲

        YAxes = new[]
        {
            new Axis
            {
                Name        = "价格（元）",
                TextSize    = 11,
                Labeler     = v => $"¥{v:F2}",
                // 🔥 关键：给顶部留空间
                MinLimit    =(double)((double)min - padding),
                MaxLimit    =(double)((double)max+ padding),

                LabelsPaint = TextPaint(SKColors.DimGray),
                NamePaint   = TextPaint(SKColors.DimGray),
            }
        };
    }

    /// <summary>删除单条历史记录并刷新图表</summary>
    private void DeleteHistoryItem(PriceHistoryItem? item)
    {
        if (item == null) return;
        try
        {
            _priceHistoryDal.Delete(item.HistoryID);
            HistoryItems.Remove(item);
            RebuildChart();                     // 移除后立即更新图表
        }
        catch (Exception ex)
        {
            _msg.ShowError($"删除失败：{ex.Message}");
        }
    }

    /// <summary>保存 DataGrid 内联编辑的历史条目到数据库，并刷新图表</summary>
    private void SaveHistoryChanges()
    {
        if (HistoryItems.Count == 0) { _msg.ShowError("当前没有可保存的历史记录"); return; }
        try
        {
            foreach (var item in HistoryItems)
                _priceHistoryDal.Update(item);

            // 重新加载以确认数据一致，同时刷新图表
            LoadHistoryAndChart();
            _msg.ShowSuccess("历史记录已保存，图表已更新");
        }
        catch (Exception ex)
        {
            _msg.ShowError($"保存失败：{ex.Message}");
        }
    }

    // ── 语音 / 文字搜索 ──────────────────────────────────────────────────────

    private static string CleanVoiceText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        var t = text.Trim();
        t = Regex.Replace(t,
            @"(多少钱|价格|现在卖多少|多少元|报价|查一下|查询|历史|走势|趋势)",
            "", RegexOptions.IgnoreCase);
        return Regex.Replace(t, @"\s+", " ").Trim();
    }

    private async void StartVoice()
    {
        IsRecognizing = true;
        SearchText    = "正在听...";
        try
        {
            var text = await _voice.RecognizeAsync();
            if (string.IsNullOrWhiteSpace(text))
            {
                _msg.ShowError("未识别到语音，请重试");
                SearchText = "";
            }
            else
            {
                SearchText = text;
                SearchByText();
            }
        }
        catch (Exception ex)
        {
            _msg.ShowError("语音识别失败：" + ex.Message);
            SearchText = "";
        }
        finally
        {
            IsRecognizing = false;
        }
    }

    private void SearchByText()
    {
        var keyword = CleanVoiceText(SearchText);

        // 关键词为空 → 恢复显示全部商品
        if (string.IsNullOrWhiteSpace(keyword))
        {
            LoadProducts();
            return;
        }

        var results = _productDal.SearchByName(keyword);
        if (results.Count == 0)
        {
            _msg.ShowError($"未找到名称包含「{keyword}」的商品");
            return;
        }

        // 将搜索结果写入 Products，下拉框随之仅显示匹配项
        Products.Clear();
        foreach (var p in results)
            Products.Add(p);

        // 精确匹配 → 直接选中；唯一结果 → 直接选中；多结果 → 提示用户从下拉选择
        var exact = results.FirstOrDefault(p =>
            p.Name.Equals(keyword, StringComparison.OrdinalIgnoreCase));

        if (exact != null)
            SelectedProduct = Products.First(p => p.ProductID == exact.ProductID);
        else if (results.Count == 1)
            SelectedProduct = Products[0];
        else
            _msg.ShowSuccess($"找到 {results.Count} 个相关商品，请从下拉列表中选择");
    }

    // ── 新增价格记录 ──────────────────────────────────────────────────────────

    private void UpdatePrice()
    {
        if (SelectedProduct == null)  { _msg.ShowError("请先选择商品");   return; }
        if (NewPrice < 0)             { _msg.ShowError("售价不能为负数"); return; }
        if (NewMarketPrice < 0)       { _msg.ShowError("市均价不能为负数"); return; }
        if (NewCostPrice < 0)         { _msg.ShowError("进价不能为负数"); return; }

        // 更新商品主表的当前售价（仅当售价有变动时）
        if (NewPrice != SelectedProduct.Price)
        {
            var p = new Product
            {
                ProductID = SelectedProduct.ProductID,
                Name      = SelectedProduct.Name,
                Barcode   = SelectedProduct.Barcode,
                Category  = SelectedProduct.Category,
                Price     = NewPrice,
                Stock     = SelectedProduct.Stock
            };
            _productDal.Update(p);
        }

        // 把日期选择器的日期 + 时间输入框的时间拼合成完整时间戳
        var changeDate = NewChangeDate.Date;
        if (TimeSpan.TryParse(NewChangeTimeStr, out var ts))
            changeDate = changeDate.Add(ts);

        // 写入价格历史（含三种价格 + 自定义时间）
        _priceHistoryDal.Insert(
            SelectedProduct.ProductID, NewPrice, NewMarketPrice, NewCostPrice, changeDate);

        _msg.ShowSuccess(
            $"「{SelectedProduct.Name}」已新增价格记录（{changeDate:yyyy-MM-dd HH:mm}）：" +
            $"售价 ¥{NewPrice:F2}  市均价 ¥{NewMarketPrice:F2}  进价 ¥{NewCostPrice:F2}");

        // 写入后把时间重置为当前，方便下次操作
        NewChangeDate    = DateTime.Now;
        NewChangeTimeStr = DateTime.Now.ToString("HH:mm");

        // 刷新：先记住 ID，LoadProducts 后从集合里找同引用对象，避免 ComboBox 丢失选中
        int selectedId = SelectedProduct.ProductID;
        LoadProducts();
        SelectedProduct = Products.FirstOrDefault(p => p.ProductID == selectedId);
    }
}
