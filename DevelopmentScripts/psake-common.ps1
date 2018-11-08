properties {
    #directories
    $root = (Get-Item $PSScriptRoot).parent.FullName

    $source_directory = [System.IO.Path]::Combine($root, 'Source')
    $application_directory = [System.IO.Path]::Combine($root, 'Applications')
    $application_xml_directory = [System.IO.Path]::Combine($root, 'ApplicationsXML')
    $development_scripts_directory = [System.IO.Path]::Combine($root, 'DevelopmentScripts')
    $version_directory = [System.IO.Path]::Combine($root, 'Version')
    $vendor_directory = [System.IO.Path]::Combine($root, 'Vendor')
    $robot_directory = [System.IO.Path]::Combine($root, 'Robot')
    $testlog_directory = [System.IO.Path]::Combine($root, 'TestLogs')
    $buildlogs_directory = [System.IO.Path]::Combine($root, 'BuildLogs')
    $pdb_directory = [System.IO.Path]::Combine($root, 'PDBs')
    $doc_directory = [System.IO.Path]::Combine($root, 'Documentation')
    $nuget_packages_directory = [System.IO.Path]::Combine($root, 'packages')
    $lib_directory = [System.IO.Path]::Combine($root, 'lib')
    $tests_directory = [System.IO.Path]::Combine($lib_directory, 'UnitTests')
    
    #build variables
    $version = '1.0.0.0'
    $server_type = 'teambranch'
    $build_type = 'DEV'
    $run_sonarQube = $false 

    $git = Test-Path -Path ([System.IO.Path]::Combine($root, '.git'))
    if ($git) {
        $branch = git rev-parse --abbrev-ref HEAD
        $branch_hash = git rev-parse --short HEAD
    }
    else {
        $branch = "unknown"
        $branch_hash = "unknown"
    }
    $build_config = "Debug"

    #assembly info variables
    $company = 'Relativity ODA LLC'
    $product = 'kCura.IntegrationPoints'
    $product_description = 'kCura.IntegrationPoints'

    #versioning database info
    $server = 'bld-mstr-01.kcura.corp'
    $database ='TCBuildVersion'
    $project = 'Development'
    $major_version = (Get-Content ..\Version\version.txt).split(".")[0]
    $minor_version = (Get-Content ..\Version\version.txt).split(".")[1]
    
    $buildid = 0

    #microsoft directories
    $microsoft_net_directory = [System.IO.Path]::Combine($env:windir,'Microsoft.NET','Framework','v4.0.30319')
    $microsoft_net64_directory = [System.IO.Path]::Combine($env:windir,'Microsoft.NET','Framework64','v4.0.30319')
    $microsoft_interop_directory = [System.IO.Path]::Combine(${env:ProgramFiles(x86)},'Microsoft.NET')
    $microsoft_vs_directory = [System.IO.Path]::Combine($env:VS110COMNTOOLS,'Common7','Tools')
    $windows_sdk_directory = [System.IO.Path]::Combine(${env:ProgramFiles(x86)}, 'Microsoft SDKs', 'Windows', 'v7.0A')
    
    $msbuild_exe = [System.IO.Path]::Combine(${env:ProgramFiles(x86)}, 'MSBuild', '14.0', 'Bin','MSBuild.exe')

    #nunit variables
    $NUnit = [System.IO.Path]::Combine($development_scripts_directory, 'NUnit.Runners', 'tools', 'nunit-console.exe')
    $NUnit_x86 = [System.IO.Path]::Combine($development_scripts_directory, 'NUnit.Runners', 'tools', 'nunit-console-x86.exe')
    $NUnit3 = [System.IO.Path]::Combine($development_scripts_directory, 'NUnit.ConsoleRunner', 'tools', 'nunit3-console.exe')    
    $NUnit_TestOutputFile = [System.IO.Path]::Combine($testlog_directory, 'Test_Output_x86.xml')

    #build variables
    $verbosity ="normal" 
    $inputfile = [System.IO.Path]::Combine($development_scripts_directory, 'build.xml')
    $targetsfile = [System.IO.Path]::Combine($development_scripts_directory, 'msbuild.targets')
    $dependencygraph = [System.IO.Path]::Combine($development_scripts_directory, 'DependencyGraph.xml')
    $internaldlls = [System.IO.Path]::Combine($development_scripts_directory, 'dlls.txt')
    $logfile = [System.IO.Path]::Combine($buildlogs_directory, 'build.log')
    $logfilewarn = [System.IO.Path]::Combine($buildlogs_directory, 'buildwarnings.log')
    $logfileerror = [System.IO.Path]::Combine($buildlogs_directory, 'builderrors.log')
    $diagnostic = "false"

    #signing variables
    $signscript = [System.IO.Path]::Combine($development_scripts_directory, 'sign.ps1')

    #nuget variables
    $nuspec_directory = [System.IO.Path]::Combine($development_scripts_directory,'NuGet')
    $nuget_exe_directory = [System.IO.Path]::Combine($vendor_directory,'NuGet')
    $nuget_exe = [System.IO.Path]::Combine($nuget_exe_directory,'NuGet.exe')
    $proget_server = 'https://proget.kcura.corp/nuget/NuGet'
    $nuget_version = $version
	
	#paket variables
	$paket_exe_directory = [System.IO.Path]::Combine($root, '.paket')
	$paket_exe = [System.IO.Path]::Combine($paket_exe_directory,'paket.exe')
	$paket_logfile = [System.IO.Path]::Combine($buildlogs_directory,'paket.log')
	$paket_config_directory = [System.IO.Path]::Combine($ENV:APPDATA, 'Paket')

    #build tool variables    
    $buildhelper_exe = [System.IO.Path]::Combine($development_scripts_directory, 'kCura.BuildHelper.exe')
    $rapbuilder_exe = [System.IO.Path]::Combine($development_scripts_directory, 'kCura.RAPBuilder.exe')
    $testrunner_exe = [System.IO.Path]::Combine($development_scripts_directory, 'kCura.TestRunner.exe')
    $buildeditor_exe = [System.IO.Path]::Combine($development_scripts_directory, 'kCura.BuildToolsEditor.exe')

    #package variable
    $package_root_directory = [System.IO.Path]::Combine($root, 'Packages')

    #sonarqube variables
    $sonarqube_exe = [System.IO.Path]::Combine($development_scripts_directory, 'MSBuild.SonarQube.Runner.Tool', 'tools', 'sonar-scanner-msbuild-4.3.1.1372-net46', 'SonarScanner.MSBuild.exe')
    $sonarqube_version = "4.3.1"
    $sonarqube_project_key = "kCura.IntegrationPoints"
    $sonarqube_project_name = "IntegrationPoints"
    $sonarqube_properties = [System.IO.Path]::Combine($development_scripts_directory, "sonarqube", "SonarQube.Analysis.xml")

    #dotCover variables
    $dotCover_exe = [System.IO.Path]::Combine($development_scripts_directory, 'JetBrains.dotCover.CommandLineTools', 'tools', 'dotCover.exe')
    $dotCover_result = [System.IO.Path]::Combine($development_scripts_directory, 'AppCoverageReport.html')

    #chromedriver
    $chromedriver_path = [System.IO.Path]::Combine($nuget_packages_directory, 'Selenium.WebDriver.ChromeDriver', 'driver', 'win32', 'chromedriver.exe')
    
    #test variables
    $tests_project_file = [System.IO.Path]::Combine($development_scripts_directory, 'IntegrationPointsTests.nunit')

    function validateVersions
    {
        $version_file_path = [System.IO.Path]::Combine($version_directory, 'version.txt')

        $version_text = Get-Content $version_file_path

        if($version_text -eq $null)
        {
            Write-Error "ERROR: Version is not defined in " $version_file_path
            $psake.build_success = $false
            exit 1
        }

        $version_array = $version_text.split(".")

        if($version_array.Length -ne 2)
        {
            Write-Error "ERROR: Version is not defined in " $version_file_path ".  Versions must defined with syntax '#.#'"
            $psake.build_success = $false
            exit 1
        }

        $maj_version = $version_array[0]
        $min_version = $version_array[1]

        #check if these are both numbers
        if(-not ($maj_version -match "[0-9]") -or -not ($min_version -match "[0-9]"))
        {
            Write-Error "ERROR: Version is not defined in " $version_file_path ".  Versions must defined with syntax '#.#'"
            $psake.build_success = $false
            exit 1
        }

        $psake.build_success = $true
    }

    # if Git is not present - we are on a test node on CI server
    if ($GIT) {
        validateVersions
    }
}