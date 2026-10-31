<p align="center">
  <img alt="Files hero image" src="./assets/ReadmeHero.png" />
</p>

<p align="center">
  <a style="text-decoration:none" href="https://github.com/StarlightJUE/Files">
    <img src="https://img.shields.io/badge/Edition-StarlightFiles_Custom-7952b3" alt="StarlightFiles Edition" /></a>
  <a style="text-decoration:none" href="https://github.com/files-community/Files">
    <img src="https://img.shields.io/badge/Upstream-Files_Community-F9B81F" alt="Files Community" /></a>
  <a style="text-decoration:none" href="https://github.com/StarlightJUE/Files/tree/custom">
    <img src="https://img.shields.io/badge/Branch-custom-brightgreen" alt="Branch custom" /></a>
  <a style="text-decoration:none" href="./CUSTOMIZATION.md">
    <img src="https://img.shields.io/badge/Guide-CUSTOMIZATION.md-blue" alt="Customization Guide" /></a>
</p>

# StarlightFiles

**StarlightFiles** 是基于 [Files (files-community/Files)](https://github.com/files-community/Files) 最新源码深度定制的个人增强分支。在完全保留官方优秀的现代文件管理体验与全部 49 种多语言本地化的基础上，针对日常使用中的高频痛点进行了深度体验优化与稳定性修复。

---

## ✨ 定制特性与优化清单 (Key Enhancements)

| 优化模块 | 痛点与改进说明 |
| :--- | :--- |
| 🎨 **视觉净化与图标** | 告别默认 Dev 角标水印，全局切换为精致的 **Preview** 应用图标与系统托盘图标；隐藏文件/文件夹左下角快捷方式小箭头角标，界面更纯粹轻爽。 |
| 🌐 **完整多语言支持** | 保留官方全部 49 种语言资源包及资源限定符，随系统语言自动无缝切换，上游翻译与本地体验两不误。 |
| 📐 **详细信息列宽与防死锁** | 彻底修复详细信息视图中列宽可能被挤压至 1px 导致文件名折叠且分割条无法拖动的死锁问题；设置安全最小列宽，重构自适应列宽算法。 |
| 🪟 **双窗格冷启动修复** | 修复冷启动或新标签页时因窗口尺寸测量延迟误判为极窄窗口、导致“始终在新标签页中打开双窗格”未自动展开的顽疾。 |
| 📂 **快速访问图标去重** | 优化快速访问项枚举逻辑，增加严格路径去重机制，防止双列并发加载时快速访问列表重复显示两遍。 |
| ⚡ **默认管理器 (Win+E) 增强** | 完善协议别名注册，改用原生文件流部署启动器，彻底解决 Windows 设置默认文件管理器后 Win+E 偶发报错。 |
| 🛡️ **自编译与启动稳定性修复** | 解决非官方 CI 环境下伪 Sentry DSN 导致的 `UriFormatException` 黑屏异常，并修复空参数协议激活越界问题。 |

> 详细的代码修改清单、设计考量与踩坑记录请查阅 [CUSTOMIZATION.md](../CUSTOMIZATION.md) 喵！

---

## 🛠️ 构建与编译 (Building from Source)

本项目推荐在以下环境中进行本地构建与调试：

* **IDE / 构建工具**：Visual Studio 2022 (v17.10+) 或 Visual Studio 2026，需安装：
  * .NET 桌面开发（.NET 10 SDK）
  * 使用 C++ 的桌面开发（MSVC v143/v180 生成工具）
  * Windows 11 SDK (10.0.26100.0 及以上)
* **构建步骤**：
  1. 使用 Visual Studio 打开 `Files.slnx`；
  2. 顶部配置选择 **Release** | **x64**；
  3. 在解决方案资源管理器中右键根节点，选择 **“还原 NuGet 程序包”**；
  4. 点击 **“生成” -> “重新生成解决方案”**，或右键 `Files.App` 选择 **“发布” -> “创建应用程序包”** 即可生成自签名安装包。

---

## 🤝 致敬开源上游 (Credits & Upstream)

StarlightFiles 基于开源项目 [Files Community / Files](https://github.com/files-community/Files) 衍生开发。
感谢原作者与全球开源社区贡献者们打造的优秀 Windows 现代文件管理器！

* 官方网站：[files.community](https://files.community/)
* 官方代码仓：[github.com/files-community/Files](https://github.com/files-community/Files)
* 官方开源协议：[MIT License](../LICENSE-MIT)
