@echo off
for %%i in (*.proto) do protoc.exe --descriptor_set_out=./protobin/%%i.protobin --include_imports %%i
for %%i in (./protobin/*.protobin) do protogen.exe ./protobin/%%i
echo 协议生成完成
pause