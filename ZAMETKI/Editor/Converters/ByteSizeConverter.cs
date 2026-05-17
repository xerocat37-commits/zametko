using System.Globalization;

namespace ZAMETKI.Editor.Converters;

public class ByteSizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not long bytes) return string.Empty;
        if (bytes <= 0) return "—";

        const long Kb = 1024;
        const long Mb = Kb * 1024;
        const long Gb = Mb * 1024;

        return bytes switch
        {
            < Kb => $"{bytes} Б",
            < Mb => $"{bytes / (double)Kb:0.#} КБ",
            < Gb => $"{bytes / (double)Mb:0.#} МБ",
            _    => $"{bytes / (double)Gb:0.#} ГБ"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
