Param(
    [Parameter(Mandatory=$True)]
    $rootPath='Dist',
    [Parameter(Mandatory=$True)]
    $appServicePath='c:\\trifolia',
    [Parameter(Mandatory=$True)]
    $appServiceBaseUrl='http://trifolia', 
    $ADConnection='TestADConnectionString', 
    $ADUsername='TestADUser', 
    $ADPassword='TestADPass', 
    $DBHost='MSSQLSERVER', 
    $DBName='trifolia', 
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
    Remove-Item "$appServicePath\*" -recurse
}

Write-Host ""

Write-Host "Copying main app service files ($rootPath\Trifolia.Web\*) to installation directory ($appServicePath)."
Copy-Item -Recurse $rootPath\Trifolia.Web\* $appServicePath

Write-Host ""

Write-Host "Updating main app service's web.config with correct parameters at $destAppWebConfigPath."
# Fix the web configs by replacing the keys with the proper values
$appWebConfig = $appWebConfig -replace "%AD_CONNECTION_STRING%", $ADConnection
$appWebConfig = $appWebConfig -replace "%AD_USERNAME%", $ADUsername
$appWebConfig = $appWebConfig -replace "%AD_PASSWORD%", $ADPassword
$appWebConfig = $appWebConfig -replace "%DATABASE_SERVER%", $DBHost
$appWebConfig = $appWebConfig -replace "%DATABASE_NAME%", $DBName
$appWebConfig = $appWebConfig -replace "%JAVA_LOCATION%", $JavaLocation
Set-Content -Path $destAppWebConfigPath -value $appWebConfig