# StarlightFiles 本地定制与自编译说明指南

本文档记录了对 [Files (files-community/Files)](https://github.com/files-community/Files) 项目的所有个人定制修改、上游自编译的隐性踩坑点及解决方案，以便今后从上游拉取更新或进行 Rebase 时快速对照与恢复。

---

## 目录
1. [定制概览与修改清单](#1-定制概览与修改清单)
2. [关键改动详解与踩坑排查](#2-关键改动详解与踩坑排查)
   - [2.1 应用标识与图标定制 (StarlightFiles & Preview 图标)](#21-应用标识与图标定制)
   - [2.2 启动黑屏与闪退修复 (Sentry 与协议路由陷阱)](#22-启动黑屏与闪退修复)
   - [2.3 保留完整多语言支持 (Multi-Language Support)](#23-保留完整多语言支持-multi-language-support)
   - [2.4 去除快捷方式小箭头角标](#24-去除快捷方式小箭头角标)
   - [2.5 设为默认文件管理器 (Win+E) 与启动器适配](#25-设为默认文件管理器-wine-与启动器适配)
   - [2.6 详细信息列宽塌陷 (1px) 与分割条拖拽死锁修复](#26-详细信息列宽塌陷-1px-与分割条拖拽死锁修复)
   - [2.7 默认双列布局 (AlwaysOpenDualPaneInNewTab) 冷启动失效修复](#27-默认双列布局-alwaysopendualpaneinnewtab-冷启动失效修复)
   - [2.8 双列布局下快速访问 (QuickAccess) 图标显示两遍修复](#28-双列布局下快速访问-quickaccess-图标显示两遍修复)
3. [后续从上游更新 (Update/Rebase) 操作指南](#3-后续从上游更新-updaterebase-操作指南)

---

## 1. 定制概览与修改清单

| 类别 | 涉及文件 | 修改目的 / 说明 |
| :--- | :--- | :--- |
| **应用标识 & 图标** | `src/Files.App/Package.appxmanifest`<br>`src/Files.App/Files.App.csproj`<br>`src/Files.App/Constants.cs`<br>`src/Files.App/Helpers/Application/AppLifecycleHelper.cs`<br>`src/Files.App/Views/SplashScreenPage.xaml`<br>`src/Files.App.Server/Tray/SystemTrayIcon.cs` | 包名/显示名称改为 `StarlightFiles`；发布者改为 `CN=Starlight`；启用自签名；全套视觉图标与系统托盘图标切为无 DEV 水印的 Preview 图标。 |
| **启动崩溃修复** | `src/Files.App/Helpers/Application/AppLifecycleHelper.cs`<br>`src/Files.App/App.xaml.cs`<br>`src/Files.App/MainWindow.xaml.cs`<br>`src/Files.App/Program.cs` | 修复非 Dev 环境下本地自编译导致的 Sentry `UriFormatException` 黑屏异常；修复 `files-preview:` 协议激活时参数越界崩溃。 |
| **多语言支持** | `src/Files.App/Strings/`<br>`src/Files.App/Package.appxmanifest`<br>`src/Files.App/Files.App.csproj` | 完整保留官方全部多语言资源与 `StringsPropertyGenerator` 默认机制，随系统语言自动切换。 |
| **UI去快捷方式箭头** | `src/Files.App.Controls/ThemedIcon/Styles/Icons.Common.xaml`<br>`src/Files.App/Views/Layouts/DetailsLayoutPage.xaml`<br>`src/Files.App/Views/Layouts/GridLayoutPage.xaml`<br>`src/Files.App/Views/Layouts/ColumnLayoutPage.xaml`<br>`src/Files.App/UserControls/NavigationToolbar.xaml` | 隐藏或不加载文件/文件夹左下角快捷方式小箭头，界面更加干净整洁。 |
| **默认管理器 (Win+E) 修复** | `src/Files.App/Package.appxmanifest`<br>`src/Files.App/ViewModels/Settings/AdvancedViewModel.cs` | 增加 `files-dev.exe` 别名满足官方启动器检查；改用 C# 原生文件流 API 落地 Launcher，彻底修复 Win+E 报“未找到应用程序”。 |
| **详细信息列宽与拖拽死锁修复** | `src/Files.App.Controls/GridSplitter/GridSplitter.Helper.cs`<br>`src/Files.App/Data/Items/DetailsLayoutColumnItem.cs`<br>`src/Files.App/Data/Models/ColumnsViewModel.cs`<br>`src/Files.App/Services/Settings/LayoutSettingsService.cs`<br>`src/Files.App/Views/Layouts/DetailsLayoutPage.xaml`<br>`src/Files.App/Views/Layouts/DetailsLayoutPage.xaml.cs` | 彻底解决详细信息视图中列宽被挤压为 1 像素导致文件名折叠且分割条无法拖动的死锁 BUG：解除 GridSplitter 的 `<= ActualWidth` 限制、配置读写与模型增加 50~80px 安全下限保护、XAML ColumnDefinition 强制绑定 MinWidth、修复自适应列宽误将大小列当成状态列返回极小宽度的计算 BUG。 |
| **默认双列冷启动失效修复** | `src/Files.App/MainWindow.xaml.cs`<br>`src/Files.App/Views/ShellPanesPage.xaml.cs` | 修复冷启动或新标签页时因 WinUI 窗口 Bounds.Width 尚未测量（值为 0）误判为 Compact 极窄窗口导致即便开启“始终在新标签页中打开双窗格”依然只显示单列的问题；并在窗口尺寸就绪及页面导航时全链路保障自动展开第二窗格。 |
| **快速访问图标重复两遍修复** | `src/Files.App/ViewModels/UserControls/Widgets/QuickAccessWidgetViewModel.cs` | 在快速访问文件夹枚举阶段增加基于路径/ID 的 `HashSet<string>` 严格去重，结合 `_refreshVersion` 版本校验，防止双列并发加载时产生重复项。 |

---

## 2. 关键改动详解与踩坑排查

### 2.1 应用标识与图标定制
- **问题背景**：官方源码本地默认仅配置为 `Dev` 版本（图标带明显的黄色 DEV 角标），且官方的 Preview/Release 打包依赖其 GitHub Actions CI 脚本（`.github/scripts/Configure-AppxManifest.ps1`）在构建时注入。
- **具体改动**：
  1. **`src/Files.App/Package.appxmanifest`**：
     - `<Identity Name="StarlightFiles" Publisher="CN=Starlight" ... />`
     - `<DisplayName>StarlightFiles</DisplayName>` 与 `<uap:VisualElements DisplayName="StarlightFiles" ...>`
     - 将所有 Logo/Tile 路径从 `Assets\AppTiles\Dev\...` 替换为 `Assets\AppTiles\Preview\...`
     - 在 `<Extensions>` 中同时声明 `files-preview` 和 `files-dev` 两个协议与执行别名。
  2. **`src/Files.App/Files.App.csproj`**：
     - `<ApplicationIcon>Assets\AppTiles\Preview\Logo.ico</ApplicationIcon>`
     - `<AppxBundlePlatforms>x64</AppxBundlePlatforms>`
     - 添加包签名属性：`AppxPackageSigningEnabled="True"`，`PackageCertificateKeyFile="Files.App_TemporaryKey.pfx"`。
  3. **`src/Files.App/Constants.cs`**：
     - 在 `KnownAppNames` 列表中添加 `"StarlightFiles"`。
  4. **`src/Files.App/Helpers/Application/AppLifecycleHelper.cs`**：
     - 在 `AppEnvironment` 属性中，将兜底分支由 `AppEnvironment.Dev` 调整为 `AppEnvironment.SideloadPreview`。
  5. **`src/Files.App/Views/SplashScreenPage.xaml`**：
     - 启动画面的居中 Logo 路径改为 `\Assets\AppTiles\Preview\SplashScreen.png`。
  6. **`src/Files.App.Server/Tray/SystemTrayIcon.cs`**：
     - 将系统托盘图标默认回退目录从 `"Dev"` 改为 `"Preview"`。

---

### 2.2 启动黑屏与闪退修复
- **踩坑 1：Sentry 初始化抛出 `UriFormatException`**
  - **现象**：编译成功并启动后，窗口呈黑屏，无法显示主界面。
  - **原因**：当环境不是 `Dev` 时，App 启动流程会调用 `ConfigureSentry()`。但官方在公开仓库中把 Sentry DSN 占位为 `"sentry.secret"`。`SentrySdk.Init()` 无法解析该伪 URL 抛出未捕获异常。
  - **修复代码**：
    - `AppLifecycleHelper.cs` / `App.xaml.cs`：
      ```csharp
      if (string.IsNullOrEmpty(Constants.AutomatedWorkflowInjectionKeys.SentrySecret) ||
          Constants.AutomatedWorkflowInjectionKeys.SentrySecret == "sentry.secret" ||
          !Uri.TryCreate(Constants.AutomatedWorkflowInjectionKeys.SentrySecret, UriKind.Absolute, out _))
      {
          return;
      }
      ```
      并在 `App.xaml.cs` 调用 `ConfigureSentry` 时包裹 `SafetyExtensions.IgnoreExceptions`。

- **踩坑 2：协议路由越界抛出 `IndexOutOfRangeException`**
  - **现象**：点击应用磁贴启动，或者从外部协议拉起时偶现黑屏或崩溃。
  - **原因**：`src/Files.App/MainWindow.xaml.cs` 中的 `InitializeApplicationAsync` 仅对 `eventArgs.Uri.AbsoluteUri == "files-dev:"` 做了判断。当使用 `files-preview:` 时走到了 `else` 分支，由于没有 `?` 参数，导致数组越界崩溃。
  - **修复代码**：
    - 在 `MainWindow.xaml.cs` 中兼容 `files-dev:`、`files-preview:`、`files:` 空参数激活及参数长度守卫；
    - 同步在 `Program.cs` 中添加对 `files-preview` 与 `files` 别名的命令行识别。

---

### 2.3 保留完整多语言支持 (Multi-Language Support)
- **说明**：
  - 完整保留 `src/Files.App/Strings/` 下的全部官方语言包、`Package.appxmanifest` 中的 `<Resources>` 列表以及 `.csproj` 中的 `AppxDefaultResourceQualifiers`。
  - 源码生成器 `StringsPropertyGenerator.cs` 保持默认读取 `en-US\Resources.resw` 生成强类型属性，无需额外改动，避免上游更新翻译文件时产生冲突。

---

### 2.4 去除快捷方式小箭头角标
- **改动说明**：
  1. **全局图标隐藏**：`src/Files.App.Controls/ThemedIcon/Styles/Icons.Common.xaml`
     - 将 `App.ThemedIcons.Shortcut` 样式添加 `<Setter Property="Visibility" Value="Collapsed" />`。
  2. **布局页面延迟/不加载**：
     - `src/Files.App/Views/Layouts/DetailsLayoutPage.xaml`
     - `src/Files.App/Views/Layouts/GridLayoutPage.xaml`（移除 `x:Phase` 属性，设置 `x:Load="False"`）
     - `src/Files.App/Views/Layouts/ColumnLayoutPage.xaml`
     - `src/Files.App/UserControls/NavigationToolbar.xaml`

---

### 2.5 设为默认文件管理器 (Win+E) 与启动器适配
- **踩坑 1：按 Win+E 报“未找到应用程序” 与 “无法加密指定的文件”**
  - **原因**：开启默认文件管理器时，注册表写入指向 `%LocalAppData%\Files\Files.App.Launcher.exe`。原版代码通过 PowerShell 跨目录复制可能静默失败；直接 `File.Copy` 则可能因 MSIX 容器 `LocalState` 的 EFS 属性抛出 `IOException`。
  - **修复**：在 `AdvancedViewModel.cs` 中改用基于数据流的纯字节复制 (`SafeCopyFileAsync`，`FileStream.CopyToAsync`)，直接从安装包读取明文数据流写入 `%LocalAppData%\Files`。
- **踩坑 2：官方预编译 Launcher 硬编码检测 `files-dev.exe`**
  - **原因**：官方自带的 `Files.App.Launcher.exe` 启动时会检查 `%LOCALAPPDATA%\Microsoft\WindowsApps\files-dev.exe`。
  - **修复**：在 `Package.appxmanifest` 的 `<uap5:AppExecutionAlias>` 中同时注册 `files-preview.exe`、`files-dev.exe` 和 `files.exe`。

---

### 2.6 详细信息列宽塌陷 (1px) 与分割条拖拽死锁修复
- **全链路修复**：
  1. **`GridSplitter.Helper.cs`**：解除 `newWidth > ActualWidth` 的死锁限制，向右拉大时无论当前宽度多小均允许生效；并以 `ColumnDefinition.MinWidth` 为安全下限。
  2. **`DetailsLayoutPage.xaml`**：所有列均添加 `MinWidth="{x:Bind ColumnsViewModel.<Col>.MinLength, Mode=OneWay}"` 强约束。
  3. **`LayoutSettingsService.cs`**：增加 `GetColumnWidth` / `SetColumnWidth` 守卫方法，若读取到小于等于 10px 的异常值，自动回退到默认标准列宽，写入时强制保证不小于 50~80px。
  4. **`DetailsLayoutColumnItem.cs` & `ColumnsViewModel.cs`**：在 `UserLength` / `UserLengthPixels` 的 Setter 中加入 `Math.Clamp(value, NormalMinLength, NormalMaxLength)` 强制防护（仅对 `IsResizable == true` 的列生效），并将文件名列的 `NormalMinLength` 提升为 80px。
  5. **`DetailsLayoutPage.xaml.cs`**：修正 `MeasureColumnEstimate` 的列索引（16 对应状态列），移除 `columnSizeToFit > 1` 限制。

---

### 2.7 默认双列布局 (AlwaysOpenDualPaneInNewTab) 冷启动失效修复
- **全链路修复**：
  1. **`MainWindow.xaml.cs`**：启动阶段参数构造时将 `boundsWidth == 0`（未完成布局测量的冷启动）纳入白名单，确保向导航参数中正确注入 `"Home"`。
  2. **`ShellPanesPage.xaml.cs`**：
     - `IsMultiPaneAvailable` 允许窗口宽为 0 时通过可用性判定；
     - 构造函数与 `MainWindow_SizeChanged` 中，仅在 `Width > 0` 且 `<= 750` 时判定为 Compact 模式；
     - 在 `WindowIsCompact` 展开还原分支与 `OnNavigatedTo` 中全覆盖检查 `AlwaysOpenDualPaneInNewTab`。

---

### 2.8 双列布局下快速访问 (QuickAccess) 图标显示两遍修复
- **修复方案**：
  - **`QuickAccessWidgetViewModel.cs`**：在后台 STA 枚举系统快速访问文件夹时，通过 `HashSet<string> seenKeys`（基于 Path 或 Id）对枚举到的项目实施严格去重，配合 `_refreshVersion` 版本号过滤并发旧任务。

---

## 3. 后续从上游更新 (Update/Rebase) 操作指南

当前本地分支架构为：
- **`main` 分支**：纯净跟踪官方上游仓库（`upstream/main`），未做任何定制修改。
- **`custom` 分支**：工作分支，包含所有个人专属定制与修复。

当官方仓库发布新代码需要跟进更新时：

### 步骤 1：在纯净的 `main` 分支拉取上游最新构建
```powershell
git checkout main
git pull upstream main
```

### 步骤 2：切换回 `custom` 分支变基 (Rebase)
```powershell
git checkout custom
git rebase main
```
