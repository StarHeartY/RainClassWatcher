using System.Collections.Generic;

namespace RainClassWatcher.Models
{
    public sealed class RainClassScanResult
    {
        public bool WindowFound { get; init; }

        public IReadOnlyList<ExerciseInfo> Exercises { get; init; }
            = [];

        public ExerciseInfo? Latest =>
            Exercises.Count > 0 ? Exercises[0] : null;

        public bool HasNewExercise =>
            Latest is not null &&
            Latest.Time == "刚刚" &&
            (Latest.Status == "未完成" ||
             Latest.Status == "未作答");
    }
}