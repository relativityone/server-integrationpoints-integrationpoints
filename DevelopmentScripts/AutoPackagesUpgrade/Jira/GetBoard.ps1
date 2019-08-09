<#  
.SYNOPSIS  
  
.DESCRIPTION  
  
.EXAMPLE 
#>  
  
[CmdletBinding()]  
Param(  
	[Parameter(Mandatory=$True)]  
	[pscredential]$Credential,  
	[Parameter(Mandatory=$True)]  
	[string]$BoardName 
)  
Begin
{
	. ".\Config.ps1"  
	. ".\Utils.ps1"  
}
Process  
{  
	Write-Verbose "Beginning of GetBoard.ps1"  
	
	$getBoardsJiraUri = "$JiraApiUri/agile/1.0/board?name=$BoardName"
  
	Write-Verbose $getBoardsJiraUri  
  
	$headers = GetBasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  
        $response = Invoke-RestMethod -Uri $getBoardsJiraUri -Method GET -Headers $headers -UseBasicParsing
	}  
    catch 
    {  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)" 
        Write-Error "GetBoard failed" -ErrorAction Stop 
	}  
    Write-Verbose "End of GetBoard.ps1"

    return $response.values
}