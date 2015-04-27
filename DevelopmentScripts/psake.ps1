Import-Module ..\Vendor\psake\tools\psake.psm1
Invoke-psake .\build.ps1 -properties @{"version"="1.3.3.7";"server_type"="local"}
