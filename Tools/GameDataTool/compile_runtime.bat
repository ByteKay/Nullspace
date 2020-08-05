set UnityEngine=bin/UnityEngine.CoreModule.dll
set GameDataDefine=bin/GameDataDefine.dll
call "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" -target:library /r:%UnityEngine% /r:%GameDataDefine% /out:Dll/GameDataRuntime.dll /recurse:Runtime\*.cs