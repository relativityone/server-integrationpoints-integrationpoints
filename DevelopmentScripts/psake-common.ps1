properties {
    #directories
    $root = hg root

    $source_directory = [System.IO.Path]::Combine($root, 'Source')
    $application_directory = [System.IO.Path]::Combine($source_directory, 'Applications')
    $application_xml_directory = [System.IO.Path]::Combine($source_directory, 'ApplicationsXML')
    $development_scripts_directory = [System.IO.Path]::Combine($root, 'DevelopmentScripts')
    $version_directory = [System.IO.Path]::Combine($root, 'Version')
    $vendor_directory = [System.IO.Path]::Combine($root, 'Vendor')
    $robot_directory = [System.IO.Path]::Combine($root, 'Robot')
    $testlog_directory = [System.IO.Path]::Combine($root, 'TestLogs')

    #build variables
    $version = '1.0.0.0'
    $server_type = 'teambranch'
    $build_type = 'DEV'
    $branch = hg branch
    $build_config = "Debug"

    #assembly info variables
    $company = 'kCura LLC'
    $product = [System.IO.Path]::GetFileName($root)
    $product_description =  [System.IO.Path]::GetFileName($root) + ' Description'

    #versioning database info
    $server = 'bld-mstr-01.kcura.corp'
    $database ='TCBuildSemanticVersion'
    $project = 'Development'
    $major_version = 1
    $minor_version = 0
    
    $buildid = 0

    #microsoft directories
    $microsoft_net_directory = [System.IO.Path]::Combine($env:windir,'Microsoft.NET','Framework','v4.0.30319')
    $microsoft_net64_directory = [System.IO.Path]::Combine($env:windir,'Microsoft.NET','Framework64','v4.0.30319')
    $microsoft_interop_directory = [System.IO.Path]::Combine(${env:ProgramFiles(x86)},'Microsoft.NET')
    $microsoft_vs_directory = [System.IO.Path]::Combine($env:VS110COMNTOOLS,'Common7','Tools')
    $windows_sdk_directory = [System.IO.Path]::Combine(${env:ProgramFiles(x86)}, 'Microsoft SDKs', 'Windows', 'v7.0A')
    
    $msbuild_exe = [System.IO.Path]::Combine( $microsoft_net64_directory,'MSBuild.exe')
    $signtool_exe = [System.IO.Path]::Combine( $windows_sdk_directory, 'Bin','signtool.exe')

    #nunit variables
    $NUnit = [System.IO.Path]::Combine($vendor_directory, 'NUnit', '2.5.10.11092','bin','net-2.0', 'nunit-console.exe')
    $NUnit_x86 = [System.IO.Path]::Combine($vendor_directory, 'NUnit', '2.5.10.11092','bin','net-2.0', 'nunit-console-x86.exe')

    #build variables
    $verbosity ="normal" 
    $inputfile = [System.IO.Path]::Combine($development_scripts_directory, 'build.xml')
    $targetsfile = [System.IO.Path]::Combine($development_scripts_directory, 'msbuild.targets')
    $dependencygraph = [System.IO.Path]::Combine($development_scripts_directory, 'DependencyGraph.xml')
    $internaldlls = [System.IO.Path]::Combine($development_scripts_directory, 'dlls.txt')
    $logfile = [System.IO.Path]::Combine($root, 'build.log')
    $logfilewarn = [System.IO.Path]::Combine($root, 'buildwarnings.log')
    $logfileerror = [System.IO.Path]::Combine($root, 'builderrors.log')
    $diagnostic ="false"

    #signing variables
    $signscript = [System.IO.Path]::Combine($development_scripts_directory, 'sign.bat')

    #nuget variables
    $nuspec_directory = [System.IO.Path]::Combine($development_scripts_directory,'NuGet')
    $nuget_exe_directory = [System.IO.Path]::Combine($vendor_directory,'NuGet')
    $nuget_exe = [System.IO.Path]::Combine($nuget_exe_directory,'NuGet.exe')
    $nuget_server = 'http://dv-scm-nuget.kcura.corp/NuGet/'
    $nuget_version = $version

    #build tool variables    
    $buildhelper_exe = [System.IO.Path]::Combine($development_scripts_directory, 'kCura.BuildHelper.exe')
    $rapbuilder_exe = [System.IO.Path]::Combine($development_scripts_directory, 'kCura.RAPBuilder.exe')
    $testrunner_exe = [System.IO.Path]::Combine($development_scripts_directory, 'kCura.TestRunner.exe')
    $buildeditor_exe = [System.IO.Path]::Combine($development_scripts_directory, 'kCura.BuildToolsEditor.exe')

    #package variable
    $package_root_directory = [System.IO.Path]::Combine($root, 'Packages')
}