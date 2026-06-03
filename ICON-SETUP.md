# 计算稿纸图标配置指南

## 当前状态

已创建SVG图标: `resources\icon.svg`

## 需要生成的图标格式

### 方案1: 在线转换(推荐)

1. 访问在线转换工具:
   - https://convertio.co/zh/svg-ico/
   - 或 https://icoconvert.com/

2. 上传 `resources\icon.svg` 文件

3. 选择输出格式为 `.ico`

4. 下载并保存到 `resources\icon.ico`

### 方案2: 使用ImageMagick

如果已安装ImageMagick,运行:

```bash
magick resources\icon.svg -background none -define icon:auto-resize=256,128,64,48,32,16 resources\icon.ico
```

### 方案3: 使用PowerShell和.NET

创建一个临时转换脚本:

```powershell
# 需要安装 System.Drawing.Common NuGet包
Add-Type -AssemblyName System.Drawing
$svgPath = "resources\icon.svg"
$icoPath = "resources\icon.ico"
# 转换逻辑...
```

## 图标规格要求

- 格式: ICO (Windows图标格式)
- 尺寸: 包含 256x256, 128x128, 64x64, 48x48, 32x32, 16x16 多种尺寸
- 背景: 透明或与主题匹配的紫色 (#5B2C8E)

## 配置位置

图标配置在 `src\CalcPad.UI\CalcPad.UI.csproj`:

```xml
<ApplicationIcon>..\..\resources\icon.ico</ApplicationIcon>
```

## 验证

生成ico文件后,重新编译项目:

```bash
dotnet build
```

图标将显示在:
- 窗口左上角标题栏
- 任务栏
- 桌面快捷方式(如果创建)
