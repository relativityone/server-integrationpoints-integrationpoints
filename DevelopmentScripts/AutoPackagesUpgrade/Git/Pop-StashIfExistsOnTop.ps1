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
	Write-Verbose "Beginning of Pop-StashIfExistsOnTop.ps1"

	$stashes = @(git -C $Path stash list)

	if($stashes.Length -gt 0)
	{
		$exists = $stashes[0].Contains($StashName)

		if($exists)
		{
			Write-Verbose "Executing stash pop"
			git -C $Path stash pop 'stash@{0}'
		}
	}
	
	Write-Verbose "End of Pop-StashIfExistsOnTop.ps1"
}