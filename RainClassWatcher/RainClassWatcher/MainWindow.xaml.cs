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

        private readonly NewExerciseDetector _newExerciseDetector = new();

        private readonly RainClassPollingService _pollingService;

        public MainWindow()
        {
            InitializeComponent();

            _pollingService = new RainClassPollingService(
                _monitor,
                TimeSpan.FromSeconds(2));

            _pollingService.ScanCompleted = OnScanCompleted;
            _pollingService.ScanFailed = OnScanFailed;

            Closed += MainWindow_Closed;
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

        private void StartMonitoring_Click(
            object sender,
            RoutedEventArgs e)
        {
            _pollingService.Start();

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            OutputBox.Text = "监控已启动，正在等待第一次检测……";
        }

        private async void StopMonitoring_Click(
            object sender,
            RoutedEventArgs e)
        {
            await _pollingService.StopAsync();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            OutputBox.Text = "监控已停止。";
        }

        private void OnScanCompleted(RainClassScanResult result)
        {
            bool shouldNotify =
                _newExerciseDetector.ShouldNotify(result);

            DispatcherQueue.TryEnqueue(() =>
            {
                OutputBox.Text =
                    FormatResult(result, shouldNotify);
            });
        }

        private void OnScanFailed(Exception exception)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                OutputBox.Text =
                    $"监控发生错误：{exception}";
            });
        }

        private void MainWindow_Closed(
            object sender,
            WindowEventArgs args)
        {
            _ = _pollingService.StopAsync();
        }

        private static string FormatResult(
    RainClassScanResult result,
    bool? shouldNotify = null)
        {
            if (!result.WindowFound)
            {
                return
                    $"监控状态：运行中\n" +
                    $"检测时间：{DateTime.Now:HH:mm:ss}\n\n" +
                    "没有找到雨课堂窗口。";
            }

            var builder = new StringBuilder();

            builder.AppendLine("监控状态：运行中");
            builder.AppendLine(
                $"检测时间：{DateTime.Now:HH:mm:ss}");

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

            if (shouldNotify == true)
            {
                builder.AppendLine(
                    "🚨 首次发现新的未作答题目，应触发提醒！");
            }
            else if (shouldNotify == false &&
                     result.HasNewExercise)
            {
                builder.AppendLine(
                    "✓ 新题仍处于“刚刚”，已去重，不重复提醒");
            }
            else
            {
                builder.AppendLine(
                    result.HasNewExercise
                        ? "🚨 判定：发现新的未作答题目！"
                        : "✓ 暂无新题");
            }

            return builder.ToString();
        }
    }
}