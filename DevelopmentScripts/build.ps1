properties {
    #step variable
    $STEP = 'build'

    #directories
    $root = hg root

    $source_directory = [System.IO.Path]::Combine($root, 'Source')
    $development_scripts_directory = [System.IO.Path]::Combine($root, 'DevelopmentScripts')
    $version_directory = [System.IO.Path]::Combine($root, 'Version')
    $vendor_directory = [System.IO.Path]::Combine($root, 'Vendor')
    $robot_directory = [System.IO.Path]::Combine($root, 'Robot')

    #build variables
    $version = '1.0.0.0'
    $server_type = 'teambranch'
    $build_type = 'DEV'
    $branch = 'default'
    $build_config = "Debug"

    #microsoft directories
    $microsoft_net_directory = [System.IO.Path]::Combine($env:windir,'Microsoft.NET','Framework','v4.0.30319')
    $microsoft_net64_directory = [System.IO.Path]::Combine($env:windir,'Microsoft.NET','Framework64','v4.0.30319')
    $microsoft_interop_directory = [System.IO.Path]::Combine(${env:ProgramFiles(x86)},'Microsoft.NET')
    $microsoft_vs_directory = [System.IO.Path]::Combine($env:VS110COMNTOOLS,'Common7','Tools')
    $msbuild_exe = [System.IO.Path]::Combine( $microsoft_net64_directory,'MSBuild.exe')

    #build variables
    $verbosity ="normal" 
    $inputfile = [System.IO.Path]::Combine($development_scripts_directory, 'Projects.xml')
    $targetsfile = [System.IO.Path]::Combine($development_scripts_directory, 'msbuild.targets')
    $dependencygraph = [System.IO.Path]::Combine($development_scripts_directory, 'DependencyGraph.xml')
    $internaldlls = [System.IO.Path]::Combine($development_scripts_directory, 'dlls.txt')
    $logfile = [System.IO.Path]::Combine($root, 'build.log')

    #signing variables
    $signscript = [System.IO.Path]::Combine($development_scripts_directory, 'sign.bat')
    $sign = ($build_type -ne 'DEV' -and $server_type -ne 'local')

    #nuget variables
    $nuspec_directory = [System.IO.Path]::Combine($development_scripts_directory,'NuGet')
    $nuget_exe_directory = [System.IO.Path]::Combine($vendor_directory,'NuGet')
    $nuget_exe = [System.IO.Path]::Combine($nuget_exe_directory,'NuGet.exe')
    $nuget_server = 'http://dv-scm-nuget.kcura.corp/NuGet/'
    $nuget_version = $version
}

task default -depends version, build, test, nuget, package

task version -precondition { $STEP -eq 'version' } {
 
}

task build -precondition { $STEP -eq 'build' } -depends build_initalize, build_projects, build_applications {
 
}

task test -precondition { $STEP -eq 'test' } {
 
}

task nuget -precondition { $STEP -eq 'nuget' } {
 
}

task package -precondition { $STEP -eq 'package' } {
 
}



task build_initalize {   
    ''
    ('='*25) + ' Build Parameters ' + ('='*25)
    'version      = ' + $version 
    'server type  = ' + $server_type 
    'build type   = ' + $build_type 
    'branch       = ' + $branch 
    'build config = ' + $build_config
    ''

    'Time: ' + (Get-Date -Format 'yyy-MM-dd HH:mm:ss')
    
    'Build Type and Server Type result in sign set to ' + $sign   

    if([System.IO.File]::Exists($logfile)) {Remove-Item $logfile}
}


task get_buildhelper {    
    & $nuget_exe @('install', 'kCura.BuildHelper', '-ExcludeVersion')
    Copy-Item $development_scripts_directory\kCura.BuildHelper\lib\kCura.BuildHelper.exe $development_scripts_directory
    $script:buildhelper_exe = [System.IO.Path]::Combine($development_scripts_directory, 'kCura.BuildHelper.exe')
}

task create_build_script -depends get_buildhelper {   
    
    & $buildhelper_exe  @(('/source:' + $root), 
                          ('/input:' + $inputfile), 
                          ('/output:' + $targetsfile), 
                          ('/graph:' + $dependencygraph), 
                          ('/dllout:' + $internaldlls), 
                          ('/vs:11.0'), 
                          ('/sign:' + $sign), 
                          ('/signscript:' + $signScript ))
                                                                                
}                                                                               
                                                                                
task build_projects -depends create_build_script {  
                                                                                
    & $msbuild_exe @(($targetsfile),   
                     ('/property:SourceRoot=' + $root),
                     ('/property:Configuration=' + $buildconfig),	
                     ('/property:BuildProjectReferences=false'),		
                     ('/target:BuildTiers'),
                     ('/verbosity:' + $verbosity),
                     ('/nologo'),
                     ('/maxcpucount'), 
                     ('/flp1:LogFile=' + $logfile))                                                                          

}

task build_applications {
'build apps'
}


