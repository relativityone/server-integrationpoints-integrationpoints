. .\psake-common.ps1


task default -depends build_applications

task get_rapbuilder -precondition { (-not [System.IO.File]::Exists($rapbuilder_exe)) } {
	exec {
		& $nuget_exe @('install', 'kCura.RAPBuilder', '-ExcludeVersion')
	}   
	Copy-Item ([System.IO.Path]::Combine($development_scripts_directory, 'kCura.RAPBuilder', 'lib', 'kCura.RAPBuilder.exe')) $development_scripts_directory
}

task build_applications -depends get_rapbuilder {
	Write-Host "DEBUG: rapbuilder_args"
	Write-Host '/source:' $root
	Write-Host '/input:' $inputfile
	Write-Host '/version:' $version
	Write-Host '/servertype:' $server_type
	Write-Host '/debug:' $diagnostic
	Write-Host '/internaldlls:' $internaldlls
	Write-Host '/sign:' ($build_type -ne 'DEV' -and $server_type -ne 'local')
	Write-Host '/signscript:' $signScript
  exec {
		& $rapbuilder_exe @(('/source:' + $root),
							('/input:' + $inputfile),                              
							('/version:' + $version),
							('/servertype:' + $server_type), 
							('/debug:' + $diagnostic),  
							('/sign:' + ($build_type -ne 'DEV' -and $server_type -ne 'local')), 
							('/signscript:' + $signScript))
	}
}





