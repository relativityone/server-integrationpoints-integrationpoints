. .\psake-common.ps1


task default -depends test


task test_initalize {

    If([System.IO.Directory]::Exists($testlog_directory)) {
        [System.IO.Directory]::Delete($testlog_directory, $true)
    }

    [System.IO.Directory]::CreateDirectory($testlog_directory)  
}

task get_testrunner {
    exec {
        & $nuget_exe @('install', 'kCura.TestRunner', '-ExcludeVersion')
    }    
    Copy-Item ([System.IO.Path]::Combine($development_scripts_directory, 'kCura.TestRunner', 'lib', 'kCura.TestRunner.exe')) $development_scripts_directory
}

task get_nunit {
    exec {
        & $nuget_exe @('install', 'NUnit.Runners', '-Version', '2.6.4', '-ExcludeVersion')
    } 
}

task test -depends get_testrunner, get_nunit, test_initalize {
    exec {
        & $testrunner_exe @(('/source:' + $root), 
                            ('/tests:' + $inputfile), 
                            ('/out:' + $testlog_directory), 
                            ('/nunitx64:' + $NUnit), 
                            ('/nunitx86:' + $NUnit_x86))
    }
}



