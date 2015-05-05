. .\common.ps1


task default -depends test


task get_testrunner {
    & $nuget_exe @('install', 'kCura.TestRunner', '-ExcludeVersion')
    Copy-Item ([System.IO.Path]::Combine($development_scripts_directory, 'kCura.TestRunner', 'lib', 'kCura.TestRunner.exe')) $development_scripts_directory
}

task test {
 
}



