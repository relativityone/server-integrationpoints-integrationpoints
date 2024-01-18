function Assert-Module
{
    [CmdletBinding()]
    param(
        [string]$Name,
        [System.Version]$Version,
        [string]$Path
    )

    $moduleFolder = "$Path\$Name\$Version"
    $loadedModule = Get-Module -Name $Name | Where-Object { $PSItem.Version -eq "$Version" }
    if(-not $loadedModule)
    {
        if((-not (Test-Path $moduleFolder)))
        {
            Save-Module -Name $Name -RequiredVersion $Version -Path $Path -Force -ErrorAction Stop
        }

        $modulePath = ((Get-ChildItem -Path $moduleFolder -Recurse -Filter "$Name.psd1")[0].FullName)
        Import-Module $modulePath -Global -ErrorAction Stop
    }
}

Function Install-NugetPackage
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, HelpMessage="Name of the Nuget package")]
        [string]$Name,
 
        [Parameter(Mandatory=$true, HelpMessage="Version of the Nuget package")]
        [string]$Version,
 
        [Parameter(Mandatory=$false, HelpMessage="Location of nuget.exe")]
        [string]$NuGetEXE,
 
        [Parameter(Mandatory=$true, HelpMessage="Location of the BuildTools folder")]
        [string]$ToolsDir
    )

    if ([string]::IsNullOrEmpty($NuGetEXE)) {
        $NuGetEXE = [System.IO.Path]::Combine($ToolsDir, "nuget.exe")
    }

    if (-not(Test-Path -Path $NuGetEXE)) {
        Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/v6.5.0/nuget.exe" -OutFile $NuGetEXE
    }
 
    & $NuGetEXE install $Name -Version $Version -OutputDirectory $ToolsDir -Verbosity "normal"
    if ($LASTEXITCODE -ne 0) {
        Throw "An error occurred while restoring packages in the buildtools directory."
    }
}
