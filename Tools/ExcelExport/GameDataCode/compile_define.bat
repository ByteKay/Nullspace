set UnityEngine=../UnityEngine.CoreModule.dll
set GameDataRuntime=../GameDataRuntime.dll
call "C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe" -target:library /r:%UnityEngine% /r:%GameDataRuntime% /out:../GameDataDefine.dll *.cs /recurse:*.cs
pause