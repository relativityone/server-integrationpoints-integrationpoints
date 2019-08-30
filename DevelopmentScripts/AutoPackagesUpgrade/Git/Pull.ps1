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
	Write-Verbose "Beginning of Pull.ps1"
	
    git -C $Path pull
    
    Fail-OnAnyErrors -CommandName "Pull.ps1"
	
	Write-Verbose "End of Pull.ps1"
}