@echo off
echo ========================================
echo 计算稿纸 (CalcPad) 快速启动
echo ========================================
echo.

cd /d "%~dp0"

echo 正在启动计算稿纸...
echo.

dotnet run --project src/CalcPad.UI/CalcPad.UI.csproj

echo.
echo 程序已退出。
echo 按任意键退出...
pause >nul
