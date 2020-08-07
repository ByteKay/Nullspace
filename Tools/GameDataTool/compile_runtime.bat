set UnityEngine=bin/UnityEngine.CoreModule.dll
set GameDataDefine=bin/GameDataDefine.dll
call "C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe" -optimize -target:library /r:%UnityEngine% /r:%GameDataDefine% /out:Dll/GameDataRuntime.dll /recurse:Runtime\*.cs