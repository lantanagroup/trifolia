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
    $DecryptionKey='E001A307CCC8B1ADEA2C55B1246CDCFE8579576997FF92E7',
    [switch][Parameter(HelpMessage='Indicates that DB migrations should not be performed')]
    $NoMigrate,
    [switch][Parameter(HelpMessage='Do backup prior to install?')]
    $NoBackup,
    [Parameter(HelpMessage='Specify Zip file location')]
    $BackupOutDir='.\'
)

Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
$appServicePathExists = Test-Path $appServicePath
$shouldBackup = !$NoBackup

if ($shouldBackup -and $appServicePathExists) {
	$absInstallDir = [IO.Path]::GetFullPath($appServicePath)
	$dt = get-date -format yyyyMMddHHmmss
	$backupZipFileName = "Trifolia-" + $dt + ".zip"
	$backupZipRelativePath = Join-Path $BackupOutDir $backupZipFileName
	$backupZipPath = [IO.Path]::GetFullPath($backupZipRelativePath)
	
	## Create the zip file
	Write-Host "Creating backup zip file $backupZipPath"
	$archiveMode = [System.IO.Compression.ZipArchiveMode]::Create
	$zip = [System.IO.Compression.ZipFile]::Open($backupZipPath, $archiveMode)
	Write-Host "Backup ZIP will be saved to $backupZipPath"

	## Get list of all core files in installation directory, excluding IIS logs
	$coreFiles = Get-ChildItem $absInstallDir -File -Recurse | ?{ $_.fullname -notmatch '\\W3SVC\\?' }

	## Add each core file to the backup ZIP
	foreach ($file in $coreFiles) {
		$relativeFileName = $file.FullName.Replace($absInstallDir, 'Website')
		Write-Host "Saving " $file.FullName " to ZIP at $relativeFileName"
		[System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $relativeFileName) | Out-Null
	}
	
	## Backup the database
	Push-Location
	Import-Module Sqlps -DisableNameChecking

	$sqlBackupFileName = $DBName + "-" + $dt + ".bak"
	$sqlServer = New-Object 'Microsoft.SqlServer.Management.SMO.Server' $DBHost
	$serverBackupDir = $sqlServer.Settings.BackupDirectory

	if ($sqlServer.Databases[$DBName]) {
		Write-Host "SQL Server's backup directory is: $serverBackupDir"
	
		$networkShare = "\\$DBHost\" + $serverBackupDir.Substring(0, 1) + "$\"
		$networkBackupPath = Join-Path $networkShare $serverBackupDir.Substring(3)
		$networkBackupFilePath = Join-Path $networkBackupPath $sqlBackupFileName
		
		Write-Host "SQL Network backup directory is: $networkBackupPath"
		Write-Host "SQL Network backup file is: $networkBackupFilePath"

		## Backup the database
		$dbbk = new-object ('Microsoft.SqlServer.Management.Smo.Backup')
		$dbbk.Action = 'Database'
		$dbbk.BackupSetDescription = "Full backup of " + $DBName
		$dbbk.BackupSetName = $DBName + " Backup"
		$dbbk.Database = $DBName
		$dbbk.MediaDescription = "Disk"
		$dbbk.Devices.AddDevice($serverBackupDir + "\" + $sqlBackupFileName, 'File')
		$dbbk.SqlBackup($sqlServer)
		
		Write-Host "Done creating backup of database, adding to backup zip"

		## Add the $networkBackupFilePath to the ZIP
		[System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $networkBackupFilePath, $sqlBackupFileName) | Out-Null
	}
	
	## DONE
	$zip.Dispose()
	Pop-Location
	Write-Host "Backup saved to " $backupZipPath
	Read-Host "Please confirm the backup, then press <enter>."
}

$currentLocation = Get-Location
$sourcePath = Join-Path $currentLocation "Trifolia.Web\"
$sourceManifestPath = Join-Path $sourcePath 'manifest.txt'
$destManifestPath = Join-Path $appServicePath 'manifest.txt'

if (!([IO.Directory]::Exists($sourcePath))) {
    Write-Host "Install.ps1 does not appear to be in the correct distribution directory. Did not find Trifolia.Web directory"
    Exit
}

$sourceAppWebConfigPath = Join-Path $sourcePath "Web.config"
$destAppWebConfigPath = Join-Path $appServicePath "Web.config"
Write-Host "Getting main app's web.config at $sourceAppWebConfigPath"
$shouldWriteWebConfig = !(Test-Path $destAppWebConfigPath)

# Create the directory for the files to be stored in
if (!([IO.Directory]::Exists($appServicePath))) {
    Write-Host "Main app service path not found at $appServicePath, creating new directory."
    New-Item -ItemType directory -Path $appServicePath | Out-Null
}

$excludeWebConfig = Join-Path $sourcePath "Web.config"
Write-Host "Copying main app service files ($sourcePath) to installation directory ($appServicePath)."

# Compare the source files to the destination files and determine if any should be removed.
# For each file compared that is >= (exists in dest, but not in source), remove the file from the destination
if (Test-Path $destManifestPath) {
    $compare = Compare-Object -ReferenceObject (Get-Content $sourceManifestPath) -DifferenceObject (Get-Content $destManifestPath) -IncludeEqual
    $removeFiles = $compare | Where-Object { $_.SideIndicator -eq '=>' }
    $removeFiles | ForEach {
        $removeFilePath = Join-Path $appServicePath $_.InputObject

        Write-Host "Removing $removeFilePath. File is no longer in manifest."

        if (Test-Path $removeFilePath) {
            Remove-Item $removeFilePath
        }
    }
}

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

# If the web.config didn't exist in the destination at the beginning of the installation
# create it now, with the values specified in the installation command
if ($shouldWriteWebConfig) {
	Write-Host "Updating main app service's web.config with correct parameters at $destAppWebConfigPath."

    $appWebConfig = Get-Content $sourceAppWebConfigPath

	# Fix the web configs by replacing the keys with the proper values
	$appWebConfig = $appWebConfig -replace "%DATABASE_SERVER%", $DBHost
	$appWebConfig = $appWebConfig -replace "%DATABASE_NAME%", $DBName
	$appWebConfig = $appWebConfig -replace "%VALIDATION_KEY%", $ValidationKey
	$appWebConfig = $appWebConfig -replace "%DECRYPTION_KEY%", $DecryptionKey

	Set-Content -Path $destAppWebConfigPath -value $appWebConfig
}

if (!$NoMigrate) {
    Write-Host "Executing database installation/upgrade"
    $migrateBin = Join-Path $sourcePath "bin"

    $migrateCommand = "Trifolia.Web\bin\migrate.exe Trifolia.DB.dll /startupDirectory:$migrateBin /startUpConfigurationFile:$destAppWebConfigPath /verbose"
    Write-Host $migrateCommand
    Invoke-Expression $migrateCommand
}

Write-Host "Done installing Trifolia Workbench"
