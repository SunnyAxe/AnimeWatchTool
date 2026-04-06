# Anime Watch Tracker

**Anime Watch Tracker** 是一个用于同步 Bangumi 追番列表、展示番剧集数、并与本地 Anime 文件夹匹配的 Windows Forms 桌面工具。

> 本程序完全使用 `vibe coding` 生成。

## 主要功能

- 同步 Bangumi 在看番剧列表
- 在本地 `anime` 或自定义目录中扫描番剧文件
- 将 Bangumi 剧集与本地文件匹配，显示可播放状态
- 单独显示每个番剧的集数和播放按钮
- 支持本地目录配置与切换
- 支持将本地扫描目录和 Bangumi 用户配置保存到 `config.json`

## 快速开始

1. 克隆仓库：

```bash
git clone https://github.com/SunnyAxe/AnimeWatchTool.git
cd AnimeWatchTool
```

2. 使用 .NET 运行：

```bash
dotnet run
```

3. 点击界面上的 `设置` 按钮，输入：

- Bangumi 用户名（需要在 https://bgm.tv/settings 中手动设置用户名，请注意“用户名”与“昵称”是不同的）
- Bangumi Token（可通过 https://next.bgm.tv/demo/access-token 生成）
- 本地 Anime 文件夹路径

4. 点击 `同步 Bangumi` 加载在看番剧列表。

## 发布

如果你想生成可执行文件，请运行：

```bash
dotnet publish AnimeWatchTool.csproj -c Release -o ./publish
```

生成结果会放在 `publish/` 文件夹下，里面包含 `AnimeWatchTool.exe`，可以直接分发给 Windows 用户。

## 项目结构

- `AnimeWatchTool.csproj` - .NET 9 Windows Forms 项目文件
- `Form1.cs` - 主窗口逻辑和数据处理
- `Form1.Designer.cs` - 主窗口 UI 布局代码
- `UserConfigForm.cs` - 设置窗口实现
- `README.md` - 项目说明
- `LICENSE` - 许可证
- `.gitignore` - 忽略构建输出和本地配置文件

## 运行环境

- .NET 9.0
- Windows

## 注意事项

- 本项目使用本地 `config.json` 保存用户配置。
- `config.json` 和 `bangumi_cache.json` 已在 `.gitignore` 中忽略，不会被提交。
- 若要正式发布，建议补充仓库描述、项目标签和发行说明。

## 许可证

本项目使用 MIT 许可证，详见 `LICENSE` 文件。