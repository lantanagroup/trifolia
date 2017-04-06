$stu3Directory = 'C:\Users\sean.mcilvenna\Code\Trifolia-OS\Trifolia.Shared\FHIR\Profiles\STU3'
$stu3ProfilesPath = Join-Path $stu3Directory "profiles-resources.xml"
$fhirNamespace = @{ fhir = "http://hl7.org/fhir" }
$profilesXpath = "/fhir:Bundle/fhir:entry/fhir:resource/fhir:StructureDefinition"
$stu3Profiles = Select-Xml -Path $stu3ProfilesPath -Namespace $fhirNamespace -XPath $profilesXpath

$stu3Profiles | ForEach {
    $profileId = Select-Xml -Xml $_.Node -Namespace $fhirNamespace -XPath "fhir:id/@value"
    $profilePath = Join-Path $stu3Directory "profile-$profileId.xml"
    Set-Content -Path $profilePath -Value $_.Node.OuterXml
}