# 计算稿纸 (CalcPad)

一个轻量级的桌面计算稿纸工具,支持公式实时计算和历史记录管理。

## 功能特性

✅ **实时计算** - 输入公式后300ms自动计算并显示结果  
✅ **数学函数** - 支持四则运算、括号、三角函数、对数等  
✅ **历史记录** - 自动保存计算历史,支持搜索和清空  
✅ **暗色主题** - 现代化UI设计,护眼舒适  
✅ **窗口置顶** - 可设置窗口始终在最前  
✅ **快捷键** - Enter计算,Esc清空,操作便捷  
✅ **单实例** - 防止重复启动  

## 技术栈

- **框架**: WPF + .NET 8
- **MVVM**: CommunityToolkit.Mvvm
- **UI组件**: MaterialDesignThemes
- **表达式引擎**: NCalc
- **数据存储**: JSON文件

## 项目结构

```
CalcPad/
├── src/
│   ├── CalcPad.Core/          # 计算引擎
│   │   ├── CalculationEngine.cs
│   │   └── ExpressionValidator.cs
│   │
│   ├── CalcPad.Models/        # 数据模型
│   │   ├── CalculationRecord.cs
│   │   └── AppSettings.cs
│   │
│   ├── CalcPad.Services/      # 服务层
│   │   ├── HistoryService.cs
│   │   ├── DebounceService.cs
│   │   └── SettingsService.cs
│   │
│   └── CalcPad.UI/            # WPF界面
│       ├── App.xaml
│       ├── MainWindow.xaml
│       ├── ViewModels/
│       │   └── MainViewModel.cs
│       └── Converters/
│           └── BoolToBrushConverter.cs
│
└── CalcPad.sln
```

## 快速开始

### 环境要求

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 (推荐)

### 编译运行

```bash
# 还原NuGet包
dotnet restore

# 编译项目
dotnet build

# 运行应用
cd src/CalcPad.UI
dotnet run
```

### 发布单文件应用

```bash
cd src/CalcPad.UI
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

发布后的可执行文件位于:
`src/CalcPad.UI/bin/Release/net8.0-windows/win-x64/publish/CalcPad.UI.exe`

## 使用说明

### 基本计算

直接在输入框中输入数学表达式:

```
2 + 3 * 4          → 14
(100 - 20) / 2     → 40
sqrt(144)          → 12
pow(2, 10)         → 1024
sin(3.14159 / 2)   → 1
```

### 支持的运算符

- `+` `-` `*` `/` - 四则运算
- `%` - 取模
- `^` - 幂运算
- `()` - 括号

### 支持的函数

- `abs(x)` - 绝对值
- `sqrt(x)` - 平方根
- `pow(x, y)` - 幂运算
- `sin(x)`, `cos(x)`, `tan(x)` - 三角函数
- `log(x)`, `log10(x)` - 对数
- `max(x, y)`, `min(x, y)` - 最大值/最小值
- `round(x)`, `floor(x)`, `ceil(x)` - 取整

### 快捷键

- `Enter` - 执行计算并保存历史
- `Esc` - 清空输入框
- `Ctrl+N` - 新建计算
- `Ctrl+D` - 清空历史
- `Ctrl+T` - 切换主题
- `Ctrl+P` - 切换置顶

## 数据存储

- **历史记录**: `%APPDATA%\CalcPad\data\history.json`
- **配置文件**: `%APPDATA%\CalcPad\data\settings.json`

超过1000条历史记录会自动归档到备份文件。

## 安全特性

- 表达式长度限制: 500字符
- 计算超时保护: 1秒
- 危险字符过滤
- 单实例运行检测

## 开发计划

- [ ] 变量定义支持 (如 `a=100`, `b=a*2`)
- [ ] 自定义函数库
- [ ] 单位转换
- [ ] 历史记录搜索功能
- [ ] 数据导出(CSV/Excel)
- [ ] 云端同步(可选)

## 许可证

MIT License

## 作者

荣耀健康科技开发团队
