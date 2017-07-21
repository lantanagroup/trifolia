Param(
    [string]$Major,
    [string]$Minor,
    [string]$Patch
)

if (!$Major -or !$Minor -or !$Patch) {
    Write-Host "No new version specified"
    exit;
}

$assemblyInfoFiles = Get-ChildItem -Path .\ -Filter AssemblyInfo.cs -Recurse -ErrorAction SilentlyContinue -Force
$newAssemblyVersion = "[assembly: AssemblyVersion(""" + $Major + "." + $Minor + "." + $Patch + ".0"")]"
$newAssemblyFileVersion = "[assembly: AssemblyFileVersion(""" + $Major + "." + $Minor + "." + $Patch + ".0"")]"
$newAppveyorVersion = "version: $Major.$Minor.$Patch-{build}"

Write-Host "AssemblyInfo value: " $newAssemblyVersion
Write-Host "AppVeyor value: " $newAppveyorVersion

# Update the AssemblyInfo.cs files
ForEach ($assemblyInfoFile in $assemblyInfoFiles) {
    $assemblyInfo = Get-Content $assemblyInfoFile.FullName -Encoding UTF8

    Write-Host "Updating assembly file" $assemblyInfoFile.FullName

    $assemblyInfo = $assemblyInfo -replace "\[assembly: AssemblyVersion\("".*?""\)\]", $newAssemblyVersion 
    $assemblyInfo = $assemblyInfo -replace "\[assembly: AssemblyFileVersion\("".*?""\)\]", $newAssemblyFileVersion

    Set-Content -Path $assemblyInfoFile.FullName -Value $assemblyInfo -Encoding UTF8
}

# Update the appVeyor YAML
if (Test-Path ".\appveyor.yml") {
    $appVeyorYaml = Get-Content ".\appveyor.yml" -Encoding UTF8

    if ($appVeyorYaml -match "version: ((\d+)[\.-])+({build})?") {
        Write-Host "Updating appVeyor.yml"
        $appVeyorYaml = $appVeyorYaml -replace "version: ((\d+)[\.-])+({build})?", $newAppveyorVersion
        Set-Content -Path ".\appveyor.yml" -Value $appVeyorYaml -Encoding UTF8
    } else {
        Write-Host "Not updating appVeyor.yml"
    }
}

exit;