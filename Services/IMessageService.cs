namespace MiniGoods.Inventory.WPF.Services;

/// <summary>
/// 消息提示服务（成功/失败）
/// </summary>
public interface IMessageService
{
    void ShowSuccess(string message);
    void ShowError(string message);
}
