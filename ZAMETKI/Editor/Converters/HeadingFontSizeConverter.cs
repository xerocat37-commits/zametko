using System.Globalization;

namespace ZAMETKI.Editor.Converters;

public class HeadingFontSizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int level ? level switch
        {
            1 => 28.0,
            2 => 22.0,
            3 => 18.0,
            _ => 22.0
        } : 22.0;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
