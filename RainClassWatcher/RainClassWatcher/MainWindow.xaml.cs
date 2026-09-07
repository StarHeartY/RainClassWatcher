using Microsoft.UI.Xaml;
using RainClassWatcher.Models;
using RainClassWatcher.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RainClassWatcher
{
    public sealed partial class MainWindow : Window
    {
        private readonly RainClassMonitor _monitor = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ReadRainClass_Click(
            object sender,
            RoutedEventArgs e)
        {
            OutputBox.Text = "正在读取雨课堂……";

            try
            {
                var result = await Task.Run(_monitor.Scan);
                OutputBox.Text = FormatResult(result);
            }
            catch (Exception ex)
            {
                OutputBox.Text = $"读取失败：{ex}";
            }
        }

        private static string FormatResult(
            RainClassScanResult result)
        {
            if (!result.WindowFound)
            {
                return "没有找到雨课堂窗口。";
            }

            var builder = new StringBuilder();

            builder.AppendLine("找到窗口：雨课堂");
            builder.AppendLine();

            if (result.Exercises.Count == 0)
            {
                builder.AppendLine("没有识别到当前习题。");
                return builder.ToString();
            }

            builder.AppendLine(
                $"检测到 {result.Exercises.Count} 道题：");

            builder.AppendLine(
                "--------------------------------");

            foreach (var exercise in result.Exercises)
            {
                builder.AppendLine(
                    $"{exercise.Page,-8} " +
                    $"{exercise.Time,-10} " +
                    $"{exercise.Status}");
            }

            var latest = result.Latest!;

            builder.AppendLine();
            builder.AppendLine("最新题目：");
            builder.AppendLine($"页码：{latest.Page}");
            builder.AppendLine($"时间：{latest.Time}");
            builder.AppendLine($"状态：{latest.Status}");
            builder.AppendLine();

            builder.AppendLine(
                result.HasNewExercise
                    ? "🚨 判定：发现新的未作答题目！"
                    : "✓ 判定：暂无新题");

            return builder.ToString();
        }
    }
}