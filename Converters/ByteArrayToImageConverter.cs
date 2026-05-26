using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MiniGoods.Inventory.WPF.Converters;

/// <summary>
/// 将 byte[] BLOB 转换为 WPF 可绑定的 BitmapImage；null / 空数组返回 null。
/// </summary>
[ValueConversion(typeof(byte[]), typeof(BitmapImage))]
public class ByteArrayToImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not byte[] bytes || bytes.Length == 0) return null;
        try
        {
            var img = new BitmapImage();
            using var ms = new MemoryStream(bytes);
            img.BeginInit();
            img.CacheOption  = BitmapCacheOption.OnLoad;
            img.StreamSource = ms;
            img.EndInit();
            img.Freeze();   // 跨线程安全
            return img;
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => null;
}
