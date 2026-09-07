# RainClassWatcher 雨课堂习题检测

RainClassWatcher 是一个基于 WinUI 3 开发的 Windows 雨课堂习题提醒工具。

它通过 Windows UI Automation 读取雨课堂桌面客户端中的习题信息。当检测到最新题目同时处于 **“刚刚”** 和 **“未完成”** 状态时，立即发送 Windows 通知并播放提示音，避免因为未及时注意到新题而错过较短的作答时间。

> RainClassWatcher 只负责检测和提醒，不会自动作答、点击或修改雨课堂中的任何内容。

## 功能

目前已经实现：

- 自动识别雨课堂窗口
- 通过 UI Automation 读取习题信息
- 解析题目页码、发布时间和作答状态
- 根据屏幕位置识别最新题目
- 每 2 秒自动检测一次
- 检测 `刚刚 + 未完成` 的新题目
- 对同一道题进行去重，避免重复提醒
- 发送 Windows Toast 通知
- 播放系统提示音
- 支持手动单次检测
- 支持发送测试通知

## 工作原理

RainClassWatcher 不使用 OCR，也不通过截图识别界面。

雨课堂桌面客户端会将部分界面元素暴露给 Windows UI Automation，例如：

```text
第14页
2小时前
未完成
```

RainClassWatcher 使用 FlaUI 读取这些 UI Automation 元素，并根据元素的 `BoundingRectangle` 判断它们在屏幕中的实际位置，将页码、时间和状态组合成对应的习题。

检测流程：

```text
雨课堂
   ↓
Windows UI Automation
   ↓
读取习题 UI 元素
   ↓
解析页码 / 时间 / 状态
   ↓
按屏幕位置确定最新题目
   ↓
刚刚 + 未完成？
   ↓
新题去重
   ↓
Windows 通知 + 提示音
```

## 新题判定

当前版本将最新一道题同时满足以下条件视为新题：

```text
时间 = 刚刚
状态 = 未完成 / 未作答
```

检测到新题后，RainClassWatcher 会为当前题目列表生成指纹。

同一个新题即使连续多次处于 `刚刚` 状态，也只会提醒一次。

## 技术栈

- C#
- .NET 10
- WinUI 3
- Windows App SDK
- MSIX
- FlaUI
  - `FlaUI.Core`
  - `FlaUI.UIA3`
- `CommunityToolkit.WinUI.Notifications`

## 开发环境

当前项目使用：

- Windows 11
- Visual Studio 2026
- .NET 10 SDK
- x64

## 项目结构

```text
RainClassWatcher/
├── Models/
│   ├── ExerciseInfo.cs
│   └── RainClassScanResult.cs
│
├── Services/
│   ├── RainClassMonitor.cs
│   ├── RainClassPollingService.cs
│   ├── NewExerciseDetector.cs
│   └── NotificationService.cs
│
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
└── MainWindow.xaml.cs
```

主要职责：

- `RainClassMonitor`
  - 访问 Windows UI Automation
  - 查找雨课堂窗口
  - 解析习题数据

- `RainClassPollingService`
  - 定期执行扫描
  - 当前检测间隔为 2 秒

- `NewExerciseDetector`
  - 判断是否出现新题
  - 防止同一道题重复提醒

- `NotificationService`
  - 发送 Windows 通知
  - 播放提示音

- `MainWindow`
  - 展示检测状态
  - 控制开始 / 停止监控
  - 提供单次检测和测试通知

## 构建

使用 Visual Studio 打开解决方案，然后将：

```text
Configuration: Debug / Release
Platform: x64
```

设置为需要的配置并运行。

也可以直接使用 Visual Studio 的本地调试：

```text
F5
```

## 使用

1. 启动雨课堂 Windows 客户端。
2. 进入课程的“习题”页面。
3. 启动 RainClassWatcher。
4. 点击 **开始监控**。
5. RainClassWatcher 将每 2 秒检查一次当前习题。
6. 检测到新的未完成习题时，Windows 会弹出通知并播放提示音。

可以通过 **测试通知** 按钮确认 Windows 通知是否能够正常工作。

## 当前限制

RainClassWatcher 目前仍处于早期开发阶段。

当前检测逻辑依赖雨课堂客户端暴露的 UI Automation 结构以及部分界面文字，例如：

```text
刚刚
未完成
已完成
第14页
```

如果雨课堂未来修改客户端 UI、Accessibility Tree 或相关文字，检测逻辑可能需要同步调整。

此外，目前建议在雨课堂的课程“习题”页面中使用监控功能。

## Roadmap

计划继续完善：

- [ ] 系统托盘后台运行
- [ ] 关闭主窗口后继续监控
- [ ] 托盘菜单中的开始 / 暂停 / 退出
- [ ] 更完善的运行状态界面
- [ ] 最近一次检测和提醒记录
- [ ] 通知开关
- [ ] 提示音开关
- [ ] 自定义检测间隔
- [ ] 雨课堂关闭或重新启动后的自动恢复
- [ ] 更严格的课程和习题区域识别
- [ ] 开机自动启动
- [ ] 正式应用图标
- [ ] Release / MSIX 发布包

## 隐私

RainClassWatcher 的检测在本地完成。

程序只通过 Windows UI Automation 读取雨课堂客户端公开给辅助功能接口的界面信息，不需要截图、OCR，也不会自动操作或提交习题。

## License

License 尚未确定。