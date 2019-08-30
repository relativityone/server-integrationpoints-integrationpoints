<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$JiraNumber,
	[Parameter(Mandatory=$True)]
	[string]$Message,
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
	Write-Verbose "Beginning of Commit.ps1"

	$commitMessage = "$JiraNumber $Message"
	git -C $Path commit -m $commitMessage
	
	Fail-OnAnyErrors -CommandName "Commit.ps1"

	Write-Verbose "End of Commit.ps1"
}