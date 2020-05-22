folder('DataTransfer-Jobs') {
}

folder('DataTransfer-Jobs/RelativitySync') {
}

buildMonitorView('DataTransfer-Jobs/RelativitySync/Nightly/Build monitor') {
	description(true)
	jobs {
		regex('/release-(.*)')
    }
}