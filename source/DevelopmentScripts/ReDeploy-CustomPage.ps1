Invoke-Sqlcmd -Query "Update EDDS.eddsdbo.ApplicationServer set state = 0 where AppGuid = 'DCF6E9D1-22B6-4DA3-98F6-41381E93C30C'" -ServerInstance "localhost" -Verbose