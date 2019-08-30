<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$BranchName,
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
	Write-Verbose "Beginning of Push.ps1"
	
	git -C $Path push origin $BranchName

	Fail-OnAnyErrors -CommandName "Push.ps1"
	
	Write-Verbose "End of Push.ps1"
}