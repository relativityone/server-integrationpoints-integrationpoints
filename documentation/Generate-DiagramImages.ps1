param (
    [string]$plantUmlLocation
)

if ($plantUmlLocation -eq "") {
    $commonToolsJarExists = Test-Path Env:\COMMONTOOLS
    if ($commonToolsJarExists) {
			
        $plantUmlLocation = "$commonToolsLocation\DevDocs\plantuml.jar"
    } else {
        $plantUmlLocation = "C:\ProgramData\chocolatey\lib\plantuml\tools\plantuml.jar"
    }
}

$plantUmlLocationIsValid = Test-Path $plantUmlLocation
if ($plantUmlLocationIsValid) {
    & java -jar $plantUmlLocation -tpng -v -r -o .\images .\*
} else {
    Write-Host "ERROR: Invalid plantUmlLocation -- $plantUmlLocation"
}

