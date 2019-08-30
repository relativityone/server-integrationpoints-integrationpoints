<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$StashName,
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
	Write-Verbose "Beginning of Stash.ps1"

	git -C $Path stash save $StashName

	Fail-OnAnyErrors -CommandName "Stash.ps1"
	
	Write-Verbose "End of Stash.ps1"
}