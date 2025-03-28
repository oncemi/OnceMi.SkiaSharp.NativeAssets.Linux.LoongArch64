# LoongArch64 编译 Skia
SkiaSharp 是基于 Google Skia 图形库 ( [skia.org](https://skia.org/) ) 的 .NET 平台跨平台 2D 图形 API。它提供了全面的 2D API，可用于跨移动、服务器和桌面模型来渲染图像。

目前暂不支持longarch64架构，需要自行编译依赖库。

### 基础准备
1. 安装依赖

```bash
sudo apt install -y gcc g++ gdb make cmake git ninja-build generate-ninja libx11-dev libxcursor-dev libfontconfig1-dev libgl1-mesa-dev

#安装OpenGL：sudo apt install libgl1-mesa-dev libgles2-mesa-dev libegl1-mesa-dev
```
可选
如果`use_system-xxx`为true的话，可能需要以下库

```bash
sudo apt install -y libjpeg-dev libharfbuzz-dev libwebp-dev
```


### 编译 2.88.x 版本
1. Python2处理
如果安装了python2，可以跳过这一步骤。
新世界操作系统（包括其他较新的发行版），都不再默认安装python2，默认安装python3。由于skia使用了python命令，会导致编译失败。还有种办法就是直接将python软链接到python3.

```bash
sudo ln -s /usr/bin/python3 /usr/bin/python
```
2. 获取源码

```bash
git clone https://github.com/mono/skia.git -b v2.88.4-preview.95
```
3. 安装扩展

```bash
cd skia
python3 tools/git-sync-deps
```
4. 修改编译错误
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

5. 创建编译参数

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
    skia_enable_pdf=false
    skia_enable_skottie=true
    extra_asmflags=[]
    extra_cflags=[ "-DSKIA_C_DLL", "-DHAVE_SYSCALL_GETRANDOM", "-DXML_DEV_URANDOM" ]
    extra_ldflags=[ "-static-libstdc++", "-static-libgcc" ]'
```
如果是旧世界操作系统，需要在`extra_cflags`和`extra_ldflags`添加`"-O2"`

编译

```bash
ninja 'SkiaSharp' -C 'out/linux/loong64'
```
编译完成之后，libSkiaSharp.so在`out/linux/loong64`目录下



### 编译 3.116.x 以上版本：
1. 获取源码

```bash
git clone https://github.com/mono/skia.git
```
2. 修改`git-sync-deps`

```bash
cd skia
nano tools/git-sync-deps
注释掉以下三行：

  #subprocess.check_call(
  #    [sys.executable,
  #     os.path.join(os.path.dirname(deps_file_path), 'bin', 'fetch-gn')])
```
这段代码的主要作用是下载、解压并配置 `gn` 工具。实际上安装完成ninja-build generate-ninja之后，就已经包含了gn工具了。

3. 安装扩展

```bash
python3 tools/git-sync-deps
```
4. 创建编译参数

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
    skia_enable_pdf=false
    skia_enable_skottie=true
    extra_asmflags=[]
    extra_cflags=[ "-DSKIA_C_DLL", "-DHAVE_SYSCALL_GETRANDOM", "-DXML_DEV_URANDOM" ]
    extra_ldflags=[ "-static-libstdc++", "-static-libgcc" ]'
```
编译

```bash
ninja 'SkiaSharp' -C 'out/linux/loong64'
```
编译完成之后，libSkiaSharp.so在`out/linux/loong64`目录下



### 参考资料
[https://github.com/mono/SkiaSharp/blob/main/native/linux/build.cake#L112](https://github.com/mono/SkiaSharp/blob/main/native/linux/build.cake#L112)

[https://github.com/mono/SkiaSharp/wiki/Building-on-Linux](https://github.com/mono/SkiaSharp/wiki/Building-on-Linux)
