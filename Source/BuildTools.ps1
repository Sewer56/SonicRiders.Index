# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location

# Build
dotnet publish ./RidersArchiveTool/RidersArchiveTool.sln -c Release -r win-x64 /p:PublishSingleFile=true /p:PublishReadyToRun=true --self-contained=false -o ./Build
dotnet publish ./RidersTextureArchiveTool/RidersTextureArchiveTool.sln -c Release -r win-x64 /p:PublishSingleFile=true /p:PublishReadyToRun=true --self-contained=false -o ./Build
dotnet publish ./IndexTool/IndexTool/IndexTool.sln -c Release -r win-x64 /p:PublishSingleFile=true /p:PublishReadyToRun=true --self-contained=false -o ./Build

# Restore Working Directory
Pop-Location