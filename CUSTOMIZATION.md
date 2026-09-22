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
3. [后续从上游更新 (Update/Rebase) 操作指南](#3-后续从上游更新-updaterebase-操作指南)

---

## 1. 定制概览与修改清单

| 类别 | 涉及文件 | 修改目的 / 说明 |
| :--- | :--- | :--- |
| **应用标识 & 图标** | `src/Files.App/Package.appxmanifest`<br>`src/Files.App/Files.App.csproj`<br>`src/Files.App/Constants.cs`<br>`src/Files.App/Helpers/Application/AppLifecycleHelper.cs`<br>`src/Files.App/Views/SplashScreenPage.xaml` | 包名/显示名称改为 `StarlightFiles`；发布者改为 `CN=Starlight`；启用自签名；全套视觉图标切为无 DEV 水印的 Preview 图标。 |
| **启动崩溃修复** | `src/Files.App/Helpers/Application/AppLifecycleHelper.cs`<br>`src/Files.App/App.xaml.cs`<br>`src/Files.App/MainWindow.xaml.cs`<br>`src/Files.App/Program.cs` | 修复非 Dev 环境下本地自编译导致的 Sentry `UriFormatException` 黑屏异常；修复 `files-preview:` 协议激活时参数越界崩溃。 |
| **语言裁剪 (仅保留中文)** | `src/Files.App/Strings/` (删除48个外语目录)<br>`src/Files.Core.SourceGenerator/Generators/StringsPropertyGenerator.cs`<br>`src/Files.App/Strings/zh-Hans/Resources.resw`<br>`src/Files.App/Helpers/Application/AppLanguageHelper.cs`<br>主应用与各相关 `.csproj` | 彻底移除所有非简体中文资源；修改生成器以 `zh-Hans` 生成强类型资源代码；补齐 `zh-Hans` 缺失的两个 Theme 字符串键；默认语言设为 `zh-Hans`。 |
| **UI去快捷方式箭头** | `src/Files.App.Controls/ThemedIcon/Styles/Icons.Common.xaml`<br>`src/Files.App/Views/Layouts/DetailsLayoutPage.xaml`<br>`src/Files.App/Views/Layouts/GridLayoutPage.xaml`<br>`src/Files.App/Views/Layouts/ColumnLayoutPage.xaml`<br>`src/Files.App/UserControls/NavigationToolbar.xaml` | 隐藏或不加载文件/文件夹左下角快捷方式小箭头，界面更加干净整洁。 |

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
