using System.Speech.Recognition;

namespace MiniGoods.Inventory.WPF.Services;

/// <summary>
/// 基于 System.Speech 的 Windows 语音识别
/// </summary>
public class WindowsVoiceRecognitionService : IVoiceRecognitionService
{
    public Task<string?> RecognizeAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                using var engine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("zh-CN"));
                engine.SetInputToDefaultAudioDevice();
                engine.LoadGrammar(new DictationGrammar());
                var result = engine.Recognize(TimeSpan.FromSeconds(10));
                return result?.Text?.Trim();
            }
            catch
            {
                return null;
            }
        }, cancellationToken);
    }
}
