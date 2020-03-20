node('role-build-agent')
{
    try
    {
        echo System.getProperty("user.dir")
        powershell "Write-Host (Get-Item -Path '.\').FullName"
        powershell "Get-Location"
        powershell "Get-ChildItem"
        powershell(script: "& '.\\Trident\\Scripts\\updateSplunkDashboard.ps1'")
    }
    catch (err)
    {
        echo err.toString()
        currentBuild.result = "FAILED"
    }
    finally
    {

    }    
}