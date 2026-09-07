using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RainClassWatcher
{
    public sealed partial class MainWindow : Window
    {
        private static readonly Regex PageRegex =
            new(@"^第\d+页$", RegexOptions.Compiled);

        private static readonly Regex TimeRegex =
            new(@"^(刚刚|\d+分钟前|\d+小时前|\d+天前)$", RegexOptions.Compiled);

        private static readonly HashSet<string> StatusTexts =
        [
            "未完成",
            "已完成",
            "未作答",
            "已作答"
        ];

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ReadRainClass_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Text = "正在读取雨课堂……";

            try
            {
                string result = await Task.Run(ReadRainClassExercises);
                OutputBox.Text = result;
            }
            catch (Exception ex)
            {
                OutputBox.Text = $"读取失败：{ex}";
            }
        }

        private static string ReadRainClassExercises()
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
                return "没有找到雨课堂窗口。";
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

            exercises = exercises
                .OrderBy(exercise => exercise.Top)
                .ToList();

            var builder = new StringBuilder();

            builder.AppendLine($"找到窗口：{rainWindow.Name}");
            builder.AppendLine();

            if (exercises.Count == 0)
            {
                builder.AppendLine("没有识别到当前习题。");
                return builder.ToString();
            }

            builder.AppendLine($"检测到 {exercises.Count} 道题：");
            builder.AppendLine("--------------------------------");

            foreach (var exercise in exercises)
            {
                builder.AppendLine(
                    $"{exercise.Page,-8} {exercise.Time,-10} {exercise.Status}");
            }

            builder.AppendLine();
            builder.AppendLine("最新题目：");

            var latest = exercises[0];

            builder.AppendLine($"页码：{latest.Page}");
            builder.AppendLine($"时间：{latest.Time}");
            builder.AppendLine($"状态：{latest.Status}");

            return builder.ToString();
        }

        private static UiText? FindNearestByVerticalPosition(
            UiText source,
            IEnumerable<UiText> candidates,
            int maxDistance)
        {
            return candidates
                .Select(candidate => new
                {
                    Element = candidate,
                    Distance = Math.Abs(candidate.CenterY - source.CenterY)
                })
                .Where(item => item.Distance <= maxDistance)
                .OrderBy(item => item.Distance)
                .Select(item => item.Element)
                .FirstOrDefault();
        }

        private sealed record UiText(
            string Name,
            int Left,
            int Top,
            int Right,
            int Bottom)
        {
            public int CenterY => (Top + Bottom) / 2;
        }

        private sealed record ExerciseInfo(
            string Page,
            string Time,
            string Status,
            int Top);
    }
}