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
	Write-Verbose "Beginning of Checkout.ps1"
	
	git -C $Path checkout $BranchName
	
	Fail-OnAnyErrors -CommandName "Checkout.ps1"

	Write-Verbose "End of Checkout.ps1"
}