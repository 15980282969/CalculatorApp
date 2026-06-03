@echo off
echo ========================================
echo 计算稿纸 (CalcPad) 发布脚本
echo ========================================
echo.

cd /d "%~dp0"

echo [1/3] 清理旧文件...
dotnet clean src/CalcPad.UI/CalcPad.UI.csproj -c Release >nul 2>&1

echo [2/3] 编译并发布单文件应用...
dotnet publish src/CalcPad.UI/CalcPad.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

echo.
echo [3/3] 发布完成!
echo.
echo 可执行文件位置:
echo src\CalcPad.UI\bin\Release\net8.0-windows\win-x64\publish\CalcPad.UI.exe
echo.
echo 按任意键退出...
pause >nul
