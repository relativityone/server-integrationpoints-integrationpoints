<#
.SYNOPSIS
Runs robocopy with a default set of flags.

.DESCRIPTION
Executes robocopy with a default set of flags (/MT /E /MOVE). Additional arguments can be added using the 'AdditionalFlags' parameter. All files in the destination directory and its subdirectories will be copied by default.
The Force flag will purge all binaries before copying files.

.OUTPUTS
Exit code of robocopy if -DryRun flag is set.

.EXAMPLE
Invoke-Robocopy -Source:"S:\SourceCode\Relativity" -Destination:"S:\NewRelPackage"

.EXAMPLE
Invoke-Robocopy -Source:"S:\SourceCode\Relativity" -Destination:"S:\NewRelPackage" -AdditionalFlags:"/MIR /XF *.config"

.EXAMPLE
Invoke-Robocopy -Source:"S:\SourceCode\Relativity" -Destination:"S:\NewRelPackage" -Files:"*.Services"
#>
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$Source,
    [Parameter(Mandatory=$true, Position=1)]
    [string]$Destination,
    [Parameter(Mandatory=$false, Position=2)]
    [string]$Files = "*.*",
    [Parameter(Mandatory=$false, Position=3)]
    [string]$AdditionalFlags,
    [Parameter()]
    [switch]$DryRun,
    [Parameter()]
    [switch]$FailWithEmptySource
)

$defaultRobocopyFlags = "/MT /E /MOVE /NFL /NP"

if ($DryRun)
{
    $AdditionalFlags += " /l"
    Write-Progress "Checking for differences between $Source and $Destination"
}
else
{
    Write-Progress "Copying $Source to $Destination"
}

# If -Verbose is present we write to the Verbose stream instead of Progress
if($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent)
{
    Invoke-Expression "robocopy '$Source' '$Destination' '$Files' $defaultRobocopyFlags $AdditionalFlags" | Write-Verbose
}
else 
{
    # Adding -Id 1 to Write-Progress gives us a separate progress feed for Robocopy
    Invoke-Expression "robocopy '$Source' '$Destination' '$Files' $defaultRobocopyFlags $AdditionalFlags" | ForEach-Object -Process `
    {
        # Whitespace at the end of Robocopy output will overwrite important messages so we exclude it
        if(-not [string]::IsNullOrEmpty($_))
        {
            Write-Progress -CurrentOperation $_ -Activity Robocopy -Id 1
        }
    }
    # Complete Robocopy progress so it doesn't stick around in Progress display
    Write-Progress -Activity Robocopy -Id 1 -Completed
}

$robocopyExitCode = $LASTEXITCODE

if ($DryRun)
{
    return $robocopyExitCode
}

# Handling the robocopy error situations outlined here: https://ss64.com/nt/robocopy-exit.html
if ($robocopyExitCode -ge 8)
{
    throw "Robocopy exited with $robocopyExitCode when copying $Source to $Destination. Check the Verbose log for more information."
}

if (@(2,3,6,7).Contains($robocopyExitCode))
{
    Write-Verbose "Some files were found in the destination, and not in the source. You may want to check the Verbose output if this is unexpected."
}

if (@(4,5,6,7).Contains($robocopyExitCode))
{
    Write-Verbose "Some files in the source were found in the destination as directories. You may want to check the Verbose output if this is unexpected."
}

if (@(0,2).Contains($robocopyExitCode))
{
    Write-Verbose "No files were copied. You may want to check the Verbose output if this is unexpected."
}