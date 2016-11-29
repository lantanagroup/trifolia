Param(
    [Parameter(Mandatory=$True, HelpMessage='The location where the installation files are stored')]
    $rootPath='Dist',
    [Parameter(Mandatory=$True, HelpMessage='The location where Trifolia should be installed at. Recommend absolute path.')]
    $appServicePath='c:\\trifolia',
    [Parameter(Mandatory=$True, HelpMessage='The publicly available url of where Trifolia will be accessible from')]
    $appServiceBaseUrl='http://trifolia',
    [Parameter(HelpMessage='The SQL server host name (or alias) that Trifolia should connect to')]
    $DBHost='MSSQLSERVER', 
    [Parameter(HelpMessage='The SQL server database name that Trifolia should connect to on the db server')]
    $DBName='trifolia',
    [Parameter(HelpMessage='The validation key is used with sessions to encrypt the token used by forms authentication. This should be changed for production environments.')]
	$ValidationKey='87AC8F432C8DB844A4EFD024301AC1AB5808BEE9D1870689B63794D33EE3B55CDB315BB480721A107187561F388C6BEF5B623BF31E2E725FC3F3F71A32BA5DFC',
    [Parameter(HelpMessage='The decryption key is used with sessions to decrypt the token used by forms authentication. This should be changed for production environments')]
	$DecryptionKey='E001A307CCC8B1ADEA2C55B1246CDCFE8579576997FF92E7',
    [Parameter(HelpMessage='The location (absolute path) to the java executable as part of either the JRE or JDK. Java 6+ is required.')]
    $JavaLocation='c:\\Program Files\\Java\\jre6\\bin\\java.exe')

$sourceAppWebConfigPath = "$rootPath\Trifolia.Web\Web.config"
$destAppWebConfigPath = "$appServicePath\Web.config"
Write-Host "Getting main app's web.config at $sourceAppWebConfigPath"
$appWebConfig = Get-Content $sourceAppWebConfigPath

Write-Host ""

# Create the directories for the files or remove the previous files from the install
if (!([IO.Directory]::Exists($appServicePath))) {
    Write-Host "Main app service path not found at $appServicePath, creating new directory."
    New-Item -ItemType directory -Path $appServicePath
} else {
    Write-Host "Main app service path found at $appServicePath, removing files from directory."
    Remove-Item "$appServicePath\*" -recurse -Exclude "appSettings.user.config"
}

Write-Host ""

Write-Host "Copying main app service files ($rootPath\Trifolia.Web\*) to installation directory ($appServicePath)."
Copy-Item -Recurse $rootPath\Trifolia.Web\* $appServicePath

Write-Host ""

Write-Host "Updating main app service's web.config with correct parameters at $destAppWebConfigPath."
# Fix the web configs by replacing the keys with the proper values
$appWebConfig = $appWebConfig -replace "%DATABASE_SERVER%", $DBHost
$appWebConfig = $appWebConfig -replace "%DATABASE_NAME%", $DBName
$appWebConfig = $appWebConfig -replace "%JAVA_LOCATION%", $JavaLocation
$appWebConfig = $appWebConfig -replace "%VALIDATION_KEY%", $ValidationKey
$appWebConfig = $appWebConfig -replace "%DECRYPTION_KEY%", $DecryptionKey
Set-Content -Path $destAppWebConfigPath -value $appWebConfig
