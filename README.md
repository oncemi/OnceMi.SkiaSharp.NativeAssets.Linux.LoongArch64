# OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64
Runtime for SkiaSharp on the LoongArch64 platform. (Support ABI1.0 and ABI2.0)  

PS:  
1. 从3.119.0之后，官方已经提供LoongArch64新世界系统架构的支持（截止2025.03.29，目前还是预览版）。  
2. 这个包3.119.0仍然提供ABI1.0和ABI2.0的支持，后续将不在提供ABI1.0的支持。请尽快切换至新世界操作系统。  
3. 尽量不要同时安装这个包和官方包  

### How to use?  
1. Install package  
   `OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64` [![NuGet Version](https://img.shields.io/nuget/v/OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64.svg?style=flat)](https://www.nuget.org/packages/OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64/)
   
2. Load Skia native library  
    ```
    internal class Program
    {
        static void Main(string[] args)
        {
            LoongArch64RuntimeNativeLoader.LoadSkiaLibrary();

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
    }
    
    internal static class LoongArch64RuntimeNativeLoader
    {
        private static readonly string RuntimeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", "linux-loongarch64", "native");

        public static void LoadSkiaLibrary()
        {
            if (!RuntimeInformationHelper.IsLoongArch64Linux())
            {
                return;
            }

            LoadLibrary(typeof(SKBitmap).Assembly, "libSkiaSharp", GetSkiaLibraryPath());
        }

        private static void LoadLibrary(Assembly assembly, string libraryName, string libraryPath)
        {
            if (!File.Exists(libraryPath))
            {
                return;
            }

            NativeLibrary.SetDllImportResolver(assembly,
                (name, asm, path) => { return name == libraryName ? NativeLibrary.Load(libraryPath) : IntPtr.Zero; });
        }

        private static string GetSkiaLibraryPath()
        {
            return Path.Combine(RuntimeDirectory, RuntimeInformationHelper.IsABI1() ? Path.Combine("ABI1.0", "libSkiaSharp.so") : "libSkiaSharp.so");
        }
    }

    /// <summary>
    /// Helper class for runtime environment information, especially for LoongArch64 architecture.
    /// </summary>
    internal static class RuntimeInformationHelper
    {
        // Pre-compile the regex pattern for kernel version matching
        private static readonly Regex KernelVersionRegex = new(
            @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
            RegexOptions.Compiled);

        private static readonly Lazy<bool> _isABI1 = new(DetermineIsABI1);

        // Version threshold for ABI2
        private static readonly Version Abi2MinVersion = new(5, 19);

        // Common executable files to read ELF mark from
        private static readonly string[] ElfExecutables = { "/usr/bin/sh", "/bin/sh", "/usr/bin/bash" };

        /// <summary>
        /// Determines if the current runtime is using ABI1 on LoongArch64 Linux.
        /// </summary>
        /// <returns>True if running on ABI1, false otherwise.</returns>
        public static bool IsABI1() => _isABI1.Value;

        /// <summary>
        /// Determines if the current runtime is LoongArch64 architecture on Linux OS.
        /// </summary>
        /// <returns>True if running on LoongArch64 Linux, false otherwise.</returns>
        public static bool IsLoongArch64Linux() =>
            RuntimeInformation.ProcessArchitecture == Architecture.LoongArch64 &&
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// Determines if the current LoongArch64 Linux environment is using ABI1.
        /// </summary>
        /// <returns>True if ABI1 is detected, false otherwise.</returns>
        private static bool DetermineIsABI1()
        {
            // Quick check - if not on LoongArch64 Linux, definitely not ABI1
            if (!IsLoongArch64Linux())
            {
                return false;
            }

            // Strategy 1: Check ELF header mark
            string elfMark = ReadELFMark();
            if (elfMark == "03") return true;
            if (elfMark == "43") return false;

            // Strategy 2: Check kernel version
            Version? kernelVersion = DetectKernelVersion();
            if (kernelVersion != null)
            {
                return kernelVersion < Abi2MinVersion;
            }

            // Strategy 3: Check OS description
            return RuntimeInformation.OSDescription.Contains("Loongnix GNU/Linux 20");
        }

        /// <summary>
        /// Detects the kernel version using available methods.
        /// </summary>
        /// <returns>The kernel version or null if detection failed.</returns>
        private static Version? DetectKernelVersion()
        {
            // Try uname syscall first
            string? kernelVersionStr = GetLinuxKernelVersionByUname();
            if (TryMatchKernelVersion(kernelVersionStr, out var version))
            {
                return version;
            }

            // Fall back to process execution
            kernelVersionStr = GetLinuxKernelVersionByProcess();
            TryMatchKernelVersion(kernelVersionStr, out version);

            return version;
        }

        /// <summary>
        /// Reads the ELF header mark from common executable files.
        /// </summary>
        /// <returns>The ELF mark as a hexadecimal string or empty string if reading failed.</returns>
        private static string ReadELFMark()
        {
            foreach (var filePath in ElfExecutables)
            {
                if (!File.Exists(filePath))
                {
                    continue;
                }

                try
                {
                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    fs.Seek(48, SeekOrigin.Begin);
                    int byteValue = fs.ReadByte();
                    if (byteValue != -1)
                    {
                        string hexValue = byteValue.ToString("X2");
                        if (hexValue is "43" or "03")
                        {
                            return hexValue;
                        }
                    }
                }
                catch (Exception)
                {
                    // Continue trying with next file
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Attempts to parse a kernel version string into a Version object.
        /// </summary>
        /// <param name="kernelVersion">The kernel version string.</param>
        /// <param name="version">The parsed Version object.</param>
        /// <returns>True if parsing succeeded, false otherwise.</returns>
        private static bool TryMatchKernelVersion(string? kernelVersion, out Version? version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(kernelVersion))
            {
                return false;
            }

            Match match = KernelVersionRegex.Match(kernelVersion);
            if (match.Success &&
                match.Groups.TryGetValue("major", out Group? major) && !string.IsNullOrWhiteSpace(major?.Value) &&
                match.Groups.TryGetValue("minor", out Group? minor) && !string.IsNullOrWhiteSpace(minor?.Value) &&
                Version.TryParse($"{major.Value}.{minor.Value}", out Version? parsedVersion))
            {
                version = parsedVersion;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the Linux kernel version by executing the uname command.
        /// </summary>
        /// <returns>The kernel version string or empty string if execution failed.</returns>
        private static string GetLinuxKernelVersionByProcess()
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "uname",
                        Arguments = "-r",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return output;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int uname(IntPtr buf);

        /// <summary>
        /// Gets the Linux kernel version using the uname system call.
        /// </summary>
        /// <returns>The kernel version string or empty string if the call failed.</returns>
        private static string? GetLinuxKernelVersionByUname()
        {
            IntPtr buf = IntPtr.Zero;
            try
            {
                buf = Marshal.AllocHGlobal(400);
                if (uname(buf) == 0)
                {
                    return Marshal.PtrToStringAnsi(buf + 130);
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                if (buf != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buf);
                }
            }
        }
    }
	```
	Full demo please visit: https://github.com/oncemi/OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64/blob/main/src/ConsoleTestApp/Program.cs  

	Tips:  
	使用本包，请勿配置龙芯社区nuget源，在引用了`SkiaSharp.NativeAssets.Linux`的情况下，可能会和本包相互覆盖。  

### LoongArch64 编译skia  
Go to: [编译指南](https://github.com/oncemi/OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64/blob/main/BUILD_ON_LOONGARCH64.md)
 
