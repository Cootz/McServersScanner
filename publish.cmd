cd $1

dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true

cd McServersScanner\bin\Release\net6.0
mkdir publish-files

cd linux-x64\publish
tar -c -a -f ..\..\publish-files\linux-x64.zip *

cd ..\..osx-x64\publish
tar -c -a -f ..\..\publish-files\osx-x64.zip *

cd ..\..win-x64\publish
tar -c -a -f ..\..\publish-files\win-x64.zip *