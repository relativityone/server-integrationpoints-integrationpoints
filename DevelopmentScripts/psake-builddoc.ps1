. .\psake-common.ps1


task default -depends build_doc

task build_doc  -precondition { $build_type -ne 'DEV' }{  
    exec {
        &  $msbuild_exe @(($targetsfile),   
                         ('/property:SourceRoot=' + $root),
                         ('/property:Configuration=' + $build_config),
                         ('/property:BuildProjectReferences=false'),
                         ('/target:BuildDoc'),
                         ('/verbosity:' + $verbosity),
                         ('/nologo'),
                         ('/maxcpucount'), 
                         ('/dfl'),
                         ('/flp:LogFile=' + $logfile),
                         ('/flp2:warningsonly;LogFile=' + $logfilewarn),
                         ('/flp3:errorsonly;LogFile=' + $logfileerror))     
    } 
}



