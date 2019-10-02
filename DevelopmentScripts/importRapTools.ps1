if (-not (Get-PsRepository -Name kCuraProgetPowershell -ErrorAction SilentlyContinue))
{
    $ProgetCredential = New-Object System.Management.Automation.PSCredential ("$ENV:ProgetUserName", (ConvertTo-SecureString $ENV:ProgetPassword -AsPlainText -Force))
    Register-PSRepository -Name kCuraProgetPowershell -InstallationPolicy Trusted -SourceLocation https://proget.kcura.corp/nuget/PowerShell -Credential $ProgetCredential
}

Save-Module -Name RAPTools -Path $PSScriptRoot -Force