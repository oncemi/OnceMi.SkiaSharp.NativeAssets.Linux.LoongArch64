@echo off

:: ���ð���������
set configuration=Release
set output=publish

dotnet pack "OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64.csproj" ^
    -p:NuspecFile="OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64.nuspec" ^
    --configuration %configuration% ^
    --output "%output%\%configuration%"

:: ɾ�����Ŀ¼
rd /q /s "bin" "obj\"