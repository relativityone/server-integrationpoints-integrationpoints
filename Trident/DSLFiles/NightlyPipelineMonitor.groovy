folder('DataTransfer-Jobs') {
}

folder('DataTransfer-Jobs/RelativitySync') {
}

buildMonitorView('DataTransfer-Jobs/RelativitySync/Nightly/Build monitor') {
	description('All nightly jobs')
    recurse(true)
    jobs {
		regex('/release-(.*)')
    }
}