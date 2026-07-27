using System;

namespace TrainingHelper.App.Configuration;

public class ModelConfig
{
        public TimeSpan CountdownStart { get; set; } = TimeSpan.FromSeconds(3);
}