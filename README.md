# OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64
Runtime for SkiaSharp on the LoongArch64 platform. (Support ABI1.0 and ABI2.0)  
(在官方没有提供原生支持之前的临时解决方案，暂不支持SkiaSharp3.x版本)  

### How to use?  
1. Install package from nuget：`OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64`  
2. Load Skia native library  
	```
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
	```
	Full demo please visit: https://github.com/oncemi/OnceMi.SkiaSharp.NativeAssets.Linux.LoongArch64/blob/main/src/ConsoleTestApp/Program.cs  

	Tips:  
	使用本包，请勿配置龙芯社区nuget源，在引用了`SkiaSharp.NativeAssets.Linux`的情况下，可能会和本包相互覆盖。  

### LoongArch64 新世界 编译skia  
1. 安装依赖  

	```bash
	sudo apt install -y gcc g++ gdb make cmake git ninja-build generate-ninja libx11-dev libxcursor-dev libfontconfig1-dev libgl1-mesa-dev  

	#安装OpenGL：sudo apt install libgl1-mesa-dev libgles2-mesa-dev libegl1-mesa-dev
	```
2. Python2处理  
	如果安装了python2，可以跳过这一步骤。  
	新世界操作系统（包括较新的发行版），都不再默认安装python2，默认安装python3。由于skia使用了python命令，会导致编译失败。还有种办法就是直接将python软链接到python3.  

	```bash
	sudo ln -s /usr/bin/python3 /usr/bin/python
	```

3. 获取源码  

	```bash
	git clone https://github.com/mono/skia.git -b v2.88.4-preview.95
	```

4. 安装扩展  

	```bash
	cd skia
	python3 tools/git-sync-deps
	```
	
5. 修改编译错误  
	直接编译会报错：  

	```bash
	../../../src/utils/SkParseColor.cpp:300:39: error: no matching function for call to 'begin'
		const auto rec = std::lower_bound(std::begin(gColorNames),
	```
	可以通过以下方式修复报错：  

	```bash
	nano src/utils/SkParseColor.cpp
	然后在头部加上
	#include <array>
	```

6. 创建编译参数  

	```bash
	gn gen 'out/linux/loong64' --args='
	 target_os="linux"
	 target_cpu="loong64"
	 is_official_build=true
	 cc="gcc"
	 cxx="g++" 
	 skia_enable_tools=false
	 skia_use_harfbuzz=false
	 skia_use_icu=false
	 skia_use_piex=true
	 skia_use_sfntly=false
	 skia_use_system_expat=false
	 skia_use_system_freetype2=false
	 skia_use_system_libjpeg_turbo=false
	 skia_use_system_libpng=false
	 skia_use_system_libwebp=false
	 skia_use_system_zlib=false
	 skia_enable_skottie=true
	 skia_use_vulkan=false
	 skia_enable_pdf=false
	 skia_enable_gpu=false
	 extra_asmflags=[]
	 extra_cflags=[ "-DSKIA_C_DLL", "-DHAVE_SYSCALL_GETRANDOM", "-DXML_DEV_URANDOM" ]
	 extra_ldflags=[ "-static-libstdc++", "-static-libgcc" ]'
	```
	编译

	```bash
	ninja 'SkiaSharp' -C 'out/linux/loong64'
	```
	编译完成之后，libSkiaSharp.so在`out/linux/loong64`目录下



### 参考资料：  

[https://github.com/mono/SkiaSharp/blob/main/native/linux/build.cake#L112](https://github.com/mono/SkiaSharp/blob/main/native/linux/build.cake#L112)  

[https://github.com/mono/SkiaSharp/wiki/Building-on-Linux](https://github.com/mono/SkiaSharp/wiki/Building-on-Linux)  

