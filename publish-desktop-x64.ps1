dotnet restore
dotnet build .\GBLive.sln -c Release --no-restore --nologo
dotnet publish .\src\GBLive.csproj -c Release -r win10-x64 /p:PublishSingleFile=true --no-self-contained --no-restore