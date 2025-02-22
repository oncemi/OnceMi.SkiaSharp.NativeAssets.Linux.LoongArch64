using System.Reflection;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace ConsoleTestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LoadSkiaNativeLibrary();

            int width = 600;
            int height = 600;

            using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                var paint = new SKPaint
                {
                    Color = SKColors.Red,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                var path = new SKPath();
                float centerX = width / 2f;
                float centerY = height / 2f;
                float size = 200;

                path.MoveTo(centerX, centerY - size / 2);
                path.CubicTo(centerX - size, centerY - size * 1.5f, centerX - size, centerY + size / 2, centerX, centerY + size);
                path.CubicTo(centerX + size, centerY + size / 2, centerX + size, centerY - size * 1.5f, centerX, centerY - size / 2);
                path.Close();

                canvas.DrawPath(path, paint);

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    // 保存为 PNG 文件
                    File.WriteAllBytes("output.png", data.ToArray());
                }
            }

            Console.WriteLine("Image save to: output.png");
        }

        static void LoadSkiaNativeLibrary()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.LoongArch64 && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string libraryPath = RuntimeInformationHelper.IsABI1()
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./runtimes/linux-loongarch64/native/ABI1.0/libSkiaSharp.so")
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./runtimes/linux-loongarch64/native/libSkiaSharp.so");
                if (!File.Exists(libraryPath))
                {
                    return;
                }
                IntPtr ptr = NativeLibrary.Load(libraryPath);
                if (ptr == IntPtr.Zero)
                {
                    throw new BadImageFormatException($"Can not load native library {libraryPath}.");
                }
            }
        }
    }
}
