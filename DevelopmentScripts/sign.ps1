$SIGNTOOL = [System.IO.Path]::Combine(${env:ProgramFiles(x86)}, "Microsoft SDKs", "Windows", "v7.1A", "Bin", "signtool.exe")
$sites = @("http://timestamp.comodoca.com/authenticode",
		   "http://timestamp.verisign.com/scripts/timstamp.dll",
		   "http://tsa.starfieldtech.com")


function SignDLL($dll) {
    Write-Host "Checking" $dll

    & $SIGNTOOL verify /pa /q $dll
    $signed = $?
	
    if(-not $signed) {

		For($i =0; $i -lt 3; $i++) {
			ForEach($site in $sites){
				Write-Host "Attempting to sign" $dll "using" $site "..."
				$proc = start-process -filepath $SIGNTOOL -argumentlist "sign /a /t $site /d 'Relativity' /du 'http://www.relativity.com' $dll" -WindowStyle hidden -Wait -PassThru
				$signed = ($proc.ExitCode -eq 0)
			
				if($signed) {
					Write-Host "Signed" $dll "Successfully!"
					break
				}
			}  
			
			if($signed) {
				break
			}
		}

		if(-not $signed) {
			exit 1
		}
    }
    else{
        Write-Host $dll "is already signed!"
    }
}

ForEach($arg in $args) {
	ForEach($dll in $arg.Split(';')) {
		if([System.IO.File]::Exists($dll)){
			SignDLL $dll
		}
		else{
			Write-Host "Cannot sign file" $dll "because it doesn't exist!"
			exit 1
		}
	}    
}