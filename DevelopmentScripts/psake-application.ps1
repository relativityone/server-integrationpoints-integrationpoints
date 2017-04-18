. .\psake-common.ps1


task default -depends build_applications

task copy_libraries {
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'Castle.Windsor.3.3.0', 'lib', 'net45', 'Castle.Windsor.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'Newtonsoft.Json.6.0.8', 'lib', 'net45', 'Newtonsoft.Json.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'SSH.NET.2013.4.7', 'lib', 'net40', 'Renci.SshNet.dll')) $lib_directory
}

task get_rapbuilder -precondition { (-not [System.IO.File]::Exists($rapbuilder_exe)) } {
    exec {
        & $nuget_exe @('install', 'kCura.RAPBuilder', '-ExcludeVersion')
    }   
    Copy-Item ([System.IO.Path]::Combine($development_scripts_directory, 'kCura.RAPBuilder', 'lib', 'kCura.RAPBuilder.exe')) $development_scripts_directory
}

task build_applications -depends get_rapbuilder, copy_libraries {
  exec {
        & $rapbuilder_exe @(('/source:' + $root),
                            ('/input:' + $inputfile),                              
                            ('/version:' + $version),
                            ('/servertype:' + $server_type), 
                            ('/debug:' + $diagnostic),  
                            ('/internaldlls:' + $internaldlls),
                            ('/sign:' + ($build_type -ne 'DEV' -and $server_type -ne 'local')), 
                            ('/signscript:' + $signScript))
    }
}





