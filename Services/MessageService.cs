using System.Windows;

namespace MiniGoods.Inventory.WPF.Services;

/// <summary>
/// 使用 MessageBox 的消息提示服务
/// </summary>
public class MessageService : IMessageService
{
    public void ShowSuccess(string message)
    {
        MessageBox.Show(message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message)
    {
        MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
