<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$False)]
	[string]$Path
)
Begin
{
	. ".\Config.ps1"  
	. ".\Utils.ps1"

	if(!$Path)
	{
		$Path = "."
	}
}
Process
{
	Write-Verbose "Beginning of Get-CurrentBranchName.ps1"

	$currentBranch = git -C $Path rev-parse --abbrev-ref HEAD
	
	Fail-OnAnyErrors -CommandName "Get-CurrentBranchName.ps1"

	Write-Verbose "End of Get-CurrentBranchName.ps1"
	$currentBranch
}