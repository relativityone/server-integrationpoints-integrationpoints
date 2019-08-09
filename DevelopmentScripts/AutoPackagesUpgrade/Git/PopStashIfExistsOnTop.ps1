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
	if(!$Path)
	{
		$Path = "."
	}
}
Process
{
	Write-Verbose "Beginning of PopStashIfExistsOnTop.ps1"

	$exists = @(git -C $Path stash list)[0].Contains($StashName)

	if($exists)
	{
		Write-Verbose "Executing stash pop"
		git -C $Path stash pop 'stash@{0}'
	}
	
	Write-Verbose "End of PopStashIfExistsOnTop.ps1"
}