. .\psake-common.ps1


task default -depends build


task build -depends build_initalize, build_projects {
 
}


task build_initalize {   
    ''
    ('='*25) + ' Build Parameters ' + ('='*25)
    'version      = ' + $version 
    'server type  = ' + $server_type 
    'build type   = ' + $build_type 
    'branch       = ' + $branch 
    'build config = ' + $build_config
    ''

    'Time: ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
    
    'Build Type and Server Type result in sign set to ' + ($build_type -ne 'DEV' -and $server_type -ne 'local')  

    if([System.IO.Directory]::Exists($buildlogs_directory)) {Remove-Item $buildlogs_directory -Recurse}
    [System.IO.Directory]::CreateDirectory($buildlogs_directory)
}


task get_buildhelper {
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
                             ('/vs:11.0'), 
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
                                                                                
task build_projects -depends create_build_script, restore_nuget {  
    exec {                                                                                
        &  $msbuild_exe @(($targetsfile),   
                         ('/property:SourceRoot=' + $root),
                         ('/property:Configuration=' + $build_config),	
                         ('/property:BuildProjectReferences=false'),		
                         ('/target:BuildTiers'),
                         ('/verbosity:' + $verbosity),
                         ('/nologo'),
                         ('/maxcpucount'), 
                         ('/dfl'),
                         ('/flp:LogFile=' + $logfile),
                         ('/flp2:warningsonly;LogFile=' + $logfilewarn),
                         ('/flp3:errorsonly;LogFile=' + $logfileerror))       
    } 
}



