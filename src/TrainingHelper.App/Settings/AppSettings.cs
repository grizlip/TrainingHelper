using System;
using System.Collections.Generic;

namespace TrainingHelper.App.Settings;

internal class AppSettings
{
    public TimeSpan WorkTime { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan BreakTime { get; set; } = TimeSpan.FromSeconds(3);
    public IEnumerable<string> Pictures { get; set; } = [];
    public TimeSpan CountdownStart { get; set; } = TimeSpan.FromSeconds(3);
}
