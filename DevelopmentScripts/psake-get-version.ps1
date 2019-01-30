. .\psake-common.ps1


task default -depends getversion


task getversion {
    exec {
        $buildVersionParams = @{
            Product = $product
            Project = $project
            MajorVersion = $major_version
            MinorVersion = $minor_version
            ServerInstance = $server
            Database = $database
            BuildType = $build_type
            ServerType = $server_type
        }

        $version = & (Join-Path $PSScriptRoot 'New-TeamCityBuildVersion.ps1') @buildVersionParams

        Write-Host "##teamcity[buildNumber '$version']"

        if ($server_type -eq 'local')
        {
            [System.IO.File]::WriteAllText([System.IO.Path]::Combine($development_scripts_directory, 'version.txt'), $version)
        }
    }
}