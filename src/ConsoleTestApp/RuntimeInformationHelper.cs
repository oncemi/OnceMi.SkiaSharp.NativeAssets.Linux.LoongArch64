using System.Diagnostics;
using System.Text.RegularExpressions;

namespace System.Runtime.InteropServices
{
    public static class RuntimeInformationHelper
    {
        public static bool IsABI1()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return false;
            }
            if (RuntimeInformation.ProcessArchitecture != Architecture.LoongArch64)
            {
                return false;
            }
            //1. 读取ELF标记判断，03为旧世界，43为新世界
            string elfMark = ReadELFMark();
            if (!string.IsNullOrWhiteSpace(elfMark))
            {
                if (elfMark == "03") return true;
                if (elfMark == "43") return false;
            }
            //2. 通过内核版本判断
            string kernelVersion = GetLinuxKernelVersion();
            //从目前已知的资料来看，新世界不会有低于内核版本为5.19的操作系统，上游从5.19开始正式支持loongarch64平台
            if (!string.IsNullOrWhiteSpace(kernelVersion))
            {
                Match match = Regex.Match(kernelVersion, @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$");
                if (match.Success
                    && match.Groups.TryGetValue("major", out Group? major) && !string.IsNullOrWhiteSpace(major?.Value)
                    && match.Groups.TryGetValue("minor", out Group? minor) && !string.IsNullOrWhiteSpace(minor?.Value)
                    && Version.TryParse($"{major.Value}.{minor.Value}", out Version? getkernelVersion) && getkernelVersion != null
                    && getkernelVersion < new Version(5, 19))
                {
                    return true;
                }
            }
            //3. 根据操作系统描述判断，其他系统自行添加代码（PS：正常情况下，以上两种方式已经够足够了）
            //Loongnix GNU/Linux 20 (DaoXiangHu)
            string osDescription = RuntimeInformation.OSDescription;
            if (osDescription.Contains("Loongnix GNU/Linux 20"))
            {
                return true;
            }
            return false;
        }

        private static string ReadELFMark()
        {
            string[] readFilePaths = new string[] { "/usr/bin/sh" };
            foreach (var filePath in readFilePaths)
            {
                if (!File.Exists(filePath))
                {
                    continue;
                }
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fs.Seek(48, SeekOrigin.Begin);
                        int byteValue = fs.ReadByte();
                        if (byteValue == -1)
                        {
                            continue;
                        }
                        string hexValue = byteValue.ToString("X2");
                        if (hexValue == "43" || hexValue == "03")
                        {
                            return hexValue;
                        }
                    }
                }
                catch { }
            }
            return string.Empty;
        }

        private static string GetLinuxKernelVersion()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "uname",
                Arguments = "-r",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim('\r', '\n', ' ');
                process.WaitForExit();
                return output;
            }
        }
    }
}