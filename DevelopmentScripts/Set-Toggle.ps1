[CmdletBinding()]
param(
    [Parameter(Mandatory = $True)]
    [String] $ServerName,
    
    [Parameter(Mandatory = $True)]
    [String] $SutUsername,
    
    [Parameter(Mandatory = $True)]
    [String] $SutPassword,

    [Parameter(Mandatory = $true)]
    [string]
    $SqlInstance,
 
    [Parameter(Mandatory = $true)]
    [string]
    $SqlUsername,

    [Parameter(Mandatory = $true)]
    [string]
    $SqlPassword,

    [Parameter(Mandatory = $true)]
    [string]
    $ToggleName,

    [Parameter(Mandatory = $true)]
    [boolean]
    $ToggleValue
)

$ToggleValueInt = if ($ToggleValue) { 1 } else { 0 }
$SwitchQuery = "USE EDDS;
DECLARE @toggleValue AS BIT
SET @toggleValue = $ToggleValueInt

DECLARE @toggleExists AS BIT
SET @toggleExists = (SELECT Count(*) FROM [EDDS].[eddsdbo].[Toggle] WHERE Name = '$ToggleName')

IF @toggleExists = 1
    UPDATE [EDDS].[eddsdbo].[Toggle] SET IsEnabled=@toggleValue WHERE Name = '$ToggleName'
ELSE
    INSERT INTO [EDDS].[eddsdbo].[Toggle] (Name, IsEnabled) VALUES ('$ToggleName', @toggleValue)

DECLARE @finalValue AS BIT
SET @finalValue = (SELECT IsEnabled FROM [EDDS].[eddsdbo].[Toggle] WHERE Name = '$ToggleName')
IF (@finalValue = @toggleValue)
    SELECT 'True'
ELSE
    SELECT 'False'"

$SutCredential = New-Object System.Management.Automation.PSCredential ("$SutUsername", (ConvertTo-SecureString $SutPassword -AsPlainText -Force))
$SutSession = New-PSSession -ComputerName $ServerName -Credential $SutCredential

Invoke-Command -Session $SutSession -ScriptBlock {
    param(
        [String] $Instance,
        [String] $Username,
        [String] $Password,
        [String] $Query
    )

    $QueryResult = Invoke-Sqlcmd -Query $Query -ServerInstance $Instance -Username $Username -Password $Password
    Write-Output "Successfully switched sync toggle?: $($QueryResult.Item(0))."

} -ArgumentList $SqlInstance, $SqlUsername, $SqlPassword, $SwitchQuery

