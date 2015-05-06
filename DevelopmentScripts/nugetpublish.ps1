. .\common.ps1


task default -depends nuget_publish

task nuget_publish -precondition { ($build_type -eq 'GOLD') -and ($branch -eq 'default' -or $branch.startsWith('release')) } {
     foreach($o in Get-ChildItem $nuspec_directory){
        
        if($o.Extension -ne '.nupkg') {continue}

        Write-Host "publishing" $o.FullName "..."

        exec {
            & $nuget_exe @('push', $o.FullName, '-Source', $nuget_server)
        }
     }    
}
