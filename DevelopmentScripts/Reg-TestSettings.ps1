[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [String]
    $RegEnv
)

Remove-Item (Join-Path $PSScriptRoot ..\FunctionalTestSettings) -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $PSScriptRoot ..\FunctionalTest.runsettings) -Force -ErrorAction SilentlyContinue

$RegressionRunSettingsFilePath = ".\DevelopmentScripts\Regression\$RegEnv.runsettings"
if (Test-Path $RegressionRunSettingsFilePath)
{
    [xml]$runSettingsDocument = Get-Content $RegressionRunSettingsFilePath
    $runSettings = $runSettingsDocument.SelectSingleNode("//RunSettings")
    $testRunParameters = $runSettings.SelectSingleNode("//TestRunParameters")
    $additionalParameters = $testRunParameters.ChildNodes.name
} else 
{
    [xml]$runSettingsDocument = New-Object System.Xml.XmlDocument
    $runSettings = $runSettingsDocument.AppendChild($runSettingsDocument.CreateNode("element", "RunSettings", $null))
    $testRunParameters = $runSettings.AppendChild($runSettingsDocument.CreateNode("element", "TestRunParameters", $null))
}

foreach($parameterKey in $PSBoundParameters.Keys)
{
    if (@("RegEnv") -notcontains $parameterKey)
    {
        if ($additionalParameters -contains $parameterKey)
        {
            $parameter = $testRunParameters.SelectSingleNode("//Parameter[@name='$parameterKey']")
        } else 
        {
            $parameter = $testRunParameters.AppendChild($runSettingsDocument.CreateNode("element", "Parameter", $null))
        }
        $parameter.SetAttribute("name", $parameterKey)
        $parameter.SetAttribute("value", $PSBoundParameters[$parameterKey])
    }
}

foreach($parameter in $testRunParameters.ChildNodes)
{
    Add-Content (Join-Path $PSScriptRoot ..\FunctionalTestSettings) "--params `"$($parameter.Name)=$($parameter.Value)`""
}

$runSettingsDocument.Save((Join-Path $PSScriptRoot ..\FunctionalTest.runsettings))
