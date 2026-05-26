namespace MiniGoods.Inventory.WPF.Services;

/// <summary>
/// 语音识别服务
/// </summary>
public interface IVoiceRecognitionService
{
    /// <summary>
    /// 识别语音并返回文本，失败返回 null
    /// </summary>
    Task<string?> RecognizeAsync(CancellationToken cancellationToken = default);
}
