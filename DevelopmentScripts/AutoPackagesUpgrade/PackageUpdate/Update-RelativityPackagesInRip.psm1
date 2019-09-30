<#  
.SYNOPSIS  
    Updates Relativity packages in Integration Points

.PARAMETER ToPackagesVersions
    Dictionary of package names and their versions to be updated within Integration Points

.PARAMETER RipSourceCodePath
    Path of Integration Points source code
#>

function Update-RelativityPackagesInRip
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]
        $ToPackagesVersions,

        [Parameter(Mandatory=$True)]
        [string]
        $RipSourceCodePath
    )

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils

    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"

    $paketDependenciesPath = Join-Path "$RipSourceCodePath" "paket.dependencies"
    $paketLockPath = Join-Path "$RipSourceCodePath" "paket.lock"
    $csProjsPath = Join-Path "$RipSourceCodePath" "*.csproj"
    $paketExePath = Join-Path "$RipSourceCodePath" ".paket\paket.exe"
    
    $packages = Get-Content -Path $paketDependenciesPath

    $ToPackagesVersions | ForEach-Object { $packages = Update-Package -PaketDependenciesAsText $packages -PackageName $_.name -ToVersion $_.version }

    try 
    {
        Set-Content -Path $paketDependenciesPath -Value $packages
    }
    catch 
    {
        Write-Error "Updating content of paket.dependencies failed with $($_.Exception.Message)" -ErrorAction Stop
    }

    Push-Location $RipSourceCodePath

    Write-Host "Starting paket update"

    try 
    {
        $paketLogs = & $paketExePath update
    }
    catch
    {
        Write-Error "Updating packages with Paket failed with $($_.Exception.Message)" -ErrorAction Stop
    }

    Write-Host "End of paket update"

    Write-Host $paketLogs

    Write-Host "Relativity version in RIP updated successfully in paket.dependencies from $oldVersion to $ToVersion" -ForegroundColor Cyan

    Pop-Location

    try 
    {
        git -C $RipSourceCodePath add $paketDependenciesPath
        git -C $RipSourceCodePath add $paketLockPath
        git -C $RipSourceCodePath add $csProjsPath
    }
    catch
    {
        Write-Error "Staging paket files failed with $($_.Exception.Message)" -ErrorAction Stop
    }

    try
    {
        $unstagedFiles = @(git diff --name-only)
        if($unstagedFiles.Length -gt 0)
        {
            Write-Host "Unstaged files found. $unstagedFiles" -ForegroundColor Yellow
            $logs = "\n\nUNSTAGED FILES:\n\n$unstagedFiles\n\n"
        }
    }
    catch
    {
        Write-Error "Determining unstaged files failed with $($_.Exception.Message)" -ErrorAction Stop
    }

    if($paketLogs.Length -ge $LogCharsLimit)
    {
        $paketLogs = $paketLogs.Substring($paketLogs.Length - $LogCharsLimit)
    }

    $logs += $paketLogs

    Write-Host "End of $($MyInvocation.MyCommand.Name)"
    
    $logs
}

function Update-Package($PaketDependenciesAsText, $PackageName, $ToVersion)
{
    $oldVersion = Find-CurrentPackageVersionInRip -PaketDependenciesAsText $PaketDependenciesAsText -PackageName $PackageName
    Set-PackageVersionInRip -PaketDependenciesAsText $PaketDependenciesAsText -PackageName $PackageName -OldVersion $oldVersion -NewVersion $ToVersion 
}

function Set-PackageVersionInRip($PaketDependenciesAsText, $PackageName, $OldVersion, $NewVersion)
{
    try
    {
        $PaketDependenciesAsText.replace("$PackageName $oldVersion", "$PackageName $NewVersion")
    }
    catch
    {
        Write-Error "Replacing Relativity versions in paket.dependencies from $oldVersion to $NewVersion failed with $($_.Exception.Message)" -ErrorAction Stop
    }
}

Export-ModuleMember -Function Update-RelativityPackagesInRip