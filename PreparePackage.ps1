Param(
    [Parameter(HelpMessage='The build configuration to package (ex: "Install Release")')]
    $buildConfig='Install Release'
)

$sourcePath = Get-Location
$distPath = Join-Path $sourcePath "Dist\"
$buildPath = Join-Path $sourcePath "Trifolia.Web\obj\$buildConfig"
$sourceAppPath = Join-Path $buildPath "Package\PackageTmp\"
$distAppPath = Join-Path $distPath "Trifolia.Web"
$sourceWebConfigPath = Join-Path $buildPath "TransformWebConfig\transformed\Web.config"
$distWebConfigPath = Join-Path $distPath "Trifolia.Web\Web.config"
$sourceMigrateToolPath = Join-Path $sourcePath "packages\EntityFramework.6.1.3\tools\migrate.exe"
$distMigrateToolPath = Join-Path $distPath "Trifolia.Web\bin\migrate.exe"
$appManifestPath = Join-Path $distAppPath "manifest.txt"

if (([IO.Directory]::Exists($distPath))) {
    Remove-Item -Recurse -Force $distPath
}

New-Item -ItemType directory -Path $distPath | Out-Null

Copy-Item -Path $sourceAppPath -Destination $distAppPath -Recurse
Copy-Item -Path $sourceWebConfigPath -Destination $distWebConfigPath
Copy-Item -Path $sourceMigrateToolPath -Destination $distMigrateToolPath
Copy-Item -Path ".\*.ps1" -Destination $distPath
Get-ChildItem $distAppPath -Recurse -File -Name > $appManifestPath