Param(
    [Parameter(Mandatory=$True, HelpMessage='The location where Trifolia should be installed at. Recommend absolute path.')]
    $appServicePath='c:\\trifolia',
    [Parameter(HelpMessage='The SQL server host name (or alias) that Trifolia should connect to')]
    $DBHost='MSSQLSERVER', 
    [Parameter(HelpMessage='The SQL server database name that Trifolia should connect to on the db server')]
    $DBName='newtdb',
    [Parameter(HelpMessage='The validation key is used with sessions to encrypt the token used by forms authentication. This should be changed for production environments.')]
	$ValidationKey='87AC8F432C8DB844A4EFD024301AC1AB5808BEE9D1870689B63794D33EE3B55CDB315BB480721A107187561F388C6BEF5B623BF31E2E725FC3F3F71A32BA5DFC',
    [Parameter(HelpMessage='The decryption key is used with sessions to decrypt the token used by forms authentication. This should be changed for production environments')]
	$DecryptionKey='E001A307CCC8B1ADEA2C55B1246CDCFE8579576997FF92E7'
)

$currentLocation = Get-Location
$sourcePath = Join-Path $currentLocation "Trifolia.Web\"

if (!([IO.Directory]::Exists($sourcePath))) {
    Write-Host "Install.ps1 does not appear to be in the correct distribution directory. Did not find Trifolia.Web directory"
    Exit
}

$sourceAppWebConfigPath = Join-Path $sourcePath "Web.config"
$destAppWebConfigPath = Join-Path $appServicePath "Web.config"
Write-Host "Getting main app's web.config at $sourceAppWebConfigPath"
$appWebConfig = Get-Content $sourceAppWebConfigPath
$shouldWriteWebConfig = !(Test-Path $destAppWebConfigPath)


# Create the directory for the files to be stored in
if (!([IO.Directory]::Exists($appServicePath))) {
    Write-Host "Main app service path not found at $appServicePath, creating new directory."
    New-Item -ItemType directory -Path $appServicePath | Out-Null
}

$excludeWebConfig = Join-Path $sourcePath "Web.config"
Write-Host "Copying main app service files ($sourcePath) to installation directory ($appServicePath)."

# Get the list of source and destination files. Get a list excluding the root web.config since we deal with that separately.
$sourceFiles = Get-ChildItem $sourcePath -Recurse | where { ! $_.PSIsContainer }
$destFiles = Get-ChildItem $appServicePath -Recurse | where { ! $_.PSIsContainer }
$actualSourceFiles = $sourceFiles | Where { $_.FullName -notlike $excludeWebConfig }

# Go through each source file. Check if the directory exists in the destionation (if not create it).
# Copy the source file to the destination always, overwriting it with -Force if it already exists.
$actualSourceFiles | Foreach {
    $destFilePath = Join-Path $appServicePath $_.FullName.Substring($sourcePath.length-1)
    $destFile = [System.IO.FileInfo]$destFilePath
    if (!([IO.Directory]::Exists($destFile.DirectoryName))) {
        New-Item -ItemType directory -Path $destFile.DirectoryName | Out-Null
    }
    Copy-Item -Path $_.FullName -Destination $destFile -Force
}

# Compare the source files to the destination files and determine if any should be removed.
# For each file compared that is >= (exists in dest, but not in source), remove the file from the destination
if ($destFiles) {
    $compare = Compare-Object -ReferenceObject $sourceFiles -DifferenceObject $destFiles
    $removeFiles = $compare | Foreach { 
        if ($_.SideIndicator -eq '=>') {
            Remove-Item -Path $_.InputObject.FullName
        }
    }
}

# If the web.config didn't exist in the destination at the beginning of the installation
# create it now, with the values specified in the installation command
if ($shouldWriteWebConfig) {
	Write-Host "Updating main app service's web.config with correct parameters at $destAppWebConfigPath."
	# Fix the web configs by replacing the keys with the proper values
	$appWebConfig = $appWebConfig -replace "%DATABASE_SERVER%", $DBHost
	$appWebConfig = $appWebConfig -replace "%DATABASE_NAME%", $DBName
	$appWebConfig = $appWebConfig -replace "%VALIDATION_KEY%", $ValidationKey
	$appWebConfig = $appWebConfig -replace "%DECRYPTION_KEY%", $DecryptionKey
	Set-Content -Path $destAppWebConfigPath -value $appWebConfig
}

Write-Host "Executing database installation/upgrade"
$migrateBin = Join-Path $sourcePath "bin"
$migrateCmd = Join-Path $sourcePath "bin\migrate.exe"
$migrateDll = Join-Path $sourcePath "bin\Trifolia.DB.dll"

$migrateCommand = "$migrateCmd $migrateDll /StartUpDirectory=$migrateBin /startUpConfigurationFile:$destAppWebConfigPath /verbose"
Write-Host $migrateCommand
Invoke-Expression $migrateCommand

Write-Host "Done installing Trifolia Workbench"