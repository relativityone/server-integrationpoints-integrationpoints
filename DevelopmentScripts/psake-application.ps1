. .\psake-common.ps1


task default -depends build_applications

task copy_libraries {
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'Castle.Windsor.3.3.0', 'lib', 'net45', 'Castle.Windsor.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'kCura.Apps.Common.Config.2.1.3', 'lib', 'net462', 'kCura.Apps.Common.Config.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'kCura.Apps.Common.Data.2.1.3', 'lib', 'net462', 'kCura.Apps.Common.Data.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'kCura.Apps.Common.Utils.2.1.3', 'lib', 'net462', 'kCura.Apps.Common.Utils.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'kCura.LongPath.1.0.10', 'lib', 'net462', 'kCura.LongPath.dll')) $lib_directory	
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'kCura.Relativity.Client.9.5.280.10', 'lib', 'net462', 'kCura.Relativity.Client.dll')) $lib_directory	
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'Relativity.ImportExport.9.5.280.10', 'lib', 'net462', 'kCura.Relativity.DataReaderClient.dll')) $lib_directory	
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'Relativity.ImportExport.9.5.280.10', 'lib', 'net462', 'kCura.WinEDDS.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'Relativity.ImportExport.9.5.280.10', 'lib', 'net462', 'kCura.WinEDDS.Core.dll')) $lib_directory	
    Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'Relativity.ImportExport.9.5.280.10', 'lib', 'net462', 'kCura.WinEDDS.TApi.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'relativity.transfer.client.1.1.4', 'lib', 'net462', 'Relativity.Transfer.Client.Aspera.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'relativity.transfer.client.1.1.4', 'lib', 'net462', 'Relativity.Transfer.Client.Core.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'relativity.transfer.client.1.1.4', 'lib', 'net462', 'Relativity.Transfer.Client.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'relativity.transfer.client.1.1.4', 'lib', 'net462', 'Relativity.Transfer.Client.FileShare.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'relativity.transfer.client.1.1.4', 'lib', 'net462', 'Relativity.Transfer.Client.Http.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'Polly.5.3.1', 'lib', 'net45', 'Polly.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'relativity.faspmanager.3.7.2.0', 'lib', 'net40', 'FaspManager.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'Newtonsoft.Json.6.0.8', 'lib', 'net45', 'Newtonsoft.Json.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'SSH.NET.2016.0.0', 'lib', 'net40', 'Renci.SshNet.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'SystemWrapper.Interfaces.0.19.0.115', 'lib', 'net45', 'SystemInterface.dll')) $lib_directory
	Copy-Item ([System.IO.Path]::Combine($nuget_packages_directory, 'ZetaLongPaths.1.0.0.16', 'lib', 'net40-full', 'ZetaLongPaths.dll')) $lib_directory
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





