. .\psake-common.ps1
. .\psake-test_dependencies.ps1

task default -depends build


task build -depends build_initalize, build_integration_points, build_rip_documentation, copy_dlls_to_lib_dir, copy_test_dlls_to_lib_dir, generate_validation_message_table, sign {
 
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
    'run_checkConfigureAwait= ' + $run_checkConfigureAwait
    'sq_target_branch= ' + $sq_target_branch
    'skip_tests    = ' + $skip_tests
    ''

    'Time: ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

    'Build Type and Server Type result in sign set to ' + ($build_type -ne 'DEV' -and $server_type -ne 'local')

    if([System.IO.Directory]::Exists($buildlogs_directory)) {Remove-Item $buildlogs_directory -Recurse}
    [System.IO.Directory]::CreateDirectory($buildlogs_directory)
}

task check_configureawait -depends get_configureawait_checker -precondition { return $RUN_CHECKCONFIGUREAWAIT } {
    & (Join-Path $development_scripts_directory "check-configureawait.ps1") -sourceDir $source_directory -toolsDir $package_root_directory -logsDir $artifacts_directory
}

task get_configureawait_checker {  
    if (![System.IO.File]::Exists($configureawait_checker_pkg)) {
        exec {
            & $nuget_exe @('install', 'ConfigureAwaitChecker.v9', '-ExcludeVersion', '-Version', $configureawait_checker_version, '-OutputDirectory', $package_root_directory)
        }
    }
    if (![System.IO.File]::Exists($resharper_commandlinetools_exe)) {
        exec {
            & $nuget_exe @('install', 'JetBrains.ReSharper.CommandLineTools', '-ExcludeVersion', '-Version', $resharper_commandlinetools_version, '-OutputDirectory', $package_root_directory)
        }
    }
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

task restore_nuget {
    dotnet --info

    write-host "executing get_paket"
    exec {
        if (& dotnet tool list -g | Select-String "paket") {
            Write-Host -f Yellow 'Skipping install of paket. It''s already installed'
        }
        else {
            dotnet tool install --global Paket --version 6.0.12 --add-source 'https://relativity.jfrog.io/artifactory/api/nuget/official-nuget-remote-cache'
        }
    }    

    exec { & paket restore }
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

task build_integration_points -depends restore_nuget, configure_paket {
    exec {
        Write-Host 'Using MSBuild' $msbuild_exe

        If(!(test-path $buildlogs_directory))
        {
            New-Item -Path $buildlogs_directory -ItemType Directory -Force
        }

        & $msbuild_exe @((Join-Path $source_directory 'kCura.IntegrationPoints.sln'),
                         ("/property:Configuration=${build_config}"),
                         ("/property:PublishWebProjects=True"),
                         ('/nodereuse:false'),
                         #('/target:BuildTiers'), # TODO: configurable
                         ('/verbosity:normal'), # TODO: configurable
                         ('/clp:ErrorsOnly'),
                         ('/nologo'),
                         ('/maxcpucount'), 
                         #('/dfl'), # ???
                         ('/flp:LogFile=' + $logfile + ';Verbosity=Normal'), # TODO: configurable
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
            "Source\Relativity.IntegrationPoints.Services\bin\x64\*.dll",
            "Source\Relativity.IntegrationPoints.Services\bin\x64\*.pdb",
            "Source\Relativity.IntegrationPoints.Services\bin\x64\*.xml",
            "Source\Relativity.IntegrationPoints.Services\bin\x64\*.config",
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
            "packages\System.Reactive\lib\net46\System.Reactive.dll"



    foreach ($file in $files)
    {
        $tmpPath = Join-Path -Path $root -ChildPath $file
        Copy-Item -path $tmpPath -Destination $tests_directory -Recurse -Force
    }

    $oiPathSrc = Join-Path -Path $root -ChildPath "Source\kCura.IntegrationPoint.Tests.Core\bin\x64\oi\*"
    $oiPathDest = Join-Path -Path $tests_directory -ChildPath "oi\"
    New-Item -Path $oiPathDest -ItemType "directory"
    Copy-Item -path $oiPathSrc -Destination $oiPathDest
}

task copy_test_dlls_to_lib_dir -depends create_lib_dir -precondition { return -not $skip_tests } {
    $test_files =
            "Source\kCura.IntegrationPoints.Common.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Common.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Common.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Common.Tests\bin\x64\*.config",
            "Source\Relativity.IntegrationPoints.FunctionalTests\SystemTests\SystemTestData",
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
            "Source\kCura.IntegrationPoints.Domain.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Domain.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Domain.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Domain.Tests.Integration\bin\x64\*.xml",
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
            "Source\Relativity.IntegrationPoints.Services.Tests\bin\x64\*.dll",
            "Source\Relativity.IntegrationPoints.Services.Tests\bin\x64\*.pdb",
            "Source\Relativity.IntegrationPoints.Services.Tests\bin\x64\*.xml",
            "Source\Relativity.IntegrationPoints.Services.Tests\bin\x64\*.config",
            "Source\Relativity.IntegrationPoints.Services.Tests.Integration\bin\x64\*.dll",
            "Source\Relativity.IntegrationPoints.Services.Tests.Integration\bin\x64\*.pdb",
            "Source\Relativity.IntegrationPoints.Services.Tests.Integration\bin\x64\*.config",
            "Source\Relativity.IntegrationPoints.Services.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.RelativitySync.Tests.Integration\bin\x64\net462\*.dll",
            "Source\kCura.IntegrationPoints.RelativitySync.Tests.Integration\bin\x64\net462\*.pdb",
            "Source\kCura.IntegrationPoints.RelativitySync.Tests.Integration\bin\x64\net462\*.config",
            "Source\kCura.IntegrationPoints.RelativitySync.Tests.Integration\bin\x64\net462\*.xml",
            "Source\kCura.IntegrationPoints.RelativitySync.Tests\bin\x64\net462\*.dll",
            "Source\kCura.IntegrationPoints.RelativitySync.Tests\bin\x64\net462\*.pdb",
            "Source\kCura.IntegrationPoints.RelativitySync.Tests\bin\x64\net462\*.config",
            "Source\kCura.IntegrationPoints.RelativitySync.Tests\bin\x64\net462\*.xml",
            "Source\kCura.IntegrationPoints.UITests\bin\x64\*.dll",
            "Source\kCura.IntegrationPoints.UITests\bin\x64\*.config",
            "Source\kCura.IntegrationPoints.UITests\bin\x64\*.pdb",
            "Source\kCura.IntegrationPoints.UITests\bin\x64\*.xml",
            "Source\kCura.IntegrationPoints.UITests\UiTestsConfig",
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
            "Source\kCura.ScheduleQueue.Core.Tests.Integration\bin\x64\*.xml",
			"Source\Rip.E2ETests\bin\x64\*.dll",
			"Source\Rip.E2ETests\bin\x64\*.pdb",
			"Source\Rip.E2ETests\bin\x64\*.config",
			"Source\Rip.E2ETests\bin\x64\*.xml",
			"Source\Relativity.IntegrationPoints.FunctionalTests\bin\x64\*.dll",
            "Source\Relativity.IntegrationPoints.FunctionalTests\bin\x64\*.pdb",
            "Source\Relativity.IntegrationPoints.FunctionalTests\bin\x64\*.config",
            "Source\Relativity.IntegrationPoints.FunctionalTests\bin\x64\*.xml"

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

task copy_web_drivers -depends create_lib_dir, build_integration_points -precondition { return -not $skip_tests } {
    Copy-Item -path $chromedriver_path -Destination $tests_directory
    Copy-Item -path $geckodriver_path -Destination $tests_directory
}

task start_sonar -depends get_sonarqube -precondition { return $RUN_SONARQUBE } {
    $args = @(
        'begin',
        ("/k:$sonarqube_project_key"),
        ("/n:$sonarqube_project_name"),
        ("/v:$branch_hash"),
        ("/s:$sonarqube_properties"),
        ("/d:sonar.branch.name=""$branch"""),
        ("/d:sonar.branch.target=""$sq_target_branch"""),
        ("/d:sonar.cs.nunit.reportsPaths=""$NUnit_TestOutputFile"""),
        ("/d:sonar.cs.dotcover.reportsPaths=""$dotCover_result"""))
    & $sonarqube_exe $args
}

task stop_sonar -precondition { return $RUN_SONARQUBE }{
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

task sign -precondition { ($build_type -ne 'DEV') -and ($server_type -ne 'local') } {
    foreach ($o in Get-ChildItem -Path $package_directory -Recurse  -Include '*.exe', '*.dll', '*.msi')
    {
        & $signscript @($o.FullName)
    }
}