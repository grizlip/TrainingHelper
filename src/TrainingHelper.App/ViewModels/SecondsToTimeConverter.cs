using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using NLog;

namespace TrainingHelper.App.ViewModels;

public class SecondsToTimeConverter : IMultiValueConverter
{
    private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
    private static readonly string timeSpanFormat = @"hh\:mm\:ss";
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 2)
        {
            if (values[0] is int seconds && values[1] is int totalSeconds)
            {
                return string.Format("{0} / {1}",TimeSpan.FromSeconds(seconds).ToString(timeSpanFormat),
                                                 TimeSpan.FromSeconds(totalSeconds - seconds).ToString(timeSpanFormat));
            }
            logger.Trace($"Expected two {typeof(int)}. Got 0 => {values[0]?.GetType()} 1 =>>{values[1]?.GetType()}  ");
        }
        else
        {
            logger.Trace($"Expected two values. Got: {values?.Count}");
        }
        return string.Empty;
    }

}
