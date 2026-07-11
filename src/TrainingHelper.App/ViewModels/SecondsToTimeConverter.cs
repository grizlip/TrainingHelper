using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TrainingHelper.App.ViewModels;

public class SecondsToTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");
        }
        throw new ArgumentException($"Expected int, got {value?.GetType()}");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
