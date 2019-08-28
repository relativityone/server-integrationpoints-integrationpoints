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
	if(!$Path)
	{
		$Path = "."
	}
}
Process
{
	Write-Verbose "Beginning of Get-CurrentBranchName.ps1"

	$currentBranch = git -C $Path rev-parse --abbrev-ref HEAD
	
	Write-Verbose "End of Get-CurrentBranchName.ps1"
	$currentBranch
}