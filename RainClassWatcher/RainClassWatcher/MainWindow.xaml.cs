using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainClassWatcher
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ReadRainClass_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Text = "正在读取雨课堂……";

            try
            {
                string result = await Task.Run(ReadRainClassUi);
                OutputBox.Text = result;
            }
            catch (Exception ex)
            {
                OutputBox.Text = $"读取失败：{ex}";
            }
        }

        private static string ReadRainClassUi()
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

            var builder = new StringBuilder();

            builder.AppendLine($"找到窗口：{rainWindow.Name}");
            builder.AppendLine();
            builder.AppendLine("检测到的文本：");
            builder.AppendLine("--------------------------------");

            var textElements = rainWindow.FindAllDescendants(condition =>
                condition.ByControlType(ControlType.Text));

            foreach (var element in textElements)
            {
                string name = element.Name;

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                builder.AppendLine(name);
            }

            return builder.ToString();
        }
    }
}