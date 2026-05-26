using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using MiniGoods.Inventory.WPF.DAL;
using MiniGoods.Inventory.WPF.Models;
using MiniGoods.Inventory.WPF.Services;

namespace MiniGoods.Inventory.WPF.ViewModels;

/// <summary>
/// 语音查询：识别结果、清洗关键词、模糊匹配商品、显示价格
/// </summary>
public class VoiceQueryViewModel : ViewModelBase
{
    private readonly ProductDAL _dal = new();
    private readonly IMessageService _msg;
    private readonly IVoiceRecognitionService _voice;

    private string _recognizedText = "";
    private bool _isRecognizing;

    public ObservableCollection<Product> MatchedProducts { get; } = new();

    public string RecognizedText { get => _recognizedText; set => SetProperty(ref _recognizedText, value); }
    public bool IsRecognizing { get => _isRecognizing; set => SetProperty(ref _isRecognizing, value); }

    public ICommand StartVoiceCommand { get; }
    public ICommand SearchByTextCommand { get; }

    public VoiceQueryViewModel(IMessageService msg, IVoiceRecognitionService voice)
    {
        _msg = msg;
        _voice = voice;
        StartVoiceCommand = new RelayCommand(_ => StartVoice(), _ => !IsRecognizing);
        SearchByTextCommand = new RelayCommand(_ => SearchByText());
    }

    private static string CleanVoiceText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        var t = text.Trim();
            t = Regex.Replace(t, @"(多少钱|价格|现在卖多少|多少元|报价|查一下|查询)", "", RegexOptions.IgnoreCase);
            t = Regex.Replace(t, @"\s+", " ").Trim();
            return t;
    }

    private async void StartVoice()
    {
        IsRecognizing = true;
        RecognizedText = "正在听...";
        try
        {
            var text = await _voice.RecognizeAsync();
            if (string.IsNullOrWhiteSpace(text))
            {
                _msg.ShowError("未识别到语音，请重试");
                RecognizedText = "";
            }
            else
            {
                RecognizedText = text;
                SearchByText();
            }
        }
        catch (Exception ex)
        {
            _msg.ShowError("语音识别失败：" + ex.Message);
            RecognizedText = "";
        }
        finally
        {
            IsRecognizing = false;
        }
    }

    private void SearchByText()
    {
        var keyword = CleanVoiceText(RecognizedText);
        MatchedProducts.Clear();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            foreach (var p in _dal.GetAll())
                MatchedProducts.Add(p);
            return;
        }
        foreach (var p in _dal.SearchByName(keyword))
            MatchedProducts.Add(p);
    }
}
