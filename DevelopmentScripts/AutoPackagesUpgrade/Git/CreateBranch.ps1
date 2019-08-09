<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$ParentBranch,
	[Parameter(Mandatory=$True)]
	[string]$BranchName,
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
	Write-Verbose "Beginning of CreateBranch.ps1"
	
	#Write-Verbose "Clean"
	#git -C $Path clean -dfx 
	Write-Verbose "GC Auto"
	git -C $Path gc --auto
	Write-Verbose "Checkout parent"
	git -C $Path checkout $ParentBranch
	Write-Verbose "Pull"
	git -C $Path pull
	Write-Verbose "Checkout branch"
	git -C $Path checkout -b $BranchName
	Write-Verbose "Push"
	git -C $Path push origin $BranchName
	
	Write-Verbose "End of CreateBranch.ps1"
}