set UnityEngine=../bin/Debug/UnityEngine.CoreModule.dll
set Runtime=Runtime
set DataFormatPath=%Runtime%/DataFormat
set Properties=%DataFormatPath%/Properties
set Stream=%DataFormatPath%/Stream
set XML=%DataFormatPath%/XML
set DataLoader=%Runtime%/DataLoader
set scripts=%Runtime%/*.cs %Properties%/*.cs %Stream%/*.cs %XML%/*.cs %DataLoader%/*.cs
call "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" -target:library /r:%UnityEngine% /out:../Dll/GameDataRuntime.dll *.cs /recurse:*.cs
pause