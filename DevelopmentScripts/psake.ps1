Import-Module ..\Vendor\psake\tools\psake.psm1

properties {

$BUILDCONFIG = 'Debug'
$BUILDTYPE = 'DEV'
$SERVERTYPE = 'local'
$VERSION = '1.0.0.0'
$COMPANY = 'Relativity ODA LLC'
$PRODUCT = 'Template'
$PRODUCTDESCRIPTION = 'Template repo for Relativity'
$PACKAGEROOT = 'C:\Packages'
$ENABLEINJECTIONS = $false
$SKIP_PUBLISH = $false

}

task default -depends runsteps

task runsteps {

Invoke-psake .\psake-version.ps1 -properties @{'version'=$VERSION;
                                               'server_type'=$SERVERTYPE;
                                               'build_config'=$BUILDCONFIG;
                                               'build_type'=$BUILDTYPE;
                                               'company'=$COMPANY;
                                               'product'=$PRODUCT;
                                               'product_description'=$PRODUCTDESCRIPTION;}
if ($psake.build_success -eq $false) { exit 1 }  

Invoke-psake .\psake-build.ps1 -properties @{'version'=$VERSION;
                                             'server_type'=$SERVERTYPE;
                                             'build_config'=$BUILDCONFIG;
                                             'build_type'=$BUILDTYPE;
                                             'enable_injections'=$ENABLEINJECTIONS;}
if ($psake.build_success -eq $false) { exit 1 }  

Invoke-psake .\psake-application.ps1 -properties @{'version'=$VERSION;
                                                   'server_type'=$SERVERTYPE;
                                                   'build_config'=$BUILDCONFIG;
                                                   'build_type'=$BUILDTYPE;}
if ($psake.build_success -eq $false) { exit 1 }  

Invoke-psake .\psake-test.ps1
if ($psake.build_success -eq $false) { exit 1 }  

Invoke-psake .\psake-nugetpack.ps1 -properties @{'version'=$VERSION;
                                                 'server_type'=$SERVERTYPE;
                                                 'build_config'=$BUILDCONFIG;
                                                 'build_type'=$BUILDTYPE;
                                                 'company'=$COMPANY;}
if ($psake.build_success -eq $false) { exit 1 }  

if ($SKIP_PUBLISH -eq $false) {

Invoke-psake .\psake-nugetpublish.ps1 -properties @{'version'=$VERSION;
                                                    'server_type'=$SERVERTYPE;
                                                    'build_config'=$BUILDCONFIG;
                                                    'build_type'=$BUILDTYPE;}
if ($psake.build_success -eq $false) { exit 1 }  

}

Invoke-psake .\psake-builddoc.ps1 -properties @{'version'=$VERSION;
                                                'server_type'=$SERVERTYPE;
                                                'build_config'=$BUILDCONFIG;
                                                'build_type'=$BUILDTYPE;}
if ($psake.build_success -eq $false) { exit 1 }  

Invoke-psake .\psake-package.ps1 -properties @{'version'=$VERSION;
                                               'product'=$PRODUCT;
                                               'package_root_directory'=$PACKAGEROOT;
                                               'server_type'=$SERVERTYPE;
                                               'build_config'=$BUILDCONFIG;
                                               'build_type'=$BUILDTYPE;}
if ($psake.build_success -eq $false) { exit 1 }  

}
