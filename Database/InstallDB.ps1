Param(
    [Parameter(Mandatory=$True)]
    $appVersion,
    [Parameter(Mandatory=$True)]
    $databaseServer,
    [Parameter(Mandatory=$True)]
    $databaseName,
    [Switch]
    $new)
	
function Get-ScriptDirectory
{
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}
	
$ScriptPath = Get-ScriptDirectory

$DDLDirectory = Join-Path "$ScriptPath" "Upgrade\DDL" -Resolve
$DataDirectory = Join-Path "$ScriptPath" "Upgrade\Data" -Resolve

if ($new) {
    sqlcmd -E -S $databaseServer -Q "IF NOT EXISTS (SELECT * FROM master.dbo.sysdatabases WHERE name = '$databaseName') CREATE DATABASE $databaseName"
    $DDLDirectory = Join-Path "$ScriptPath" "New\DDL" -Resolve
    $DataDirectory = Join-Path "$ScriptPath" "New\Data" -Resolve
}

$DDLQuery = Join-Path "$DDLDirectory\" "$appVersion*.sql"
$DataQuery = Join-Path "$DataDirectory\" "$appVersion*.sql"

Write-Host "Install/Update scripts will be applied to $databaseServer, database $databaseName"
Write-Host "Looking for DDL scripts with $DDLQuery"

$updateScripts = Get-ChildItem $DDLQuery | Sort-Object Name

foreach ($cUpdateScript in $updateScripts) {    
    $scriptToRun = $cUpdateScript.FullName
    
    if ($scriptToRun -eq $null) {
        continue;
    }
    
    Write-Host "Running script: $scriptToRun"
    sqlcmd -E -i $scriptToRun -S $databaseServer -d $databaseName
}

Write-Host "Looking for Data scripts with $DataQuery"

$updateScripts = Get-ChildItem $DataQuery | Sort-Object Name

foreach ($cUpdateScript in $updateScripts) {    
    $scriptToRun = $cUpdateScript.FullName
    
    if ($scriptToRun -eq $null) {
        continue;
    }
    
    Write-Host "Running script: $scriptToRun"
    sqlcmd -E -i $scriptToRun -S $databaseServer -d $databaseName
}

if ($new) {
    Write-Host "Creating a new database requires that you define an internal organization for Trifolia"
    $organizationName = Read-Host "Organization Name (short)"

    sqlcmd -E -S $databaseServer -d $databaseName -Q "INSERT INTO organization (name, isInternal) VALUES ('$organizationName', 1)"

    Write-Host "Creating new database requires an administrative user"
    $adminUserName = Read-Host "Username"
    $adminFirstName = Read-Host "First Name"
    $adminLastName = Read-Host "Last Name"
    $adminEmail = Read-Host "Email"
    $adminPhone = Read-Host "Phone"

    sqlcmd -E -S $databaseServer -d $databaseName -Q "INSERT INTO [user] (userName, organizationId, firstName, lastName, email, phone) SELECT '$adminUserName', id, '$adminFirstName', '$adminLastName', '$adminEmail', '$adminPhone' FROM organization WHERE name = '$organizationName'"
    sqlcmd -E -S $databaseServer -d $databaseName -Q "INSERT INTO [user_role] (userId, roleId) SELECT u.id, r.id FROM [user] u, [role] r WHERE u.userName = '$adminUserName' AND r.isAdmin = 1"
}

Write-Host "Done creating/upgrading database"