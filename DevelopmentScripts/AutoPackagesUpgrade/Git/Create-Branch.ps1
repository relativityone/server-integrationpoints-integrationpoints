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
	. ".\Config.ps1"  
	. ".\Utils.ps1"
	
	if(!$Path)
	{
		$Path = "."
	}
}
Process
{
	Write-Verbose "Beginning of Create-Branch.ps1"
	
	try
	{
		Write-Verbose "Clean"
		git -C $Path clean -dfx 
		Write-Verbose "GC Auto"
		git -C $Path gc --auto
		Write-Verbose "Checkout parent"
		.\Git\Checkout.ps1 -BranchName $ParentBranch -Path $Path
		Write-Verbose "Pull"
		.\Git\Pull.ps1 -Path $Path
		Write-Verbose "Checkout branch"
		git -C $Path checkout -b $BranchName
		Write-Verbose "Push"
		.\Git\Push.ps1 -BranchName $BranchName -Path $Path
	}
	catch
	{
		Write-Error "Creating branch failed with $($_.Exception.Message)" -ErrorAction Stop
	}

	Fail-OnAnyErrors -CommandName "Create-Branch.ps1"
	
	Write-Verbose "End of Create-Branch.ps1"
}