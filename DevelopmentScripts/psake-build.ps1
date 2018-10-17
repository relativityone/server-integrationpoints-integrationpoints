. .\psake-common.ps1


task default -depends build


task build -depends build_initalize, start_sonar, build_projects, build_rip_documentation, copy_chrome_driver, stop_sonar, generate_validation_message_table {
 
}


task build_initalize {   
    ''
    ('='*25) + ' Build Parameters ' + ('='*25)
    'version      = ' + $version 
    'server type  = ' + $server_type 
    'build type   = ' + $build_type 
    'branch       = ' + $branch 
    'build config = ' + $build_config
    'enable_injections = ' + $enable_injections
    'run_sonarqube = ' + $run_sonarqube
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
        & $nuget_exe @('install', 'kCura.BuildHelper', '-ExcludeVersion')
    }      
    Copy-Item ([System.IO.Path]::Combine($development_scripts_directory, 'kCura.BuildHelper', 'lib', 'kCura.BuildHelper.exe')) $development_scripts_directory
}

task create_build_script -depends get_buildhelper {   
    exec {
        & $buildhelper_exe @(('/source:' + $root), 
                             ('/input:' + $inputfile), 
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
                                                                                
task build_projects -depends create_build_script, restore_nuget, configure_paket{  
    exec {     
        if ($build_type -eq 'DEV' -And $enable_injections) {
            $Injections = 'EnableInjections'
        }
        	
        Write-Host 'Based on' $build_type 'Injection is set to' $Injections

        Write-Host 'Using MSBuild' $msbuild_exe 'with targets file' $targetsfile
        
        &  $msbuild_exe @(($targetsfile),   
                         ('/property:SourceRoot=' + $root),
                         ('/property:Configuration=' + $build_config),    
                         ('/property:BuildProjectReferences=false'),    
                         ('/property:Injections=' + $Injections),
                         ('/nodereuse:false'),                         
                         ('/target:BuildTiers'),
						 ('/verbosity:quiet'),
						 ('/property:WarningLevel=1'),
                         ('/nologo'),
                         ('/maxcpucount'), 
                         ('/dfl'),
                         ('/flp:LogFile=' + $logfile),
                         ('/flp2:warningsonly;LogFile=' + $logfilewarn),
                         ('/flp3:errorsonly;LogFile=' + $logfileerror))       
    } 
}

task build_rip_documentation {
    # & nant package_documentation -buildfile:$root\DevelopmentScripts\build.build "-D:root=$root" "-D:buildconfig=$BUILDCONFIG" "-D:action=package_documentation" "-D:buildType=$BUILDTYPE"
    Write-Warning "Ignoring nant command (documentation build step). Please add nant to nuget packages and rewrite this task if needed."
}

task copy_chrome_driver -depends build_projects{
    If(!(test-path $tests_directory))
    {
        New-Item -Path $tests_directory -ItemType "directory"
    }
	Copy-Item -path $chromedriver_path -Destination $tests_directory
}

task start_sonar -depends get_sonarqube -precondition { return $RUN_SONARQUBE } {  
    $args = @(
        'begin',
        ("/k:$sonarqube_project_key"),
        ("/n:$sonarqube_project_name"),
        ("/v:$branch_hash"),
        ("/s:$sonarqube_properties"))
    & $sonarqube_exe $args
}

task stop_sonar -precondition { return $RUN_SONARQUBE }{
    $args = @(
        'end')

    & $sonarqube_exe $args
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
