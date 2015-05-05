. .\common.ps1


task default -depends build_applications

task get_rapbuilder {
    exec {
        & $nuget_exe @('install', 'kCura.RAPBuilder', '-ExcludeVersion')
    }   
    Copy-Item ([System.IO.Path]::Combine($development_scripts_directory, 'kCura.RAPBuilder', 'lib', 'kCura.RAPBuilder.exe')) $development_scripts_directory
}

task build_applications -depends get_rapbuilder {
  
}


<# use as template

task build_app1 {
    exec {
        & $rapbuilder_exe @(($targetsfile),   
                            ('/destinationpath:' + [System.IO.Path]::Combine($application_directory, 'kCura.Project1.rap')),
                            ('/applicationschema:' + [System.IO.Path]::Combine($application_directory, 'kCura.Project1.xml')), 
                            ('/assembly:' + [System.IO.Path]::Combine($source_directory, 'kCura.Project1', 'bin', 'kCura.Project1.dll')),		
                            ('/resourcefile:' + [System.IO.Path]::Combine($source_directory, 'kCura.Project1', 'bin','Image1.png')),			
                            ('/custompage:11111111-1111-1111-1111-111111111111=' + [System.IO.Path]::Combine($source_directory, 'CustomPages','kCura.Project1')),
                            ('/applicationversion:' + $version),
                            ('/servertype:' + $serverType), 
                            ('/debug:' + $diagnostic),  
                            ('/internaldlls:' + $dllout),
                            ('/sign:' + $sign),
                            ('/signscript:' + $signScript))
    }
}

#>