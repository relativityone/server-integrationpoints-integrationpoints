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
	
	Write-Verbose "End of Commit.ps1"
}