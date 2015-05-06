. .\common.ps1


task default -depends package, sign


task package_initalize {
    $script:package_directory = [System.IO.Path]::Combine($package_root_directory, $Product, $branch, $version)
    $script:package_bin_directory = [System.IO.Path]::Combine($package_directory, 'bin')
    $script:package_nuget_directory = [System.IO.Path]::Combine($package_directory, 'NuGet')
    $script:package_pdb_directory = [System.IO.Path]::Combine($package_directory, 'PDBs - INTERNAL USE ONLY')

    [System.IO.Directory]::CreateDirectory($package_directory)
    [System.IO.Directory]::CreateDirectory($package_bin_directory)
    [System.IO.Directory]::CreateDirectory($package_nuget_directory)
    [System.IO.Directory]::CreateDirectory($package_pdb_directory)
}

task package -depends package_initalize <# package_sample #> { 

    Copy-Item -Path ([System.IO.Path]::Combine($nuspec_directory, '*')) -Destination $package_nuget_directory -Include '*.nupkg'
}

<# use as template

task package_sample {
    Copy-Item -Path ([System.IO.Path]::Combine($source_directory, 'kCura.Sample', 'bin', '*')) -Destination $package_bin_directory -Include '*.exe' '*.dll'
    Copy-Item -Path ([System.IO.Path]::Combine($source_directory, 'kCura.Sample', 'bin', '*')) -Destination $package_pdb_directory -Include '*.pdb'
}

#>

task sign -precondition { ($build_type -ne 'DEV') -and ($server_type -ne 'local') } {
    foreach($o in Get-ChildItem -Path $package_directory -Recurse) {
        exec {
            & $signscript @($o.FullName, $signtool_exe)
        }
    }
}
