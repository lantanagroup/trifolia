Param(
    [Parameter(Mandatory=$True, Position=1)]
    [string] $sqlServerName,
    [Parameter(Mandatory=$True, Position=2)]
    [string] $databaseName = 'templatedb',
    [Parameter(Mandatory=$True, Position=3)]
    [string] $installDir,
    [Parameter(Position=5)]
    [string] $outDir = '.\'
)

Import-Module Sqlps -DisableNameChecking
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression

# $sqlServerName = "PRODUCTION3"
# $databaseName = "templatedb"
# $installDir = "E:\Websites\TdbManagement"
# $outDir = "c:\users\sean.mcilvenna"
$PSVersionTable.PSVersion
#write-host (resolve-path "FileSystem::$($installDir)").ProviderPath

$absOutDir = (Resolve-Path "FileSystem::$($outDir)" -ErrorAction Stop).ProviderPath
$absInstallDir = (Resolve-Path "FileSystem::$($installDir)" -ErrorAction Stop).ProviderPath
$dt = get-date -format yyyyMMddHHmmss
$sqlBackupFileName = $databaseName + "-" + $dt + ".bak"
$backupZipFileName = "Trifolia-" + $dt + ".zip"
$backupZipPath = [IO.Path]::GetFullPath($absOutDir + "\" + $backupZipFileName)

Write-Host "Backup ZIP will be saved to $backupZipPath"

$sqlServer = New-Object 'Microsoft.SqlServer.Management.SMO.Server' $sqlServerName
$serverBackupDir = $sqlServer.Settings.BackupDirectory

Write-Host "Server backup directory is: $serverBackupDir"

$networkBackupPath = "\\$sqlServerName\" + $serverBackupDir.Substring(0, 1) + '$\' + $serverBackupDir.Substring(3)
$networkBackupFilePath = $networkBackupPath + '\' + $sqlBackupFileName

Write-Host "Network backup directory is: $networkBackupPath"

## Backup the database
$dbbk = new-object ('Microsoft.SqlServer.Management.Smo.Backup')
$dbbk.Action = 'Database'
$dbbk.BackupSetDescription = "Full backup of " + $databaseName
$dbbk.BackupSetName = $databaseName + " Backup"
$dbbk.Database = $databaseName
$dbbk.MediaDescription = "Disk"
$dbbk.Devices.AddDevice($serverBackupDir + "\" + $sqlBackupFileName, 'File')
$dbbk.SqlBackup($sqlServer)

## Create the zip file
Write-Host "Creating backup zip file $backupZipPath"
$archiveMode = [System.IO.Compression.ZipArchiveMode]::Create
$zip = [System.IO.Compression.ZipFile]::Open($backupZipPath, $archiveMode)

## Get list of all core files in installation directory, excluding IIS logs
$coreFiles = Get-ChildItem "FileSystem::$($absInstallDir)" -File -Recurse | ?{ $_.fullname -notmatch '\\W3SVC\\?' }

## Add each core file to the backup ZIP
foreach ($file in $coreFiles) {
    $relativeFileName = $file.FullName.Replace($absInstallDir, 'Website')
	Write-Host "Saving " $file.FullName " to ZIP at $relativeFileName"
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $relativeFileName)
}

## Add the $networkBackupFilePath to the ZIP
[System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $networkBackupFilePath, $sqlBackupFileName)

## DONE
$zip.Dispose()
Write-Host "Backup saved to " $backupZipPath
