# StarlightFiles 本地定制与自编译说明指南

本文档记录了对 [Files (files-community/Files)](https://github.com/files-community/Files) 项目的所有个人定制修改、上游自编译的隐性踩坑点及解决方案，以便今后从上游拉取更新或进行 Rebase 时快速对照与恢复。

---

## 目录
1. [定制概览与修改清单](#1-定制概览与修改清单)
2. [关键改动详解与踩坑排查](#2-关键改动详解与踩坑排查)
   - [2.1 应用标识与图标定制 (StarlightFiles & Preview 图标)](#21-应用标识与图标定制)
   - [2.2 启动黑屏与闪退修复 (Sentry 与协议路由陷阱)](#22-启动黑屏与闪退修复)
   - [2.3 仅保留简体中文 (zh-Hans Only) 与 SourceGenerator 适配](#23-仅保留简体中文-zh-hans-only)
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
| **应用标识 & 图标** | `src/Files.App/Package.appxmanifest`<br>`src/Files.App/Files.App.csproj`<br>`src/Files.App/Constants.cs`<br>`src/Files.App/Helpers/Application/AppLifecycleHelper.cs`<br>`src/Files.App/Views/SplashScreenPage.xaml` | 包名/显示名称改为 `StarlightFiles`；发布者改为 `CN=Starlight`；启用自签名；全套视觉图标切为无 DEV 水印的 Preview 图标。 |
| **启动崩溃修复** | `src/Files.App/Helpers/Application/AppLifecycleHelper.cs`<br>`src/Files.App/App.xaml.cs`<br>`src/Files.App/MainWindow.xaml.cs`<br>`src/Files.App/Program.cs` | 修复非 Dev 环境下本地自编译导致的 Sentry `UriFormatException` 黑屏异常；修复 `files-preview:` 协议激活时参数越界崩溃。 |
| **语言裁剪 (仅保留中文)** | `src/Files.App/Strings/` (删除48个外语目录)<br>`src/Files.Core.SourceGenerator/Generators/StringsPropertyGenerator.cs`<br>`src/Files.App/Strings/zh-Hans/Resources.resw`<br>`src/Files.App/Helpers/Application/AppLanguageHelper.cs`<br>主应用与各相关 `.csproj` | 彻底移除所有非简体中文资源；修改生成器以 `zh-Hans` 生成强类型资源代码；补齐 `zh-Hans` 缺失的两个 Theme 字符串键；默认语言设为 `zh-Hans`。 |
| **UI去快捷方式箭头** | `src/Files.App.Controls/ThemedIcon/Styles/Icons.Common.xaml`<br>`src/Files.App/Views/Layouts/DetailsLayoutPage.xaml`<br>`src/Files.App/Views/Layouts/GridLayoutPage.xaml`<br>`src/Files.App/Views/Layouts/ColumnLayoutPage.xaml`<br>`src/Files.App/UserControls/NavigationToolbar.xaml` | 隐藏或不加载文件/文件夹左下角快捷方式小箭头，界面更加干净整洁。 |
| **默认管理器 (Win+E) 修复** | `src/Files.App/Package.appxmanifest`<br>`src/Files.App/ViewModels/Settings/AdvancedViewModel.cs` | 增加 `files-dev.exe` 别名满足官方启动器检查；改用 C# 原生文件 API 落地 Launcher，彻底修复 Win+E 报“未找到应用程序”。 |
| **详细信息列宽与拖拽死锁修复** | `src/Files.App.Controls/GridSplitter/GridSplitter.Helper.cs`<br>`src/Files.App/Data/Items/DetailsLayoutColumnItem.cs`<br>`src/Files.App/Data/Models/ColumnsViewModel.cs`<br>`src/Files.App/Services/Settings/LayoutSettingsService.cs`<br>`src/Files.App/Views/Layouts/DetailsLayoutPage.xaml`<br>`src/Files.App/Views/Layouts/DetailsLayoutPage.xaml.cs` | 彻底解决详细信息视图中列宽被挤压为 1 像素导致文件名折叠且分割条无法拖动的死锁 BUG：解除 GridSplitter 的 `<= ActualWidth` 限制、配置读写与模型增加 50~80px 安全下限保护、XAML ColumnDefinition 强制绑定 MinWidth、修复自适应列宽误将大小列当成状态列返回极小宽度的计算 BUG。 |
| **默认双列冷启动失效修复** | `src/Files.App/MainWindow.xaml.cs`<br>`src/Files.App/Views/ShellPanesPage.xaml.cs` | 修复冷启动或新标签页时因 WinUI 窗口 Bounds.Width 尚未测量（值为 0）误判为 Compact 极窄窗口导致即便开启“始终在新标签页中打开双窗格”依然只显示单列的问题；并在窗口尺寸就绪及页面导航时全链路保障自动展开第二窗格。 |
| **快速访问图标重复两遍修复** | `src/Files.App/ViewModels/UserControls/Widgets/QuickAccessWidgetViewModel.cs` | 修复双列布局下左右窗格同时加载 HomePage 并发触发全局单例 QuickAccessWidgetViewModel 的 RefreshWidgetAsync 时，由于循环内部跨 await 交织执行导致项目被重复双份插入集合的严重 BUG；增加并发互斥锁、路径去重集合与 UI 线程原子批量替换。 |

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
     - 添加包签名属性：`PackageCertificateKeyFile="Files.App_TemporaryKey.pfx"`，`PackageCertificateThumbprint` 等。
  3. **`src/Files.App/Constants.cs`**：
     - 在 `KnownAppNames` 列表中添加 `"StarlightFiles"`。
  4. **`src/Files.App/Helpers/Application/AppLifecycleHelper.cs`**：
     - 在 `AppEnvironment` 属性中，将兜底分支由 `AppEnvironment.Dev` 调整为 `AppEnvironment.SideloadPreview`。
  5. **`src/Files.App/Views/SplashScreenPage.xaml`**：
     - 启动画面的居中 Logo 路径改为 `Assets/AppTiles/Preview/SplashScreen.scale-200.png`。

---

### 2.2 启动黑屏与闪退修复
- **踩坑 1：Sentry 初始化抛出 `UriFormatException`**
  - **现象**：编译成功并启动后，窗口呈黑屏，无法显示主界面。
  - **原因**：当环境不是 `Dev` 时，App 启动流程会调用 `ConfigureSentry()`。但官方在公开仓库中把 Sentry DSN 占位为 `"sentry.secret"`。`SentrySdk.Init()` 无法解析该伪 URL 抛出未捕获异常，导致 `OnLaunched` 提前终止。
  - **修复代码**：
    - `AppLifecycleHelper.cs` / `App.xaml.cs`：
      ```csharp
      if (string.IsNullOrEmpty(Constants.AutomatedTestTokens.SENTRY_SECRET) ||
          !Uri.TryCreate(Constants.AutomatedTestTokens.SENTRY_SECRET, UriKind.Absolute, out _))
      {
          return;
      }
      ```
      并在 `ConfigureSentry()` 外围包裹 `try { ... } catch { }`，防止崩溃。

- **踩坑 2：协议路由越界抛出 `IndexOutOfRangeException`**
  - **现象**：点击应用磁贴启动，或者从外部协议拉起时偶现黑屏或崩溃。
  - **原因**：`src/Files.App/MainWindow.xaml.cs` 中的 `OnActivated` 仅对 `eventArgs.Uri.AbsoluteUri == "files-dev:"` 做了判断。当使用 `files-preview:` 时走到了 `else` 分支，执行 `eventArgs.Uri.AbsoluteUri.Split('?')[1]`，由于没有 `?` 参数，导致数组越界崩溃。
  - **修复代码**：
    - `MainWindow.xaml.cs`：
      ```csharp
      if (eventArgs.Uri.AbsoluteUri.StartsWith("files:", StringComparison.OrdinalIgnoreCase) ||
          eventArgs.Uri.AbsoluteUri.StartsWith("files-preview:", StringComparison.OrdinalIgnoreCase) ||
          eventArgs.Uri.AbsoluteUri.StartsWith("files-dev:", StringComparison.OrdinalIgnoreCase))
      {
          var parsedArgs = eventArgs.Uri.AbsoluteUri.Split('?');
          if (parsedArgs.Length > 1 && !string.IsNullOrWhiteSpace(parsedArgs[1]))
          {
              // 解析 query
          }
          // 否则正常导航
      }
      ```
    - 同步在 `Program.cs` 中添加对 `files-preview:` 的处理。

---

### 2.3 仅保留简体中文 (zh-Hans Only)
- **改动说明**：
  1. **清理多余目录**：删除了 `src/Files.App/Strings/` 下除 `zh-Hans` 外的全部 48 种外语目录。
  2. **`StringsPropertyGenerator.cs` 适配**：
     - 原版源码生成器硬编码了 `EndsWith("en-US\\Resources.resw")`，删除 `en-US` 后会导致 `Strings` 类无法生成。
     - 改为 `EndsWith("Resources.resw")`，使其直接读取 `zh-Hans` 对应的资源文件。
  3. **补全 `zh-Hans/Resources.resw` 缺失键**：
     - 原生官方中文 resw 文件相比英文基准缺失了 `LightThemeLabel` 与 `DarkThemeLabel`。
     - 补全后方可通过生成器编译：
       ```xml
       <data name="LightThemeLabel" xml:space="preserve">
         <value>浅色主题</value>
       </data>
       <data name="DarkThemeLabel" xml:space="preserve">
         <value>深色主题</value>
       </data>
       ```
  4. **工程文件默认语言**：
     - `Files.App.csproj`、`Files.App.BackgroundTasks.csproj`、`Files.App.Server.csproj` 中设为 `<DefaultLanguage>zh-Hans</DefaultLanguage>`。
     - `AppLanguageHelper.cs` 默认语言回退设为 `"zh-Hans"`。

---

### 2.4 去除快捷方式小箭头角标
- **改动说明**：
  1. **全局图标隐藏**：`src/Files.App.Controls/ThemedIcon/Styles/Icons.Common.xaml`
     - 将 `App.ThemedIcons.Shortcut` 样式的根节点设置为 `Visibility="Collapsed"`。
  2. **布局页面延迟/不加载**：
     - `src/Files.App/Views/Layouts/DetailsLayoutPage.xaml`
     - `src/Files.App/Views/Layouts/GridLayoutPage.xaml`（注意：移除 `x:Phase` 属性，设置 `x:Load="False"`）
     - `src/Files.App/Views/Layouts/ColumnLayoutPage.xaml`
     - `src/Files.App/UserControls/NavigationToolbar.xaml`

---

### 2.5 设为默认文件管理器 (Win+E) 与启动器适配
- **踩坑 1：按 Win+E 报“未找到应用程序” 与 “无法加密指定的文件”**
  - **原因**：开启默认文件管理器时，注册表写入指向 `%LocalAppData%\Files\Files.App.Launcher.exe`。原版代码通过 PowerShell 跨目录复制静默失败；后续改用 `File.Copy` 时，由于 Windows MSIX 容器的 `LocalState` 带有隔离加密属性，底层的 Win32 `CopyFileW` 尝试同步加密属性导致抛出 `System.IO.IOException: 无法加密指定的文件`，启动器本体未能落地。
  - **修复**：在 `AdvancedViewModel.cs` 中改用基于数据流的纯字节复制 (`SafeCopyFileAsync`，`FileStream.CopyToAsync`)，直接从安装包读取明文数据流写入 `%LocalAppData%\Files`，彻底剥离 EFS 加密属性，确保启动器文件 100% 成功写入。
- **踩坑 2：官方预编译 Launcher 硬编码检测 `files-dev.exe`**
  - **原因**：官方自带的 `Files.App.Launcher.exe` 是预编译原生 C++ 程序，启动时会检查 `%LOCALAPPDATA%\Microsoft\WindowsApps\files-dev.exe`。若检测失败会弹窗并自动回滚注册表。但我们使用的是 Preview 风格配置，此前只注册了 `files-preview.exe`。
  - **修复**：在 `Package.appxmanifest` 的 `<uap5:AppExecutionAlias>` 中同时注册 `files-preview.exe`、`files-dev.exe` 和 `files.exe`，满足 Launcher 校验并实现多别名兼容。

---

### 2.6 详细信息列宽塌陷 (1px) 与分割条拖拽死锁修复
- **现象**：
  在详细信息视图（Details Layout）下，文件名称及其他列仅剩 1 像素，文件名被彻底折叠遮挡，整行只剩图标可见；上方表头的分割条（GridSplitter）鼠标按住无法拖宽，双击或自适应也无法恢复；通过 `Ctrl + 滚轮` 放大视图可以看清（因为滚轮将视图切换到了网格/卡片模式）。
- **根因分析**：
  1. **GridSplitter 拖拽死锁（Deadlock）**：在 `GridSplitter.Helper.cs` 中，拖动判定条件为 `if (newWidth > ActualWidth)`，这里的 `ActualWidth` 为 Splitter 自身宽度（12px）。若列宽因故萎缩至 `<= 12px`（如 1px），鼠标单次拖动增量（1~3px）导致 `newWidth` 永远无法跨越 12px，使 `SetColumnWidth` 永远返回 `false`，从而彻底卡死无法向右拖大。
  2. **缺少最小列宽绑定**：`DetailsLayoutPage.xaml` 中的表头 `ColumnDefinition` 均未绑定 `MinWidth`，布局系统与拖拽逻辑均没有下限拦截。
  3. **自适应列宽（AutoFit）索引错位**：`DetailsLayoutPage.xaml.cs` 的 `MeasureColumnEstimate` 将列索引 15 误当成同步状态列（实际 15 是 `ItemSize` 文件大小列，16 才是同步状态），直接把大小字符长度（如 1~4）作为像素宽度返回；且 `if (columnSizeToFit > 1)` 会直接跳过极小宽度，导致无法重置自适应。
  4. **配置反序列化缺乏防灾兜底**：一旦本地配置 `user_settings.json` 被写入了 `NameColumnWidth: 1`，应用启动时会直接加载该坏值，且无自我纠正机制。
- **全链路修复**：
  1. **`GridSplitter.Helper.cs`**：解除 `newWidth > ActualWidth` 的死锁限制，向右拉大时无论当前宽度多小均允许生效；并以 `ColumnDefinition.MinWidth` 为安全下限。
  2. **`DetailsLayoutPage.xaml`**：所有列均添加 `MinWidth="{x:Bind ColumnsViewModel.<Col>.MinLength, Mode=OneWay}"` 强约束。
  3. **`LayoutSettingsService.cs`**：增加 `GetColumnWidth` / `SetColumnWidth` 守卫方法，若读取到小于等于 10px 的异常值，自动回退到默认标准列宽（如 Name 240px、DateModified 200px、Size 100px），写入时强制保证不小于 50~80px。
  4. **`DetailsLayoutColumnItem.cs` & `ColumnsViewModel.cs`**：在 `UserLength` / `UserLengthPixels` 的 Setter 中加入 `Math.Clamp(value, NormalMinLength, NormalMaxLength)` 强制防护（仅对 `IsResizable == true` 的列生效，避免将 20px 固定图标列错误拉大到 50px 导致图标异常放大与复选框缩进错位），并将文件名列的 `NormalMinLength` 提升为 80px。
  5. **`DetailsLayoutPage.xaml.cs`**：修正 `MeasureColumnEstimate` 的列索引（16 对应状态列），移除 `columnSizeToFit > 1` 限制，确保无论在任何状态下双击分隔条或自适应列宽均可恢复正常尺寸。

---

### 2.7 默认双列布局 (AlwaysOpenDualPaneInNewTab) 冷启动失效修复
- **现象**：
  在设置中开启了“始终在新标签页中打开双窗格 (`AlwaysOpenDualPaneInNewTab = true`)”，但在冷启动/重新打开应用时，默认依然是单列/单窗格，未能自动展开右侧窗格，需要用户手动点击双窗格按钮。
- **根因分析**：
  1. **冷启动窗口未测量尺寸为 0 误判**：在应用冷启动初始化阶段，WinUI 3 窗口的 `Bounds.Width` 尚未测量，其值为 `0`。在 `MainWindow.xaml.cs` 中，`RightPaneNavPathParam` 判断了 `boundsWidth > Constants.UI.MultiplePaneWidthThreshold (750)`，因为 `0 > 750` 为 `false`，导致 `RightPaneNavPathParam` 被强行赋予 `null`。
  2. **多窗格可用性检测死锁**：在 `ShellPanesPage.xaml.cs` 中，`IsMultiPaneAvailable` 检查 `MainWindow.Instance.Bounds.Width > 750`，构造函数将 `_WindowIsCompact` 赋值为 `0 <= 750 -> true`。不仅构造函数中的 `AddPane()` 未触发，甚至后续窗口尺寸就绪触发 `MainWindow_SizeChanged` 时，由于此前未曾记录 `_wasRightPaneVisible`，导致 `WindowIsCompact` 由 `true` 变 `false` 时直接跳过补开窗格。
  3. **导航参数分支遗漏**：在 `ShellPanesPage.OnNavigatedTo` 中，对于通过路径字符串（如普通启动或新建标签页传递 `"Home"`）激活的分支，未包含根据 `AlwaysOpenDualPaneInNewTab` 打开第二窗格的逻辑。
- **全链路修复**：
  1. **`MainWindow.xaml.cs`**：启动阶段参数构造时将 `boundsWidth == 0`（未完成布局测量的冷启动）纳入白名单，确保向导航参数中正确注入 `"Home"`。
  2. **`ShellPanesPage.xaml.cs`**：
     - `IsMultiPaneAvailable` 允许窗口宽为 0 时通过可用性判定；
     - 构造函数与 `MainWindow_SizeChanged` 中，仅在 `Width > 0` 且 `<= 750` 时判定为 Compact 模式，避免冷启动未就绪时被误判为极窄窗口；
     - 在 `WindowIsCompact` 展开还原分支中，若开启了 `AlwaysOpenDualPaneInNewTab` 且当前只有 1 个 Pane，自动补开第二窗格；
     - 在 `OnNavigatedTo` 中，全覆盖检查 `AlwaysOpenDualPaneInNewTab`，确保冷启动、恢复会话或新建标签页时均稳稳打开双列。

---

### 2.8 双列布局下快速访问 (QuickAccess) 图标显示两遍修复
- **现象**：
  在双列布局模式下，主页（Home）的“快速访问（Quick Access）”小组件中的固定图标（如桌面、下载、文档等）会异常重复显示两遍；而在单列布局下则表现正常。
- **根因分析**：
  1. **单例共享与并发异步交织（Interleaving）**：`QuickAccessWidgetViewModel` 在 DI 容器中注册为全局单例 (`AddSingleton`)，维护了一个全局共享的 `Items` 集合。当双列布局开启时，左右两个窗格同时加载 `HomePage` 并触发 `HomePage_Loaded` -> `RefreshWidgetAsync()`。
  2. **缺少并发互斥锁**：原版 `RefreshWidgetAsync` 在清空 `Items` 之后，在 `await foreach` 遍历系统快速访问已知文件夹时，每次循环均调用了 `await STATask.RunPooled(...)`。由于 `await` 让出当前调度队列，导致左窗格与右窗格的两个异步刷新任务交替并发执行，交织着向同一个全局单例 `Items` 集合中执行 `Items.Insert()`，导致全部图标均被重复插入两遍。
  3. **缺乏数据源唯一性去重机制**：原逻辑未对系统快速访问路径或标识符进行唯一性校验。
- **修复方案**：
  1. **`QuickAccessWidgetViewModel.cs`**：
     - 引入 `SemaphoreSlim _refreshLock = new(1, 1)`，对刷新逻辑实施严格的互斥锁保护，彻底杜绝并发重入；
     - 在后台通过 `HashSet<string> seenKeys`（基于 Path 或 Id）对枚举到的项目实施严格去重；
     - 先将项收集至临时列表 `List<WidgetFolderCardItem>`，完全收集完成后再切换至 UI 线程，通过一次性原子清空与重新添加，彻底消除集合交替重入与重复项问题，同时避免刷新期间界面白屏闪烁；
     - 在 `Dispose()` 中安全释放 `_refreshLock` 信号量资源。

---

## 3. 后续从上游更新 (Update/Rebase) 操作指南

当前本地分支架构为：
- **`main` 分支**：纯净跟踪官方上游仓库（`origin/main`），未做任何定制修改，方便随时直接拉取最新的 Nightly 构建代码。
- **`custom` 分支**：当前工作分支，包含所有个人专属定制与崩溃修复的单次独立提交。

当官方仓库发布新代码需要跟进更新时，只需两步：

### 步骤 1：在纯净的 `main` 分支拉取上游最新构建 (Nightly)
```powershell
# 切换到 main 分支并拉取官方最新代码
git checkout main
git pull
```

### 步骤 2：切换回 `custom` 分支变基 (Rebase)
```powershell
# 切换到定制分支，将定制提交自动重放对齐到最新的 main 之上
git checkout custom
git rebase main
```

### 步骤 3：可能遇到的冲突与应对策略
1. **`src/Files.App/Strings/` 外语目录被上游重新拉出**：
   - 上游新提交可能会修改其他语言的 resw 文件。如果发生冲突，可以直接删除冲突的外语目录：
     ```powershell
     # 保持只留下 zh-Hans 即可
     git rm -rf src/Files.App/Strings/<other-langs>
     ```
2. **`StringsPropertyGenerator.cs` 冲突**：
   - 保留 `EndsWith("Resources.resw")` 的判断逻辑。
3. **`zh-Hans/Resources.resw` 冲突**：
   - 接受上游新翻译的同时，确认 `LightThemeLabel` 与 `DarkThemeLabel` 是否依然存在（若上游已经补齐则无需重复添加）。
4. **`AppLifecycleHelper.cs` / `MainWindow.xaml.cs` 冲突**：
   - 重点检查 Sentry 初始化的空值/合法性判断，以及协议 `files-preview:` 的分支判断。
5. **`Package.appxmanifest` 冲突**：
   - 保留 `Identity`、`DisplayName`、`Starlight` 签名以及 `Assets\AppTiles\Preview` 资产配置。

### 步骤 4：本地编译与打包发布
- 在 Visual Studio 中打开 `Files.slnx`。
- 选择 `Files.App` 项目 -> 右键 -> **发布** (Publish) -> **创建应用程序包**。
- 选择 **旁加载 (Sideloading)**，使用当前证书 `CN=Starlight` 签名生成 `.msixbundle` / `.msix`。
