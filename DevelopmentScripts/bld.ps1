Import-Module ..\Vendor\psake\tools\psake.psm1

$BUILDCONFIG = "Debug"
$BUILDTYPE = "DEV"
$VERSION = "1.0.0.0"
$COMPANY = "kCura LLC"
$PRODUCT = "Template"
$PRODUCTDESCRIPTION = "Template repo for kCura"
$BUILD = $true
$APPS = $true
$TEST = $false
$NUGET = $false
$PACKAGE = $false

$SHOWHELP = $false


for ($i = 0; $i -lt $args.count; $i++){

    switch -Regex ($args[$i]){
        "^[/-]v"      {$VERSION = $args[$i + 1]; $i++}
        "^[/-]a"      {$BUILD   = $false}
        "^[/-]no"     {$APPS    = $false}
        "^[/-]t"      {$TEST    = $true}
        "^[/-]nu"     {$NUGET   = $true}
        "^[/-]p"      {$PACKAGE = $true}       
                
        "^debug$"   {$BUILDCONFIG = "Debug"}
        "^release$" {$BUILDCONFIG = "Release"}

        "^dev$"   {$BUILDTYPE = "DEV"}
        "^alpha$" {$BUILDTYPE = "ALPHA"}
        "^beta$"  {$BUILDTYPE = "BETA"}
        "^rc$"    {$BUILDTYPE = "RC"}
        "^gold$"  {$BUILDTYPE = "GOLD"}

        "\?"   {$SHOWHELP = $true}
        "help" {$SHOWHELP = $true}
    }
}

write-host "buildconfig is" $BUILDCONFIG
write-host "buildtype   is" $BUILDTYPE
write-host "version     is" $VERSION
write-host "build   step is set to" $BUILD
write-host "apps    step is set to" $APPS
write-host "test    step is set to" $TEST
write-host "nuget   step is set to" $NUGET
write-host "package step is set to" $PACKAGE

if($SHOWHELP) {

Write-Host ""
write-host "Use this script to peform a full build of all projects."
write-host "This build is the same as the build that happens on the TeamCity server. "
write-host ""
write-host "usage: build [debug|release] [dev|alpha|beta|rc|gold] [-version VERSION] [-apps] [-noapps] [-test] [-nuget] [-package]"
write-host ""
write-host "options:"
write-host ""
write-host "-v[ersion]       sets the version # for the build, default is 1.0.0.0 (example: 1.3.3.7)"  
write-host "-a[pps]          skips the build step, continues to only build apps"
write-host "-no[apps]        skips build apps step"
write-host "-t[est]          runs nunit test step"
write-host "-nu[get]         runs the nuget pack step"
write-host "-p[ackage]       runs the package step"
Write-Host ""           

exit

}

if($VERSION -ne "1.0.0.0") {

    if($VERSION -eq "latest") {Invoke-psake .\psake-get-version.ps1 -properties @{'server_type'='local';'product'='%PRODUCT%';};}

    Invoke-psake .\psake-version.ps1 -properties @{'version'=$VERSION;'company'=$COMPANY;'product'=$PRODUCT;'product_description'=$PRODUCTDESCRIPTION;}

    
}

#Invoke-psake .\psake-build.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
#Invoke-psake .\psake-application.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
#Invoke-psake .\psake-test.ps1
#Invoke-psake .\psake-nuget.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
#Invoke-psake .\psake-package.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
#Invoke-psake .\psake-email.ps1 -properties @{'buildid'='74271';'root'='C:\SourceCode\Mainline';}