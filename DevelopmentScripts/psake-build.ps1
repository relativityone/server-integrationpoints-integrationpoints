. .\psake-common.ps1
. .\psake-test_dependencies.ps1

task default -depends build


task build -depends build_initalize, start_sonar, build_projects, build_rip_documentation, copy_dlls_to_lib_dir, copy_test_dlls_to_lib_dir, copy_web_drivers, run_coverage, stop_sonar, generate_validation_message_table {
 
}


task build_initalize {   
    ''
    ('='*25) + ' Build Parameters ' + ('='*25)
    'version       = ' + $version 
    'server type   = ' + $server_type 
    'build type    = ' + $build_type 
    'branch        = ' + $branch 
    'build config  = ' + $build_config
    'run_sonarqube = ' + $run_sonarqube
    'skip_tests    = ' + $skip_tests
    ''

    'Time: ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
    
    'Build Type and Server Type result in sign set to ' + ($build_type -ne 'DEV' -and $server_type -ne 'local')  

    if([System.IO.Directory]::Exists($buildlogs_directory)) {Remove-Item $buildlogs_directory -Recurse}
    [System.IO.Directory]::CreateDirectory($buildlogs_directory)
}

task get_sonarqube -precondition { (-not [System.IO.File]::Exists($sonarCube_exe)) } {    
    exec {
    & $nuget_exe @('install', 'MSBuild.SonarQube.Runner.Tool', '-ExcludeVersion', '-Version', $sonarqube_version)
    }      
}

task get_buildhelper -precondition { (-not [System.IO.File]::Exists($buildhelper_exe)) } {
    exec {
        & $nuget_exe @('install', 'kCura.BuildHelper', '-ExcludeVersion', '-Source', $proget_server)
    }      
    Copy-Item ([System.IO.Path]::Combine($development_scripts_directory, 'kCura.BuildHelper', 'lib', 'kCura.BuildHelper.exe')) $development_scripts_directory
}

task create_build_script -depends get_buildhelper { 
    $build_input_file = $inputfile; 
    if ($skip_tests)
    {
        $build_input_file = $inputfile_noTests
    }

    exec {
        & $buildhelper_exe @(('/source:' + $root), 
                             ('/input:' + $build_input_file), 
                             ('/output:' + $targetsfile), 
                             ('/graph:' + $dependencygraph), 
                             ('/dllout:' + $internaldlls), 
                             ('/vs:14.0'), 
                             ('/sign:' + ($build_type -ne 'DEV' -and $server_type -ne 'local')), 
                             ('/signscript:' + $signScript))
    }                                                                      
}  

task restore_nuget {

    foreach($o in Get-ChildItem $source_directory){
       
       if($o.Extension -ne '.sln') {continue}

        exec {
            & $nuget_exe @('restore', $o.FullName)
        } 
    }   
} 

task configure_paket {

	if (Get-ChildItem ENV:PaketUserName -ErrorAction SilentlyContinue) {
		exec {
			
			Write-Host 'Configuring credentials for ProGet server.'
			
			Remove-Item $paket_config_directory\*
			
			& $paket_exe config add-credentials $proget_server --username $ENV:PaketUserName --password $ENV:PaketPassword --authtype ntlm --log-file $paket_logfile --verbose
		} 
	} else {
		Write-Host 'Configuring credentials for ProGet server will be skipped'
	}
}                                                                             
                                                                                
task build_projects -depends create_build_script, restore_nuget, configure_paket {  
    exec {     
        Write-Host 'Using MSBuild' $msbuild_exe 'with targets file' $targetsfile

        If(!(test-path $buildlogs_directory))
        {
            New-Item -Path $buildlogs_directory -ItemType Directory -Force
        }
        
        &  $msbuild_exe @(($targetsfile),   
                         ('/property:SourceRoot=' + $root),
                         ('/property:Configuration=' + $build_config),    
                         ('/property:BuildProjectReferences=false'),    
                         ('/nodereuse:false'),                         
                         ('/target:BuildTiers'),
						 ('/verbosity:normal'),
                         ('/clp:ErrorsOnly'),
                         ('/nologo'),
                         ('/maxcpucount'), 
                         ('/dfl'),
                         ('/flp:LogFile=' + $logfile + ';Verbosity=Normal'),
                         ('/flp2:warningsonly;LogFile=' + $logfilewarn),
                         ('/flp3:errorsonly;LogFile=' + $logfileerror))       
    } 
}

task build_rip_documentation {
    # & nant package_documentation -buildfile:$root\DevelopmentScripts\build.build "-D:root=$root" "-D:buildconfig=$BUILDCONFIG" "-D:action=package_documentation" "-D:buildType=$BUILDTYPE"
    Write-Warning "Ignoring nant command (documentation build step). Please add nant to nuget packages and rewrite this task if needed."
}

task create_lib_dir {
    If (Test-Path $tests_directory)
    {
        Remove-Item -Recurse -Force $tests_directory
    }
    New-Item -Path $tests_directory -ItemType "directory"
}

task copy_dlls_to_lib_dir -depends create_lib_dir {
    $files =
            "Source\kCura.IntegrationPoints.Agent\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Agent\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Agent\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Agent\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Common\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Common\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Common\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Common\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Config\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Config\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Config\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Config\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Contracts\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Contracts\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Contracts\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Contracts\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Core.Contracts\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Core.Contracts\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Core.Contracts\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Core.Contracts\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Core\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Core\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Core\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Core\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Data\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Data\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Data\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Domain\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Domain\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Domain\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Domain\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Email\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Email\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Email\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Email\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.EventHandlers\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.EventHandlers\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.EventHandlers\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.EventHandlers\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Core\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Core\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Core\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Core\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FtpProvider\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FtpProvider\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FtpProvider\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FtpProvider\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FtpProvider.Connection\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FtpProvider.Connection\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FtpProvider.Connection\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FtpProvider.Connection\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FtpProvider.Helpers\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FtpProvider.Helpers\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FtpProvider.Helpers\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FtpProvider.Helpers\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FtpProvider.Parser\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FtpProvider.Parser\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FtpProvider.Parser\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FtpProvider.Parser\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.ImportProvider\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.ImportProvider\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.ImportProvider\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.ImportProvider\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.ImportProvider.Parser\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.ImportProvider.Parser\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.ImportProvider.Parser\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.ImportProvider.Parser\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.LDAPProvider\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.LDAPProvider\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.LDAPProvider\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.LDAPProvider\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Management\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Management\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Management\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Management\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Services\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Services\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Services\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Services\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Services.Interfaces.Private\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Services.Interfaces.Private\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Services.Interfaces.Private\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Services.Interfaces.Private\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.SourceProviderInstaller\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.SourceProviderInstaller\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.SourceProviderInstaller\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.SourceProviderInstaller\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Web\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Web\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Web\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Web\bin\x64\*.xml",
            "Source\kCura.ScheduleQueue.AgentBase\bin\x64\*.dll",
            "Source\kCura.ScheduleQueue.AgentBase\bin\x64\*.config",
            "Source\kCura.ScheduleQueue.AgentBase\bin\x64\*.pdb",
            "Source\kCura.ScheduleQueue.AgentBase\bin\x64\*.xml",
            "Source\kCura.ScheduleQueue.Core\bin\x64\*.dll",
            "Source\kCura.ScheduleQueue.Core\bin\x64\*.config",
            "Source\kCura.ScheduleQueue.Core\bin\x64\*.pdb",
            "Source\kCura.ScheduleQueue.Core\bin\x64\*.xml",
            "Source\Provider\bin\*.dll",
            "Source\Provider\bin\*.config",
            "Source\Provider\bin\*.xml",
            "Source\Provider\bin\*.pdb",
            "Source\JsonLoader\bin\*.dll",
            "Source\JsonLoader\bin\*.config",
            "Source\JsonLoader\bin\*.xml",
            "Source\JsonLoader\bin\*.pdb"

    

    foreach ($file in $files)
    {
        $tmpPath = Join-Path -Path $root -ChildPath $file        
        Copy-Item -path $tmpPath -Destination $tests_directory -Recurse -Force
    }
   
    $oiPathSrc = Join-Path -Path $root -ChildPath "Source\kCura.IntegrationPoint.Tests.Core\oi\unmanaged\*"
    $oiPathDest = Join-Path -Path $tests_directory -ChildPath "oi\"
    New-Item -Path $oiPathDest -ItemType "directory"
    Copy-Item -path $oiPathSrc -Destination $oiPathDest
}

task copy_test_dlls_to_lib_dir -depends create_lib_dir -precondition { return -not $skip_tests } {
    $test_files =
            "Source\kCura.IntegrationPoint.Tests.Core\ExternalDependencies",
            "Source\kCura.IntegrationPoint.Tests.Core\TestData",
            "Source\kCura.IntegrationPoint.Tests.Core\TestDataExtended",
            "Source\kCura.IntegrationPoint.Tests.Core\TestDataImportFromLoadFile",
            "Source\kCura.IntegrationPoint.Tests.Core\TestDataSaltPepper",
            "Source\kCura.IntegrationPoint.Tests.Core\TestDataText",
            "Source\kCura.IntegrationPoint.Tests.Core\app.config",
            "Source\kCura.IntegrationPoint.Tests.Core\bin\x64\*.dll",
            "Source\kCura.IntegrationPoint.Tests.Core\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoint.Tests.Core\bin\x64\*.xml",
            "Source\kCura.IntegrationPoint.Tests.Core\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Agent.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Agent.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Agent.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Agent.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Agent.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Agent.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Agent.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Agent.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Core.Contracts.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Core.Contracts.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Core.Contracts.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Core.Contracts.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Core.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Core.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Core.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Core.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Core.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Core.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Core.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Core.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Data.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Data.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Data.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Data.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Data.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Data.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Data.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Data.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.DocumentTransferProvider.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Domain.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Domain.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Domain.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Domain.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Email.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Email.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Email.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Email.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.EventHandlers.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.EventHandlers.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.EventHandlers.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.EventHandlers.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.EventHandlers.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.EventHandlers.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.EventHandlers.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.EventHandlers.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FtpProvider.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FtpProvider.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FtpProvider.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FtpProvider.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FtpProvider.Helpers.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FtpProvider.Helpers.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FtpProvider.Helpers.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FtpProvider.Helpers.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.FtpProvider.Parser.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.FtpProvider.Parser.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.FtpProvider.Parser.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.FtpProvider.Parser.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.ImportProvider.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.ImportProvider.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.ImportProvider.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.ImportProvider.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.ImportProvider.Parser.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.ImportProvider.Parser.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.ImportProvider.Parser.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.ImportProvider.Parser.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.ImportProvider.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.ImportProvider.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.ImportProvider.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.ImportProvider.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.ImportProvider.Tests.Integration\TestDataForImport",
            "Source\kCura.IntegrationPoints.LDAPProvider.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.LDAPProvider.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.LDAPProvider.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.LDAPProvider.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Management.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Management.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Management.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Management.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Services.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Services.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Services.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Services.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Services.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Services.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Services.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Services.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.UITests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.UITests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.UITests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.UITests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Web.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Web.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Web.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Web.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Web.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Web.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Web.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Web.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.ScheduleQueue.Core.Tests\bin\x64\*.dll",
            "Source\kCura.ScheduleQueue.Core.Tests\bin\x64\*.pdb",
            "Source\kCura.ScheduleQueue.Core.Tests\bin\x64\*.config",
            "Source\kCura.ScheduleQueue.Core.Tests\bin\x64\*.xml",
            "Source\kCura.ScheduleQueue.Core.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.ScheduleQueue.Core.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.ScheduleQueue.Core.Tests.Integration\bin\x64\*.config",
            "Source\kCura.ScheduleQueue.Core.Tests.Integration\bin\x64\*.xml"

    foreach ($file in $test_files)
    {
        $tmpPath = Join-Path -Path $root -ChildPath $file        
        Copy-Item -path $tmpPath -Destination $tests_directory -Recurse -Force
    }
    $testsConfigPath = Join-Path -Path $root -ChildPath "Source\kCura.IntegrationPoint.Tests.Core\app.config"
    $testsConfigDestinationPath = Join-Path -Path $tests_directory -ChildPath "IntegrationPointsTests.config"
    $testsConfigDestinationPath2 = Join-Path -Path $development_scripts_directory -ChildPath "IntegrationPointsTests.config"
    Copy-Item -path $testsConfigPath -Destination $testsConfigDestinationPath
    Copy-Item -path $testsConfigPath -Destination $testsConfigDestinationPath2
}

task copy_web_drivers -depends create_lib_dir, build_projects -precondition { return -not $skip_tests } {
	Copy-Item -path $chromedriver_path -Destination $tests_directory
	Copy-Item -path $geckodriver_path -Destination $tests_directory
}

task start_sonar -depends get_sonarqube -precondition { return $RUN_SONARQUBE -and [System.IO.File]::Exists($sonarCube_exe)} {     
    $args = @(
        'begin',
        ("/k:$sonarqube_project_key"),
        ("/n:$sonarqube_project_name"),
        ("/v:$branch_hash"),
        ("/s:$sonarqube_properties"),
        ("/d:sonar.cs.nunit.reportsPaths=""$NUnit_TestOutputFile"""),
        ("/d:sonar.cs.dotcover.reportsPaths=""$dotCover_result"""))
    & $sonarqube_exe $args    
}

task stop_sonar -precondition { return $RUN_SONARQUBE -and [System.IO.File]::Exists($sonarCube_exe)}{
    $args = @(
        'end')

    & $sonarqube_exe $args
}

task get_dotcover -precondition { (-not [System.IO.File]::Exists($dotCover_exe)) }{
    exec {
        & $nuget_exe @('install', 'JetBrains.dotCover.CommandLineTools', '-ExcludeVersion')
    }    
}

task run_coverage -depends get_dotcover, get_testrunner, get_nunit, test_initalize -precondition { return $RUN_SONARQUBE } -ContinueOnError {
    exec{
        $arg = @(
            "analyse",
            ("/TargetExecutable=$testrunner_exe"),
            ("/TargetArguments=/source:$root /tests:$inputfile /out:$testlog_directory /nunit3:$NUnit3"),
            ("/Output=""AppCoverageReport.html"""),
            ("/ReportType=""HTML""")
            ("/Filters=""-:*Test*"""))

        Write-Host $arg
        & $dotCover_exe $arg
        Write-Host "Coverage complete"
        
    }    
}

task generate_validation_message_table{
    $valDir = $source_directory + ".\kCura.IntegrationPoints.Core\Validation\"
    $xml = $valDir + "\ValidationMessages.xml"
    $xsl = $valDir + "\ValidationMessages.xsl"
    $output = $development_scripts_directory + "\ValidationMessages.html"
    $xslt = New-Object System.Xml.Xsl.XslCompiledTransform;
    $xslt.Load($xsl);
    $xslt.Transform($xml, $output);

    Write-Host "generated" +  $output;
}
