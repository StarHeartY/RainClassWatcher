using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using RainClassWatcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RainClassWatcher.Services
{
    public sealed class RainClassMonitor
    {
        private static readonly Regex PageRegex =
            new(@"^第\d+页$", RegexOptions.Compiled);

        private static readonly Regex TimeRegex =
            new(@"^(刚刚|\d+分钟前|\d+小时前|\d+天前)$",
                RegexOptions.Compiled);

        private static readonly HashSet<string> StatusTexts =
        [
            "未完成",
            "已完成",
            "未作答",
            "已作答"
        ];

        public RainClassScanResult Scan()
        {
            using var automation = new UIA3Automation();

            var desktop = automation.GetDesktop();

            var rainWindow = desktop
                .FindAllChildren(condition =>
                    condition.ByControlType(ControlType.Window))
                .FirstOrDefault(element =>
                    element.Name?.Contains(
                        "雨课堂",
                        StringComparison.OrdinalIgnoreCase) == true);

            if (rainWindow is null)
            {
                return new RainClassScanResult
                {
                    WindowFound = false
                };
            }

            var texts = rainWindow
                .FindAllDescendants(condition =>
                    condition.ByControlType(ControlType.Text))
                .Select(element =>
                {
                    var rect = element.BoundingRectangle;

                    return new UiText(
                        element.Name ?? string.Empty,
                        rect.Left,
                        rect.Top,
                        rect.Right,
                        rect.Bottom);
                })
                .Where(text =>
                    !string.IsNullOrWhiteSpace(text.Name) &&
                    text.Right > text.Left &&
                    text.Bottom > text.Top)
                .ToList();

            var pageElements = texts
                .Where(text => PageRegex.IsMatch(text.Name))
                .ToList();

            var timeElements = texts
                .Where(text => TimeRegex.IsMatch(text.Name))
                .ToList();

            var statusElements = texts
                .Where(text => StatusTexts.Contains(text.Name))
                .ToList();

            var exercises = new List<ExerciseInfo>();

            foreach (var page in pageElements)
            {
                var time = FindNearestByVerticalPosition(
                    page,
                    timeElements,
                    maxDistance: 120);

                var status = FindNearestByVerticalPosition(
                    page,
                    statusElements,
                    maxDistance: 120);

                if (time is null || status is null)
                {
                    continue;
                }

                exercises.Add(new ExerciseInfo(
                    page.Name,
                    time.Name,
                    status.Name,
                    page.Top));
            }

            var sortedExercises = exercises
                .OrderBy(exercise => exercise.Top)
                .ToList();

            return new RainClassScanResult
            {
                WindowFound = true,
                Exercises = sortedExercises
            };
        }

        private static UiText? FindNearestByVerticalPosition(
            UiText source,
            IEnumerable<UiText> candidates,
            double maxDistance)
        {
            return candidates
                .Select(candidate => new
                {
                    Element = candidate,
                    Distance =
                        Math.Abs(candidate.CenterY - source.CenterY)
                })
                .Where(item => item.Distance <= maxDistance)
                .OrderBy(item => item.Distance)
                .Select(item => item.Element)
                .FirstOrDefault();
        }

        private sealed record UiText(
            string Name,
            double Left,
            double Top,
            double Right,
            double Bottom)
        {
            public double CenterY => (Top + Bottom) / 2;
        }
    }
}