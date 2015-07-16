Import-Module ..\Vendor\psake\tools\psake.psm1

$BUILDCONFIG = 'Debug'
$BUILDTYPE = 'DEV'
$VERSION = '1.0.0.0'
$COMPANY = 'kCura LLC'
$PRODUCT = 'Template'
$PRODUCTDESCRIPTION = 'Template repo for kCura'

#
# Uncomment one or more lines below to execute. Run in Windows Powershell ISE to debug
#

#Invoke-psake .\version.ps1 -properties @{'version'=$VERSION;'company'=$COMPANY;'product'=$PRODUCT;'product_description'=$PRODUCTDESCRIPTION;}
#Invoke-psake .\build.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
#Invoke-psake .\application.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
#Invoke-psake .\test.ps1
#Invoke-psake .\nuget.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
#Invoke-psake .\package.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
#Invoke-psake .\psake-email.ps1 -properties @{'buildid'=72966;}

