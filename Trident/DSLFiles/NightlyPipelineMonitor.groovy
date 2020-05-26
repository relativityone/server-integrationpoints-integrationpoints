folder('DataTransfer-Jobs') {
}

folder('DataTransfer-Jobs/RelativitySync') {
}

folder('DataTransfer-Jobs/RelativitySync/Nightly') {
}

buildMonitorView('DataTransfer-Jobs/RelativitySync/Nightly/Monitor')
{
	description('All nighlty jobs')
	recurse(true)
	jobs {
		regex('/release-(.*)')	}
}