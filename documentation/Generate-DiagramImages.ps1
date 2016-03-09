param (
    [string]$plantUmlLocation = "C:\ProgramData\chocolatey\lib\plantuml\tools\plantuml.jar"
)

& java -jar $plantUmlLocation -tpng -v -r -o .\images .\