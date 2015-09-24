$root = hg root
Import-Module $root\Vendor\psake\tools\psake.psm1

$BUILDCONFIG = "Debug"
$BUILDTYPE = "DEV"
$VERSION = "1.0.0.0"
$EDITOR = $false
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

        "^[/-]e" {$EDITOR = $true} 
    }
}

write-host "buildconfig is" $BUILDCONFIG
write-host "buildtype   is" $BUILDTYPE
write-host "version     is" $VERSION
write-host "show editor is" $EDITOR
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
write-host "-e[ditor]        opens Build Helper Project Editor to edit the build.xml file" 
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

if($EDITOR) {

    Invoke-psake $root\DevelopmentScripts\psake-editor.ps1 

    exit
}

if($VERSION -ne "1.0.0.0") {

    if($VERSION -eq "latest") {
        Invoke-psake  $root\DevelopmentScripts\psake-get-version.ps1 -properties @{'server_type'='local';};
	if ($psake.build_success -eq $false) { exit 1 }  
	
        $VERSION = [System.IO.File]::ReadAllText([System.IO.Path]::Combine($root, 'DevelopmentScripts', 'version.txt'))
    }

    Invoke-psake $root\DevelopmentScripts\psake-version.ps1 -properties @{'version'=$VERSION;
                                                                          'server_type'='local';
                                                                          'build_config'=$BUILDCONFIG;
                                                                          'build_type'=$BUILDTYPE;} 
    if ($psake.build_success -eq $false) { exit 1 }   
}

if($BUILD){
    Invoke-psake $root\DevelopmentScripts\psake-build.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
    if ($psake.build_success -eq $false) { exit 1 }
}

if($APPS){
    Invoke-psake $root\DevelopmentScripts\psake-application.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
    if ($psake.build_success -eq $false) { exit 1 }
}

if($TEST){
    Invoke-psake $root\DevelopmentScripts\psake-test.ps1
    if ($psake.build_success -eq $false) { exit 1 }
}

if($NUGET){
    Invoke-psake $root\DevelopmentScripts\psake-nugetpack.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
    if ($psake.build_success -eq $false) { exit 1 }
}

if($PACKAGE){
    Invoke-psake $root\DevelopmentScripts\psake-package.ps1 -properties @{'version'=$VERSION;'server_type'='local';'build_config'=$BUILDCONFIG;'build_type'=$BUILDTYPE;}
    if ($psake.build_success -eq $false) { exit 1 }
}
