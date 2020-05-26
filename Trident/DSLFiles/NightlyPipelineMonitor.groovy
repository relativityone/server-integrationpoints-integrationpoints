folder('DataTransfer-Jobs') {
}

folder('DataTransfer-Jobs/RelativitySync') {
}

buildMonitorView('DataTransfer-Jobs/RelativitySync/Nightly')
{
	description('All nighlty jobs')
	recurse(true)
	jobs {
		regex('/release-(.*)')	}
}