param(
    [Parameter(Mandatory=$true)][string]$Target,
    [Parameter(Mandatory=$true)][string]$ProjectName
)

# Définir les chemins racine et types
$Root = $PSScriptRoot + "/../"
$TypesIndex = $PSScriptRoot + "\..\types\index.json"

# Construire les différentes versions
dotnet publish --configuration release --self-contained --runtime osx-arm64 $Root
dotnet publish --configuration release --self-contained --runtime linux-x64 $Root
dotnet publish --configuration release --self-contained --runtime win-x64 $Root

# Publier sur le registre
& ~/.azure/bin/bicep publish-extension `
  $TypesIndex `
  --bin-osx-arm64 "$Root\bin\release\net8.0\osx-arm64\publish\$ExtensionName" `
  --bin-linux-x64 "$Root\bin\release\net8.0\linux-x64\publish\$ExtensionName" `
  --bin-win-x64 "$Root\bin\release\net8.0\win-x64\publish\$ExtensionName.exe" `
  --target $Target `
  --force
