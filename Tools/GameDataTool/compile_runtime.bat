set UnityEngine=../bin/UnityEngine.CoreModule.dll
set GameDataDefine=../bin/GameDataDefine.dll
set CommonUtils=../bin/CommonUtils.dll
call "C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe" -optimize -target:library /r:%UnityEngine% /r:%GameDataDefine% /r:%CommonUtils% /out:../bin/GameDataRuntime.dll /recurse:Runtime\*.cs