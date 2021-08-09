folder('DataTransfer-Jobs') {
}

folder('DataTransfer-Jobs/RelativitySync') {
}


buildMonitorView('DataTransfer-Jobs/RelativitySync/Monitor')
{
	description('All nighlty jobs')
	recurse(true)
	jobs {
		regex('(Nightly.*)((release(.*))|(develop))')	
	}
	statusFilter(StatusFilter.ENABLED)
}