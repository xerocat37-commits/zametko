using System.Globalization;
using ZAMETKI.Editor.Serialization;

namespace ZAMETKI.Editor.Converters;

public class BlockContentPreviewConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => NoteContentSerializer.Preview(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
