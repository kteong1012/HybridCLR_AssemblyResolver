## 用于检查热更程序集是否引用了被裁剪的AOT类型

help:
-aot_dir <directory_path> : directory of aot which hotupdate dlls depend on
-hot_files <dll1_path>;<dll2_path> : hotupdate dlls, split by ';'
"

example:
```batch
set PLATFORM=Android
set AOT_DIR=UnityProject/HybridCLRData/AssembliesPostIl2CppStrip/%PLATFORM%
set HOT_FILES=UnityProject/HybridCLRData/HotUpdateDlls/%PLATFORM%/HotUpdate.dll

dotnet AssemblyResolver.dll -aot_dir %AOT_DIR% -hot_files %HOT_FILES%
```
