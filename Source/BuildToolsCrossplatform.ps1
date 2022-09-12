# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location

# Build
dotnet publish ./RidersArchiveTool/RidersArchiveTool.sln -c Release --self-contained=false -o ./Build/CrossPlatform
dotnet publish ./RidersTextureArchiveTool/RidersTextureArchiveTool.sln -c Release --self-contained=false -o ./Build/CrossPlatform
dotnet publish ./IndexTool/IndexTool/IndexTool.sln -c Release --self-contained=false -o ./Build/CrossPlatform
dotnet publish ./LayoutToJsonTool/LayoutToJson/LayoutToJson.sln -c Release --self-contained=false -o ./Build/CrossPlatform
dotnet publish ./GcaxDatInjector/GcaxDatInjector.sln -c Release --self-contained=false -o ./Build/CrossPlatform

# Restore Working Directory
Pop-Location