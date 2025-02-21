@echo off

:: 设置包生成配置
set configuration=Release
set output=publish

dotnet pack "OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64.csproj" ^
    -p:NuspecFile="OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64.nuspec" ^
    --configuration %configuration% ^
    --output "%output%\%configuration%"

:: 删除输出目录
rd /q /s "bin" "obj\"