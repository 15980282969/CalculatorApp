@echo off
echo ========================================
echo 图标转换工具
echo ========================================
echo.
echo 注意: 需要安装 ImageMagick 或使用在线转换工具
echo.
echo 推荐方案:
echo 1. 访问 https://convertio.co/zh/svg-ico/
echo 2. 上传 resources\icon.svg
echo 3. 下载 icon.ico 并放到 resources\ 目录
echo.
echo 或者使用 ImageMagick 命令行:
echo magick resources\icon.svg -background none -define icon:auto-resize=256,128,64,48,32,16 resources\icon.ico
echo.
pause
