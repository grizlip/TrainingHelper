using System;
using System.Collections.Generic;

namespace TrainingHelper.App.Models;

internal interface IRoundDefinition
{
    TimeSpan Interval { get; }
    TimeSpan Break { get; }
    bool Work { get; }
    int Repetitions { get; }
    IEnumerable<string> RoundImagesPaths { get; }
}